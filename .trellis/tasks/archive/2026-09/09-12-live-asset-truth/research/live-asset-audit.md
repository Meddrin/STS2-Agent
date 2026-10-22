# Evidence: contract contradictions in live assets

## Full set of screens (`GameStateService.ResolveNonModalScreen`:6802-6866)

UNLOCK, CARD_SELECTION, CARDS_VIEW, MULTIPLAYER_LOBBY, COMBAT, GAME_OVER, REWARD, CHEST, REST,
SHOP, EVENT, MAP, CHARACTER_SELECT, BUNDLE_SELECTION, CAPSTONE_SELECTION, CRYSTAL_SPHERE, TIMELINE,
FAKE_MERCHANT, PATCH_NOTES, CARD_INSPECT, RELIC_INSPECT, FEEDBACK, MAIN_MENU, UNKNOWN (24 in total).

Screens compared on the python side: MAIN_MENU / MODAL / REWARD / COMBAT / CARD_SELECTION /
CHARACTER_SELECT / GAME_OVER / MULTIPLAYER_LOBBY / UNKNOWN. The other 15 non-modal screens have no branch.

## 1. Mutually exclusive assertions on open_timeline

| File | Line | Content |
|---|---|---|
| `scripts/test-main-menu-active-run.ps1` | 92 | `Assert-ActionAvailable -State $state -ActionName "open_timeline"` |
| Same | 114 | Wait condition `... -contains "open_timeline"` |
| Same | 117-152 | Calls `open_timeline`/`choose_timeline_epoch`/`confirm_timeline_overlay`/`close_main_menu_submenu` |
| `scripts/run_sts2_validation.py` | 1548-1550 | Comment: with a save the timeline button is disabled (`NMainMenu.UpdateTimelineButtonBehavior`), and `open_timeline` is not part of the active-run contract |
| Same | 1827 | Comment: the timeline is opened only when there is no save |

## 2. Epoch index

`TimelineIndexContractTests.ExecutorIndexesTheSameSlotListTheStateExposes`:
`ResolveTimelineSlot` uses `GameStateService.GetTimelineSlots` (`GameStateService.cs:6353-6366`,
filtering only `NotObtained`) and asserts there must be no second filtering; `NonActionableSlotsAreRejectedExplicitly`:
a non-actionable slot gives 409 `invalid_target`, `option_index_space = "timeline.slots[].index"`.
The python `option_index=0` (`:1937`) therefore is not guaranteed to hit.

## 3. The switch of the lobby flow

`scripts/test-multiplayer-lobby-flow.ps1:655-733`: five branches BUNDLE_SELECTION / EVENT / CARD_SELECTION / REWARD / MAP,
everything else `throw`s (`:733`). CAPSTONE_SELECTION (`NCapstoneSubmenuStack`) and UNLOCK (`NUnlockScreen`)
appear in normal map progression (see the screen table in `skills/sts2-mcp-player/references/screen-playbooks.md`).

## 4. Impact of the python branch gaps

- `settle_main_menu` (`:1283-1307`) handles only MODAL and timeline actions and would wait until timeout on UNLOCK/CAPSTONE;
- `collect_rewards_if_needed` (`:1332-1345`) recognizes only REWARD;
- `suite_new_run_lifecycle` mostly bypasses the UI with debug commands, so it is insensitive to these screens, but any section that needs real UI progression
  (such as `suite_main_menu_active_run`) would be affected.

## Not confirmed live

The script changes are verified only at the syntax/encoding/contract level; "actually no longer times out / no longer crashes" can be confirmed only on a machine with the game.

