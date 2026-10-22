# Evidence

## Deliverables

| File | Change |
|---|---|
| `STS2AIAgent/Game/MenuTransitionPolicy.cs` | Added pure policies `IsSubmenuObserved` (null -> false; observed only if the target type includes subclasses) and `IsFlagObserved` (source node invalid -> false) |
| `STS2AIAgent/Game/GameActionService.cs` | Three wait helpers changed to "node destroyed -> `break` + uniform final judgment"; four handlers tightened |
| `STS2AIAgent.Tests/MenuWaitObservationTests.cs` (new) | 4 tests (2 truth tables + 2 source contracts) |

### Specific changes

- `WaitForMainMenuSubmenuOpenAsync`: `return true` -> `break`, final judgment `IsSubmenuObserved(current screen type, typeof(TSubmenu))`; still `true` when the target really becomes the active screen.
- `WaitForCharacterSelectionTransitionAsync`: `return true` -> `break`, the final judgment requires the node to be still valid and the character id to match the request.
- `WaitForLobbyReadyTransitionAsync`: `return ready` -> `break` + `IsFlagObserved(sourceNodeValid, observed, requested)`; check `IsInstanceValid` before reading `isReady`.
- `select_deck_card` (:2097-2118): with no clickable target (combat hand metadata / card grid metadata / `NChooseACardSelectionScreen`) -> 409 `invalid_action`, no longer returning `pending`.
- `confirm_timeline_overlay` (:732-805): the three branches re-fetch the button before clicking and require `IsInstanceValid`+`IsVisibleInTree`+`IsEnabled`, throwing 503 when it cannot be fetched; the silent empty click of the tutorial branch became an explicit error.
- `crystal_set_tool` (:1658-1689): after the click read back `minigame.CrystalSphereTool == tool` and only then `completed`, otherwise `pending`.
- `run_console_command` (:5046-5064): `completed = screenStable && !consoleTimedOut`, with `status`/`stable` derived in sync.

## Acceptance

| Criterion | Evidence |
|---|---|
| The three dangerous patterns are gone | `MenuWaitObservation.NoLostNodeSuccess` PASS (source contract assertion) |
| Pure policy + truth table | `SubmenuType` (5 combinations), `SurvivingSource` (8 combinations) PASS |
| Four handlers | `ConsoleTimeout` PASS; the other three are covered by source review + `dotnet build` type checking |
| Falsifiable | Breaking all four at once (both policies returning true, the submenu wait `break`->`return true`, removing `!consoleTimedOut`) -> all 4 tests red (`PASS=307 FAIL=4`), and 311 PASS after restoring |

## Gates

C# unit tests **PASS=311 FAIL=0**; `dotnet build` 0 warnings 0 errors; `check_verification_gates.py` passed;
MCP 159 OK; `check_release_package.py` passed; preflight passed.

## Live-only verification / known deviations

- Whether the target submenu has already become the active screen when the main menu is destroyed (deciding whether `open_timeline` is `completed` or `pending`), reachability of real overlays, the timeline index contract.
- The 409 of `select_deck_card` is a caller-visible behavior change; `GameStateService.CanSelectDeckCard` still lists it in `available_actions` on screens of the same kind (**availability and executability remain asymmetric**; fully tightening it requires changing `GameStateService.cs`, beyond the scope of this task).
- One deliberate deviation from the literal requirement of the design: `NChooseACardSelectionScreen` is kept as a legal target (it has its own `WaitForChooseCardSelectionResolutionAsync` observation path), otherwise a legal click channel would be killed.
