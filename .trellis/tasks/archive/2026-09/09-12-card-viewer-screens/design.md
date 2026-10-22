# Design

## 1. Early-return branch (`ResolveNonModalScreen`)

Insert two after the `CARDS_VIEW` early return and before the E4 generic grid early return, in exactly the same form as `NCardRewardSelectionScreen`:

```csharp
// Both of these carry visible grid card holders, so the generic grid branch below would call them
// CARD_SELECTION and send the model after select_deck_card, an action they deliberately do not offer.
if (currentScreen is NCardLibrary)
{
    return "CARD_LIBRARY";
}

if (currentScreen is NCardPileScreen)
{
    return "CARD_PILE";
}
```

**Why a dedicated name instead of `MAIN_MENU`**: the switch really does have a catch-all `NSubmenu => "MAIN_MENU"` arm,
but for "overlays in the main menu family that can only be closed" this repo always gives a dedicated name: `PATCH_NOTES` is the precedent,
and `SKILL.md:116` specifically writes "close it with `close_main_menu_submenu`" for it.
Reporting `MAIN_MENU` would make the model see the "continue game / new run" guidance, which does not apply to this screen at all.

## 2. Escape for the in-run `NCardLibrary`: move the stack lookup up one level

`GetMainMenuSubmenuStack` currently hardcodes `is NMainMenuSubmenuStack`. The in-run library hangs on `NRunSubmenuStack`,
and both are sibling subclasses of `NSubmenuStack` (`NMainMenuSubmenuStack : NSubmenuStack`,
`NRunSubmenuStack : NSubmenuStack`). `SubmenusOpen` and `Pop()` are both defined on the base class
(`NSubmenuStack.cs:50,95`), so:

- change the lookup to `is NSubmenuStack` and rename it `GetSubmenuStack` (the old name would lie about the in-run stack);
- sync three references: `GameStateService.cs:1445` (`CanCloseMainMenuSubmenu`),
  `GameActionService.cs:643` (executor), and the signature itself.

**Scope disclosure**: this change also gives the other submenus hosted on `NRunSubmenuStack` (compendium / bestiary /
relic collection / potion lab / run history / settings / stats / pause menu,
see `NRunSubmenuStack.cs:120-200`) access to `close_main_menu_submenu`. Today they are likewise dead ends or misreported,
and `Pop()` is equally correct for them; but this is a neighborhood fixed along the way that needs live confirmation, and it must be written into the evidence.

## 3. Escape for `NCardPileScreen`: widen the set `close_cards_view` can close

Current state: the first line of `GetCardsViewBackButton` is `is not NCardsViewScreen -> null`; `IsCardsViewClosed` also hardcodes
`currentScreen is not NCardsViewScreen`.

The change: collapse "closable card viewer screens" into **one** predicate, which the probe, the executor, and the wait condition all call:

```csharp
// Screens that draw the same visible card grid and are left with their BackButton.
private static bool IsClosableCardViewer(IScreenContext? screen) =>
    screen is NCardsViewScreen or NCardPileScreen;
```

- `CanCloseCardsView` -> use `GetCardsViewBackButton(...) != null` (that helper internally switches to the predicate above);
- `IsCardsViewClosed` -> `!IsClosableCardViewer(currentScreen)` (the two inspect-screen branches are unchanged);
- the else branch of `ExecuteCloseCardsViewAsync` has **no logic change**: it already takes the BackButton via `ForceClick()`,
  and the BackButton node names of both screens are `"BackButton"`, both already `Enable()`d in `_Ready`.
- Note the BackButton of `NCardPileScreen` is an `NButton`, which `NCardPileScreen.cs:115-117` already `Enable()`s;
  `ForceClick()` bypasses the input state, consistent with the other executors in this repo.

## 4. Docs

- `docs/api.md`: add `CARD_LIBRARY` / `CARD_PILE` rows to the Screen enum table (near `:87`);
  change `CARDS_VIEW` at `:88` and the `close_cards_view` section at `:1650` to the "closable card viewer screens" wording.
- `SKILL.md` (**inside the shared contract block**, between the markers `<!-- BEGIN/END SHARED PLAY CONTRACT -->`):
  add two routing lines near `CARDS_VIEW`/`CARD_INSPECT`:
  - `CARD_LIBRARY`: return with `close_main_menu_submenu`;
  - `CARD_PILE`: return with `close_cards_view`.

## Tests (`STS2AIAgent.Tests/`, falsifiable)

1. Early-return order: the index of `NCardLibrary` / `NCardPileScreen` < the index of E4's `GetVisibleGridCardHolders`,
   and the slice up to E4 for each contains the matching `return "...";` (same shape as `RewardScreenContractTests`).
2. `close_cards_view` same source: the probe `CanCloseCardsView` and the executor `IsCardsViewClosed` both decide only via
   `IsClosableCardViewer` / `GetCardsViewBackButton`, and that decision covers both `NCardsViewScreen` and `NCardPileScreen`
   (follow the "available means executable" style of `DeckSelectionAvailabilityTests`).
3. Stack lookup uses the base class: assert the body of `GetSubmenuStack` has `is NSubmenuStack` and **not** `is NMainMenuSubmenuStack`,
   and that `CanCloseMainMenuSubmenu` and the executor both call it.
4. Keep the existing ones green: `RewardScreenContractTests`, `DeckSelectionAvailabilityTests`, `ScreenResolutionContractTests`.

## Pitfalls

- `GameStateService.cs` / `GameActionService.cs` are not in the test project's compile set (they are members of
  `SourceCoverageTests.KnownUncompiledSources`), so after changing them you must run
  `dotnet build STS2AIAgent/STS2AIAgent.csproj -c Release --no-incremental` for type checking.
- The `api-facts` gate extracts the screen enum from `ResolveNonModalScreen` and compares sets with `docs/api.md`, so **both places must change together**.
- The `SKILL.md` contract block is embedded at mod build time (`csproj:32-33`) and the markers must not be deleted; after changing, run
  `dotnet build STS2AIAgent/STS2AIAgent.csproj` to confirm the embedding still holds.
- This repo has `core.autocrlf=true` on Windows; make only minimal hunks and do not rewrite whole files.

