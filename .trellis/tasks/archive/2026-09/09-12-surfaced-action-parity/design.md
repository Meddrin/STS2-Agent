# Design

## ① `selection.can_confirm`

Current state:

- payload (`:4439-4441`): `can_confirm = hasCombatHandSelection ? combatHandSelection.CanConfirm : hasCardGridSelection && cardGridSelection.CanConfirm`
- action (`CanConfirmSelection`:942-954): additionally requires `(RequiresConfirmation || MinSelect < MaxSelect)`.

The fix (pick the one with the smaller breakage surface at implementation time, and write it into the evidence):

- **A**: change the payload's `can_confirm` to reuse `CanConfirmSelection(currentScreen)`;
- **B**: remove the extra "is it worth confirming" condition in `CanConfirmSelection`, so both sides are simply `CanConfirm`.

Boundary of judgment: if A, a single-select grid that needs no confirmation changes from `can_confirm=true` to `false`: a semantic convergence
("does a confirm action exist now"); if B, the model may be guided to click a screen that needs no confirmation. First read the existing assertions in
`DeckSelectionAvailabilityTests` / `RewardFlowContractTests` / `CombatDiagnosticsContractTests`
before deciding. Whichever is chosen, the payload and the action must come from one and the same decision.

## ② `modal.can_confirm`

Payload `:4946` `can_confirm = confirmButton != null` -> change to `CanConfirmModal(currentScreen)`.
First read `FtueModalPolicyTests`: `ExposeConfirm = hasUsableConfirmButton || IsFtueType` means an FTUE
popup lets `confirm_modal` through even when there is **no** usable confirm button (`confirm_modal` goes through paths like `CloseFtueDirectly`).
After the payload aligns with it, the three agree: "FTUE with no button + can_confirm=true + confirm_modal available".

## ③ descriptor

`:397` `requires_index = true` -> `false` (`resolve_rewards` does not consume an index).

## (4) Reward button filtering

Add `IsEnabled` to `CanChooseRewardCard` / `CanSkipRewardCards` (consistent with `CanClaimReward`),
and `IsVisibleInTree` if necessary. First confirm whether the node types of `GetCardRewardOptions` / `GetCardRewardAlternativeButtons`
have these two members; if not, do not add them, and write the conclusion into the evidence.

## (5) Crystal sphere

`GetCrystalSphereMinigame` already requires `currentScreen is NCrystalSphereScreen`, so the reachability of "the screen exists but is not visible"
is doubtful. After evaluating, give a conclusion: if reachability does not hold, **do not change** (write it into the evidence); if there is a counterexample, add a visibility filter.

## Tests (`SurfacedActionParityTests.cs`, source contracts)

- the payload `can_confirm` shares a source with `CanConfirmSelection(` (assert in the shape chosen under (1));
- the `can_confirm` of the modal payload references `CanConfirmModal(`;
- the `resolve_rewards` descriptor fragment contains `requires_index = false`;
- the bodies of `CanChooseRewardCard`/`CanSkipRewardCards` contain `IsEnabled`;
- every assertion is falsifiable (revert the matching change -> red -> restore).

## Pitfalls

- `GameStateService.cs` is not in the test project's compile set: after changing it you must run
  `dotnet build STS2AIAgent/STS2AIAgent.csproj -c Release --no-incremental` for type checking.
- **Parallel editing**: the worker of `09-12-reward-screen-truth` is changing `ResolveNonModalScreen` of the same file at the same time
  (6802-6866). This task changes only 390-400 / 900-960 / 1563-1567 / 1595-1610 / 4420-4445 / 4930-4950,
  and must never touch 6802-6866; use apply_patch with exact context, and do not overwrite whole files.
- Existing tests (`CompactViewFidelityTests`, `FtueModalPolicyTests`, `RewardFlowContractTests`,
  `DeckSelectionAvailabilityTests`) may cover these branches; read before changing.

