# Deepen the native tool alignment test from "comparing names" to "comparing parameters and branch coverage"

## Background

`mcp_server/tests/test_native_tool_alignment.py` currently compares only **tool names**:

- it does not compare the parameter schema (drift of `option_index`/`card_index`/`target_index` on same-named tools would not be found);
- it does not check whether the `switch` of C# `NativeMcpServer.ExecuteToolAsync` covers every action the Python side exposes,
  and conversely does not check whether Python exposes actions C# does not have at all (an orphan tool would blow up only at runtime).

## Goals

1. Deepen the assertions inside the existing test file (no new duplicate file): tool name set + **parameter name set** + **action branch coverage**.
2. Conflicts/orphans must have an explicit exemption list, and each exemption carries a reason comment (nothing is allowed through silently).

## Acceptance criteria

- [x] Two-way action set assertion: Python-callable actions ⊆ the C# `ExecuteAsync` switch branches (no orphans),
      and reverse differences must fall into the commented exemption list.
- [x] Parameter name assertion: for same-named actions, the Python side (`client.py` method signatures / the registered inputSchema)
      agrees with the C# side (the parsing of `NativeMcpServer` or the parameter column of the `docs/api.md` action contract table).
- [x] Assertions are falsifiable: temporarily deleting a branch or changing a parameter name in the C# switch must turn the test red (record the actual output).
- [x] The existing gate (`python scripts/check_verification_gates.py`) and the full MCP tests pass.

## Out of scope

- Do not change the action contract block of `docs/api.md` (the 55 action names are checked by a gate).
- Do not change C# or Python production code; this task only deepens tests (if a test exposes a real defect, report first and then decide whether to open a new task).
