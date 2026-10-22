# Evidence

## Deliverables

| File | Change |
|---|---|
| `mcp_server/tests/test_scene_field_alignment.py` (new) | 5 cases: parse the C# `SceneFieldSets` and the Python `_SCENE_FIELD_SETS` and compare two-way by (scene, collection); also includes "Python fields must be within the export schema" and a parsing sanity guard |
| `mcp_server/tests/test_packaged_game_data.py` (new) | 2 shallow tests: `data/eng/*.json` parses; entry `id` is non-empty and unique (using `object_pairs_hook` to catch duplicate keys); skip when the directory is missing (so a future deletion decision is not blocked) |
| `mcp_server/src/sts2_mcp/server.py` | Only the `_SCENE_FIELD_SETS` field sets + 3 comment lines pointing to the cross-language test (`+24/-0`) |
| `mcp_server/tests/test_game_data_tools.py` | 3 scene field assertions changed from self-referencing to independent literals; the event assertion adds `type` and a new `act` dropped assertion |
| `mcp_server/data/README.md` | Rewritten into three facts: packaged snapshot / the only runtime data source is the mod's `GET /data/{collection}` / the schema is inconsistent with the export and must not serve as a field basis |

## Differences (real red output before the fix)

```
- combat.potions   whole group missing (C# has it, Python does not)
- combat/powers    ← allow_negative
- event/events     ← act, type
- shop/cards       ← is_x_cost, is_x_star_cost, keywords, star_cost, target
- shop/potions     ← pool, target_type, usage
- shop/relics      ← is_melted
```

(Two more than the research doc found: `event/events` and `shop/potions`.) After the fix the difference is 0.

## Acceptance

| Criterion | Evidence |
|---|---|
| Red first, then green | 2 failures before the fix; after the fix `Ran 5 tests ... OK` |
| Two-way reverse detection | Adding `color` only in C# -> `only_in_csharp=['color']`; adding it only in Python -> `only_in_python=['color']` |
| Shallow data tests are falsifiable | A probe file missing `id` -> reports the entry index; invalid JSON -> reports a parse failure; the probe was deleted |
| Self-referencing assertions fixed | The three assertions in `test_game_data_tools.py` changed to independent literals |
| Integrity | All temporary counter-check edits restored (SHA256 identical to before starting: `GameDataFilter.cs 7E18E918…`, `keywords.json 4549E411…`, `potions.json CE3F2424…`) |

## Gates

MCP 167 OK; all 5 gates of `check_verification_gates.py` passed; `check_release_package.py --source-root .` passed;
preflight all green.

## Leftover decision item

Whether to keep `mcp_server/data/eng/` (20 JSON files, about 1.2 MiB, read by no code, shipped with the package) is **for the user to decide**;
the README already states it is a snapshot rather than a data source, and the tests skip automatically when the directory is missing.
