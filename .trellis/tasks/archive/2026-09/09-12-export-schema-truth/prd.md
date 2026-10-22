# Align the GameDataFilter scene field sets with the real export

## Background

`SceneFieldSets` at `STS2AIAgent/Agent/GameDataFilter.cs:12-30` decides the metadata fields received by the in-game Agent loop
(`AgentLoop.cs:529`) and the native MCP (`NativeMcpServer.cs:474-479`).
`ProjectFields` (:125-137) **silently skips** nonexistent fields, so a misspelled field name raises no error and only leaves the
model unable to see the information:

| scene/collection | Listed in the table but not in the export at all | In the export but not selected (information missing) |
|---|---|---|
| combat/monsters (:17) | **`hp` / `damage` / `block`** | `min_hp` / `max_hp` / `damage_values` / `block_values` |
| shop/potions (:24) | **`target`** (the export has `target_type`) | `target_type` / `usage` / `pool` |
| combat/cards (:16) | - | `is_x_star_cost` |
| combat/powers (:18) | - | `allow_negative` |
| shop/relics (:23) | - | `pool` / `is_melted` |
| shop/cards (:22) | - | `target` / `is_x_cost` / `star_cost` / `is_x_star_cost` |
| event/events (:28) | - | `type` / `act` |

All three monsters items being wrong means **not one of the most critical combat HP/damage fields was passed down**.
Existing tests do not catch it: `AgentLoopTests.cs:8-32` asserts only the `name` of combat/cards; on the Python side
`test_game_data_tools.py` asserts against a self-referencing field table (change the table and the assertion follows).

## Goals

1. Fix the C# scene field sets so that all of them are real export fields.
2. Turn the "export field manifest" into a **source of truth that tests can reference**, and add a two-way regression test: "fields in the table must exist in the export manifest,
   and manifest fields must appear in the export code" (the old code must turn red).
3. Also add the fields decisions actually need (see the right column of the table above), introducing no nonexistent field names.

## Acceptance criteria

- [x] `SceneFieldSets` no longer contains nonexistent names such as `hp`/`damage`/`block`/`target`.
- [x] Add the export manifest constant (compilable offline into the test project), listing the real fields per collection.
- [x] Test one: every (scene, collection, field) of `SceneFieldSets` exists in the export manifest; it must be red before the fix.
- [x] Test two: every field of the export manifest appears as a literal inside the matching
      `ExportXxx()` method body of `GameDataExportService.cs`: reverse drift detection; if some field is really not a literal, degrading to a full-text scan is allowed with the reason recorded.
- [x] Test three: `GameDataFilter.KnownCollections` agrees with the collection key set of the export manifest.
- [x] Existing C# / MCP tests all green; `check_verification_gates.py` and preflight pass.

## Out of scope

- Do not change the Python-side `server.py:_SCENE_FIELD_SETS` (after field-by-field checking there are **no** nonexistent field names, so no change is needed;
  cross-language consistency is recorded in the research file as a known gap for this round).
- Do not change the response structure of `/data/{collection}`, and do not add a schema endpoint.
- Do not change the silent-skip semantics of `ProjectFields` (it is just no longer fed wrong names).
