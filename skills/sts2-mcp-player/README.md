# STS2 MCP Player Usage Guide

In-game auto-play and this skill share the same play contract: the in-game agent uses the shared play contract of SKILL.md as the system prompt, then injects, per the current screen, the corresponding section of screen-playbooks.md and strategy.md (injecting the section index when no section matches), rather than the whole document.

This is a skill for Codex/Agent use, aiming to play or validate *Slay the Spire 2* more reliably through the `sts2-ai-agent` MCP interface.

The core problems it solves:

- Read the state before every decision, then decide the action
- Do not misuse `proceed`, stale indices, or outdated screen information
- Distinguish three scenarios: ordinary play, full validation, and debug injection
- Prefer the lean guided tool set, to reduce distraction from too many MCP tools
- In ordinary play, write each meaningful decision into `agent_knowledge/run_logs/` for later review

## When to Use

Invoke it in these cases:

- Have the Agent play a whole STS2 run directly
- Have the Agent take over flows such as combat, events, shop, rewards, and rest sites
- Verify that MCP behavior matches the game state
- Reproduce a specific combat or state problem when debug actions are enabled

## How to Trigger

Mention the skill name directly in the prompt:

```text
Use $sts2-mcp-player to continue the current run, prefer the stable flow, and do not click debug carelessly.
```

You can also write it like this:

```text
Use $sts2-mcp-player to check whether the current reward flow has any anomaly, and tell me the key state.
```

```text
Use $sts2-mcp-player to run the full chain from the main menu to a new run, and do MCP validation when necessary.
```

## Recommended Preconditions

Ordinary play:

- The game has started
- The mod is loaded
- The MCP server is reachable

Validation mode:

- First run `scripts/build-mod.ps1`
- Then confirm `/health` is normal

Debug mode:

- Explicitly enable `STS2_ENABLE_DEBUG_ACTIONS=1`
- Use `run_console_command` only during development and when reproducing problems

## Standard Usage

### 1. Ordinary Play

Have the Agent take over the current position:

```text
Use $sts2-mcp-player to take over the current position.
First check health and game state,
then proceed by the state-first rules, and handle card selection, reward, and shop as the skill specifies.
```

### 2. Single-Point State Handling

Handle only the current screen:

```text
Use $sts2-mcp-player to handle only the current screen.
If it is CARD_SELECTION now, settle this step, stop, and report the result.
```

### 3. Combat Takeover

```text
Use $sts2-mcp-player to handle the current combat.
Do not waste free value, decide from the latest hand and enemy state, and do not reuse old indices.
```

### 4. Flow Validation

```text
Use $sts2-mcp-player to validate the current MCP flow.
Prefer the guided tool set, and switch to the full profile only when necessary.
Check whether state.available_actions is consistent with the current screen.
```

### 5. Acting as the Planning Layer for In-Game Auto-Play

```text
Use $sts2-mcp-player to act as the planning layer for the dual-layer auto-play in progress.
First get_planner_briefing to see the current strategy and Jev's confidence trend,
and when an adjustment is needed use update_play_strategy to write a macro goal and per-action-category hints; do not act yourself.
```

### 6. Debug Reproduction

```text
Use $sts2-mcp-player to reproduce a potion/card-selection related bug.
Use run_console_command only when debug actions are detected as available,
otherwise fall back to the ordinary MCP flow.
```

## How This Skill Works

Its core loop is simple:

1. First `health_check`
2. Read the state once before each decision: `decide` (gets the compact `state`, `available_actions`, and `scene_guidance` in one go)
   or `get_game_state`
3. Call only actions that truly exist in `available_actions`
4. The `state` returned by `act` is the input of the next decision; reread only on `pending`, a screen change, or when the full payload is needed (`raw_state=true`)
5. Recompute all indices from the latest payload; when an index is rejected, correct it per the `field` / `valid_indices` / `valid_field` of `error.details`
6. Maintain a per-run decision log following the `agent_knowledge/run_logs/README.md` template, recording the seed, character, route, stage choices, and reasons

This makes it especially suited to handling these error-prone situations:

- In `REWARD`, card rewards and the underlying reward items are not the same thing
- `CARD_SELECTION` overrides the original room screen
- `SHOP` is divided into the outer room and the inner inventory
- `EVENT` may return to the event itself after combat
- In `MAIN_MENU` the timeline gate can block starting a run

## Tool Usage Suggestions

Prefer the guided profile (that tier has 17 tools in total; the full list is whatever `ESSENTIAL_TOOLS` in `scripts/test-mcp-tool-profile.ps1` says).
The most commonly used for play are:

- `health_check`
- `get_game_state`
- `get_raw_game_state`
- `get_available_actions`
- `decide` (one read gives `state` / `available_actions` / `scene_guidance` together)
- `get_relevant_game_data` / `get_game_data_item` / `get_game_data_items`
- `wait_until_actionable`
- `act`

For review and troubleshooting there are also `get_decision_log` (reads back accepted decisions and their reasons), `get_run_summary` (the run overview), and
`diff_state` (the per-path difference of two states).

In dual-layer decision mode (Jev executes action by action and the LLM only sets strategy), an external Agent can act as the planning layer instead of playing cards in person:
`get_planner_briefing` reads the current strategy, Jev's recent choices, and the confidence trend, and `update_play_strategy` writes a one-sentence macro goal
`goal` plus `posture` / `instructions` / `option_hints` written per **action category**; omitted fields keep their values.
For the rules see "Planning for the In-Game Loop" in SKILL.md.

When `play_running` of `health_check` is `true`, in-game auto-play is playing cards and the mod will not block an external `act`:
then either pause auto-play first or act only as the planning layer, and do not operate on both sides at once.

Use the full profile only in these cases:

- Per-action coverage testing is needed
- The legacy per-action tools need to be validated
- The tool exposure difference between guided and full needs to be compared explicitly

## Common Cautions

- Do not treat `completed` as absolute completion; still look at the returned `state`
- The `state` returned by `act` is the compact `agent_view` (the same shape as `get_game_state`), not the full `/state`; pass `raw_state=true` to see the full payload
- When an index is rejected, do not resend the same index: read `error.details.valid_indices` and `valid_field` and then correct it
- Do not use `proceed` carelessly in the reward flow
- Do not assume a single `select_deck_card` always ends a multi-select flow
- Do not conclude the shop has been fully handled just because compact `shop.open=true` (in raw state it is called `shop.is_open`)
- Do not rely on `run_console_command` when debug is unavailable

## Related Files

- Main skill description:
  [SKILL.md](./SKILL.md)
- Per-screen playbooks:
  [screen-playbooks.md](./references/screen-playbooks.md)
- Choice strategy (route, rest, shop, potions, focus fire, multiplayer division of labor):
  [strategy.md](./references/strategy.md)
- Debugging and validation notes:
  [debug-and-validation.md](./references/debug-and-validation.md)
- Remote connection checklist:
  [remote-connection.md](./references/remote-connection.md)
- UI metadata:
  [openai.yaml](./agents/openai.yaml)

## One-Line Template

If you cannot be bothered to write one, just use this line:

```text
Use $sts2-mcp-player to take over the current STS2 MCP session, advance by the state-first workflow, and at each key stage report the current screen, available actions, and the reason for the next decision.
```
