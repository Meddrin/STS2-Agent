# Execution checklist

1. Read `STS2AIAgent/Agent/GameDataFilter.cs`, `mcp_server/src/sts2_mcp/server.py:140-230`,
   `mcp_server/tests/test_game_data_tools.py:100-270`,
   `mcp_server/tests/test_legacy_action_coverage.py:15-35` (the repo root probing pattern),
   `STS2AIAgent/Agent/GameDataExportSchema.cs`, `mcp_server/data/README.md`.
2. First write `test_scene_field_alignment.py` and get it **red** (record each difference).
3. Fix `_SCENE_FIELD_SETS` in `server.py` per the differences until the tests are green.
4. Change the self-referencing assertions in `test_game_data_tools.py` to independent expected values.
5. Change `mcp_server/data/README.md` and add `test_packaged_game_data.py`.
6. Counter-check self-test: add a field only in C# -> red; add a field only in Python -> red; delete the id of one JSON -> red; restore.
7. Verify:
   ```bash
   cd mcp_server; uv run --locked python -m unittest discover -s tests
   python scripts/check_verification_gates.py
   python scripts/check_release_package.py --source-root .
   ```
8. Do not commit or push; report the changed files, the list of difference fixes (before/after), the evidence for each acceptance criterion, and
   the current state of "whether to keep data/eng is for the user to decide".
