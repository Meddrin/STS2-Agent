# STS2 AI Agent Current Development Roadmap

> Historical roadmap, archived on 2026-09-09. The "current" in the title, the task list, and the acceptance status apply only to 2026-03-11; the single entry point for current status is [PRODUCT_PLAN_CURRENT.md](../PRODUCT_PLAN_CURRENT.md).

Updated: `2026-03-11`

---

## Overall Progress

| Phase | Description | Status |
| --- | --- | --- |
| Phase 0A | Environment setup | Done |
| Phase 0B | Decompilation and reverse-engineering survey | Done |
| Phase 1A | Protocol freeze | Done |
| Phase 1B | Mod skeleton + `/health` | Done |
| Phase 1C | Minimal vertical slice | Done |
| Phase 2 | Combat state extraction | Done |
| Phase 3 | Combat action execution | Done |
| Phase 4A | Map / rewards / chests | Code complete, awaiting unified live validation |
| Phase 4B | Events / rest sites | Code complete, awaiting unified live validation |
| Phase 4C | Shop | Code complete, awaiting unified live validation |
| Phase 5 | Full-chain coverage (character select / potions / Modal / Game Over) | Code complete, awaiting unified live validation |
| Phase 6 | Integration and regression | Not started |

---

## Current Capability Inventory

### HTTP API

| Endpoint | Status |
| --- | --- |
| `GET /health` | Verified |
| `GET /state` | Verified |
| `GET /actions/available` | Verified |
| `POST /action` | Verified |

### Implemented Actions

| Action | Status |
| --- | --- |
| `end_turn` | Verified |
| `play_card` | Verified |
| `choose_map_node` | Verified |
| `claim_reward` | Verified |
| `choose_reward_card` | Verified |
| `collect_rewards_and_proceed` | Verified |
| `proceed` | Verified |
| `continue_run` | Code complete, awaiting live validation |
| `abandon_run` | Code complete, awaiting live validation |
| `open_character_select` | Code complete, awaiting live validation |
| `open_timeline` | Code complete, awaiting live validation |
| `close_main_menu_submenu` | Code complete, awaiting live validation |
| `choose_timeline_epoch` | Code complete, awaiting live validation |
| `confirm_timeline_overlay` | Code complete, awaiting live validation |
| `skip_reward_cards` | Code complete, awaiting live validation |
| `select_deck_card` | Code complete, awaiting live validation |
| `open_chest` | Code complete, awaiting live validation |
| `choose_treasure_relic` | Code complete, awaiting live validation |
| `choose_event_option` | Code complete, awaiting live validation |
| `choose_rest_option` | Code complete, awaiting live validation |
| `open_shop_inventory` | Code complete, awaiting live validation |
| `close_shop_inventory` | Code complete, awaiting live validation |
| `buy_card` | Code complete, awaiting live validation |
| `buy_relic` | Code complete, awaiting live validation |
| `buy_potion` | Code complete, awaiting live validation |
| `remove_card_at_shop` | Code complete, awaiting live validation |
| `select_character` | Code complete, awaiting live validation |
| `embark` | Code complete, awaiting live validation |
| `use_potion` | Code complete, awaiting live validation |
| `discard_potion` | Code complete, awaiting live validation |
| `confirm_modal` | Code complete, awaiting live validation |
| `dismiss_modal` | Code complete, awaiting live validation |
| `return_to_main_menu` | Code complete, awaiting live validation |

### Implemented State Fields

| Field | Status |
| --- | --- |
| `combat.player` / `combat.hand` / `combat.enemies` | Implemented and verified |
| `run.deck` / `run.relics` / `run.potions` / `run.gold` | Implemented, awaiting unified live validation |
| `map.current_node` / `map.available_nodes` | Verified |
| `map.rows` / `map.cols` / `map.starting_node` / `map.boss_node` / `map.second_boss_node` / `map.nodes` | Implemented, awaiting unified live validation |
| `reward.*` / `selection.*` | Implemented, awaiting unified live validation |
| `chest.*` | Implemented, awaiting unified live validation |
| `event.*` | Implemented, awaiting unified live validation |
| `rest.*` | Implemented, awaiting unified live validation |
| `shop.*` | Implemented, awaiting unified live validation |
| `character_select.*` | Implemented, awaiting unified live validation |
| `modal.*` | Implemented, awaiting unified live validation |
| `game_over.*` | Implemented, awaiting unified live validation |

### MCP Tools

MCP tools currently registered and available:

- `health_check`
- `get_game_state`
- `get_available_actions`
- `end_turn`
- `play_card`
- `continue_run`
- `abandon_run`
- `open_character_select`
- `open_timeline`
- `close_main_menu_submenu`
- `choose_timeline_epoch`
- `confirm_timeline_overlay`
- `choose_map_node`
- `claim_reward`
- `choose_reward_card`
- `skip_reward_cards`
- `select_deck_card`
- `collect_rewards_and_proceed`
- `proceed`
- `open_chest`
- `choose_treasure_relic`
- `choose_event_option`
- `choose_rest_option`
- `open_shop_inventory`
- `close_shop_inventory`
- `buy_card`
- `buy_relic`
- `buy_potion`
- `remove_card_at_shop`
- `select_character`
- `embark`
- `use_potion`
- `discard_potion`
- `confirm_modal`
- `dismiss_modal`
- `return_to_main_menu`

---

## Task List

### T-1: Phase 4A Live Regression

- [ ] Verify the full `map.nodes` graph structure
- [ ] Verify `skip_reward_cards`
- [ ] Verify `select_deck_card`
- [ ] Verify choosing a chest relic and then `proceed`

### T-2: Phase 4B Event System

- [x] `EventPayload`
- [x] `choose_event_option`
- [x] MCP / documentation sync
- [ ] Verify event branch switching
- [ ] Verify the proceed option after an event finishes
- [ ] Verify the nested-combat event chain

### T-3: Phase 4B Rest Site System

- [x] `RestPayload`
- [x] `choose_rest_option`
- [x] `select_deck_card` covers upgrade card selection
- [ ] Verify the ordinary rest chain
- [ ] Verify SMITH entering card selection and returning

### T-4: Phase 4C Shop System

- [x] `ShopPayload`
- [x] `open_shop_inventory` / `close_shop_inventory`
- [x] `buy_card` / `buy_relic` / `buy_potion`
- [x] `remove_card_at_shop`
- [ ] Verify the inventory toggle
- [ ] Verify buying cards / relics / potions
- [ ] Verify that card removal enters card selection and then returns to the shop or the map

### T-5: Phase 5 Full-Chain Coverage

- [x] `continue_run`
- [x] `abandon_run`
- [x] `open_character_select`
- [x] `open_timeline`
- [x] `close_main_menu_submenu`
- [x] `choose_timeline_epoch`
- [x] `confirm_timeline_overlay`
- [x] `CharacterSelectPayload`
- [x] `ModalPayload`
- [x] `GameOverPayload`
- [x] `select_character`
- [x] `embark`
- [x] `use_potion`
- [x] `discard_potion`
- [x] `confirm_modal`
- [x] `dismiss_modal`
- [x] `return_to_main_menu`
- [ ] Verify character select through run start
- [ ] Verify FTUE / Modal interception and release
- [ ] Verify in-combat potion use / discard
- [ ] Verify Game Over returning to the main menu

### T-6: Phase 6 Integration and Regression

- [ ] Run the key chain of one complete run end to end
- [ ] Record failure scenarios and logs
- [ ] Converge the remaining API / MCP / documentation drift

---

## Recommended Execution Order

1. Do unified live validation first, and stop adding new features.
2. Order the validation by how blocking each item is: character select / Modal / map / events / rest sites / shop / rewards / chests / Game Over.
3. Insert `use_potion` / `discard_potion` into the in-combat regression script.
4. When problems are found, fix only the defects that block a complete run, and avoid refactoring in passing.

---

## Current Risks

1. On Windows a loaded Mod DLL cannot be hot-swapped, so every live validation depends on "close the game -> install -> reopen".
2. `GameStateService.cs` is still a high-conflict file, so later bug fixes must avoid widening the change surface in passing.
3. Events, rest sites, shop, and Modal all depend on the UI tree structure, which is what most easily breaks after a game update.
4. `map.nodes` only provides structure and runtime state and does no route scoring; path planning should stay in the upper-level agent.
5. The current code has full-chain coverage, but until Phase 6 nothing counts as "live closed loop accepted".
