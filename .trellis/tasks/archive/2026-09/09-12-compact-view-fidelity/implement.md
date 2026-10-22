# Execution checklist

1. Read `GameStateService.cs:2973-3040` (compact top level), `:3110-3168` (combat), `:3190-3230` (run),
   `:3480-3510` (modal / chest), `:3540-3690` (card compact builders), `:4722-4736` (unlock),
   `:7580-7660` (raw powers / players summaries), `:7750-7790` (raw intents).
   Go by the **real field names**; do not copy the examples in this file verbatim.
2. Add fields incrementally per landing points 1 / 2 / 3 of the design; do not change existing fields (backward compatibility).
3. Create `STS2AIAgent.Tests/CompactViewFidelityTests.cs` with 4 static methods
   (`PowersReachTheCompactCombatView`, `IntentNumbersReachTheCompactCombatView`,
   `CardAndRelicIdsReachTheCompactViews`, `OverlayAndPartyReachTheCompactView`),
   all using source contracts + raw-side same-name cross-checks. **Do not change TestRunner.cs / csproj**.
4. Falsifiability self-check: delete fields one by one, run the tests to see red, then restore.
5. Sync the docs: the agent_view-related paragraphs in `docs/api.md`, and the paragraphs in `skills/sts2-mcp-player/SKILL.md` that reference these fields
   (leave the two SHARED PLAY CONTRACT markers alone).
6. Verify:
   ```powershell
   dotnet build STS2AIAgent/STS2AIAgent.csproj -c Release --no-incremental
   dotnet run --project STS2AIAgent.Tests/STS2AIAgent.Tests.csproj -c Release
   cd mcp_server; uv run --locked python -m unittest discover -s tests
   python scripts/check_verification_gates.py
   ```
7. Do not commit or push; report the changed files, the field list (added key -> data source line number), the evidence for each acceptance criterion, and
   the "live-only verification" items (whether these fields are non-empty and correct in a real fight).
