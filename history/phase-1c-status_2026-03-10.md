# Phase 1C Status

> Historical record, archived on 2026-09-09. The status and next steps below apply only to 2026-03-10 and do not represent current tasks; see [PRODUCT_PLAN_CURRENT.md](../PRODUCT_PLAN_CURRENT.md) for the current status.

Updated: `2026-03-10`

## Implemented Interfaces

- `GET /health`
- `GET /state`
- `GET /actions/available`
- `POST /action`

## Implemented and Verified

- `end_turn`
- `play_card`
- `choose_map_node`
- `claim_reward`
- `choose_reward_card`
- `collect_rewards_and_proceed`
- `proceed`
- MCP `health_check`
- MCP `get_game_state`
- MCP `get_available_actions`
- MCP `end_turn`
- MCP `play_card`
- MCP `choose_map_node`
- MCP `claim_reward`
- MCP `choose_reward_card`
- MCP `collect_rewards_and_proceed`
- MCP `proceed`

## Implemented but Not Yet Verified Live

- `skip_reward_cards`
- `select_deck_card`
- `map.nodes`

## Current `GET /state` Coverage

- Top-level fields: `state_version`, `run_id`, `screen`, `in_combat`, `turn`, `available_actions`
- Combat fields: `combat.player`, `combat.hand`, `combat.enemies`
- Build fields: `run.gold`, `run.deck`, `run.relics`, `run.potions`
- Map fields: `map.current_node`, `map.available_nodes`, `map.rows`, `map.cols`, `map.starting_node`, `map.boss_node`, `map.second_boss_node`, `map.nodes`
- Reward fields: `reward.pending_card_choice`, `reward.can_proceed`, `reward.rewards`, `reward.card_options`, `reward.alternatives`
- Card selection fields: `selection.kind`, `selection.prompt`, `selection.cards`

## Covered `screen` Values

- `MAIN_MENU`
- `CHARACTER_SELECT`
- `MAP`
- `COMBAT`
- `EVENT`
- `SHOP`
- `REST`
- `REWARD`
- `CHEST`
- `CARD_SELECTION`
- `GAME_OVER`
- `UNKNOWN`

## Current Limitations

- On Windows a loaded Mod DLL cannot be hot-swapped; the game must be restarted after installing a new version
- The stable-state checks for `end_turn`, `choose_map_node`, and `proceed` are still conservative and need more live coverage
- `collect_rewards_and_proceed` still keeps the minimal strategy of "automatically collect rewards and pick the first card", which suits unattended progress but is not suitable as a build decision interface
- `claim_reward` now provides an explicit entry for reward buttons, used to first enter the card reward sub-screen and then hand over to `choose_reward_card` / `skip_reward_cards`
- `skip_reward_cards` currently acts on the first alternative button in the UI, which by default corresponds to skipping the reward; if a mod later rewrites the order of reward alternatives, the recognition will need to be tightened
- `select_deck_card` currently implements single selection with automatic confirmation, which suits card removal; multi-select scenarios come later
- `map.nodes` currently exports a graph structure and a runtime state snapshot, and does not yet provide built-in route scoring logic; route decisions should be made by an upper-level LLM or strategy engine

## Next Steps

1. Restart the game and verify the completeness of `map.nodes` live
2. Check the start point, boss nodes, forks, and link order against the UI
3. Add a route-planning prompt template on the MCP side so the LLM makes decisions based primarily on `map.nodes`
4. Continue with shop purchase and card removal scenario regression
