# ADR 0002: Two MCP tool surfaces, one contract

Status: **Accepted, not implemented for now** (records the decision and the triggers)
Date: 2026-09-17

> Later addendum (2026-09-21): the table below records the tool counts on the day of writing. The tool surface has kept growing since: at that time
> the `guided` profile had 15 tools, 14 of which have the same name and parameters on both surfaces, and `wait_for_event` was still
> sidecar-only. For the live list, use `ESSENTIAL_TOOLS` in `scripts/test-mcp-tool-profile.ps1`; this document no longer restates it.
>
> Further addendum (2026-09-23): after the two-layer planning tools `get_planner_briefing` / `update_play_strategy` were added,
> `guided` has 17 tools, 16 shared by both surfaces, and `wait_for_event` is still sidecar-only. The two implementations of `update_play_strategy`
> share the `PlayStrategyUpdate` merge (omitted fields keep their old values), which confirms this ADR's "one contract" principle:
> before that, each surface wrote its own merge, and the HTTP surface reset omitted fields to defaults.

## Background

This mod has **two MCP servers** that implement the same tool surface:

| | Location | Runtime form | Tools |
| --- | --- | --- | --- |
| Native | `STS2AIAgent/Server/NativeMcpServer.cs` (952 lines) + the schema in `AgentTools.Mcp` | inside the mod process, `/mcp` endpoint | 9 |
| Sidecar | `mcp_server/src/sts2_mcp/server.py` (1102 lines) | separate Python process, calls the mod over HTTP | 9 + `wait_for_event` / `run_console_command` + the legacy tools of the layered / full profiles |

The nine tools with the same names (`health_check`, `get_game_state`, `get_raw_game_state`,
`get_available_actions`, `wait_until_actionable`, `get_game_data_item`,
`get_game_data_items`, `get_relevant_game_data`, `act`) are written out twice: tool name,
input schema, parameter names, and description text.

**This is the cross-language version of the ADR 0001 problem**, with one decisive difference: in ADR 0001 both implementations live in the same class
and can be merged into one traversal. These two cannot. The native surface exists so that **no Python install is needed**: players who subscribe on the Workshop
can open the game and use `/mcp`. The sidecar exists for the FastMCP ecosystem, tool profile tiers, and a connection that does not
live and die with the game process. Both have to stay.

So the question is not "merge the implementations" but: **how many sources of truth should this contract have?**

Today there are two, held together by a three-way comparison in `mcp_server/tests/test_native_tool_alignment.py` (438 lines):
tool names registered in Python <-> `AgentTools.Mcp` <-> the switch branches of `ExecuteToolAsync` (both directions),
the Python inputSchema <-> the `arguments` keys the C# switch actually parses,
the frozen parameter columns in `docs/api.md` <-> the schemas of the full-profile legacy tools.

This test is **green today, and the exemption table is empty** (`_DOC_ARGUMENT_EXEMPTIONS` is `{}`).
The only exception is that `run_console_command` is deliberately excluded from the legacy tools: it is registered separately only when
`STS2_ENABLE_DEBUG_ACTIONS` is true. So the two sides **really are** consistent today.

## Why not change it now

The reasoning is the same as the one recorded in ADR 0001, but the conclusion is the opposite.

The duplication in ADR 0001 was a **net loss**: 910 lines answering the same question, no side adding any capability, and the docs
teaching people to maintain the duplication. That is not the case here: each implementation buys something real (zero dependencies / ecosystem and tiers), and only the **declaration** layer is duplicated.

The duplication in the declaration layer is already pinned by a mechanical comparison, the comparison is bidirectional, and the exemption table is empty. Replacing it with a generated single manifest
(one JSON/schema that C# and Python each read or generate from) has a concrete price: one more build step,
one more artifact to package, the strongly typed schema in `AgentTools.cs` becoming runtime-parsed, and, most important,
**the change would touch the one entry point that players can use without installing anything**.

The lesson of v0.12.4: when changing the judgment surface the agent can see, all green offline does not count. There is no ready live baseline here
that could prove "the two MCP surfaces behave the same before and after the refactor"; one has to exist before doing it.

## Decision

**Keep both implementations, but guard the consistency of the contract as a contract, not as a coincidence.**

Concretely:

1. `test_native_tool_alignment.py` is the guard on this seam, and it is not optional. Its three-way comparison and the **empty exemption table**
   are facts this ADR depends on; adding an entry to the exemption table records a real divergence and must state a reason and a deadline.
2. When adding an MCP tool, add it on both sides. This is the opposite of ADR 0001 and must be written explicitly in the `AGENTS.md`
   steps, because two opposite rules in the same repository are the easiest to mix up.
3. Do not write more patch-style contract tests for this duplication. The lesson of ADR 0001: patching duplication makes it look
   managed. Either rely on this one overall comparison, or eliminate the duplication.

## Triggers (if any one is met, re-evaluate)

- The first **non-temporary** exemption appears in `_DOC_ARGUMENT_EXEMPTIONS`, which means the two sides can no longer truly align;
- The **behavior** (not just the signature) of a same-named tool diverges between the two sides and is found in the live game;
- The tool count exceeds 15, or the schema declarations on either side exceed 300 lines;
- A third consumer (for example a standalone TypeScript client) needs the same schema.

If any of the first three holds, the premise of this ADR that "the duplication is acceptable" no longer holds.

## If it is done, how

In the order validated by ADR 0001, without skipping steps:

1. First get a live baseline: a full sample of `tools/list` and of each tool call on both MCP surfaces, saved to disk;
2. Then extract a single declaration source (tool name, description, inputSchema) and generate both sides from it;
3. Then replay and compare against the baseline; it is done only when the two surfaces show zero divergence;
4. `test_native_tool_alignment.py` changes shape accordingly: from "compare two implementations" to "assert that neither side has anywhere to put
   its own tool names or parameters", matching how `ActionSurface.*` is written today.

## Related

- [ADR 0001: Keep one judgment for the action surface](0001-single-action-surface.md)
- [Architecture constraints](../../.trellis/spec/mod/architecture.md)
- `mcp_server/tests/test_native_tool_alignment.py`
