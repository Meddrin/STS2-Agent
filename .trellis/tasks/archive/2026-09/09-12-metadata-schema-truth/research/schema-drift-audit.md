# Evidence: drift between the two scene field tables

## Locations on both sides

- C#: `SceneFieldSets` at `STS2AIAgent/Agent/GameDataFilter.cs:12-33`,
  consumed in `GameBridge.GetRelevantGameDataJsonAsync` (`GameBridge.cs:130-142`) ->
  `AgentLoop` (in-game) and `NativeMcpServer` (native MCP, off by default).
- Python: `_SCENE_FIELD_SETS` at `mcp_server/src/sts2_mcp/server.py:150`,
  consumed in `get_relevant_game_data` (`:758-785`) -> `get_game_data_items_fields` (`:342-369`) ->
  `client.get_game_data_collection` -> `GET /data/{collection}`.

## Measured differences (from the investigation)

| (scene, collection) | C# | Python | Gap |
|---|---|---|---|
| `combat.powers` | 6 items (including `allow_negative`) | 5 items | C# has an extra `allow_negative` |
| `shop.cards` | 11 items | 6 items | C# has extra `target`/`is_x_cost`/`star_cost`/`is_x_star_cost`/`keywords` |
| `shop.relics` | includes `is_melted` | none | C# has an extra `is_melted` |
| `combat.potions` | present (added last round) | no such key | Python lacks the whole combination |

(For the other combinations, go by the measured difference at implementation time; the table above comes from this round's read-only investigation.)

## Why the tests did not catch it

- The expected values at `test_game_data_tools.py:114-172` on the Python side come from `",".join(_SCENE_FIELD_SETS[...])`,
  **self-referencing**; the only "real schema" assertion at `:200-224` covers only `events`.
- The `GameDataExportSchemaTests` newly added on the C# side last round constrain only the C# table <-> export code, and do not involve Python.
- No test in the whole repo reads both sides at once (`SceneFieldSetView` appears only in C# and `AgentLoopTests`).

## Current state of `mcp_server/data/eng/` (for making the README tell the truth)

- 20 JSON files + README, about 1.2 MiB, `git ls-files mcp_server/data` = 21 files (tracked).
- Packaging paths: `mcp_server/pyproject.toml:23,26` (wheel/sdist force-include),
  `scripts/package-release.ps1:148`, `scripts/check_release_package.py:57` (only checks that the directory exists).
- **No code reads it** (multiple `rg` searches found no read point); the only runtime data source is the mod's HTTP export.
- Examples of differences from the export schema: `potions.json` has no `usage`/`target_type`; `relics.json` has no `is_melted`;
  `cards.json` has extra `image_url`/`beta_image_url`, etc.
- The existing README claims it is "used for MCP server data queries and AI decision support": **inconsistent with the code**.
