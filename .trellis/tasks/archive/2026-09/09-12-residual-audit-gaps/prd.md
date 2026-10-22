# Close out two audit leftovers

## Background

Two confirmed gaps were left at the end of the previous round:

1. **Documentation debt for `crystal_clear_cell`**: both the implementation and the registered schema accept the optional parameter `tool` (`big`/`small`),
   while the action contract block of `docs/api.md` (`docs/api.md:1033`) lists only `x`, `y`.
   The previous round worked around it with an **explicit exemption** (`mcp_server/tests/test_native_tool_alignment.py:289-291`),
   held by "the exemption fails once it becomes stale". The reason for "freezing" the contract block no longer holds: the api-doc gate compares only the set of action names
   (the regex at `scripts/check_verification_gates.py:326-339` ends at the closing backtick; descriptions and parameters do not take part in the check).
2. **Asymmetry between availability and executability of `select_deck_card`**: `GameStateService.GetDeckSelectionOptions`
   has a generic fallback at the end (`STS2AIAgent/Game/GameStateService.cs:1718-1723`):
   "if the current screen subtree has a visible `NGridCardHolder`, it counts as selectable cards". The execution side
   (`GameActionService.ExecuteSelectDeckCardAsync`, guard at :2106-2117) requires one of combat hand metadata /
   card grid metadata / `NChooseACardSelectionScreen`, and otherwise returns 409.
   So the three kinds of screens `NCardRewardSelectionScreen`, `NCardPileScreen`, and `NCardLibrary` appear in
   `available_actions` yet inevitably fail, while `/state.selection` also reports a normal-looking
   `deck_card_select` (taking 1/1 when `min/max` metadata is missing), tricking the model into it.

## Goals

1. Add the optional parameter `tool` to `crystal_clear_cell` in the contract block, and **delete** that exemption (so the test returns to a strict cross-check).
2. Delete the generic fallback branch of `GetDeckSelectionOptions`, so that "available => executable" holds at the same source;
   the 409 guard on the execution side stays as a defensive branch.

## Acceptance criteria

- [x] The `crystal_clear_cell` row in the `docs/api.md` contract block contains `tool`, and the row still starts with a "backtick action name"
      (otherwise the regex of the api-doc gate would miss it).
- [x] `_DOC_ARGUMENT_EXEMPTIONS` no longer contains `crystal_clear_cell`; the full MCP tests pass.
- [x] `GetDeckSelectionOptions` no longer has the `currentScreen is Node rootNode` fallback branch;
      the first three parts (`NCardGridSelectionScreen` / `NChooseACardSelectionScreen` / combat hand) are kept.
- [x] Added a falsifiable regression test: a source contract asserts the fallback branch is gone, and asserts the execution-side guard still exists with the same criteria.
- [x] The existing `selection.cards[]` invariant still holds: `BuildSelectionPayload` shares its source with availability,
      and after the fallback is deleted these three kinds of screens no longer produce a `selection` section (the precondition does not fire, rather than the assertion failing).
- [x] `check_verification_gates.py`, the full C# and MCP tests, and preflight pass.

## Out of scope (explicitly recorded, not fixed in this task)

- The generic branch of `ResolveNonModalScreen` (`GameStateService.cs:6742-6747`) still returns
  `CARD_SELECTION` on the reward card choice overlay, shadowing the `NCardRewardSelectionScreen => "REWARD"` arm. Changing it would change the external semantics of `/state.screen`
  (the doc screen table, skill routing, and verification scripts are all affected), which needs separate evaluation and live confirmation; this round only records the evidence.
- The "correct handling" of `NCardPileScreen` / `NCardLibrary` (neither is a selection screen, and for the latter `HolderPressed` only opens details)
  is outside the scope of this task; this round only guarantees they are no longer misreported as card-selectable.
