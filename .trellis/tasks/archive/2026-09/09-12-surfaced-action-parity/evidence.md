# Evidence

## Deliverables

| File | Change |
|---|---|
| `STS2AIAgent/Game/GameStateService.cs` | 4 places: `:397` `resolve_rewards` -> `requires_index = false`; `CanSkipRewardCards` -> `GetCardRewardAlternativeButtons(...).Any(button => button.IsEnabled)` (and a comment on `CanChooseRewardCard` explaining why a filter is **not added**); `:4448` `selection.can_confirm` -> `CanConfirmSelection(currentScreen)`; `:4955` `modal.can_confirm` -> `CanConfirmModal(currentScreen)` |
| `STS2AIAgent.Tests/SurfacedActionParityTests.cs` | New, 6 source contract tests |
| `STS2AIAgent.Tests/TestRunner.cs` | Appended 6 `SurfacedAction.*` registration lines |

## Conclusions item by item

| # | Conclusion | Basis |
|---|---|---|
| (1) | The payload now calls `CanConfirmSelection(currentScreen)` directly (option A of the design) | A shared source eliminates the window of "the payload says it can confirm, the action is not exposed"; `docs/api.md:1428` and `screen-playbooks.md:55` already describe the `CanConfirmSelection` rule, so the old code deviated from the docs |
| (2) | The payload changed to `CanConfirmModal(currentScreen)` | An FTUE popup without its own button can still be confirmed (`FtueModalPolicy.ExposeConfirm`), and the old `confirmButton != null` under-reported |
| (3) | `requires_index = false` | The executor does not consume an index; the description "may carry option_index" at `docs/api.md:1028` still holds |
| (4) | `CanSkipRewardCards` gains `IsEnabled`; `CanChooseRewardCard` does **not** | See the next section |
| (5) | **No change** | See the next section |

## Hard evidence for (4) / (5) (the prd requires it be written into the evidence)

**(4) `choose_reward_card` cannot take a filter, and should not**:

1. `MegaCrit.Sts2.Core.Nodes.Cards.Holders.NCardHolder : Control`, and `Control` has no `IsEnabled`;
   the clickable state of reward card choice is the private `_isClickable`, flipped by `NCardRewardSelectionScreen.DisableCardsForShortTimeAfterOpening()`.
2. Measured compile: changing to `.Any(holder => holder.IsEnabled)` ->
   `GameStateService.cs(913,73): error CS1061: "NCardHolder" does not contain a definition for "IsEnabled"`.
3. The executor `ExecuteChooseRewardCardAsync` picks the card with `selected.EmitSignal(NCardHolder.SignalName.Pressed, selected)`,
   **bypassing** `_isClickable` => `Count > 0` is exactly the set the executor can honor, and adding any enabled/visible filter would only create false negatives.

Control group (skip can take it): `NCardRewardAlternativeButton : NButton : NClickableControl`, `IsEnabled` is a public property,
the same source as `CanClaimReward` reading `NRewardButton.IsEnabled`; `GetCardRewardAlternativeButtons` already filters `IsVisibleInTree()`.

**(5) The crystal sphere gets no visibility filter (unreachable)**:

- `ActiveScreenContext.GetCurrentScreen()` yields `NCrystalSphereScreen` only at `NOverlayStack.Peek()`;
  on `Push` of that screen `Visible` stays true (`AfterOverlayOpened` only tweens `modulate:a`, `AfterOverlayHidden` only `_proceedButton.Disable()`).
- When the map/capstone covers the overlay stack, `GetCurrentScreen` returns earlier with `NMapScreen` / `CanstoneContainer`.
- The first line of `GetCrystalSphereMinigame` is already `is not NCrystalSphereScreen -> null`, so the screen type check is the whole gate.
- `SurfacedAction.CrystalSphereScreenGuard` pins this shape: if someone later really adds an `IsVisibleInTree` filter it goes red (verified in the falsifiability self-check).

## Acceptance

| Criterion | Evidence |
|---|---|
| (1)(2) same source | Assert `can_confirm=CanConfirmSelection(currentScreen)` and `can_confirm=CanConfirmModal(currentScreen)` |
| (3) | Assert `name="resolve_rewards",requires_target=false,requires_index=false` |
| (4) falsifiable | `SurfacedAction.SkipRewardCardsEnabledFilter`; `SurfacedAction.ChooseRewardCardCollection` pins the "no filter" status quo |
| Full gates | Reverting the 6 shapes one by one -> `PASS=341 FAIL=6` (all 6 matching assertions red) -> after restoring **347 PASS / 0 FAIL**; build 0 warnings; all 6 gates green; preflight exit 0 |

## Wrap-up correction (added by the main agent, see the commit in the next section)

After `CanSkipRewardCards` was tightened, the executor `ExecuteSkipRewardCardsAsync` still wrote `alternatives.First()` =>
a new window of "exposure says it can, execution may click the first button that is disabled". The executor was changed to
`alternatives.First(button => button.IsEnabled)` (that the set is non-empty is guaranteed by the 409 guard),
and the assertion `SurfacedAction.SkipTargetsEnabledAlternative` was added to pin that both ends share the same criterion.

## Live-only verification

- Under an FTUE popup with `modal.can_confirm=true`, `confirm_modal` can really finish it on a real machine.
- For a single-select grid with `RequiresConfirmation=false`, after `selection.can_confirm` becomes false the model no longer sends an extra `confirm_selection`.
- Whether the `IsEnabled` filter of `skip_reward_cards` is always equivalent to the old behavior on a real reward screen.
- The live behavior of `resolve_rewards` without an index.
- The unreachability of "the crystal sphere is invisible but still resolved" (the conclusion is based on decompiled paths, not runtime observation).

