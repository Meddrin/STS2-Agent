# Execution checklist

1. Read: `skills/sts2-mcp-player/SKILL.md` (the whole file, especially 25 and 48-141),
   `skills/sts2-mcp-player/references/screen-playbooks.md` (especially 85-100),
   `STS2AIAgent/Agent/AgentTools.cs`, `STS2AIAgent/Agent/PlayPrompt.cs`,
   `STS2AIAgent.Tests/McpPlayerSkillTests.cs`, `README.md:175-190`, `README.zh-CN.md:175-190`,
   the guided tool registration in `mcp_server/src/sts2_mcp/server.py` (confirm health_check exists on the MCP side).
2. Change the SKILL.md wording and the playbooks marking per the design; align both README languages.
3. Write the new test class `HealthCheckParityTests.cs` (or in the whitelist form of the design), **do not** change TestRunner.cs / csproj.
4. Falsifiability self-check: change the wording back to an imperative -> the new test goes red -> restore.
5. Verify:
   ```powershell
   dotnet build STS2AIAgent/STS2AIAgent.csproj -c Release --no-incremental
   dotnet run --project STS2AIAgent.Tests/STS2AIAgent.Tests.csproj -c Release
   python scripts/check_verification_gates.py
   cd mcp_server; uv run --locked python -m unittest discover -s tests
   ```
6. Do not commit or push; report the changed files, the red/green evidence, and the "live-only verification" items (whether the model really skips a round because of this).
