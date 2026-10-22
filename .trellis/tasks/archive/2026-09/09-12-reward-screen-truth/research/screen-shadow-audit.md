# Evidence: screen name shadowing

## Early-return branch order (`GameStateService.cs:6802-6866`)

| Order | Condition | Returns | Line |
|---|---|---|---|
| E1 | `currentScreen is NUnlockScreen` | UNLOCK | 6804-6807 |
| E2 | `TryGetCombatHandSelection(...)` | CARD_SELECTION | 6809-6813 |
| E3 | `currentScreen is NCardsViewScreen` | CARDS_VIEW | 6815-6818 |
| E4 | Generic visible grid holder (excluding the bundle screen) | CARD_SELECTION | 6820-6825 |
| E5 | `GetMultiplayerTestScene() != null` | MULTIPLAYER_LOBBY | 6827-6830 |
| E6 | `FindActiveCombatRoom(...) != null` | COMBAT | 6832-6835 |

In the switch `NCardRewardSelectionScreen => "REWARD"` is at `:6840`, permanently shadowed by E4.

## Arms shadowed by E4 but with "the same value" (no impact)

`NChooseACardSelectionScreen` (6841), the four `NDeck*Select` (6842), and `NCardGridSelectionScreen` (6843)
all return `CARD_SELECTION`, the same value as E4.

## Key screens not shadowed (basis for the negative conclusion)

`rg -n "NGridCardHolder" extraction/decompiled` -> only 4 places create holders at runtime:
`NCardRewardSelectionScreen.cs:188`, `NChooseACardSelectionScreen.cs:181`, `NUnlockCardsScreen.cs:93`,
`NCardGrid.cs:787` (reused by the selection screens / pile screen / library).
`NRewardsScreen` / `NTreasureRoom` / `NMerchantRoom` / `NMerchantInventory` / `NRestSiteRoom` /
`NEventRoom` / `NCombatRoom` each have no `rg` hits => their arms are reachable.

## Impact surface

- `skills/sts2-mcp-player/SKILL.md:105`: `CARD_SELECTION` -> `select_deck_card` (this screen does not provide it).
- `docs/api.md:85/890/1359/1377`: the preconditions of the reward actions say `REWARD`, while the runtime reports `CARD_SELECTION`.
- The `while screen == "REWARD"` loop at `scripts/run_sts2_validation.py:1334` turns false on that overlay
  (whether it really causes problems depends on the entry point, a live item).

## Existing assets that need no change after the fix (verified)

- `ScreenResolutionContractTests.cs:12-40` parses only the text after `return currentScreen switch`; the new early-return branch is not in that slice.
- `UnlockScreenContractTests.cs:21-33` asserts `unlockScreenIndex < visibleGridIndex` and `return "UNLOCK";` between them,
  and the newly added branch sandwiched in the middle still holds.
- The api-facts of `check_verification_gates.py` will catch an extra `REWARD` with `CODE_SCREEN_EARLY_RETURN`,
  but `REWARD` is already in the screen enum of `docs/api.md`, so the set comparison does not fire.
