# Evidence

## Deliverables

| File | Change |
|---|---|
| `mcp_server/src/sts2_mcp/client.py` | Deleted the unreachable `execute_action("choose_timeline_epoch", option_index=option_index, ...)` block after `invite_ai_teammate` (:393-400); `0 added 9 deleted`, no other refactoring |
| `mcp_server/tests/test_client_actions.py` (new) | 6 cases + AST guard |

## Acceptance

| Criterion | Evidence |
|---|---|
| Dead code gone, delete only no add | `git diff --numstat` -> `0 9`; the `choose_timeline_epoch` definition follows right after `return` |
| 55 methods traversed dynamically | The method list comes from `ast` parsing (not hand-written), and is asserted to equal exactly `{the 54 of _LEGACY_ACTION_TOOLS}` ∪ `{run_console_command}` |
| 6 parameter mapping shapes | Measured distribution: no-arg 33, `option_index` 15, `play_card(card_index,target_index)`, `resolve_rewards(option_index,card_index)`, `choose_rest_option/use_potion(option_index,target_index)`, `crystal_set_tool(tool)`/`crystal_clear_cell(x,y,tool)`/`run_console_command(command)` |
| One POST per method + correct action/keys/client_context | `_request.assert_called_once_with("POST","/action",payload=...,is_action=True)`; the sentinel values are all distinct, so a wrong key is always caught |
| stdlib only, no new dependencies | Imports only `ast`/`unittest`/`unittest.mock`/`pathlib`; `pyproject.toml`/`uv.lock` unchanged |
| Falsifiable | All three mutations caught: a wrong `play_card` key (runtime red), a wrong `tool_name` for `choose_map_node` (AST + runtime red), re-inserting the unreachable block (all 3 AST guard checks red, runtime still green -> exactly why the AST guard is needed) |

## Gates

`cd mcp_server; uv run --locked python -m unittest discover -s tests` -> **159 tests OK** (this task +6).

## Out of scope

Whether the mod side accepts these actions is unverified (a live verification matter).
