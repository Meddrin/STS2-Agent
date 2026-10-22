# Evidence

## Deliverables

| File | Change |
|---|---|
| `mcp_server/src/sts2_mcp/knowledge.py` | `_repo_root()` -> `Path \| None` (env first -> search upward for `mcp_server/pyproject.toml` -> **explicitly return None** when not found, deleting the `parents[3]` guess); when there is no repo root `_default_knowledge_root()` falls back to `Path.cwd()/agent_knowledge` and logs a WARNING with `logging` (without polluting stdio); added `_reference_files()`, which returns `[]` when there is no repo root |
| `mcp_server/tests/test_knowledge.py` (new) | 28 cases |
| `mcp_server/tests/test_handoff.py` (new) | 11 cases |
| `mcp_server/README.md` | The environment variable table gains `STS2_AGENT_REPO_ROOT` / `STS2_AGENT_KNOWLEDGE_DIR` |

## Acceptance

| Criterion | Evidence |
|---|---|
| No `parents[3]` fallback | `rg -n "parents\[3\]" knowledge.py` -> no output (exit 1) |
| Root resolution matrix | `RepoRootResolutionTests` 6 cases: env first / marker hit / neither -> `None` / `KNOWLEDGE_DIR` first / checkout-mode default / installed-mode fallback to cwd with `assertFalse(root.is_relative_to(install_dir))` + `assertLogs(...WARNING)` |
| `reference_files` empty without a root | `ReferenceFileTests` 3 cases (planner/combat `== []`, with a root they point to `docs/game-knowledge/*.md`) |
| Prescribed coverage | combat key normalization (`Cultist`+`cultist` -> `cultist_x2`), section append idempotence and the `##` boundary, combat/event create -> append -> `to_payload()`, `ValueError` for empty `event_id`/`summary`/`note`, handoff messages (`handoff_type`/`reset_context`/5 instructions/`knowledge_updates` increments) |
| Writes only tempfile | All use `Sts2KnowledgeBase(root_dir=<tempdir>)`; `git status --porcelain agent_knowledge` has no output, and the directory still has only `run_logs/` |

## Gates

`cd mcp_server; uv run --locked python -m unittest discover -s tests` -> **159 tests OK** (this task +39).

## Known deviations

- Added minimal deduplication in `_append_section_line` along the way (an identical bullet line is not appended twice); real calls all carry a UTC timestamp, so normal behavior is unchanged.
- The "deduplicated and sorted `enemy_ids`" in the design was not changed: `enemy_ids` in `handoff.py` has always been passed through as is (constrained by "do not change the external message structure"), and deduplication is guaranteed by `combat_key` and the knowledge file path.
