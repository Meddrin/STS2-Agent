# Make the scene field table a cross-language consistent fact

## Background

The same knowledge of "scene -> which fields to query" was written once on each of two consumption chains, and **has already drifted**:

| Surface | Location | Current state |
|---|---|---|
| C# (in-game Agent + native MCP) | `SceneFieldSets` at `STS2AIAgent/Agent/GameDataFilter.cs:12-33` | Already aligned with the export schema (fixed last round), with two-way drift tests |
| Python (external MCP, default path) | `_SCENE_FIELD_SETS` at `mcp_server/src/sts2_mcp/server.py:150` | Measured to disagree with the C# table: `combat.powers` 5 items vs C# 6 (missing `allow_negative`); `shop.cards` 6 items vs C# 11; `shop.relics` lacks `is_melted`; the `combat/potions` newly added in C# is unknown to Python |

More troublesome is how the tests on the two sides prove themselves:

- The C# side (`GameDataExportSchemaTests` at `AgentLoopTests.cs:895-970`) does a two-way comparison against `GameDataExportSchema`, **a real cross-check**;
- The Python side (`mcp_server/tests/test_game_data_tools.py:114-172`) takes its assertion values from
  `",".join(_SCENE_FIELD_SETS[...])`: **self-referencing**, change the table and it follows, catching no typo or omission.

So a cross-language inconsistency such as "the same action returns different fields" would never be discovered by anyone.

## Goals

1. Add a **cross-language alignment test** in `mcp_server/tests/`: parse the C# `GameDataFilter.SceneFieldSets`
   and the Python `_SCENE_FIELD_SETS`, make a two-way set assertion by (scene, collection), and give difference details.
2. Fix the current drift (using the C# table as the baseline, since it is the one aligned with the export schema verified this round).
3. Fix the self-referencing assertions of `mcp_server/tests/test_game_data_tools.py`: at least make the
   assertions on the "scene field table" no longer take values from the table under assertion itself, but reference an independent expected value (it may be a snapshot copy of the scene field table,
   but it must form a cross constraint with the C# alignment test).
4. Make `mcp_server/data/eng/README.md` tell the truth: it is a **packaged snapshot**, not a runtime data source,
   and its schema is not consistent with the export schema of `GET /data/{collection}` (lacking `usage`/`target_type`/`is_melted`, etc.).
   Also add a shallow test (each JSON parses, entries have a unique `id`) to make sure this data does not corrupt silently.

## Acceptance criteria

- [x] The new cross-language test **must be red before the fix** (record the real red output, listing the difference of each (scene, collection)).
- [x] After the fix the sets on both sides are equal; the test turns red in both directions, "adding a field only in C#" and "adding a field only in Python".
- [x] The scene field assertions of `test_game_data_tools.py` are no longer self-referencing (changed to independent expectations + cross-language cross constraint).
- [x] `mcp_server/data/eng/README.md` explicitly states the three things "snapshot, not a runtime data source, schema inconsistent with the export".
- [x] The new shallow data test turns red on "deleting the id field of some JSON" or "writing invalid JSON".
- [x] `uv run --locked python -m unittest discover -s tests` all green with the count increased;
      `check_verification_gates.py` and `check_release_package.py --source-root .` pass.

## Out of scope

- Do not change `GameDataFilter` (the C# side is already aligned and has its own two-way tests).
- Do not change the export endpoint or export fields.
- **Do not delete** `mcp_server/data/eng/` (1.2 MiB of tracked data; deletion is an irreversible decision left for the user;
  this task only stops it from misleading people).
