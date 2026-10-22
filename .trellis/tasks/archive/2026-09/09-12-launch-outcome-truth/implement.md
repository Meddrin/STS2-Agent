# Execution checklist

1. Read `STS2AIAgent/Agent/AgentRuntime.cs:40-60 / :220-230 / :713-765`,
   `STS2AIAgent/Multiplayer/DualInstanceCoordinator.cs` (the whole file, short),
   `STS2AIAgent/Multiplayer/LocalDualInstanceLauncher.cs:1-60 / :300-330`,
   `STS2AIAgent/Game/GameActionService.cs:5085-5130`.
2. Add `Agent/DualLaunchOutcome.cs` (enum + pure policy) and add it to the test project's compile list.
3. Change `DualInstanceCoordinator`: first add the structured method, with the original method delegating (text unchanged).
4. Change `AgentRuntime`: add the field/property and assign branch by branch.
5. Change `GameActionService.ExecuteInviteAiTeammateAsync`: delete the substring check and read the enum instead.
6. Write `DualLaunchOutcomeTests.cs` and register it in `AllTests` of `TestRunner.cs`.
7. Verify:
   ```powershell
   dotnet run --project STS2AIAgent.Tests/STS2AIAgent.Tests.csproj -c Release
   dotnet build STS2AIAgent/STS2AIAgent.csproj -c Release --no-incremental
   cd mcp_server; uv run --locked python -m unittest discover -s tests
   python scripts/check_verification_gates.py
   ```
8. Do not commit or push; hand the list of changed files and the test output to the main agent.
