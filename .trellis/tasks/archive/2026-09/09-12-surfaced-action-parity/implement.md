# Execution checklist

1. Read: `STS2AIAgent/Game/GameStateService.cs` lines 390-400, 895-960, 1091-1095, 1500-1570,
   4430-4445, 4930-4950; `STS2AIAgent/Game/FtueModalPolicy.cs`;
   `STS2AIAgent.Tests/FtueModalPolicyTests.cs`, `DeckSelectionAvailabilityTests.cs`,
   `RewardFlowContractTests.cs`.
2. Change per the design ((1)(2) must share a source; (3) change directly; (4) add filtering per node capability; (5) give a conclusion).
3. Write `SurfacedActionParityTests.cs` and register it at the end of `AllTests` in `TestRunner.cs`
   (**other workers are also appending in parallel**: if the patch fails, re-read and retry; never overwrite whole files or delete others' lines).
4. Falsifiability self-check: revert the changes one by one -> the matching assertion must go red -> restore.
5. Verify:
   ```powershell
   dotnet build STS2AIAgent/STS2AIAgent.csproj -c Release --no-incremental
   dotnet run --project STS2AIAgent.Tests/STS2AIAgent.Tests.csproj -c Release
   python scripts/check_verification_gates.py
   ```
6. Do not commit or push; report the changed files, the red/green evidence, and the "live-only verification" list.

**Parallel editing boundary**: `09-12-reward-screen-truth` is changing `ResolveNonModalScreen` (6802-6866) at the same time.
Do not touch that section; confine your changes to the regions you listed yourself.

