# Make every surfaced action/field agree with the execution criterion

## Background

The probes' exhaustive conclusion: action-level "exposed vs executable" already share a source (the same `CanX` drives both `available_actions`
and the descriptor list); the remaining contradictions are between **payload-derived values** and **action criteria**:

| # | Symptom | Evidence |
|---|---|---|
| (1) | `selection.can_confirm=true` but `confirm_selection` is not exposed | The payload `GameStateService.cs:4439-4441` uses `gridMetadata.CanConfirm`; the action `CanConfirmSelection` (`:942-954`) additionally requires `RequiresConfirmation \|\| MinSelect < MaxSelect` |
| (2) | The FTUE modal has `modal.can_confirm=false` but `confirm_modal` is available | The payload at `:4946` looks only at `confirmButton != null`; the action `CanConfirmModal` (`:1563-1567`) uses `FtueModalPolicy.ExposeConfirm` (FTUE lets it through even without a button) |
| (3) | The `resolve_rewards` descriptor falsely reports `requires_index=true` | `:395-397`; compare `collect_rewards_and_proceed` (`:402-404`), which is `false` |
| (4) | `skip_reward_cards` / `choose_reward_card` do not filter button availability | `CanChooseRewardCard` (`:907-910`) looks only at `GetCardRewardOptions().Count > 0`; `CanSkipRewardCards` (`:912-915`) looks only at the alternative button count; the convention in the same file (`CanClaimReward` `:902-905`) requires `IsEnabled` |
| (5) | The crystal sphere exposure looks only at `IsFinished` | `CanPlayCrystalSphere` (`:1091-1095`) `minigame is { IsFinished: false }`, without looking at screen visibility |

## Goals

Every "can do" signal the model reads can be accepted by `act`; every descriptor that "must carry an index" really needs an index.

## Acceptance criteria

- [x] The payload and the action decision of (1) (2) share a source (the same function/expression), with falsifiable source contract tests.
- [x] (3) corrected to `requires_index = false`.
- [x] (4) The filter condition is consistent with the convention in the same file (`IsEnabled`, and `IsVisibleInTree` if necessary); falsifiable.
- [x] (5) Give a conclusion (change or not + reason) and write it into the evidence.
- [x] Full C# tests / gates / preflight green.

## Out of scope

- Do not change `GameActionService.cs` (the executor already shares the source; this round only aligns the payload with it).
- Change the docs only if the existing `docs/api.md` description conflicts with the corrected criterion; do no unrelated rewriting.

