# Evidence

## Deliverables

| File | Change |
|---|---|
| `STS2AIAgent/Agent/GameDataExportSchema.cs` (new) | The real export field lists of 7 collections (copied verbatim from each `ExportXxx()` in `GameDataExportService.cs`) + `TryGetFields` |
| `STS2AIAgent/Agent/GameDataFilter.cs` | `SceneFieldSets` corrected: monsters changes to `min_hp/max_hp/moves/damage_values/block_values`; the `target` of potions -> `pool/usage/target_type`; added `combat/potions`; added `is_x_star_cost`/`allow_negative`/`pool`/`is_melted`/`target`/`type`/`act`; added an internal read-only view for tests |
| `STS2AIAgent.Tests/AgentLoopTests.cs` | Appended 3 tests (+78 lines, existing tests unchanged) |

## Acceptance

| Criterion | Evidence |
|---|---|
| Red before the fix | `FAIL SceneFieldsExistInTheExportSchema`: `combat/monsters: 'hp'/'damage'/'block' is not exported`, `shop/potions: 'target' is not exported` |
| Green after the fix | `PASS GameDataExportSchema.SceneFields / ExportCode / KnownCollections` |
| Two-way drift detection | Add `ghost_field` to the manifest -> `ExportCode` red; delete `characters` from the manifest -> `KnownCollections` red; `min_hp`->`hp` -> `SceneFields` red |
| collection key sets agree | `KnownCollectionsMatchTheExportSchema` PASS |

## Gates

C# **PASS=311 FAIL=0**; `dotnet build` 0 warnings 0 errors; MCP 159 OK; `check_verification_gates.py` passed; `check_release_package.py` passed; preflight passed.

## Live-only verification / known gaps

- Runtime export content is unverified (the manifest is a static source transcription; real `/data/{collection}` return values need the game online).
- Reverse detection covers only top-level fields; the internal field names of nested sub-objects such as `vars[]`/`moves[]` are not in the manifest.
- The Python `_SCENE_FIELD_SETS` and the C# manifest still have no shared source of truth (cross-language machine verification is a follow-up task).
- The actual benefit of the new `combat/potions` needs runtime confirmation.

## Notes

To record "red before the fix" a temporary harness outside the repo was used (`%TEMP%\sts2-schema-redcheck`, a one-off build artifact, not cleaned up successfully, unrelated to the repo).
