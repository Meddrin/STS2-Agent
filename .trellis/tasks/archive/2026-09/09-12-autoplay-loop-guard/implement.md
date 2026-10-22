# Execution checklist

1. Read `Agent/AutoPlayRecovery.cs` (the whole file, it is short), `Agent/AgentLoop.cs:600-680`,
   the `AccountTurn`/`ApplyPlayResult`/`ObservePlayCompletionAsync` section of `Agent/AgentRuntime.cs`,
   `Agent/SessionBudgetGuard.cs`, `Agent/StopKindPolicy.cs`, and where `AgentTurnResult` is defined.
2. First write `NoProgressPolicy.cs` + `NoProgressPolicyTests.cs` (behavioral tests).
3. Then change `AgentTurnResult` + the pending return path of `AgentLoop`, then change the `AutoPlayRecovery` state machine.
4. Write `AutoPlayRecoveryTests.cs` (if a test class of the same name already exists, append methods rather than creating a duplicate class).
5. Verify:
   ```powershell
   dotnet build STS2AIAgent/STS2AIAgent.csproj -c Release --no-incremental
   dotnet run --project STS2AIAgent.Tests/STS2AIAgent.Tests.csproj -c Release
   python scripts/check_verification_gates.py
   ```
6. Falsifiability self-check: remove the threshold check -> the spinning test turns red; count the unconfirmed branch as a failure -> the false-halt test turns red; restore.
7. Do not commit or push; report the changed files, the state machine branch table, the evidence for each acceptance criterion, and explicitly mark
   "whether a long settlement causing pending really occurs in the real game" as a live item.
