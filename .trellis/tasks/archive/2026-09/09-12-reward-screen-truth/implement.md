# Execution checklist

1. Read `GameStateService.cs:6800-6870` (the whole of `ResolveNonModalScreen`),
   `STS2AIAgent.Tests/UnlockScreenContractTests.cs` (the existing order contract pattern),
   the `api-facts` extraction logic of `scripts/check_verification_gates.py` (`CODE_SCREEN_EARLY_RETURN`).
2. Insert the new early-return branch (see the design).
3. Create `RewardScreenContractTests.cs` with 3 assertions, and **do not** change TestRunner.cs / csproj (the main agent registers them).
4. Falsifiability self-check: temporarily move the new branch **after** the generic grid branch -> your new assertions 1/2 must turn red -> restore.
5. Verify:
   ```powershell
   dotnet build STS2AIAgent/STS2AIAgent.csproj -c Release --no-incremental
   dotnet run --project STS2AIAgent.Tests/STS2AIAgent.Tests.csproj -c Release
   python scripts/check_verification_gates.py
   ```
6. Do not commit or push; report the changed files, the red/green evidence, and the "live-only verification" list.
