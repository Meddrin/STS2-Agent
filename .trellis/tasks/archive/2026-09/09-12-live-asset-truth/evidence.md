# Evidence

## Deliverables

| File | Change |
|---|---|
| `scripts/test-main-menu-active-run.ps1` | Deleted the hard `open_timeline` assertion at `:92` and the timeline interaction sequence at `:117-152`; the wait condition now uses `continue_run`; the output changed to the diagnostic field `open_timeline_available_with_active_run` |
| `scripts/run_sts2_validation.py` | The `choose_timeline_epoch` of `suite_new_run_lifecycle` now takes the `index` of the first `is_actionable` slot in `timeline.slots[]`, with a guard (raising `ValidationError` when there is no usable slot); added a 24-value screen classification comment block; `settle_main_menu` / `continue_from_main_menu_if_needed` gain a `confirm_unlock` arm |
| `scripts/test-multiplayer-lobby-flow.ps1` | The screen switch gains `CAPSTONE_SELECTION` (`choose_capstone_option` + `option_index = capstone.options[0].i`) and `UNLOCK` (`confirm_unlock`); the fallback `throw` carries `screen=` and `available_actions=[...]` |

Total `+88 / -53`, no new repo files. The checked-out `scripts/__pycache__/` is ignored by `.gitignore:12`.

## Acceptance

| Criterion | Evidence |
|---|---|
| Both sides agree on timeline | The ps1 no longer requires `open_timeline`; the python side originally only output a diagnostic field. Consistent with `run_sts2_validation.py:1548-1550` (`NMainMenu.UpdateTimelineButtonBehavior` disables that button when there is a save) |
| Epoch index is not hardcoded | Red: changing the index back to `option_index=0`, payload `slots=[{index:0,is_actionable:false},{index:1,is_actionable:true}]` -> `RED: mixed payload did NOT pick the first actionable slot (index 1)` (live it would be 409 `invalid_target`); green: it takes `option_index 1`. Guard red: changing it to `if False:` -> `TypeError: 'NoneType' object is not subscriptable` (unreadable), and after restoring it goes back to `ValidationError` |
| lobby switch coverage | Used the AST to extract the real `Invoke-LocalRunProgressionStep` + a mock `Invoke-Action`: `PASS: CAPSTONE_SELECTION -> choose_capstone_option option_index 0`, `PASS: UNLOCK -> confirm_unlock`, `PASS: MAP -> null`, `PASS: unsupported fallback names screen=CRYSTAL_SPHERE`. Red: moving the label so it does not match -> falls to the fallback and reports `screen=` and `available_actions=[...]` |
| python screen coverage and list | Red/green of the `confirm_unlock` arm of `settle_main_menu`: without the arm `AssertionError: []` (live it shows as a wait timeout), with the arm it returns from `UNLOCK` to `MAIN_MENU` |
| Encoding / compile / gate | Both ps1 are still pure ASCII (`nonAscii=0, bom=False`; the gate requires a BOM only for scripts containing non-ASCII, so it was not broken); `py_compile` OK; `Parser::ParseFile` gives 0 errors on both ps1; `--only script-encoding` ok |
| Full gates | C# 347 PASS / 0 FAIL; MCP 167 OK; all 6 gates green; preflight exit 0 |

## Screen consumption list (item 4 of the prd requires it be written down)

- **Consumed by scripts through the Mod API**: `MAIN_MENU`, `CHARACTER_SELECT`, `COMBAT`, `CARD_SELECTION`, `REWARD`, `GAME_OVER`, `MULTIPLAYER_LOBBY`, `TIMELINE`, `UNLOCK`, `UNKNOWN`.
- **Not entered, advanced via the debug console instead**: `MAP`, `EVENT`, `CHEST`, `REST`, `SHOP`, `FAKE_MERCHANT`, `CAPSTONE_SELECTION`, `BUNDLE_SELECTION`, `CRYSTAL_SPHERE`.
- **Player-driven, scripts should not open them**: `CARDS_VIEW`, `CARD_INSPECT`, `RELIC_INSPECT`, `PATCH_NOTES`, `FEEDBACK`.
- `MODAL` is not among the 24 values of `ResolveNonModalScreen`; it is produced by the modal path and belongs to `dismiss_blocking_modal` / `resolve_modals`.

## Live-only verification

- Whether the active-save main menu really no longer fails for lack of `open_timeline`, and whether after `dismiss_modal` it can wait for `continue_run` and enter the save.
- Real timeline: when slot 0 is non-actionable the first actionable slot is selected without returning 409; when there is no actionable slot an explicit `ValidationError` is given.
- After death `settle_main_menu` confirms `confirm_unlock` layer by layer on a real `UNLOCK` overlay back to a usable menu.
- The multiplayer script no longer throws `Unsupported run progression state` when crossing `CAPSTONE_SELECTION` / `UNLOCK`.

