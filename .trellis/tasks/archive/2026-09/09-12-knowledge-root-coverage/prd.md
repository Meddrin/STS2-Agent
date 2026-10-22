# Fix knowledge root directory resolution, and add tests for knowledge / handoff

## Background

`mcp_server/src/sts2_mcp/knowledge.py:12-30`:

```python
def _repo_root() -> Path:
    env = os.getenv("STS2_AGENT_REPO_ROOT")
    ...
    for parent in ...:                     # look for mcp_server/pyproject.toml
        ...
    return current.parents[3]              # <- fallback: pure guesswork
```

In a wheel/pipx install `__file__` is at `<site-packages>/sts2_mcp/knowledge.py`,
searching upward cannot find `mcp_server/pyproject.toml`, so `parents[3]` returns the **venv or the Python install root**:

- `_default_knowledge_root()` -> `<venv>/agent_knowledge`, and the write branch `mkdir(parents=True, exist_ok=True)`
  silently creates a directory inside the venv (lost on upgrade/reinstall, and the user can never find it);
- `reference_files` (:334-339 / :366-371) points to `<venv>/docs/game-knowledge/*.md` (which does not exist).

`STS2_AGENT_REPO_ROOT` appears in the whole repo only at knowledge.py:13, and the README environment variable table does not have it either.

Also `knowledge.py` (about 540 lines) and `handoff.py` (about 210 lines) have **zero tests**:
`rg -n 'knowledge|handoff' mcp_server/tests` has no hits.

## Goals

1. Remove the `parents[3]` guess: when no repo marker is found, **do not guess**; use an explicit, visible, writable fallback directory instead,
   and say so in the log (never write to stdout: the stdio transport protocol must stay clean).
2. `reference_files` returns an empty list when there is no repo root, instead of pointing to fake paths inside the venv.
3. Add offline tests for `knowledge.py` and `handoff.py` (all injectable via `root_dir` or environment variables,
   never writing the repo's real `agent_knowledge/`).
4. Add `STS2_AGENT_REPO_ROOT` / `STS2_AGENT_KNOWLEDGE_DIR` to the README environment variable table.

## Acceptance criteria

- [x] `knowledge.py` contains no unfounded fallback like `parents[3]`.
- [x] The root resolution matrix has tests: env override first / use the repo root when the marker is found / fall back to
      `Path.cwd()/agent_knowledge` when both fail (or an equivalent confirmed in the design), and **not landing inside the install directory**.
- [x] `reference_files` is an empty list when there is no repo root (tested).
- [x] New tests cover: combat key normalization and parsing, section append idempotence, the combat/event entry lifecycle
      (create -> append -> `to_payload()`), `ValueError` for empty `event_id`/`summary`,
      handoff message structure (`handoff_type`/`reset_context`/instruction count/number of knowledge writes increasing with the optional parameter).
- [x] Tests write only `tempfile` directories; the repo `agent_knowledge/` keeps only `run_logs/`.
- [x] `cd mcp_server; uv run --locked python -m unittest discover -s tests` all green.

## Out of scope

- Do not change the directory structure of knowledge files (`combat/global/{solo,groups}/<key>.md`, `events/global/<id>.md`).
- Do not decide the fate of `mcp_server/data/eng/` (a user decision item).
- Do not touch the skill-side `agent_knowledge/run_logs/` convention.
