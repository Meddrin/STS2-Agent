# Make the live verification assets match the current contract

## Background

The live verification assets under `scripts/` assert, in several places, contracts the mod does not currently provide. They can only run when the game is available,
so the contradictions lay dormant for a long time and would surface on a real machine as "failures" or "crashes". Four are confirmed:

1. `test-main-menu-active-run.ps1` hard-asserts that the active-save main menu must have `open_timeline` (`:92`,
   the wait condition at `:114`, the full timeline interaction sequence at `:117-152`). The python version gives the opposite conclusion for the same state
   (`run_sts2_validation.py:1548-1550`): with a save, the main menu timeline button is disabled
   (`NMainMenu.UpdateTimelineButtonBehavior`), and `open_timeline` is not part of the active-run contract;
   `:1827` again states that the timeline is opened only when there is no save. The two scripts make mutually exclusive assertions about the same state.
2. `run_sts2_validation.py:1935-1946` hardcodes the `option_index` of `choose_timeline_epoch` to 0.
   The executor contract (`TimelineIndexContractTests` pins `GameActionService.ResolveTimelineSlot`) requires the
   index to hit an actionable slot in `timeline.slots[]`; the slot list filters only `NotObtained`,
   so 0 may be a non-actionable slot -> 409 `invalid_target`.
3. The screen switch at `test-multiplayer-lobby-flow.ps1:655-733` lists only 5 kinds of screens and `throw`s
   `"Unsupported run progression state"` for everything else; normal map progression will meet `CAPSTONE_SELECTION` (act selection)
   and `UNLOCK`, and the script would crash midway.
4. Screen name coverage gap on the python side: `run_sts2_validation.py` recognizes only 9 screen names
   (MAIN_MENU / MODAL / REWARD / COMBAT / CARD_SELECTION / CHARACTER_SELECT / GAME_OVER /
   MULTIPLAYER_LOBBY / UNKNOWN), while `ResolveNonModalScreen` now returns 24 (see research for the comparison table);
   the other screens have no branch and would time out or be misjudged.

## Goals

Make the script assertions share a source with the state they read: no longer require actions the mod does not provide, no longer hardcode indices,
and check the screen branches against the return set of `ResolveNonModalScreen`.

## Acceptance criteria

- [x] `test-main-menu-active-run.ps1` and the python version agree on the timeline conclusion for the active-save main menu
      (neither side asserts that `open_timeline` must be available), and keep diagnostic output such as `open_timeline_available`.
- [x] The index of `choose_timeline_epoch` is taken from the first `is_actionable` slot among the state's `timeline.slots[].index`
      and is no longer hardcoded; leave a comment in the script pointing to `TimelineIndexContractTests`.
- [x] The screen switch of `test-multiplayer-lobby-flow.ps1` covers the screens normal map progression will meet
      (at least `CAPSTONE_SELECTION`, `UNLOCK`), and the fallback error carries the screen name and available actions.
- [x] The python side adds handling for screens that appear in the normal flow, or explicitly explains them (written into script comments);
      the final list of "which screen is consumed by which flow" goes into the evidence.
- [x] PowerShell scripts containing non-ASCII keep the UTF-8 BOM (the `script-encoding` gate),
      `python -m py_compile` passes, and the full gates are green.
- [x] Full C# / MCP tests, gates, and preflight pass.

## Out of scope

- Do not rewrite the scripts' advancement strategy (do not change the python debug-command route into a screen-by-screen UI route).
- Do not build a new offline test skeleton for behavior that can "only be verified live"; this round guarantees syntax, encoding, contract comments, and branch reachability.

