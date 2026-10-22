# Execution checklist

1. Read `mcp_server/src/sts2_mcp/knowledge.py` (the whole file, about 540 lines) and `handoff.py` (about 210 lines),
   and confirm every `_repo_root()` / `_default_knowledge_root()` call site.
2. Change root resolution per the design; `reference_files` returns `[]` when there is no repo root.
3. Write `tests/test_knowledge.py` and `tests/test_handoff.py` (stdlib `unittest`, injecting `tempfile`).
4. Fill in the README environment variable table.
5. Verify:
   ```bash
   cd mcp_server
   uv run --locked python -m unittest discover -s tests -v
   ```
   and confirm that `git status` has **no** new files under `agent_knowledge/`.
6. Do not commit or push; report the new files, the test counts, and the evidence for "nothing written into the repo".
