# Delete the dead code in client.py and add audit tests for the 55 per-action methods

## Background

`invite_ai_teammate` of `mcp_server/src/sts2_mcp/client.py` (:384-391) still has a whole block of **unreachable code** (:393-400)
hanging after its `return`:

```python
def invite_ai_teammate(self) -> dict[str, Any]:
    return self.execute_action("invite_ai_teammate", ...)

    return self.execute_action(                 # <- never reachable
        "choose_timeline_epoch",
        option_index=option_index,              # <- option_index does not exist in this scope
        ...
    )

def choose_timeline_epoch(self, option_index: int) -> dict[str, Any]:  # the correct method is below
```

If someone mistakenly deletes the `return` line above or adjusts the indentation, it immediately raises `NameError`; for now it merely hides
the real implementation of `invite_ai_teammate` in noise that "looks like a second method".

More importantly: these 55 per-action convenience methods have **no per-method tests**,
and `test_native_tool_alignment.py` compares only tool names, not parameters. Stuffing a parameter into the wrong key (`card_index` into
`option_index`), a wrong `client_context`, a method name inconsistent with the action name, or one call sending two POSTs
would not be caught by existing tests.

## Goals

1. Delete the dead code at :393-400, keeping the behavior of `invite_ai_teammate` unchanged.
2. Add `mcp_server/tests/test_client_actions.py`: verify method by method the `action` name of
   `POST /action`, the index parameter mapping, and `client_context`.
3. Add an AST guard: **exactly one** `execute_action` call inside each per-action method body
   (before the fix this would have flagged the unreachable second block; note that AST counts calls, so dead code is counted too,
   which is exactly what lets it pin this kind of leftover).

## Acceptance criteria

- [x] `client.py` no longer has an unreachable `execute_action` block; `git diff` deletes only and adds nothing.
- [x] The new test traverses all 55 methods (the method list is taken dynamically from the class definition, not a hardcoded list),
      and for each method asserts one `POST /action`, the correct `action` name, the correct payload keys,
      the correct `client_context`, and `is_action=True`.
- [x] Parameter mapping covers 6 shapes: no-arg (38), `option_index` (15), `play_card(card_index,target_index)`,
      `resolve_rewards(card_index,option_index)`, `choose_rest_option/use_potion(option_index,target_index)`,
      `crystal_set_tool(tool)`/`crystal_clear_cell(x,y,tool)`/`run_console_command(command)`.
- [x] The tests use only stdlib `unittest` + `unittest.mock`, add no dependencies, and use no network.
- [x] `cd mcp_server; uv run --locked python -m unittest discover -s tests` all green with the count increased.

## Out of scope

- Do not change `execute_action` itself (retry/reconciliation is already covered by `test_action_replay_safety.py`).
- Do not change the tool registration and profile structure of `server.py`.
- Do not verify whether the mod side accepts these actions (a live verification matter).
