# Design: ground the live assets

## 1. `test-main-menu-active-run.ps1` (active-save menu)

The python conclusion is the contract: with a save, `open_timeline` is disabled. The change:

- Delete the `Assert-ActionAvailable ... "open_timeline"` at `:92`;
- Change the wait condition at `:112-115` to the real flag of the active-save menu (`continue_run` or `abandon_run`, consistent with `:88`);
- Delete the timeline sequence at `:117-152` (that path is already covered by `suite_new_run_lifecycle` of `run_sts2_validation.py`
  in the no-save menu); if diagnostics are wanted, change it to "report `open_timeline_available` and then skip".

## 2. The epoch index of `run_sts2_validation.py`

```python
timeline_payload = timeline_state.get("timeline") or {}
slots = list(timeline_payload.get("slots") or [])
actionable = next((slot for slot in slots if slot.get("is_actionable")), None)
if actionable is None:
    raise ValidationError("choose_timeline_epoch is available but no slot reports is_actionable")
timeline_state = ensure_action_ok(
    client.action("choose_timeline_epoch", option_index=int(actionable["index"])),
    "choose_timeline_epoch",
)["data"]["state"]
```

Contract basis: `TimelineIndexContractTests.ExecutorIndexesTheSameSlotListTheStateExposes`
(the executor and the state use the same slot list, with no second filtering) and `NonActionableSlotsAreRejectedExplicitly`
(a non-actionable slot returns 409 `invalid_target`).

## 3. The screen switch of `test-multiplayer-lobby-flow.ps1`

Add to the switch at `:655`:

- `"CAPSTONE_SELECTION"`: `choose_capstone_option` + `option_index = 0`;
- `"UNLOCK"`: `confirm_unlock`.

The fallback `throw` already carries a full JSON dump; just confirm that it contains the screen name and the available actions.

## 4. The python screen name list

In the run-progression area of `run_sts2_validation.py` (near `settle_*` / `collect_rewards_if_needed`)
add a comment block: divide the 24 return values of `ResolveNonModalScreen` into "consumed by the normal flow / bypassed by debug commands / never appear",
and add handling for the screens that really appear in the normal flow but have no branch. The classification conclusion goes into the evidence.
`api-facts` gate extracts the screen enum from the source, and the comment shares the same source with it, with no duplicate maintenance.

## Pitfalls

- `scripts/*.ps1` containing non-ASCII must have a UTF-8 BOM; after changing, run `--only script-encoding`.
- After changing python, run `python -m py_compile scripts/run_sts2_validation.py`.
- Do not introduce a new dependency on `STS2_ENABLE_DEBUG_ACTIONS` (the gating suite already covers debug commands).

