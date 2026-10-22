# Evidence

## Deliverables

| File | Change |
|---|---|
| `STS2AIAgent/Game/GameStateService.cs` | Added `using ...Screens.CardLibrary;`; `ResolveNonModalScreen` adds two early returns after `CARDS_VIEW` and before the generic grid (`NCardLibrary → "CARD_LIBRARY"`, `NCardPileScreen → "CARD_PILE"`); added the shared predicate `IsClosableCardViewer` (`NCardsViewScreen or NCardPileScreen`); `GetCardsViewBackButton` uses that predicate + `Node` to take `BackButton`; `GetMainMenuSubmenuStack` → `GetSubmenuStack` (matches the base class `NSubmenuStack`) |
| `STS2AIAgent/Game/GameActionService.cs` | `ExecuteCloseMainMenuSubmenuAsync` uses `GetSubmenuStack`; the parameter of `WaitForMainMenuSubmenuCloseAsync` moves up to `NSubmenuStack` (required to compile); `IsCardsViewClosed` inserts an early return via the shared predicate **before** the existing `NCardsViewScreen` check so the wait condition covers `CARD_PILE` |
| `docs/api.md` | Screen enum adds `CARD_LIBRARY` / `CARD_PILE`; the `CARDS_VIEW` description, the action list row, and the `close_cards_view` section changed to the "closable card viewer screens" wording |
| `skills/sts2-mcp-player/SKILL.md` | Two routing lines added inside the shared contract block; the marker pair stays paired |
| `STS2AIAgent.Tests/CardViewerScreenContractTests.cs` | Added 3 source contract tests |
| `STS2AIAgent.Tests/TestRunner.cs` | Appended 3 registration lines |

## Acceptance

| Criterion | Evidence |
|---|---|
| Both predicates precede the generic grid | `GameStateService.cs:6840-6850` precedes `GetVisibleGridCardHolders` at `:6861`; red/green below |
| Dedicated names, not MAIN_MENU | Return values are `CARD_LIBRARY` / `CARD_PILE`; reason in the design (`PATCH_NOTES` precedent) |
| In-run library is closable | After `GetSubmenuStack` matches the base class, the in-run `NRunSubmenuStack` also hits `CanCloseMainMenuSubmenu` |
| Pile screen is closable | Probe/executor/wait condition share the one source `IsClosableCardViewer`; the executor `ForceClick()`s its `BackButton` |
| `docs/api.md` enum in sync | `api-facts`: all **26 screens** appear in the doc enum |
| `SKILL.md` + embedded contract | `Skill.McpPlayerContract` and `Parity.*` all green; mod build 0 warnings |
| Falsifiable | Three groups of "break -> red -> restore -> green" (see below) |
| Full gates | C# **351 PASS / 0 FAIL**; MCP 165 OK; all 7 gates green; preflight **372 PASS / 0 FAIL** |

## Red/green evidence

All commands are `dotnet run --project STS2AIAgent.Tests/STS2AIAgent.Tests.csproj -c Release`:

1. Move both early-return branches after the generic grid ->
   `FAIL CardViewer.BranchesPrecedeGrid / NCardLibrary must resolve before its own visible card grid can report CARD_SELECTION.` -> green after restoring
2. Change `IsClosableCardViewer` back to recognize only `NCardsViewScreen` ->
   `FAIL CardViewer.ClosableViewerSameSource / Expected '{returnscreenisNCardsViewScreen;}' to contain 'screenisNCardsViewScreenorNCardPileScreen'.` -> green after restoring
3. Change `GetSubmenuStack` back to `is NMainMenuSubmenuStack` ->
   `FAIL CardViewer.SubmenuStackBase / ... to contain 'currentisNSubmenuStacksubmenuStack'` -> green after restoring

## Two implementation choices (reviewers should know)

1. **The last line of** `IsCardsViewClosed` **is kept** as `return currentScreen is not NCardsViewScreen;`,
   because `ScreenResolutionContractTests.InspectOverlaysCloseThroughTheirOwnClose` pins that literal,
   and that test is outside the write scope of this task. The implementation instead inserts an early return via `IsClosableCardViewer` **before** it,
   which brings the new screens into the same set without breaking the existing assertion. If that test may be changed later, it can be collapsed to a single line.
2. Besides the sync points listed in the design, there are 2 small **compile-required** changes:
   `using ...Screens.CardLibrary;`, and moving up the parameter type of `WaitForMainMenuSubmenuCloseAsync`.

## Neighborhood fixed along the way (scope disclosure)

After the stack lookup moved up to the base class, the other submenus hosted on `NRunSubmenuStack`
(compendium / bestiary / relic collection / potion lab / run history / settings / stats / pause menu,
see `NRunSubmenuStack.cs:120-200`) also get `close_main_menu_submenu`.
They were likewise dead ends (or misreported screen names) before, and `Pop()` is equally correct for them, but **this is a neighborhood fixed along the way that needs live confirmation**.

## Live-only verification

- Whether the screen really closes after clicking the BackButton of `NCardPileScreen` (`ForceClick()` bypasses the input state,
  so only a real machine can prove that `Released` really pops the stack).
- Whether the in-run `NCardLibrary` returns to the correct previous screen after `NSubmenuStack.Pop()`.
- The real-machine behavior of the other in-run submenus above.
- Whether `/state.screen` and `available_actions` on a real machine match the new screen names/new actions.

