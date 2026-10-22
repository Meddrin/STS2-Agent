# Design: export manifest + two-way drift tests

## Real export fields (from `GameDataExportService.cs`)

| collection | Line | Fields |
|---|---|---|
| cards | :42-67 | id,name,description,description_raw,type,rarity,target,cost,is_x_cost,star_cost,is_x_star_cost,color,damage,block,keywords,tags,vars,upgrade |
| relics | :79-84 | id,name,description,rarity,pool,is_melted |
| potions | :95-101 | id,name,description,rarity,pool,usage,target_type |
| events | :112-117 | id,name,type,act,description,options |
| powers | :128-133 | id,name,description,type,stack_type,allow_negative |
| characters | :144-155 | id,name,description,starting_hp,starting_gold,max_energy,orb_slots,gender,color,starting_deck,starting_relics,starting_potions |
| monsters | :166-173 | id,name,type,min_hp,max_hp,moves,damage_values,block_values |

## Landing points

### 1. New file `STS2AIAgent/Agent/GameDataExportSchema.cs` (pure BCL)

```csharp
internal static class GameDataExportSchema
{
    public static readonly IReadOnlyDictionary<string, string[]> Collections; // copied verbatim from the table above
    public static bool TryGetFields(string collection, out IReadOnlyList<string> fields);
}
```

The comment states: **this is the field contract of `GET /data/{collection}`**; any export change must sync here, and test two catches reverse drift.
Add it to the `<Compile Include>` of `STS2AIAgent.Tests/STS2AIAgent.Tests.csproj`.

### 2. `STS2AIAgent/Agent/GameDataFilter.cs`

- `combat/cards` gains `is_x_star_cost`; `combat/monsters` changes to
  `id,name,type,min_hp,max_hp,moves,damage_values,block_values`; `combat/powers` gains `allow_negative`.
- `shop/cards` gains `target,is_x_cost,star_cost,is_x_star_cost`; `shop/relics` gains `pool,is_melted`;
  `shop/potions` changes to `id,name,description,rarity,pool,usage,target_type`.
- Add `combat/potions` (same field set as shop/potions): using potions in combat is a common path, and it currently falls back to the whole entry.
- `event/events` gains `type,act`.
- Expose a read-only view for tests: `internal static IReadOnlyDictionary<string, Dictionary<string, string[]>> SceneFieldSetView => SceneFieldSets;`
  (or change the field to `internal static readonly`; not public, so the API surface is not widened).

### 3. Tests (added inside `GameDataFilterTests` in `STS2AIAgent.Tests/AgentLoopTests.cs`)

- `SceneFieldsExistInExportSchema`: iterate `SceneFieldSetView`, and every field must be ∈ the corresponding collection of `GameDataExportSchema`;
  collect all violating fields and report once (easier to fix). **It must be red before the fix** (`hp`/`damage`/`block`/`target`).
- `ExportSchemaAppearsInExportCode`: for each collection, take
  a method body such as `AgentSourceFixture.MethodBody("Game/GameDataExportService.cs", "ExportMonsters")`,
  and assert that each field name appears as `\b<field>\s*=` (the C# anonymous object property notation). For non-literal fields,
  degrade to a full-text scan and write the reason in an implementation comment.
- `KnownCollectionsMatchExportSchema`: `GameDataFilter.KnownCollections` equals the manifest key set.
- Assertion self-check: change the `min_hp` of `monsters` back to `hp`, and the test must go red.

## Known gaps (write into the evidence)

The Python-side field table and the C# manifest have no shared source of truth; this round relies on each side's own tests plus manual cross-checking for consistency.
Machine verification would need a new endpoint (`/data/{collection}/schema`) or a repo-level shared file, and is a follow-up task.
