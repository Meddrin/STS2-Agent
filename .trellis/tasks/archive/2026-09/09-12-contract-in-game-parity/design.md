# Design

## Key distinction: one file serves two kinds of readers

| Reader | Text received | Tool set |
|---|---|---|
| In-game auto-play | `PlayPrompt.PlaySystem` = shared contract block (50-139) + playbooks | `AgentTools.Play` (no health_check) |
| External MCP client | The whole SKILL.md (including the Quick Start, decision log, etc. outside the block) | The guided tool set of `mcp_server` (**has** health_check, `server.py:619`) |
| Native MCP (the mod's built-in endpoint) | SKILL.md as `instructions` (`NativeMcpServer.cs:374`) | `AgentTools.Mcp` (**has** health_check) |

So the fix must be "make the wording hold in-game", not "delete health_check from the docs".

## The change

1. The Quick Start of `SKILL.md`: change "Call `health_check` once at session start." into a statement that holds for the
   **orchestrator layer**, and make clear that the in-game loop has no such tool and should not try. Suggested shape
   (finalize per the writing style at implementation time, but it must satisfy acceptance criteria 1 and 2):
   - one sentence saying that connection checking is the responsibility of the external MCP client/orchestrator;
   - the in-game loop starts directly from `get_game_state`.
2. Add a test (`STS2AIAgent.Tests/`, registered by the main agent):
   `McpPlayerSkillTests` already has the assertion "SKILL.md shared block <-> PlayPrompt.PlayContract match";
   the new test adds a **tool-name inclusion** assertion on top of it:
   - parse the tool name literals in the definition of `AgentTools.Play` (including `ReadOnly`);
   - parse the candidates in the shared contract block wrapped in backticks that look like tool names (taking what appears in `PlayPrompt.PlaySystem` as authoritative);
   - assert that the **callable tool names** mentioned in the contract block ⊆ the Play set; names that belong only to MCP such as `health_check`
     must appear in a "non-invocation tone" (at implementation time judge this by "whether it appears in an imperative context like Call/Use",
     or the sturdier approach: make the tool names allowed in the contract block an explicit whitelist and assert that the two match).
   The assertion must be falsifiable: temporarily change `health_check` back to an imperative sentence -> red.
3. The debug instruction at `screen-playbooks.md:93`: mark it "external MCP only (needs `STS2_ENABLE_DEBUG_ACTIONS=1`)",
   and give an in-game executable alternative (for example, play out that act through the normal flow).
4. The tool list at line 183 of both README language versions: align with `AgentTools.cs` (remove health_check or state that it is MCP only).

## Pitfalls

- The contract block is cut by `PlayPrompt.ExtractSharedContract` via `IndexOf` on the markers, **the marker lines themselves must not be deleted**,
  and a second pair of markers with the same name must not be added.
- `McpPlayerSkillTests` asserts "file content matches the embedded contract"; after changing SKILL.md it must be rerun.
- Changes under `skills/**` enter the mod build (EmbeddedResource), so after changing run
  `dotnet build STS2AIAgent/STS2AIAgent.csproj` to confirm the embedding still holds.
