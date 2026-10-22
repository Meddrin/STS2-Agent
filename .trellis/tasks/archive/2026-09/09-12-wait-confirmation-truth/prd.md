# A vanished node does not mean the target was reached: fix the menu waits and four action handlers

## Background

The menu waits in `GameActionService.cs` treat "the node I was watching was destroyed" as "evidence that the target state was reached":

| Wait helper | Line | Dangerous pattern | Consequence |
|---|---|---|---|
| `WaitForMainMenuSubmenuOpenAsync` | :5609-5630 | `if (!IsInstanceValid(screen)) return true;` | A destroyed main menu (for example entering the game directly) also counts as "submenu opened" |
| `WaitForCharacterSelectionTransitionAsync` | :5826-5829 | The same `return true;` | The character select screen vanishing != that character being selected |
| `WaitForLobbyReadyTransitionAsync` | :5880-5883 | `return ready;` | When `ready=true` a destroyed room is reported as "ready" |

`open_timeline` (:588 `PushSubmenuType<NTimelineScreen>` -> wait) is currently the only directly affected action:
once the main menu is destroyed, `status` directly reports `"completed"`. `WaitForMainMenuSubmenuOpenAsync` also has
two other call sites: `open_character_select` (:540) and the invite chain (:4809/:4816).

Four handlers to be tightened in the same batch (all located, all cases of "the reported result is more optimistic than the facts"):

1. `select_deck_card` (:2028+): the guard is wider than the executor's recognition: `CanSelectDeckCard` lets it through but when neither the combat hand nor the card grid
   metadata matches it clicks nothing and then returns only `pending`.
2. `confirm_timeline_overlay` (:702+): the three overlay branches reuse the same click path and do not revalidate availability before clicking.
3. `crystal_set_tool` (:1607+): `status` is hardcoded to `"completed"`, and evidence is needed to confirm whether the return value of `TrySetCrystalSphereTool`
   is really equivalent to "the tool has switched".
4. `run_console_command` (:4924+): `consoleTimedOut` goes only into the message and not into `status`: after the command task times out,
   as long as the screen is stable it reports `completed`.

## Goals

1. The three wait rules change to "success must be **observed**": a vanished source node can only make the wait **stop**, not make it **succeed**.
2. The rules are expressed as pure policy and tested offline (the current test project cannot compile `GameActionService.cs`).
3. The four handlers: either give observable evidence of success, or explicitly report `pending`/409/503; silent optimism is not allowed.

## Acceptance criteria

- [x] The three dangerous patterns are gone, and the replacement logic returns false/pending when "the target state was not observed".
- [x] New/extended pure policy (`STS2AIAgent/Game/MenuTransitionPolicy.cs`, already compiled into the test project) carries the decision rules,
      with truth table tests (every input combination asserted; breaking it turns it red).
- [x] `select_deck_card`: when no clickable option can be located throw 409 `invalid_action` (must not return `pending` to cover it up).
- [x] `confirm_timeline_overlay`: revalidate that the target is available before each click; confirm by waiting after the click, and `pending` if unconfirmed.
- [x] `crystal_set_tool`: if the return value of `TrySetCrystalSphereTool` does not constitute evidence that "the tool has switched",
      change to read-back confirmation (read the current tool exposed by `GameStateService`), and still report `completed` on success.
- [x] `run_console_command`: must not report `completed` when the command task times out.
- [x] Full gates pass (C# unit tests, MCP unit tests, `check_verification_gates.py`, `check_release_package.py`, preflight).

## Out of scope

- Do not add error codes (keep using `invalid_action` / `invalid_request` / `invalid_target` / `state_unavailable`).
- Do not change `GetSubmenuType<NCharacterSelectScreen>()` of `open_character_select`: decompilation confirms it is lazily created
  and never returns null (see the research file), so B8 of the earlier audit was a false positive.
- Do not change the action contract block of `docs/api.md`.
