# Design: cross-language comparison

## 1. New test `mcp_server/tests/test_scene_field_alignment.py`

Parse the text of two source files (without importing C#, without depending on the game):

```python
CSHARP_FILTER = "STS2AIAgent/Agent/GameDataFilter.cs"
# Repo root resolution follows the marker probing of test_legacy_action_coverage.py:20-25 (do not trust CWD)
```

C# side parsing strategy: locate the initialization block of `SceneFieldSets` and extract with two layers of regex, `["scene"] = new Dictionary<string, string[]>`
and `["collection"] = new[] { "a", "b", ... }`.
The Python side does directly `from sts2_mcp.server import _SCENE_FIELD_SETS`.

Assertions:

- `set(csharp[(scene, coll)]) == set(python[(scene, coll)])`, printing the differences separately as
  `only_in_csharp` / `only_in_python`;
- the scene sets themselves are equal in both directions (preventing a new scene from landing on one side only);
- the failure message includes "the full set of export fields of that collection" (from `STS2AIAgent/Agent/GameDataExportSchema.cs`),
  to help judge whether a field is "missing" or "written but nonexistent".

The Python-side collection name may be `_SCENE_FIELD_SETS` (private), and the test can import the private name directly (an in-package test).

## 2. Fix the drift

Use C# as the baseline to fill in Python:

- `combat.powers` gains `allow_negative`;
- `shop.cards` gains `target` / `is_x_cost` / `star_cost` / `is_x_star_cost` / `keywords`;
- `shop.relics` gains `is_melted` (and `pool`);
- add `combat.potions` (same field set as `shop.potions`);
- for the other (scene, collection) pairs, align one by one according to the differences in the test output.

Note: extra fields on the Python side merely make `get_relevant_game_data` return more keys (the filter is `key in item`),
which is the safe direction; but **equality on both sides** is still the standard, to avoid "the other side slowly drifting again".

## 3. Self-referencing assertions in `test_game_data_tools.py`

Change the expected values of `test_get_relevant_game_data_uses_scene_fields_for_combat/_for_shop/_for_event`
into explicit literals (consistent with the fixed table), and add a comment pointing to the cross-language test.
This way "changing the table" must change two places at once, and the cross-language test and the C#-side test form a triangular constraint.

## 4. Make `mcp_server/data/eng/` tell the truth

- Rewrite `README.md` into three points: (1) these files are a **packaged snapshot of one game version**;
  (2) the only runtime data source is the mod's `GET /data/{collection}` (`client.py:96-97`);
  (3) their schema is **inconsistent** with the export schema (for example `potions.json` has no `usage`/`target_type`,
  `relics.json` has no `is_melted`), and must not be used as a field basis.
- New test `test_packaged_game_data.py`: iterate `data/eng/*.json`,
  assert (1) each file parses as a list or dict; (2) when it has entries each has a non-empty `id`;
  (3) `id` is unique within the same file. The failure message gives the file name and the broken entry index.

## Risks

- Cross-language parsing is text-level: if the C# table format differs from expected, go by the real shape read at implementation time,
  and paste the parsed (scene, collection) count and a sample in the evidence.
