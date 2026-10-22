# Design: dynamic traversal + single-call guard

## 1. Dead code removal

Keep the `return` of `invite_ai_teammate` at `client.py:391-392` and delete the whole block at :393-400
(including the blank lines after it), making sure the definition of the `choose_timeline_epoch` method still follows right after.

## 2. `mcp_server/tests/test_client_actions.py`

Data-driven, to avoid hand-writing 55 entries:

```python
SENTINELS = {"option_index": 11, "card_index": 12, "target_index": 13, "x": 14, "y": 15,
             "tool": "small", "command": "cmd"}

def _per_action_methods(cls):    # use inspect.getmembers + AST to decide: execute_action appears in the method body
    ...
```

For each method:

1. `inspect.signature` binds sentinel values (except `self`) -> call;
2. `patch.object(client, "_request", return_value={"ok": True})`, and assert
   `mock.assert_called_once_with("POST", "/action", payload=..., is_action=True)`;
3. assert `payload["action"] == method name`, `payload["client_context"] == {"source":"mcp","tool_name":method name}`;
4. assert the index sentinel values passed in appear on the **correct** payload keys (`card_index` must not land on `option_index`).

For the existing pattern see `tests/test_crystal_sphere_tools.py:79-106`; `DummyClient`/`RecordingClient`
are at `test_waits.py:23` and `test_crystal_sphere_tools.py:11-21`.

## 3. AST guard

Parse the `client.py` source with `ast` and count `self.execute_action(...)` calls in each class method:

- exactly 1 -> pass;
- 0 -> report "the method does not send an action";
- >= 2 -> report "the method body has duplicates/dead code (a suspected leftover block)".

This guard is independent of runtime behavior and can directly pin the kind of leftover deleted this round. Also assert that the set of method names does not conflict with
the action list of `_legacy_action_tools` / `server.py` (reuse if existing tests already cover it, do not repeat).

## 4. Running conventions

```bash
cd mcp_server
uv run --locked python -m unittest discover -s tests -v
```

Resolve the repo path following `test_legacy_action_coverage.py:20-25` (`Path(__file__).resolve().parents[N]` + marker),
and do not depend on CWD. Use only stdlib.
