# Evidence: waits and optimistic judgments

## The three dangerous patterns (original text)

```csharp
// WaitForMainMenuSubmenuOpenAsync (GameActionService.cs:5609-5630)
var currentScreen = ActiveScreenContext.Instance.GetCurrentScreen();
if (currentScreen is TSubmenu) { return true; }
if (!GodotObject.IsInstanceValid(screen)) { return true; }      // <- a destroyed main menu also counts as success

// WaitForCharacterSelectionTransitionAsync (:5826-5829)
if (!GodotObject.IsInstanceValid(screen)) { return true; }      // <- the same

// WaitForLobbyReadyTransitionAsync (:5880-5883)
if (!GodotObject.IsInstanceValid(screen)) { return ready; }     // <- false success when ready=true
```

`WaitForLobbyAscensionTransitionAsync` (:5896+) writes `return false;` at the same position, which shows that "unconfirmed means failure"
is this repo's established stance, and these three slipped through.

## Call sites

- `WaitForMainMenuSubmenuOpenAsync`: :540 (open_character_select), :588 (open_timeline),
  :4809 / :4816 (the `StartLocalFourPlayerHostAsync` invite chain).
- The `open_timeline` response decides `status` directly from that bool (:588-600).
- `WaitForLobbyReadyTransitionAsync`: the `ready_multiplayer_lobby` / `unready` paths.

## Current state of the four handlers

| Action | Line | Symptom |
|---|---|---|
| `select_deck_card` | :2050-2130 | The guard uses `CanSelectDeckCard`; the executor needs `TryGetCombatHandSelectionMetadata` or `TryGetCardGridSelectionMetadata` to hit |
| `confirm_timeline_overlay` | :702-800 | tutorial/unlock/other three branches, with no revalidation before clicking |
| `crystal_set_tool` | :1607-1645 | `TrySetCrystalSphereTool` returning true means `completed` |
| `run_console_command` | :4968-5018 | `consoleTimedOut` goes only into the message; `status = stable ? completed : pending` |

## False positive (do not fix)

The `mainMenu.SubmenuStack.GetSubmenuType<NCharacterSelectScreen>()` of `open_character_select`
(`GameActionService.cs:545`) **never returns null**: decompilation
`extraction/decompiled/MegaCrit.Sts2.Core.Nodes.Screens.MainMenu/NMainMenuSubmenuStack.cs:156-207`
shows the method is lazily created (`if (_x == null) { _x = X.Create(); AddChildSafely(...); } return _x;`).
B8 of the E2 audit report is a false positive.

## Live-only verification

Reachability of real overlays and post-click behavior, the timeline index contract: offline can only prove "no longer optimistic", not "succeeds on a real machine".
