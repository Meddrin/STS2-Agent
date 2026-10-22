# Evidence

## Deliverables

| File | Change |
|---|---|
| `STS2AIAgent/Agent/DualLaunchOutcome.cs` (new) | Enum `Idle/InProgress/Succeeded/Rejected/Failed/Canceled` + pure policy `IsFailure`/`IsInProgress`; no Godot / MegaCrit references |
| `STS2AIAgent/Agent/AgentRuntime.cs` | `_dualLaunchOutcome` + the `DualLaunchOutcome` property; every return/catch branch of `LaunchDualInstanceCoreAsync` assigns explicitly (gate preemption failure -> `InProgress`, no longer reusing the last outcome) |
| `STS2AIAgent/Multiplayer/DualInstanceCoordinator.cs` | Added `HostLocalCoopResultAsync` returning `(bool Ok, string Message)`; the original `HostLocalCoopAsync` delegates, with the signature and message text unchanged verbatim |
| `STS2AIAgent/Game/GameActionService.cs` | `ExecuteInviteAiTeammateAsync` removes the Chinese substring check: `Succeeded->completed`, `InProgress->pending`, the rest (including `Idle`) -> 409 `invite_failed`, with `outcome` in details |
| `STS2AIAgent.Tests/DualLaunchOutcomeTests.cs` (new) + csproj include + TestRunner registration | 6 tests |

## Acceptance

| Criterion | Evidence |
|---|---|
| Enum/pure policy compile offline and are in the compile list | `DualLaunchOutcome.OfflineCompilable` PASS (asserts no `using Godot`/`using MegaCrit`); the csproj has the `<Compile Include>` added |
| Every branch writes the outcome | `DualLaunchOutcome.EveryBranchRecords` PASS: `_dualLaunchOutcome =` appears 6 times inside the method body, and the gate branch is `InProgress` |
| No more substring matching on `DualStatus` | `DualLaunchOutcome.LanguageIndependent` PASS; `rg 'Contains\("(失败|请先|找不到)"'` has 0 hits across the repo |
| Behavioral tests (truth table / language independence / cancellation exception classification) | `TruthTable` (6 values asserted one by one), `OnlySuccessCompletes`, `LanguageIndependent`, `EveryBranchRecords` all PASS |
| Coordinator is structured and the original signature/text unchanged | `DualLaunchOutcome.CoordinatorContract` PASS; the message string diff is unchanged verbatim |
| Falsifiable | Changing `IsFailure` to include `Succeeded` -> 2 tests turn red (`exit=1`), and 311 PASS after restoring |

## Gates (full rerun on 2026-09-12)

`dotnet run --project STS2AIAgent.Tests/STS2AIAgent.Tests.csproj -c Release` → **PASS=311 FAIL=0**
`dotnet build STS2AIAgent/STS2AIAgent.csproj -c Release --no-incremental` -> 0 warnings 0 errors
`python scripts/check_verification_gates.py` -> passed (api-doc 55 actions)
`python scripts/check_release_package.py --source-root .` -> passed
`powershell -File scripts/preflight-release.ps1` → `Static preflight complete.`
`uv run --locked python -m unittest discover -s tests` (mcp_server) -> 159 tests OK

## Live-only verification

- End-to-end HTTP semantics (a real two-instance success = `completed`; on an English client a failure must = 409 `invite_failed`; a concurrent invite = `pending`).
- Three paths await live coverage: success / English client failure / repeated invite.

## Notes

`HostLocalCoopAsync` now has no call sites in production code and is kept as a compatibility entry (its delegation is locked by the `CoordinatorContract` test).
