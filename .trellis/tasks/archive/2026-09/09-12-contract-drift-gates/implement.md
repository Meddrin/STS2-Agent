# Execution checklist

1. Read `scripts/check_verification_gates.py` (focus on the `GATES` registration and how the three existing gates are written),
   `scripts/test-verification-gates.ps1`, `docs/api.md:1-160` (version, screen enum, port),
   `ResolveNonModalScreen` in `STS2AIAgent/Game/GameStateService.cs`,
   `mcp_server/README.md:1-140`, `_LEGACY_ACTION_TOOLS` in `mcp_server/src/sts2_mcp/server.py`,
   `.github/workflows/validate.yml`.
2. Implement `check_api_facts`, first only outputting the extraction results (a dry run printing the counts), and change it into assertions once confirmed correct.
3. Falsifiability self-check one by one: break the version number / delete a screen name / change the port -> see red three separate times -> restore.
4. Fix the tool list in `mcp_server/README.md` and add the two-way set test.
5. Add the CI step; add cases to `test-verification-gates.ps1` (mind the BOM).
6. Verify:
   ```powershell
   python scripts/check_verification_gates.py
   powershell -ExecutionPolicy Bypass -File scripts/test-verification-gates.ps1
   cd mcp_server; uv run --locked python -m unittest discover -s tests
   python scripts/check_release_package.py --source-root .
   ```
7. Do not commit or push; report the changed files, the extraction results and red/green evidence of each gate, and a note that "it has not run on real CI".
