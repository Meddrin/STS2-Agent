# Evidence: the 16 uncompiled source files

## Coverage comparison (git ls-files vs csproj Compile Include)

- tracked `STS2AIAgent/**/*.cs`: 79 (including `ModEntry.cs`);
- csproj `<Compile Include>`: 49 entries (including 3 wildcards), 63 files after expansion;
- set difference 16, 18007 lines in total:

```
  6850  STS2AIAgent/Game/GameStateService.cs
  5786  STS2AIAgent/Game/GameActionService.cs
  1482  STS2AIAgent/Ui/AgentOverlayHost.cs
  1108  STS2AIAgent/Agent/AgentRuntime.cs
   474  STS2AIAgent/Game/GameDataExportService.cs
   417  STS2AIAgent/Server/Router.cs
   366  STS2AIAgent/Server/GameEventService.cs
   317  STS2AIAgent/Multiplayer/LocalDualInstanceLauncher.cs
   240  STS2AIAgent/Agent/GameBridge.cs
   227  STS2AIAgent/Multiplayer/DualInstanceCoordinator.cs
   190  STS2AIAgent/Server/HttpServer.cs
   160  STS2AIAgent/Ui/UiFactory.cs
   145  STS2AIAgent/Game/GameThread.cs
   125  STS2AIAgent/Localization/LocSource.cs
    73  STS2AIAgent/ModEntry.cs
    47  STS2AIAgent/Vision/ScreenshotService.cs
```

## Root cause

The files above `using` Godot / `MegaCrit.Sts2.*` (the game DLLs), and the offline compiler cannot get the references, so they never entered the
compile set of the test project. The existing source contract tests (`AgentSourceFixture.Read`) can only assert "the text shape inside the file"
and cannot assert "whether the file itself is parseable C#".

## Partial mitigation already present (does not mean coverage)

Some tests use string assertions to cover **parts** of the uncompiled files:
`TimelineIndexContractTests` (GameActionService.ResolveTimelineSlot),
`UnlockScreenContractTests` / `RewardFlowContractTests` (branches of GameStateService),
`McpPlayerSkillTests` (SKILL.md matches the embedded contract), but all depend on the file being correct "exactly at the spot where the author wrote the assertion",
and syntax errors in the other 99% of the file are caught by no one.

## Spike record (run by the main agent before this task started)

1. Add the Roslyn Reference to the csproj (the snippet in the design above);
2. `_RoslynSpike.cs`: a `[ModuleInitializer]` calls `CSharpSyntaxTree.ParseText("class A { void B() { } }")`
   and asserts 0 errors;
3. `dotnet run --project STS2AIAgent.Tests -c Release` -> prints `ROSLYN_SPIKE_OK`, all 335 tests PASS;
4. Both changes rolled back (`git checkout -- STS2AIAgent.Tests/STS2AIAgent.Tests.csproj` + deleting the spike file).

## To confirm (CI side only)

The above was verified only on local Windows + SDK 9.0.311; CI's `windows-latest` + `dotnet-version: 9.0.x`
sets up the same kind of SDK (`Roslyn/bincore` ships with the SDK), but this repo had no push this round, so CI did not run.

