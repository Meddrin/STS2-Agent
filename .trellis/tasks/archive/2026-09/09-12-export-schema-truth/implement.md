# Execution checklist

1. Read the 7 `ExportXxx()` of `STS2AIAgent/Game/GameDataExportService.cs` line by line (:35-180),
   and copy the field names into the manifest; do not copy the existing table of `GameDataFilter`.
2. Add `Agent/GameDataExportSchema.cs` and add it to the test project's compile list.
3. First write the three tests (they should turn red at this point because of `hp`/`damage`/`block`/`target`), and record the red output.
4. Change `SceneFieldSets` in `GameDataFilter.cs` to turn the tests green.
5. Verify:
   ```powershell
   dotnet run --project STS2AIAgent.Tests/STS2AIAgent.Tests.csproj -c Release
   dotnet build STS2AIAgent/STS2AIAgent.csproj -c Release --no-incremental
   cd mcp_server; uv run --locked python -m unittest discover -s tests
   python scripts/check_verification_gates.py
   ```
6. Counter-check self-test: temporarily break a field name, confirm the test turns red, then change it back.
7. Do not commit or push; report the actual output snippets of "red before the fix -> green after the fix".
