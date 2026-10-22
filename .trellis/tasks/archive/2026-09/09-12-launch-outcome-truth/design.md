# Design: structured launch outcome

## Data flow

```
GameActionService.ExecuteInviteAiTeammateAsync (GameActionService.cs:5085-5130)
  └─ await AgentRuntime.LaunchDualInstanceAsync        <- currently returns no value, so only the text can be read back
        └─ LaunchDualInstanceCoreAsync (AgentRuntime.cs:713-765)
              ├─ gate preemption failed (_dualLaunchGate.WaitAsync(0) fails) -> InProgress
              ├─ team message / team control pending   -> Rejected
              ├─ CoopLaunchPolicy.GetError != null    → Rejected
              ├─ await HostLocalCoopAsync(...)        -> Succeeded / Failed (needs structuring)
              ├─ OperationCanceledException           → Canceled
              └─ other exceptions                      -> Failed
```

## Landing points

### 1. New file `STS2AIAgent/Agent/DualLaunchOutcome.cs` (pure BCL, no Godot / no MegaCrit references)

```csharp
internal enum DualLaunchOutcome { Idle, InProgress, Succeeded, Rejected, Failed, Canceled }

internal static class DualLaunchOutcomePolicy
{
    public static bool IsFailure(DualLaunchOutcome outcome);    // Rejected / Failed / Canceled
    public static bool IsInProgress(DualLaunchOutcome outcome); // InProgress
}
```

Add it to the `<Compile Include>` of `STS2AIAgent.Tests/STS2AIAgent.Tests.csproj` (right after the similar pure policy files).

### 2. `STS2AIAgent/Agent/AgentRuntime.cs`

- Fields and properties: `private DualLaunchOutcome _dualLaunchOutcome = DualLaunchOutcome.Idle;` /
  `public DualLaunchOutcome DualLaunchOutcome => _dualLaunchOutcome;`
- **Every return / catch path** of `LaunchDualInstanceCoreAsync` (:713-765) writes the outcome:
  - gate cannot be acquired -> `InProgress` (a concurrent launch is in flight, neither success nor failure)
  - team message pending (:726-729), `CoopLaunchPolicy` rejection (:731-737) -> `Rejected`
  - coordinator returns (:744) -> `Ok ? Succeeded : Failed`
  - cancellation (:746-749) -> `Canceled`; other exceptions (:750-753) -> `Failed`
- The write sits at the same place as the matching `_dualStatus` assignment; writing only the text without the outcome is not allowed.

### 3. `STS2AIAgent/Multiplayer/DualInstanceCoordinator.cs`

- Add `public static async Task<(bool Ok, string Message)> HostLocalCoopResultAsync(CancellationToken)`,
  branch mapping: non-MAIN_MENU `(false, msg)`; 4-player lobby creation exception `(false, msg)`; `!launch.Ok`
  `(false, launch.Message)`; success `(true, success message)`; **cancellation is still rethrown** (not swallowed).
- The existing `HostLocalCoopAsync` is changed to delegate and return `Message`, with the signature and message text unchanged verbatim.

### 4. `STS2AIAgent/Game/GameActionService.cs:5088-5130`

- Keep the `CoopLaunchPolicy.GetError` precheck (409 `invalid_action`).
- Delete the substring check at :5103-5106 and read `AgentRuntime.Instance.DualLaunchOutcome` instead:

| Outcome | Response |
|---|---|
| `Succeeded` | 200, `status:"completed"`, `message = DualStatus` |
| `InProgress` | 200, `status:"pending"`, `stable:false`, `message = DualStatus` |
| `Rejected`/`Failed`/`Canceled` | 409 `invite_failed`, details carry `action`/`screen`/`outcome` |
| `Idle` (theoretically unreachable) | 409 `invite_failed` (does not falsely report success) |

The error code keeps the existing `invite_failed`; no new error code is added.

### 5. Tests `STS2AIAgent.Tests/DualLaunchOutcomeTests.cs` + `TestRunner.cs` registration

- Truth table: assert `IsFailure`/`IsInProgress` for each of the 6 enum values (must be falsifiable: changing one branch turns it red).
- Language independence: a source contract assertion (`AgentSourceFixture.Read("Game/GameActionService.cs")`)
  proves `Contains("失败"` is gone and a `DualLaunchOutcome` read exists; use reflection/the enum truth table to prove the decision does not consume display text.
- Add a "every branch writes the outcome" contract assertion (`_dualLaunchOutcome =` appears >= 5 times, and is `InProgress` inside the gate branch).

## Compatibility and rollback

- HTTP surface: the success-path response body fields are unchanged; the failure path changes from "false success" to 409, which is exactly the behavior this task fixes.
- Rollback: four files + one new test file, committed separately, can be reverted as a whole.
