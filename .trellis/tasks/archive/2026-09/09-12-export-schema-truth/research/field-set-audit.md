# Evidence: scene field sets vs the real export

## Two independent consumption chains (different tables used)

| Chain | Entry | Field table | Data source |
|---|---|---|---|
| In-game Agent + native MCP | `GameBridge.cs:130-142` -> `GameDataFilter.ProjectRelevant` | C# `SceneFieldSets` (:12-30) | ModelDb inside the game process |
| External MCP (default) | `server.py:758 get_relevant_game_data` -> `_SCENE_FIELD_SETS` (server.py:150-219) | Python table | HTTP `GET /data/{collection}` (`Router.cs:196-212`, returns the whole set) |

After field-by-field checking, the Python table has an empty **(a) side** (no nonexistent field names): combat/monsters uses
`min_hp,max_hp,moves,damage_values,block_values`, and shop/potions does not request `target`.
This round's bug is only in the C# chain.

## Silence

C# `ProjectFields` (:125-137) and Python `server.py:366` both write only
`if (key in item)`, so missing fields raise no error and no warning; the error branches cover only unknown collections and unavailable data.

## Why existing tests did not catch it

- The assertion values in `mcp_server/tests/test_game_data_tools.py:114-172` come from
  `",".join(_SCENE_FIELD_SETS[...])`: **self-referencing**, change the table and it follows.
- `STS2AIAgent.Tests/AgentLoopTests.cs:8-32` asserts only that combat/cards keeps `name` and drops `flavor`.
- Neither side has assertions of monsters / potions / relics against the real export schema.

## A static dataset cannot serve as a schema

`mcp_server/data/eng/*.json` (20 files, only packaged, no code reads them) is inconsistent with the runtime export:
`monsters.json` contains `min_hp_ascension/max_hp_ascension/image_url`,
`potions.json` **does not have** `usage/target_type`.

## Known gaps

`COMBAT_SCREEN_KEYWORDS` and other constants in `server.py` versus the C# `DetectScene` (GameDataFilter.cs:32-51)
have not been compared item by item whether equivalent (beyond the scope of this round's field difference).
