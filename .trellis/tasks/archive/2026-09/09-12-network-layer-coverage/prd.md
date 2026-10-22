# Cover the network_server configuration and the 8 layered knowledge tools

## Background

`mcp_server/tests/` currently has 7 files (`test_action_replay_safety`, `test_agent_contract`,
`test_crystal_sphere_tools`, `test_game_data_tools`, `test_legacy_action_coverage`,
`test_native_tool_alignment`, `test_waits`),
and `rg -n 'network_server|handoff|knowledge' mcp_server/tests` has **no hits**:

- `network_server.py` (:21-235): `NetworkServerConfig`, `_env_flag`, `_normalize_path`,
  `_build_auth_provider`, `create_network_app`, `parse_args`, `run_network_server_async` are all untested;
- The 8 layered tools of `server.py` (:625-724) are also untested.

This code decides "whether you can connect, whether auth is required, whether the path is right", the layer that is easiest to break and hardest to notice.

## Goals

1. Add `mcp_server/tests/test_network_server.py`: cover the pure logic and argument parsing (including the error path of mutually exclusive arguments).
2. Add `mcp_server/tests/test_layered_tools.py`: cover the message structure of the 8 layered tools,
   and verify that the "two tools that do not touch sts2" (`complete_combat_handoff`/`complete_event_handoff`) really do not call `get_state`.
3. Fully offline: do not connect to the real `127.0.0.1:8080`, do not bind real ports, do not write the repo's `agent_knowledge/`.

## Acceptance criteria

- [x] `NetworkServerConfig` defaults (host/port/path/token), and `auth_enabled` is False with an empty token (asserted).
- [x] The truth table of `_env_flag` (1/true/yes/empty/unset) is covered with `patch.dict(os.environ)`.
- [x] The boundaries of `_normalize_path` (with/without a leading slash, empty string, repeated slashes) are asserted.
- [x] `parse_args`: defaults come from environment variables; when `--transport sse` and `--stateless-http` are both given,
      `parser.error` -> `SystemExit(2)` (assert the exit code).
- [x] `create_network_app` returns `(server, client, app)` and the routes are registered (`/healthz` etc.);
      the healthz success/failure branches use a fake client or a local stub, **without depending on the real mod**.
- [x] Each of the 8 layered tools is called once (`asyncio.run(server.get_tool(name)).fn(...)`,
      see `test_crystal_sphere_tools.py:45-47`), asserting the key keys exist;
      all knowledge writes land in a `tempfile` directory (`STS2_AGENT_KNOWLEDGE_DIR` or
      `patch("sts2_mcp.server.Sts2KnowledgeBase")`, which must be set before `create_server`).
- [x] After the tests the repo `agent_knowledge/` has no new files (`git status` evidence).
- [x] `uv run --locked python -m unittest discover -s tests` all green.

## Out of scope

- `run_network_server_async` (really binding a port, running uvicorn) is not unit tested; the tests only assert that its argument assembly can be read.
- The fact that `network_server.py` has no client injection point is **not fixed by changing production code** (if really necessary, change the patching approach of the tests).
- Do not change the return structure of any tool.
