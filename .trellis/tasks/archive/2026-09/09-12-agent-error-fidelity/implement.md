# Execution checklist

1. Read `GameBridge.cs:60-100`, `AgentLoop.cs:600-680`, `Server/ApiException.cs`,
   the `WriteErrorAsync` section of `Server/Router.cs` (about :310-340),
   the shared contract block of `skills/sts2-mcp-player/SKILL.md` (lines 48-131, including the error-related wording).
2. First find all consumers of the `error` field: `rg -n '"error"|error\.' STS2AIAgent/Agent skills/ docs/api.md`,
   and list every place affected by "string becomes object" (this is the only breaking risk of this change).
3. Create `Agent/AgentErrorEnvelope.cs` (pure BCL); adding it to the test project compile list is **done by the main agent**;
   you only write the file and the test class (`STS2AIAgent.Tests/AgentErrorEnvelopeTests.cs`).
4. Change the catch of `AgentLoop`; also change `GameBridge` if needed, keeping a single source for field names.
5. Update the wording of the shared contract block in SKILL.md (leave the marker alone).
6. Verify:
   ```powershell
   dotnet build STS2AIAgent/STS2AIAgent.csproj -c Release --no-incremental
   dotnet run --project STS2AIAgent.Tests/STS2AIAgent.Tests.csproj -c Release
   cd mcp_server; uv run --locked python -m unittest discover -s tests
   python scripts/check_verification_gates.py
   ```
7. Falsifiability self-check: change `CodeFor` to always return `internal_error` -> the truth table turns red -> restore.
8. Do not commit or push; report the changed files, the list of "consumers of the error field" with the compatibility conclusion, the evidence for each acceptance criterion, and the live items.
