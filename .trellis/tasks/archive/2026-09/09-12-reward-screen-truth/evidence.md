# Evidence

## Deliverables

| File | Change |
|---|---|
| `STS2AIAgent/Game/GameStateService.cs` | Added an early-return branch inside `ResolveNonModalScreen` (after `CARDS_VIEW`, before the generic visible grid branch): `if (currentScreen is NCardRewardSelectionScreen) return "REWARD";` (+9 lines, including 4 comment lines) |
| `STS2AIAgent.Tests/RewardScreenContractTests.cs` | New. 3 source contract assertions: reward branch index < generic grid index; `return "REWARD";` between the two; in the switch after the generic grid branch `NChooseACardSelectionScreen` / the four `NDeck*Select` / `NCardGridSelectionScreen` are still `=> "CARD_SELECTION"` |
| `STS2AIAgent.Tests/TestRunner.cs` | Appended 1 registration line `RewardScreen.BranchPrecedesGrid` |

## Acceptance

| Criterion | Evidence |
|---|---|
| The reward branch precedes the generic grid | `GameStateService.cs:6820-6827` (early return) comes before `:6829-6832` (`GetVisibleGridCardHolders(rootNode).Count > 0`); `git diff` has only this one hunk |
| The order is pinned and falsifiable | Temporarily moving the new branch after the generic grid branch: `FAIL RewardScreen.BranchPrecedesGrid`, and **only it** fails (`UnlockScreen.MixedCardGrid` and `ScreenResolution.MappingTable` still PASS); after restoring 347/0 |
| Other shadowed arms' values unchanged | The third assertion requires, in the slice after `visibleGridIndex`, the three `=> "CARD_SELECTION"` shapes one by one; the whole `ScreenResolution.MappingTable` (25 rows) keeps passing |
| Existing tests all green | C# 347 PASS / 0 FAIL; MCP 167 OK; all 6 gates green (`api-facts` rechecked `24 screens ... all appear in the docs/api.md enum`); `check_release_package.py --source-root .` passed; `preflight-release.ps1` exit 0 |
| Doc/script check conclusion | **No doc changes needed** (details in the next section) |

## Doc check (the conclusion the prd requires be written down)

After the fix existing assets become **more accurate** and need no changes:

- `docs/api.md`: the screen enum at `:85` already contains `REWARD`; the preconditions of the reward actions at `:1320/:1342/:1359/:1377/:1391` always said `screen = "REWARD"`: before the change the runtime contradicted this contract, and only after the change is it consistent.
- `skills/sts2-mcp-player/SKILL.md`: `:104` has a `REWARD` route, and `:105` associates `CARD_SELECTION` with `select_deck_card`. The reward overlay used to misreport `CARD_SELECTION` while **not providing** that action; after the fix it no longer misleads.
- `scripts/run_sts2_validation.py`: the `while screen == "REWARD"` loop of `collect_rewards_if_needed` lines up with that overlay again.

The `api-facts` gate extracts early-return branches with `CODE_SCREEN_EARLY_RETURN`; the new branch only yields the already-existing `REWARD`, so the set comparison does not fire.

## Live-only verification

- On a real machine the reward card choice overlay really reports `REWARD` in `/state.screen` (including variants such as multiple reward items, bundles, multiple card choices).
- The real effect of the semantic flip `CARD_SELECTION -> REWARD` on model routing (no longer hunting for a nonexistent `select_deck_card`).
- Whether the loop of `collect_rewards_if_needed` changes behavior under the new screen name (depends on the entry point).

## Out of scope (recorded, not touched)

`NCardPileScreen` (viewing a pile) and `NCardLibrary` (the compendium) have no switch arm and still resolve to `UNKNOWN`. Giving them new screen names needs the skill-side routing semantics to be defined first; this round only records.

