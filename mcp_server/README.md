# STS2 MCP Server

To play with others as an ordinary player: **do not install this directory**. In game, press F8 -> auto-play / invite a teammate.

External clients (Cursor / Claude / Codex): open **Native MCP** on the in-game **Connect** page and copy the actual address shown on the page (the same port as the HTTP API, not necessarily 8080, and certainly not 8765). No Python / uv needed.

`mcp_server/` is an optional Python FastMCP sidecar, giving developers **stdio**, the **layered/full** profiles, and contract tests. It wraps the Mod's HTTP API one more layer.

Run the sidecar from the repository or the GitHub release package root: `scripts/start-mcp-stdio.ps1` (stdio) or
`scripts/start-mcp-network.ps1` (HTTP). The release package also carries `scripts/test-mcp-tool-profile.ps1`,
for checking the profile tool surface without connecting to the game.

## Tool Profile

`STS2_MCP_TOOL_PROFILE` (or `create_server(tool_profile=...)` in code) selects the profile:

- `guided`: the default profile, 17 tools: `health_check`, `get_game_state`, `get_raw_game_state`,
  `get_available_actions`, `get_decision_log`, `get_run_summary`, `get_scene_guidance`, `diff_state`,
  `get_game_data_item`, `get_game_data_items`, `get_relevant_game_data`, `wait_for_event`,
  `wait_until_actionable`, `decide`, `act`, `get_planner_briefing`, `update_play_strategy`. For their purposes see the "Current Tools" table below;
  the live list is whatever `ESSENTIAL_TOOLS` in `scripts/test-mcp-tool-profile.ps1` says
- `layered`: for layered orchestration of a primary / secondary Agent, exposing 8 additional handoff / knowledge tools on top of guided:
  `get_planner_context`, `create_planner_handoff`, `get_combat_context`, `create_combat_handoff`,
  `complete_combat_handoff`, `append_combat_knowledge`, `append_event_knowledge`, `complete_event_handoff`
- `full`: includes the layered tools and continues to expose the legacy per-action tools, suitable for validation and compatibility testing

## Current Tools

The table below lists the 17 guided tools with a one-line purpose each. **The per-parameter contract of each tool is not repeated in this file**: go by
each tool's own docstring and input schema in [src/sts2_mcp/server.py](src/sts2_mcp/server.py); a change made there cannot drift from here.

| Tool | Purpose |
|------|------|
| `health_check` | Whether the Mod is loaded and reachable |
| `get_game_state` | Compact `agent_view` state; `compact_agent_view: false` means falling back to the full payload |
| `get_raw_game_state` | The full raw `/state`, for looking up fields the compact view lacks |
| `get_available_actions` | Currently available actions and their `requires_index` / `requires_target` / target hints |
| `get_decision_log` | The most recently accepted decisions and their reasons, newest last |
| `get_run_summary` | Character, floor, Act, boss, HP, gold, energy, and deck / relic / potion counts (with a team block in multiplayer) |
| `get_scene_guidance` | The current screen's strategy and playbook, returning `screen` / `scene` / `guidance` / `playbook` |
| `diff_state` | The per-path difference (before/after values) of two `/state` snapshots, with `truncated` marking a hit on the cap |
| `get_game_data_item` | Look up a single game metadata entry by id |
| `get_game_data_items` | Look up game metadata in batch by comma-separated ids |
| `get_relevant_game_data` | Relevant metadata with fields trimmed automatically for the current scene (use this by default) |
| `wait_for_event` | Wait for one matching `/events/stream` event |
| `wait_until_actionable` | Wait until actionable again, returning `matched` (an event hit) and `actionable` |
| `decide` | One call gives `state` + `available_actions` + `scene_guidance`, i.e. everything one decision needs to read |
| `act` | The unified action entry point (action name + parameters, with `reason` attached); returns the compact `state` after the action |
| `get_planner_briefing` | The planning briefing for dual-layer mode: the current strategy, the dual-layer / Jev switches, the run summary, and Jev's recent choices and confidence trend (forwards `GET /strategy`) |
| `update_play_strategy` | Update the strategy Jev follows: `goal` (a one-sentence macro goal) / `posture` / `instructions` / `option_hints` per action category; omitted fields keep their values |

For the extra tools of layered / full see "Tool Profile" above; for the legacy per-action tools see the block below.

The metadata read by `get_game_data_item`, `get_game_data_items`, and `get_relevant_game_data` all comes from the running Mod (`GET /data/{collection}`), and the package no longer carries any game data snapshot.

The strategy text in `get_scene_guidance` and `decide` is the same `skills/sts2-mcp-player/references/strategy.md` that the in-game loop injects (plus the same `screen-playbooks.md`), and the "screen -> section" mapping and the shared keys on both sides are compared item by item by `tests/test_scene_guidance_alignment.py` (C# is authoritative); the per-option `event_id` / `event_options` of the `EVENT` screen exist only in the sidecar, and the native MCP surface returns only the four shared keys.

<!-- BEGIN LEGACY ACTION TOOLS -->
<!-- The bullets below are the full profile's per-action tools. They are bound to
     _LEGACY_ACTION_TOOLS in src/sts2_mcp/server.py by tests/test_legacy_action_coverage.py,
     so adding or removing a legacy tool without updating both sides fails the MCP tests. -->

Combat:

- `play_card`
- `end_turn`
- `use_potion`
- `discard_potion`

Rooms / flow advancement:

- `continue_run`
- `continue_game_over`
- `abandon_run`
- `save_and_quit`
- `open_character_select`
- `open_timeline`
- `close_main_menu_submenu`
- `choose_timeline_epoch`
- `confirm_timeline_overlay`
- `select_character`
- `embark`
- `unready`
- `increase_ascension`
- `decrease_ascension`
- `switch_profile`
- `choose_map_node`
- `proceed`
- `dismiss_game_over_wait`
- `confirm_unlock`
- `close_cards_view`
- `close_map_screen`
- `open_chest`
- `choose_treasure_relic`
- `choose_event_option`
- `crystal_set_tool`
- `crystal_clear_cell`
- `choose_rest_option`
- `open_shop_inventory`
- `close_shop_inventory`
- `buy_card`
- `buy_relic`
- `buy_potion`
- `remove_card_at_shop`
- `return_to_main_menu`

Multiplayer / teaming:

- `host_multiplayer_lobby`
- `join_multiplayer_lobby`
- `ready_multiplayer_lobby`
- `disconnect_multiplayer_lobby`
- `invite_ai_teammate`
- `continue_ai_teammate`

Rewards / card selection:

- `claim_reward`
- `choose_reward_card`
- `skip_reward_cards`
- `collect_rewards_and_proceed`
- `resolve_rewards`
  - `option_index` can be omitted (defaults to the first reward card), or the backward-compatible `card_index` alias can be used instead
- `select_deck_card`
- `confirm_selection`
- `choose_capstone_option`
- `choose_bundle`
- `confirm_bundle`

Modal:

- `confirm_modal`
- `dismiss_modal`
<!-- END LEGACY ACTION TOOLS -->

Development-time debugging (not affected by the profile, registered only when `STS2_ENABLE_DEBUG_ACTIONS=1`, off by default):

- `run_console_command`: runs one game dev-console command, for development and validation only
- `inject_event_churn`: emits `option_index` synthetic `debug_churn` events to fill the queue of a slow `/events/stream` subscriber,
  to verify "a full queue closes that subscriber" rather than silently dropping events; `option_index` must be greater than the single-subscriber queue capacity (256), otherwise the Mod returns 400 `invalid_request`

For parameter details see the docstring of each.

## Usage Notes

This MCP is no longer small, so what really affects stability is not only "whether the tools exist" but also "whether the model calls them at the right rhythm":

1. Call `health_check` first at the start of a session.
2. Read the state once before each decision: `decide` (gets the state, actions, and guidance in one go) or `get_game_state`.
3. Call only actions that appear in the current `available_actions`; attach a short `reason` to `act`, so the player UI and the decision log can explain this step's choice.
4. The `state` returned by `act` is the input of the next decision and need not be read again; reread only on `pending` / a screen change, and pass `raw_state=True` only when the full payload is needed (about 4,000-9,500 tokens each time).
5. The `state` of `wait_until_actionable` has the same shape as `act` / `get_game_state` (compact) and likewise supports `raw_state=True`: waiting across an animation does not need the full payload.
6. When an index is rejected the error object carries `field` / `submitted` / `valid_indices` / `valid_field`: correct the index accordingly instead of resending the same value.
7. Prefer high-level actions, and do not break up flows that can be combined (priorities below).

**`outcome_unknown` (the action response was lost)**: when `status` is `outcome_unknown`, the action may already have executed, and only the response did not come back.
The client does **one** `/state` reconciliation only, and puts the state it read in `reconciliation.state` (compact, with the `compact_agent_view` marker);
`reconciliation.state_read` / `action_effect_compared` (always `false`) / `action_outcome` (always `"unknown"`) say
"the state was read, but the action's effect was not compared against it", and `succeeded` / `status` describe that read rather than the action itself.
**Never replay the action automatically**: look at the reconciled state first, then decide the next step.

`get_game_state` returns the compact `agent_view` by default; when the Mod does not expose `agent_view` it falls back to the full `/state` with `compact_agent_view: false`, which is a degradation signal and not the regular contract. A batch of keys is renamed in compact (the shop-open flag is `shop.open` in compact and `shop.is_open` in raw state); before reading compact, see the section `compact field rename table` ("compact field rename table") of `docs/api.md`.

When `guided` / `layered` use the unified `act`, crystal sphere actions additionally accept `x`, `y`, and `tool`: `crystal_clear_cell` must pass coordinates (and may also pass `tool="big"|"small"`), and `crystal_set_tool` passes only `tool`; the full board comes from `get_game_state().crystal_sphere`.

High-level action priority: reward room `collect_rewards_and_proceed`; rest site `choose_rest_option`; in the shop first `open_shop_inventory`, and to leave the inner inventory first `close_shop_inventory`; chest `open_chest -> choose_treasure_relic -> proceed`; for `MODAL` prefer `confirm_modal` / `dismiss_modal`.

## Recommended Companion Skill

When operating this Mod through MCP with an external AI Agent, also load [sts2-mcp-player](../skills/sts2-mcp-player/SKILL.md): the in-game auto-play decides according to this contract. A client that connects only the MCP tools without loading the skill can also click legal actions, but will not reach the effect of in-game auto-play.

## Cost Fields

The card payload exposes both `costs_x` / `star_costs_x` (whether it is an X-cost card) and `energy_cost` / `star_cost` (the current cost, including temporary in-combat modifiers), because in STS2 these two kinds of cost are rewritten dynamically: energy costs such as `Bullet Time`, and star costs / star X costs such as `Stardust`. Judging by a static cost table would misjudge.

## Environment Variables

| Variable | Default | Effect |
|------|------|------|
| `STS2_API_BASE_URL` | `http://127.0.0.1:8080` | Mod API address |
| `STS2_AGENT_REPO_ROOT` | Auto-detected (searching upward from the `sts2_mcp` package location for a directory containing `mcp_server/pyproject.toml`) | Locates the repository root and the `docs/game-knowledge/*.md` of `reference_files`; when it cannot be detected in a wheel / pipx install it returns an empty list instead of guessing a path |
| `STS2_AGENT_KNOWLEDGE_DIR` | `agent_knowledge/` under the repository root; when not inside a checkout it falls back to the current working directory and logs a WARNING | Where the combat / event runtime knowledge files are saved (not written into the Python install directory) |
| `STS2_API_READ_TIMEOUT` | `10` (seconds) | Read timeout for `GET` requests and state reconciliation |
| `STS2_API_ACTION_TIMEOUT` | `75` (seconds) | Read timeout for `POST /action`; an action returns only after the game stabilizes (`continue_game_over` waits up to 60 seconds), so it must be clearly longer than the read timeout |
| `STS2_API_MAX_RETRIES` | `2` | Retry count for retryable read-type requests; action requests are never replayed automatically |
| `STS2_ENABLE_DEBUG_ACTIONS` | Unset / `0` | Enables the development-time debug tools (`run_console_command`, `inject_event_churn`); keeping it off is recommended for release |

## Runtime Knowledge Base

The `layered` / `full` profiles automatically maintain a simple knowledge base keyed by stable id:

```text
agent_knowledge/
  combat/global/solo/cultist_x1.md
  combat/global/groups/cultist_x2+slime_large_x1.md
  events/global/cleric.md
```

- Combat files are aggregated and sorted by `enemy_id_xcount` and do not depend on localized names; event files are named by `event_id`
- When there is no chapter field yet the directory lands in `global/` first; appended content automatically carries `run_id`, `floor`, `screen`, and a UTC timestamp
- When not inside a repository checkout (a wheel / pipx install) paths are no longer guessed: the knowledge base lands in `agent_knowledge/` of the current working directory, which can be pinned with `STS2_AGENT_KNOWLEDGE_DIR`

The `reference_files` returned by `get_planner_context` / `get_combat_context` are the entry point of the offline knowledge base, and the keys are the corresponding id fields in the state (mapping in `docs/game-knowledge/agent-reference.md`): `cards` (`card_id`), `monsters` / `monster_behaviors` (`enemy_id`), `potions` (`potion_id`), `events` (`event_id`), `relics` (`relic_id`), `powers` (`power_id`), `characters`, `playbook`.

## Primary / Secondary Agent Handoff

When the primary Agent handles route and room decisions and the secondary Agent handles combat alone, the recommended connection is:

1. The primary Agent calls `create_planner_handoff` before each non-combat decision; when `screen=COMBAT` it calls `create_combat_handoff` instead and hands the returned package to the combat Agent as a whole.
2. After combat ends the combat Agent calls `complete_combat_handoff`, and the primary Agent treats `planner_summary` as the compressed memory of the previous combat, then continues with the next `create_planner_handoff`.
3. Events work the same way: decide according to the `event` and `event_knowledge` in `create_planner_handoff`, and after settlement call `complete_event_handoff` to write the result back.

## Local Start

```powershell
cd "<repo-root>/mcp_server"
uv sync
uv run sts2-mcp-server
```

It runs over `stdio` by default, suited for connecting directly to an MCP client.

## Local Validation and Pre-Release Checks

Unit tests and import self-check (standard-library unittest, no game needed; CI runs the same command):

```powershell
cd "<repo-root>/mcp_server"
uv run --locked python -m unittest discover -s tests -v
uv run python -c "from sts2_mcp.server import create_server; create_server(); print('MCP_IMPORT_OK')"
```

Read the state once while the Mod is online, to verify the HTTP path:

```powershell
cd "<repo-root>/mcp_server"
uv run python -c "from sts2_mcp.client import Sts2Client; import json; print(json.dumps(Sts2Client().get_state(), ensure_ascii=False, indent=2))"
```

Before release:

```powershell
dotnet build "<repo-root>/STS2AIAgent/STS2AIAgent.csproj" -c Release
python -m py_compile "<repo-root>/mcp_server/src/sts2_mcp/client.py" "<repo-root>/mcp_server/src/sts2_mcp/server.py"
```

Live scripts can also be run from the source repository: `scripts/start-game-session.ps1 -EnableDebugActions` launches the game, and
`scripts/test-debug-console-gating.ps1` (add `-EnableDebugActions` to run the enabled state) verifies the debug tool gating.
The release package carries only the sidecar's launch / profile check scripts; do not look in the release package for these live scripts.

For the release process entry see [release-readiness.md](../docs/release-readiness.md); for the current version, acceptance evidence, and remaining gaps see the [current status page](https://github.com/CharTyr/STS2-Agent/blob/main/PRODUCT_PLAN_CURRENT.md).
