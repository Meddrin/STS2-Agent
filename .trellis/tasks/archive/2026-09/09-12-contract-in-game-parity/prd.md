# Make the embedded play contract satisfiable in-game

## Background

Lines 48-141 of `skills/sts2-mcp-player/SKILL.md` are the shared contract block (`<!-- BEGIN SHARED PLAY CONTRACT -->` to
`<!-- END SHARED PLAY CONTRACT -->`, embedded body lines 50-139). It is packed into the mod as an EmbeddedResource by `STS2AIAgent/STS2AIAgent.csproj:32-33`,
extracted by `PlayPrompt.cs:43-47`, and `PlayPrompt.BuildPlaySystem()`
(`PlayPrompt.cs:57-71`) splices it into the system prompt of **in-game auto-play** (rebuilt every step at `AgentLoop.cs:115-119`).

The problem: the first hard requirement in the contract **cannot be satisfied in-game**.

```markdown
SKILL.md:52  Call `health_check` once at session start.
SKILL.md:53  ... with `health_check` only at session start ...
```

The in-game tool set is `AgentTools.Play` (`STS2AIAgent/Agent/AgentTools.cs:70-78`) = `ReadOnly` (7 tools) + `act`,
**without `health_check`**; `health_check` exists only in `AgentTools.Mcp` (`:80-83`, defined at `:82`),
used by the native MCP endpoint (`NativeMcpServer.cs:414/454`).

Consequences (verified): the model emits that call -> the switch in `AgentLoop` has no match -> `AgentLoop.cs:547` returns
`{"error":"Unknown tool 'health_check'"}`. It does not crash, but every session wastes a round for nothing;
if no `act` is produced within 8 rounds, it ends with the round-limit error at `AgentLoop.cs:435-437`.
Worse is the self-contradiction: the same contract passage lists the tools available in-game at `PlayPrompt.cs:62`,
while the requirement above it names a tool that is not on that list.

A second issue of the same kind: `skills/sts2-mcp-player/references/screen-playbooks.md:93` requires entering the fight with debug
`fight THE_ARCHITECT_EVENT_ENCOUNTER`: in-game there is no console tool,
`act` has no `command` parameter slot (`AgentTools.cs:7-26`, `AgentLoop.cs:605-610`),
`run_console_command` is not in `available_actions` (`GameStateService.cs:2528-2810`),
and `act` is judged illegal directly at `AgentLoop.cs:593-603`. That playbook is also embedded (`csproj:33`).

## Goals

Make the text embedded for the in-game model **say only what it can do**, without weakening the contract for external MCP clients
(the guided tool set on the `mcp_server` side **does** have `health_check`, `server.py:619`).

## Acceptance criteria

- [x] The in-game system prompt no longer requires calling a tool that does not exist in-game: the wording of `health_check` changes from "must call"
      to a statement that holds for the in-game loop (for example, saying that the check is the responsibility of the external MCP client/orchestrator),
      and does **not delete** the existence and documentation of `health_check` on the MCP side.
- [x] The set of tool names in the embedded text ⊆ the tool name set of `AgentTools.Play` (add a falsifiable test).
- [x] The debug-only instruction in the playbooks is explicitly marked "external MCP only / needs `STS2_ENABLE_DEBUG_ACTIONS=1`",
      and no longer appears in a tone that is reachable in-game.
- [x] `McpPlayerSkillTests` (`STS2AIAgent.Tests/McpPlayerSkillTests.cs:7-36`, asserting SKILL.md matches
      `PlayPrompt.PlayContract`) stays green, or is updated per its real constraints.
- [x] Check `skills/sts2-mcp-player/SKILL.md:25` (the MCP-side tool list) and the corresponding statements in the README (if also untrue)
      together; `README.md:183`/`README.zh-CN.md:183` claim "built-in tools match in-game auto-play and include health_check",
      which does not match `AgentTools.cs:70-78` and must be fixed or explained.
- [x] Both marker lines stay present and paired (`PlayPrompt.ExtractSharedContract` depends on them).
- [x] Full C# / MCP tests, gates, and preflight pass.

## Out of scope

- Do not add `health_check` to `AgentTools.Play` (the in-game loop is itself in-process,
  and the "check the mod connection" semantics do not hold; this is a design judgment, explained in the design).
- Do not change the other requirements in the contract that were verified satisfiable (the probes checked item by item: all action names the contract mentions exist in
  `BuildAvailableActionNames`, and the compact fields are all there too).
