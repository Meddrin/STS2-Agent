# Evidence

## Deliverables

| File | Change |
|---|---|
| `docs/api.md:1034` | The `crystal_clear_cell` row in the contract block gains the optional parameter: `(\`x\`, \`y\`, optional \`tool\`: \`big\` / \`small\`)`; the row still starts with a backtick action name (the api-doc regex is unaffected) |
| `mcp_server/tests/test_native_tool_alignment.py:285-291` | `_DOC_ARGUMENT_EXEMPTIONS` emptied to `{}`, the comment changed to "both sides are now consistent; a stale exemption still fails" |
| `STS2AIAgent/Game/GameStateService.cs:1711-1725` | Deleted the generic fallback branch of `GetDeckSelectionOptions` (`currentScreen is Node rootNode` -> subtree scan), kept the three-part criteria and the trailing empty return, and wrote down "why the subtree scan is deliberately not done" |
| `STS2AIAgent.Tests/DeckSelectionAvailabilityTests.cs` (new) | 2 source contract assertions (`AvailabilityMatchesTheExecutableSet` / `ExecutorKeepsItsGuard`) |

## Acceptance

| Criterion | Evidence |
|---|---|
| The contract row contains `tool` and its shape is not broken | Parse result `crystal_clear_cell docs args = ['tool','x','y']`; the gate still reports `55 actions match` |
| Exemption deleted, MCP all green | `_DOC_ARGUMENT_EXEMPTIONS = {}`; `uv run --locked python -m unittest discover -s tests` -> 159 OK (snapshot at the time) |
| Fallback branch gone, first three parts kept | `git diff` is -7/+6 (delete the branch + add an explanatory comment); the three-part criteria are still at `:1690-1715` |
| New regression tests runnable | After registration by the main agent: `PASS DeckSelectionAvailability.MatchesExecutableSet` / `PASS DeckSelectionAvailability.ExecutorKeepsGuard` |
| The invariant's precondition does not fire | `CanSelectDeckCard` and `BuildSelectionPayload` (`:4323-4329`) call the same source; when 0 cards are returned `selection` is null, so the antecedent of "selection non-null => available_actions contains this action" does not hold |
| Falsifiable | Deleting `tool` from the doc -> `test_documented_action_arguments_match_registered_legacy_tools` red; adding the exemption back -> "stale exemption" red; adding the fallback back -> `AvailabilityMatchesTheExecutableSet` red; deleting the 409 guard -> `ExecutorKeepsItsGuard` red |

## Gates

C# full 335 PASS / 0 FAIL; `dotnet build` 0 warnings 0 errors; MCP 167 OK;
`check_verification_gates.py` (5 gates), `check_release_package.py`, and preflight all green.

## Live-only verification

- On the three kinds of screens (`NCardRewardSelectionScreen` / `NCardPileScreen` / `NCardLibrary`) `select_deck_card`
  really no longer appears in `available_actions`, and `/state.selection` no longer appears (needs `state-invariants`).
- Reward card choice can still be completed via `choose_reward_card`.

## Out of scope (recorded)

The generic branch of `ResolveNonModalScreen` still makes the reward card choice overlay report `CARD_SELECTION` (shadowing
`NCardRewardSelectionScreen => "REWARD"`). Changing it would change the external semantics of `/state.screen` and needs separate evaluation.
