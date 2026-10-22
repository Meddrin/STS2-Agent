# Evidence

## Deliverables

| File | Change |
|---|---|
| `mcp_server/tests/test_native_tool_alignment.py` | Expanded (+310/-1): added 3 tests and source parsing helpers; the original name comparison test is kept as is |

Zero production code changes; the `NativeMcpServer.cs` temporarily changed during the counter-check was restored by SHA256 (`59A67FC5…4044`).

## New assertions

| Test | Coverage |
|---|---|
| `test_native_switch_covers_the_python_guided_surface` | Two-way branch coverage: Python guided tool set <-> the `ExecuteToolAsync` switch branches (exempting `wait_for_event`, with a reason); and asserts `AgentTools.Mcp` (the tools/list advertised surface) ⊆ switch |
| `test_shared_guided_tools_use_identical_argument_names` | Parameter names of same-named tools: the Python inputSchema <-> the `arguments` keys actually parsed by C# (recursively merging `ExecuteActAsync`/`WaitUntilActionableJsonAsync`; `timeout_seconds` is derived from the body of the `ReadTimeoutSeconds` method); prints the differences on failure |
| `test_documented_action_arguments_match_registered_legacy_tools` | The `docs/api.md` contract block (55) <-> the legacy tools actually registered in the full profile (54): two-way set + parameter names; exempts `run_console_command` (registered separately under debug) and `crystal_clear_cell: {tool}` (the contract block is frozen), and an exemption becoming stale reports an additional error |

Measured extracted values (not spinning): 9 switch branches; `act` -> `['action','card_index','option_index','target_index','tool','x','y']`; `wait_until_actionable` -> `['timeout_seconds']`.

## Falsifiable (real red)

1. Delete the C# `case "act":` -> `FAIL test_native_switch_covers_the_python_guided_surface`
2. `ReadInt(arguments,"option_index")` -> `"index"` -> `FAIL test_shared_guided_tools_use_identical_argument_names` (prints both columns of parameter names)
3. Remove the `crystal_clear_cell` exemption -> `FAIL test_documented_action_arguments_match_registered_legacy_tools`

## Gates

`uv run --locked python -m unittest discover -s tests` -> **159 tests OK** (this task +3);
`python scripts/check_verification_gates.py` -> passed (`55 actions match`).

## Leftovers

The `docs/api.md` action contract block does not register the optional parameter `tool` of `crystal_clear_cell` (the contract block is frozen by a gate and this task may not change it), and this was turned into an explicit exemption;
it is suggested to open a separate task later to fix the docs or confirm it can be ignored.
