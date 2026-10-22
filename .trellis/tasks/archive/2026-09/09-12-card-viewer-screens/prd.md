# Give the deck/pile viewer screens proper names and let them be escaped

## Background

`NCardLibrary` (the compendium / card library) and `NCardPileScreen` (the draw/discard/exhaust pile opened in combat)
both create visible `NGridCardHolder`s through `NCardGrid`, so they are swallowed by the generic grid early-return branch of `ResolveNonModalScreen`
(E4, `GameStateService.cs:6829-6832`), and `/state.screen` reports a **false** `CARD_SELECTION`.

But since 4c4, `GetDeckSelectionOptions` **deliberately** returns empty for them
(a comment at `GameStateService.cs:1725-1731` explains), and `DeckSelectionAvailabilityTests` pins that.
So the screen has neither `select_deck_card` nor `confirm_selection`, and the `selection` section is null,
yet the `CARD_SELECTION` route at `skills/sts2-mcp-player/SKILL.md:105` tells the model to call `select_deck_card`.

Reachability is empirical, not speculative:

- `NCombatCardPile.ShowScreen(_pile, ...)` pushes `NCardPileScreen` (`Control, ICapstoneScreen`)
  into `NCapstoneContainer`;
  in the `NRun.Instance != null` branch `ActiveScreenContext.GetCurrentScreen()` looks at **first**
  `NCapstoneContainer.Instance.CurrentCapstoneScreen` (`ActiveScreenContext.cs:66-69`) => the pile screen is taken as the current screen;
- `NCardLibrary : NSubmenu` is created by both `NMainMenuSubmenuStack` **and** `NRunSubmenuStack` through
  `GetSubmenuType(typeof(NCardLibrary))` (`NRunSubmenuStack.cs:155-164`);
  `GetCurrentScreen` `Peek()`s it when `submenuStack.SubmenusOpen` (`ActiveScreenContext.cs:56-60`).

The two screens differ in escape ability, which is the core of this task:

| Screen | Can it be escaped today? |
|---|---|
| `NCardLibrary` (main menu side) | Yes: `close_main_menu_submenu` works through the `NSubmenu` + `NMainMenuSubmenuStack` check of `CanCloseMainMenuSubmenu` |
| `NCardLibrary` (in-run side) | **No**: `GetMainMenuSubmenuStack` recognizes only `NMainMenuSubmenuStack`, while in-run uses `NRunSubmenuStack` (the two are siblings, both inheriting `NSubmenuStack`) => `available_actions` is empty => dead end |
| `NCardPileScreen` | **No**: `CanCloseCardsView` requires `is NCardsViewScreen`; `GetProceedButton` goes through `IRoomWithProceedButton` / a child node search, and this screen has only a button named `"BackButton"` (already `Enable()`d at `NCardPileScreen.cs:115-117`) => `available_actions` is empty => dead end |

## Goals

1. Each of the two screens reports a **dedicated** screen name and no longer masquerades as `CARD_SELECTION`;
2. Both screens have a genuinely usable close action, so `available_actions` is no longer empty.

## Acceptance criteria

- [x] In `ResolveNonModalScreen` the checks for `NCardLibrary` and `NCardPileScreen` precede the E4 generic grid branch
      (the same early-return shape as `NCardRewardSelectionScreen`/`NUnlockScreen`).
- [x] Screen names take the dedicated values `CARD_LIBRARY` / `CARD_PILE`, and **do not** report `MAIN_MENU`
      (reason in the design: the repo always gives a dedicated name to "overlays that can only be closed", with `PATCH_NOTES`/`TIMELINE` as precedents).
- [x] The in-run `NCardLibrary` gets `close_main_menu_submenu`: the submenu stack lookup changes to the base class `NSubmenuStack`.
- [x] `NCardPileScreen` gets `close_cards_view`: the probe and the executor use the **same set** for "closable card viewer screens",
      the executor clicks its `BackButton`, and the wait condition can recognize that the screen has closed.
- [x] The Screen enum in `docs/api.md` gains two names (the `api-facts` gate extracts the enum from the source, and without them it goes red);
      the precondition of the `close_cards_view` section and the `CARDS_VIEW` description are updated in sync.
- [x] The `SKILL.md` routing gains `CARD_LIBRARY` / `CARD_PILE` lines; the marker pair stays paired,
      `McpPlayerSkillTests` and `HealthCheckParityTests` stay green (the embedded contract still matches `PlayPrompt.PlayContract`).
- [x] Added source contract tests (falsifiable): early-return order, `close_cards_view` probe/executor same source, stack lookup using the base class.
- [x] Full C# / MCP tests, 7 gates, and preflight all green.

## Out of scope

- Do not change action names or add actions (`close_cards_view` / `close_main_menu_submenu` already exist).
- `NCardLibrary` does not use `close_cards_view` (it is hosted by the submenu stack, and two close paths would confuse the model).
- No capstone screen support is added beyond `NCardPileScreen`.
- Live verification: whether the button click really closes the screen can only be confirmed on a real machine (see the "live-only verification" list in the evidence).

