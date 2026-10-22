# Design: offline coverage of network_server and the layered tools

## 1. `tests/test_network_server.py`

| Target | Technique |
|---|---|
| `NetworkServerConfig` defaults | Construct directly + assert `host/port/path`, with `auth_enabled` changing with the token |
| `_env_flag` | `patch.dict(os.environ, {...}, clear=False)` truth table |
| `_normalize_path` | Parameterized inputs -> expected outputs |
| `_build_auth_provider` | Empty token -> `None`; non-empty -> returns a `StaticTokenVerifier` instance |
| `parse_args` | Set environment variables via `patch.dict` then `parse_args([])`; mutually exclusive arguments use `assertRaises(SystemExit)` + `.code == 2` |
| `create_network_app` | `patch("sts2_mcp.network_server.Sts2Client")` injects a fake; assert the returned triple and that the routes exist; call `root_endpoint` directly and assert `ok=True` |
| healthz branches | A fake client makes `get_health()` return ok / raise an exception, asserting both the 200 and 503 responses |

Do not actually bind a port; do not call `run_network_server_async`.

## 2. `tests/test_layered_tools.py`

Reuse the existing `DummyClient` pattern (`test_waits.py:23`, `test_agent_contract.py:61`):

```python
with tempfile.TemporaryDirectory() as tmp:
    os.environ["STS2_AGENT_KNOWLEDGE_DIR"] = tmp      # must be before create_server
    server = create_server(tool_profile="layered", client=DummyClient(...))
    fn = server.get_tool("get_planner_context").fn
    payload = asyncio.run(fn())
```

Note to wrap it in `patch.dict` so that environment variables do not leak into other tests.

Cases:

- `get_planner_context` / `create_planner_handoff`: the pure branch without an event (nothing is written to disk when state has no `event.event_id`),
  assert the key set and the key points of the `instructions` text;
- `get_combat_context` / `create_combat_handoff`: give a state with `combat.enemies`, and assert `combat_key`/deduplicated sorting;
- `complete_combat_handoff` / `complete_event_handoff`: assert `DummyClient.get_state` is **not called**
  (they should not touch sts2): this is a behavioral contract, not an implementation detail;
- `append_combat_knowledge` / `append_event_knowledge`: assert the write lands at the expected relative path inside the tmp directory.

## 3. Running conventions

Use only stdlib `unittest`/`unittest.mock` and `tempfile`; add no dependencies (`--locked` would fail);
put files into `mcp_server/tests/`, named `test_*.py`, with no `__init__.py`.
