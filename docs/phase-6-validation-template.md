# Phase 6 live validation record template

Copy this template into a real validation record, to accumulate the final evidence for "is it ready to release".

---

## Basic information

- Validation date:
- Validated by:
- Git commit:
- Game version:
- Mod build configuration: `Release`
- MCP start method:
- Notes:

---

## Static preflight

- [ ] `preflight-release.ps1` passes
- [ ] `build-mod.ps1 -Configuration Release` passes
- [ ] `test-mod-load.ps1 -DeepCheck` passes

Output summary:

```text
Paste the key output
```

---

## Flow validation

### 1. Run start flow

- [ ] If a save exists: `continue_run` or `abandon_run`
- [ ] `MAIN_MENU -> open_character_select`
- [ ] If the single-player entry is gated by the timeline: `open_timeline -> close_main_menu_submenu -> open_character_select`
- [ ] If the timeline has an unlockable epoch: `choose_timeline_epoch -> confirm_timeline_overlay`
- [ ] `CHARACTER_SELECT` is readable
- [ ] `select_character`
- [ ] `embark`
- [ ] `MODAL` / FTUE can be handled
- [ ] Successfully enters `MAP`

Result:

```text
Record screen changes, available_actions, and anomalies
```

### 2. Map and combat flow

- [ ] `choose_map_node`
- [ ] `play_card`
- [ ] `end_turn`
- [ ] `use_potion` or `discard_potion`
- [ ] Combat ends successfully

Result:

```text
Record combat target indices, potion behavior, and whether any pending state remains
```

### 3. Reward flow

- [ ] `claim_reward`
- [ ] `choose_reward_card`
- [ ] `skip_reward_cards`
- [ ] `collect_rewards_and_proceed`

Result:

```text
Record whether switching between reward sub-flows is stable
```

### 4. Chest flow

- [ ] `open_chest`
- [ ] `choose_treasure_relic`
- [ ] `proceed`

Result:

```text
Record the state change after relic selection
```

### 5. Event flow

- [ ] Normal event options
- [ ] Leave after `event.is_finished=true`
- [ ] Event with a nested combat

Result:

```text
Record event transitions, nested combat, and whether stale state remains
```

### 6. Rest site flow

- [ ] `choose_rest_option` -> `HEAL`
- [ ] `choose_rest_option` -> `SMITH`
- [ ] `select_deck_card`
- [ ] `proceed`

Result:

```text
Record whether smith returns smoothly after entering card selection
```

### 7. Shop flow

- [ ] `open_shop_inventory`
- [ ] `buy_card`
- [ ] `buy_relic`
- [ ] `buy_potion`
- [ ] `remove_card_at_shop`
- [ ] `close_shop_inventory`
- [ ] `proceed`

Result:

```text
Record inner and outer state, price changes, and the return path after card removal
```

### 8. Wrap-up flow

- [ ] Enters `GAME_OVER`
- [ ] `game_over` fields are complete
- [ ] `return_to_main_menu`

Result:

```text
Record whether exit is stable and whether it gets stuck on the summary page
```

---

## Issue list

| Severity | Scenario | Symptom | Repro steps | Suspected root cause |
| --- | --- | --- | --- | --- |
| P0/P1/P2 |  |  |  |  |

---

## Conclusion

- [ ] Can release
- [ ] Can go to a limited trial, but known limitations remain
- [ ] Cannot release, fixes needed

Conclusion notes:

```text
List the blocking items clearly; do not just write "mostly usable"
```
