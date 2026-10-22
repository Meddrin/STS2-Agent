# Design: do not guess the root directory + injectable tests

## 1. Root resolution

```python
def _repo_root() -> Path | None:
    env = os.getenv("STS2_AGENT_REPO_ROOT")
    if env: return Path(env).expanduser().resolve()
    for parent in Path(__file__).resolve().parents:
        if (parent / "mcp_server" / "pyproject.toml").is_file(): return parent
    return None                      # explicitly "do not know"; no longer guessing

def _default_knowledge_root() -> Path:
    env = os.getenv("STS2_AGENT_KNOWLEDGE_DIR")
    if env: return Path(env).expanduser().resolve()
    repo = _repo_root()
    if repo is not None: return repo / "agent_knowledge"
    logging.getLogger(__name__).warning(
        "STS2 knowledge root is not inside a repository checkout; using %s. "
        "Set STS2_AGENT_KNOWLEDGE_DIR to pin it.", fallback)
    return fallback                 # Path.cwd() / "agent_knowledge"
```

All call sites are updated as `_repo_root()` becomes optional (`_default_knowledge_root`,
the `reference_files` of `build_planner_context` and `build_combat_context` -> `[]` when `None`).
**Forbidden**: `print` (stdio transport protocol); use `logging`.

## 2. `tests/test_knowledge.py`

All use `Sts2KnowledgeBase(root_dir=<tempdir>)`; when root resolution itself is needed, use `patch.dict(os.environ)` +
`patch.object(knowledge, "__file__", ...)` or the like, or include `_repo_root` directly in the tests
(testability first: `_repo_root` may get an optional `start: Path | None = None` parameter for injecting the starting point).

Suggested cases:

- `_combat_key([{"enemy_id":"Cultist"},{"enemy_id":"cultist"}]) == "cultist_x2"`;
  `_parse_combat_key_part("cultist_x3") == ("cultist", 3)`;
- `_append_section_line` idempotent (appending the same content repeatedly does not repeat it), behavior across `##` boundaries;
- `resolve_combat_entry(create_if_missing=True)` generates `combat/global/solo/<key>.md`,
  after appending `to_payload()["relative_path"]` is correct, and the content contains `run_id=`/`floor=`/`screen=`;
- `resolve_event_entry_by_id("")` -> `ValueError`;
- `_enumerate_paths` forks/cycles deduplicated.

## 3. `tests/test_handoff.py`

`Sts2HandoffService(Sts2KnowledgeBase(root_dir=tmp))`:

- planner message: `handoff_type == "planner"`, `reset_context is True`, the instructions count is 5;
- combat message: no combat context -> `ValueError`; with `combat.enemies` -> `combat_key` / the deduplicated and sorted `enemy_ids`;
- `complete_combat_handoff`: empty summary -> `ValueError`; non-empty -> the number of `knowledge_updates` increases with the optional note;
- `complete_event_handoff`: with `option_index=1` the written line contains `option_index=1`.

## 4. Docs

`mcp_server/README.md` environment variable table gains two rows (`STS2_AGENT_REPO_ROOT`, `STS2_AGENT_KNOWLEDGE_DIR`),
and states that the installed-mode default lands in `agent_knowledge/` under the current working directory.
