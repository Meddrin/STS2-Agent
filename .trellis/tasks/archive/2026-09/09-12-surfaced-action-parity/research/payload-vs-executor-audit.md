# Evidence: differences between the payload and the execution criterion

## Probes' exhaustive conclusion

Action-level "exposed vs executable" already share a source: `BuildAvailableActionNames` (`GameStateService.cs:2528-2810`) and
`BuildAvailableActionsPayload` (`~330-560`) both call the same group of `CanX`; each branch of `ExecuteAsync` in `GameActionService`
rechecks the same `CanX`. The remaining differences are in the payload's **derived values**.

## file:line item by item

| # | Location | Code read |
|---|---|---|
| (1) | `GameStateService.cs:943-954` | `CanConfirmSelection`: the combat branch requires `RequiresConfirmation && CanConfirm`; the grid branch requires `CanConfirm && (RequiresConfirmation \|\| MinSelect < MaxSelect)` |
| ① | `GameStateService.cs:4439-4441` | `can_confirm = hasCombatHandSelection ? combatHandSelection.CanConfirm : hasCardGridSelection && cardGridSelection.CanConfirm` |
| ② | `GameStateService.cs:1563-1567` | `CanConfirmModal` → `FtueModalPolicy.ExposeConfirm(GetOpenModal()?.GetType().Name, hasButton)` |
| ② | `GameStateService.cs:4946` | `can_confirm = confirmButton != null` |
| ② | `FtueModalPolicy.cs` | `ExposeConfirm = hasUsableConfirmButton \|\| IsFtueType(modalTypeName)` |
| ③ | `GameStateService.cs:395-397` | `resolve_rewards` descriptor `requires_index = true` |
| (3) | `GameStateService.cs:402-404` | `collect_rewards_and_proceed` descriptor `requires_index = false` (control) |
| ④ | `GameStateService.cs:902-905` | `CanClaimReward` = `GetRewardButtons(...).Any(button => button.IsEnabled)` |
| ④ | `GameStateService.cs:907-910` | `CanChooseRewardCard` = `GetCardRewardOptions(currentScreen).Count > 0` |
| ④ | `GameStateService.cs:912-915` | `CanSkipRewardCards` = `GetCardRewardAlternativeButtons(currentScreen).Count > 0` |
| ⑤ | `GameStateService.cs:1091-1095` | `CanPlayCrystalSphere` = `GetCrystalSphereMinigame(...) is { IsFinished: false }` |
| (5) | `GameStateService.cs:1049-1053` | The first line of `GetCrystalSphereMinigame` is `currentScreen is not NCrystalSphereScreen -> null` |

## Impact

When the model decides by `selection.can_confirm`/`modal.can_confirm`, it may meet "the signal says yes, `act` says no"
(the reverse inconsistency of (1) (2) induces redundant or missing confirm actions). The `requires_index` of the descriptor (3) is used by the MCP
guided tool as the basis for parameter validation, and a false report makes the caller pass an extra, ignored index.

## Not confirmed live

The runtime behavior after the fix (whether the model makes fewer mistakes as a result) can only be observed live; offline can only pin "the two decisions share a source".

