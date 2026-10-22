# Evidence

## Deliverables

| File | Change |
|---|---|
| `skills/sts2-mcp-player/SKILL.md` | The first two items of the shared contract block Quick Start rewritten: connection checking belongs to the external MCP client/orchestrator, and the in-game loop has no `health_check`; in-game it starts from `get_game_state -> get_available_actions -> act`. Marker pair untouched |
| `skills/sts2-mcp-player/references/screen-playbooks.md` | The debug `fight THE_ARCHITECT_EVENT_ENCOUNTER` at `:93` is marked **external MCP only** (needs `STS2_ENABLE_DEBUG_ACTIONS=1`), with an in-game reachable alternative given (take the event's own non-fatal options, never send `PROCEED`) |
| `README.md`, `README.zh-CN.md` | The tool list at `:183` changed to "MCP tools have one more `health_check` on top of the built-in auto-play tool surface", aligned with `AgentTools.cs` |
| `STS2AIAgent.Tests/HealthCheckParityTests.cs` | New, 3 assertions (the Play surface has no health_check / the embedded prompt names only Play surface tools / the MCP surface still documents health_check and the markers are paired) |
| `STS2AIAgent.Tests/TestRunner.cs` | Appended 3 `Parity.*` registration lines |

Production code under `STS2AIAgent/Agent/**` was not touched (`git status --porcelain -- STS2AIAgent/Agent/` is empty).

## Acceptance

| Criterion | Evidence |
|---|---|
| The in-game prompt no longer requires a nonexistent tool | `health_check` appears only once in the contract block, in the non-imperative tone of "in-game play loop has no `health_check`"; red/green below |
| Tool name set ⊆ `AgentTools.Play` | `Parity.EmbeddedPromptUsesOnlyPlayTools`: asserted on both `PlayPrompt.PlayContract` and `PlayPrompt.PlaySystem`. Changing SKILL.md:52 back to `Call \`health_check\` once at session start.` -> `FAIL ... the shared play contract instructs the in-game model to call a tool its play surface cannot dispatch: health_check`; PASS after restoring |
| Debug instruction in the playbooks is marked | The current text at `:93` contains `external MCP only` and `STS2_ENABLE_DEBUG_ACTIONS=1` |
| `McpPlayerSkillTests` stays green | `PASS Skill.McpPlayerContract` (still asserts `ExtractSharedContract(skill) == PlayPrompt.PlayContract`) |
| SKILL.md:25 and README checked | `:25` is the MCP-side subagent configuration list (including `health_check`/`wait_for_event`), correct for MCP, kept; both README languages were changed |
| Markers paired | `Parity.McpSurfaceKeepsHealthCheck` asserts BEGIN/END each appear exactly once and in order |
| Full gates | C# 347 PASS / 0 FAIL; MCP 167 OK; all 6 gates green; preflight exit 0 |

## Decision criteria (implementation choice, for review)

The rule for "invocation tone": within the same sentence (split by line / `; ` / `. `) a call-type verb or a `->` chain appears,
and the verb can be exempted by negative forms such as `do not` / `never` / `instead of ...`.
It does **not** use "shape of a backtick identifier" as the criterion: many backticks in the contract block are action names (`choose_map_node`,
`collect_rewards_and_proceed` ...), and judging by shape would produce widespread false positives.
The candidate tool name set is taken from `AgentTools.Mcp` plus an explicit external-only list (`health_check` / `wait_for_event` /
`run_console_command`).

## Live-only verification

- Whether the in-game model really no longer sends `health_check`, wasting one fewer round per run (offline proves only that the prompt text no longer requires it).
- How the per-step rebuilt injection of `PlayPrompt.PlaySystem` behaves in a real run.

## Left untouched (correct MCP-side wording)

`skills/sts2-mcp-player/README.md`, `references/remote-connection.md`, `references/debug-and-validation.md`
still write `health_check`: these are MCP-side docs, the platform really has that tool, and changing them would introduce new errors.

