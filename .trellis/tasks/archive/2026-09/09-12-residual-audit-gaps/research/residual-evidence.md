# Evidence

## The facts on both sides of crystal_clear_cell

| Surface | Fact |
|---|---|
| Contract block | `docs/api.md:1033`: action name + description, with only `x`, `y` in the parentheses |
| Outside the contract block | `docs/api.md:1072` (the `tool` of the request body table) and `:1478-1488` (parameter notes and examples) both already write `tool` |
| Registered schema | `ActionToolSpec("crystal_clear_cell", ...)` at `server.py:77` + `action_tool(x, y, tool=None)` at `:426-433` |
| Implementation | `GameActionService.cs:1679-1765`; the optional branch `if (request.tool != null)` at `:1715-1736`; an invalid value -> `:1777` |
| Exemption | `test_native_tool_alignment.py:289-291` + the comment at `:285-288` |

The regex at `check_verification_gates.py:336` takes only the action name, so adding parameters does not affect the api-doc gate;
but `test_native_tool_alignment.py:415-435` would fail because of "the exemption becoming stale", so both places must change together.

## The asymmetry of select_deck_card

- Availability: `CanSelectDeckCard` (`GameStateService.cs:916-923`) = `GetDeckSelectionOptions(...).Count > 0`.
- `GetDeckSelectionOptions` (`:1688-1726`) has four parts: `NCardsViewScreen` -> empty;
  `NCardGridSelectionScreen` -> visible grid holders; `NChooseACardSelectionScreen` -> same;
  combat hand -> `hand.ActiveHolders`; **generic fallback -> `GetVisibleGridCardHolders(rootNode)`**.
- Execution-side guard: `GameActionService.cs:2106-2117` (`!isCombatHandSelection && !isCardGridSelection &&
  currentScreen is not NChooseACardSelectionScreen` -> 409 `invalid_action`).
- Screens wrongly let through: `NCardRewardSelectionScreen` (the correct entry is `choose_reward_card`),
  `NCardPileScreen` (a pure viewer, `HolderPressed` not wired),
  `NCardLibrary` / `NCardLibraryGrid` (`HolderPressed -> ShowCardDetail`).
- Origin of the invariant: `scripts/run_sts2_validation.py:464-471` and `scripts/test-state-invariants.ps1:252-259`
  assert "selection non-null => available_actions contains `select_deck_card`".
  `BuildSelectionPayload` shares its source with availability (`GameStateService.cs:4325-4345`), so deleting the fallback makes the precondition not fire,
  rather than making the assertion fail.

## Confirmed boundaries

- The reverse asymmetry does not exist: the first line of the execution entry is the `CanSelectDeckCard` guard (`GameActionService.cs:2069`),
  so the executable set must be a subset of the available set.
- The 5 sealed subclasses of `NCardGridSelectionScreen` (deck/simple/upgrade/transform/enchant)
  all self-declare `_prefs`/`_selectedCards` and do not take the fallback.
