# Design: two-way sets + parameter names

## Data sources (all from source text, no need to run the game)

| Surface | Location | Extraction method |
|---|---|---|
| C# action branches | The `"name" =>` branches of `ExecuteAsync` in `STS2AIAgent/Game/GameActionService.cs` | Regex |
| C# native MCP branches | The switch branches of `STS2AIAgent/Server/NativeMcpServer.cs` | Regex |
| Python actions | `_LEGACY_ACTION_TOOLS` of `mcp_server/src/sts2_mcp/server.py` + the guided tool mapping | Regex/AST |
| Python parameters | Per-method signatures in `client.py` (`inspect.signature` or AST) | AST |
| Doc contract | The parameter column of the `<!-- BEGIN ACTION CONTRACT -->` block of `docs/api.md` | Regex parsing inside the block |

## Assertions

1. `python_actions - csharp_switch_actions == exempt` (exempt is empty or every entry carries a comment).
2. `csharp_switch_actions - python_actions == exempt_reverse` (explicit about which actions are deliberately not exposed).
3. For actions of the same name: `python_param_names(action) == contract_param_names(action)`
   (ignoring internal parameters such as `client_context`; the list must be written in the test).
4. On failure print the set differences (not assert one by one, to ease locating).

## Testability notes

- The existing `test_legacy_action_coverage.py` may already cover part of the set relations: read it first, extend rather than copy.
- Path resolution follows the marker probing at `test_legacy_action_coverage.py:20-25`, not depending on CWD.
- Do no heavy work at import time; put the parsing into `setUpClass`.
