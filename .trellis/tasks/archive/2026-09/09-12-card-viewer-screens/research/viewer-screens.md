# Evidence: reachability and escape ability of the two viewer screens

## What turns them into the current screen (empirical, not speculative)

`ActiveScreenContext.GetCurrentScreen()` (`extraction/decompiled/.../ActiveScreenContext.cs`) order:
FeedbackScreen -> OpenModal -> InspectCard -> InspectRelic -> LogoAnimation -> MainMenu (PatchNotes/SubmenuStack.Peek/itself)
-> **NRun branch**:

```
66: if (NCapstoneContainer.Instance.CurrentCapstoneScreen != null) return ...CurrentCapstoneScreen;
70: if (NMapScreen.Instance.IsOpen) return NMapScreen.Instance;
74: if (NOverlayStack.Instance.ScreenCount > 0) return NOverlayStack.Instance.Peek();
...
```

- **Pile screen**: `NCardPileScreen : Control, ICapstoneScreen` (`NCardPileScreen.cs:28`),
  pushed by `NCombatCardPile` calling `NCapstoneContainer.ShowScreen(_pile, Hotkeys)`
  (inside `NCapstoneContainer.ShowScreen`, `CurrentCapstoneScreen = screen`, `:139`)
  => hits line 66.
- **Library**: `NCardLibrary : NSubmenu` (`NCardLibrary.cs:33`),
  `NMainMenuSubmenuStack` and `NRunSubmenuStack` each hold one (`NRunSubmenuStack.cs:155-164`)
  => hits `submenuStack.Peek()` of the MainMenu branch (`:56-60`).

## Today's screen names and available actions

Both screens create visible `NGridCardHolder`s (`NCardLibraryGrid : NCardGrid`, `NCardLibraryGrid.cs:21`;
`NCardPileScreen`'s `_grid = GetNode<NCardGrid>("CardGrid")`, `:118`)
=> captured by the E4 generic grid branch => `/state.screen = "CARD_SELECTION"`.

But `GetDeckSelectionOptions` returns empty for them (the comment at `GameStateService.cs:1725-1731` explicitly names
`NCardRewardSelectionScreen, NCardPileScreen, NCardLibrary`),
and `DeckSelectionAvailabilityTests` pins that => the screen has **no** `select_deck_card` / `confirm_selection`.

The remaining candidate actions were checked one by one:

| Action | Probe function | Why it does not hold today |
|---|---|---|
| `close_cards_view` | `CanCloseCardsView` -> `GetCardsViewBackButton` | First line `is not NCardsViewScreen -> null` |
| `close_main_menu_submenu` (library, main menu side) | `CanCloseMainMenuSubmenu` | Holds (`NSubmenu` + `NMainMenuSubmenuStack.SubmenusOpen`) |
| `close_main_menu_submenu` (library, in-run side) | Same as above | **Does not hold**: `GetMainMenuSubmenuStack` recognizes only `NMainMenuSubmenuStack`, and the in-run stack is `NRunSubmenuStack` |
| `proceed` | `GetProceedButton` | Neither screen is an `IRoomWithProceedButton`, and there is no `NProceedButton` among the child nodes (the pile screen has only `BackButton`) |

=> The pile screen and the in-run library have an empty `available_actions`: a dead end.

## BackButton of the two screens (why clicking is viable)

- `NCardPileScreen._Ready`: `_backButton = GetNode<NButton>("BackButton");` and
  `_backButton.Enable()` (`:115-117`).
- `NCardLibrary` is hosted by the submenu stack, and leaving the stack uses `NSubmenuStack.Pop()` (base class, `:95`).

## Class hierarchy of the stacks (decides the fix)

```
NSubmenuStack (abstract, : Control)   — SubmenusOpen :50, Push :77, Pop :95, Peek :125
├── NMainMenuSubmenuStack             - main menu side
└── NRunSubmenuStack                  - in-run side (hosts card library / compendium / bestiary /
                                        relic collection / potion lab / run history / settings /
                                        stats / pause menu, see :120-200)
```

The two are **siblings**, so `is NMainMenuSubmenuStack` cannot find the in-run stack; `is NSubmenuStack` hits both.

## Live-only verification

- Whether the screen really closes after clicking the BackButton of `NCardPileScreen` (`ForceClick` bypasses the input state, consistent with the other executors,
  but only a real machine can prove that the button's `Released` signal really pops the stack).
- Whether the in-run library returns to the correct previous screen after `Pop()`.
- The actual real-machine behavior of the other in-run submenus fixed along the way (compendium / bestiary / ...).

