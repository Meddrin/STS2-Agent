# Evidence

## Deliverables

| File | Change |
|---|---|
| `mcp_server/tests/test_network_server.py` (new) | 23 cases |
| `mcp_server/tests/test_layered_tools.py` (new) | 12 cases |

Zero production code changes (`network_server.py`/`server.py`/`pyproject.toml`/`uv.lock` have no difference from HEAD).

## Acceptance

| Criterion | Evidence |
|---|---|
| Config defaults + empty token does not enable auth | `NetworkServerConfigTests` asserts host/port/transport/path/tool_profile/api_base_url/token/log_level and both branches of `auth_enabled` |
| `_env_flag` truth table | `EnvFlagTests`: 1/true/True/TRUE/`"  yes  "`/on/0/false/no/2/enabled/empty string/pure whitespace; measuring calibrated one real behavior: pure whitespace does not fall back to the default (an explicit non-true value) |
| `_normalize_path` boundaries | `NormalizePathTests`: 9 kinds of input (including `/mcp/`, `//mcp`, multi-segment, empty string) |
| `parse_args` | `ParseArgsTests`: 10 fields taken from environment variables; `sse`+`--stateless-http` -> `ctx.exception.code == 2` |
| `create_network_app` + both healthz branches | `NetworkAppTests`: patch `Sts2Client` to inject a fake client (no connection to the real 8080, no port binding); assert the returned triple, the timeout configuration, and the `/`, `/healthz`, `/mcp` routes; healthz normal 200 / `Sts2ApiError` -> 503 (keeping code/retryable) / ordinary exception -> 500 |
| 8 layered tools | Each tool is called once directly; all writes land in `tempfile` (`patch.dict` sets `STS2_AGENT_KNOWLEDGE_DIR`, before `create_server`); assert the deduplicated sorting of `jaw_worm_x2+louse_x1` and zero writes on the no-event branch |
| Two tools that do not touch sts2 | `complete_combat_handoff`/`complete_event_handoff` assert `client.get_state_calls == 0` |
| The repo is not dirtied | After running the full suite `agent_knowledge/` still has only `run_logs/` |

## Gates

`cd mcp_server; uv run --locked python -m unittest discover -s tests` -> **159 tests OK** (this task +35).

## Not proven (live only)

- `run_network_server_async` was not called (really binding a port + uvicorn.serve), and its argument assembly is covered only indirectly through `NetworkServerConfig`/`parse_args`.
- The real transport layer (port listening, remote MCP clients over HTTP/SSE, the bearer auth handshake) needs end-to-end live verification.
