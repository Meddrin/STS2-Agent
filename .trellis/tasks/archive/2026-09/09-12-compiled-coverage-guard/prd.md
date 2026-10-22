# Add a CI safety net for the 16 uncompiled mod source files

## Background

CI (`.github/workflows/validate.yml`) runs only `STS2AIAgent.Tests`, and that test project only
`<Compile Include>`s 63 of the 79 tracked `.cs` files (including the `Config/*`, `Llm/*`,
`Loc.Strings.*` wildcards). **16 files, 18007 lines, are completely invisible in CI**: even syntax errors would not be caught by any
automated check:

| File | Lines |
| --- | --- |
| Game/GameStateService.cs | 6850 |
| Game/GameActionService.cs | 5786 |
| Ui/AgentOverlayHost.cs | 1482 |
| Agent/AgentRuntime.cs | 1108 |
| Game/GameDataExportService.cs | 474 |
| Server/Router.cs | 417 |
| Server/GameEventService.cs | 366 |
| Multiplayer/LocalDualInstanceLauncher.cs | 317 |
| Agent/GameBridge.cs | 240 |
| Multiplayer/DualInstanceCoordinator.cs | 227 |
| Server/HttpServer.cs | 190 |
| Ui/UiFactory.cs | 160 |
| Game/GameThread.cs | 145 |
| Localization/LocSource.cs | 125 |
| ModEntry.cs | 73 |
| Vision/ScreenshotService.cs | 47 |

Root cause: they depend on the game DLLs (Godot/sts2) and cannot be compiled offline. But a **syntax-level** check can be done with zero dependencies: the .NET SDK ships with
Roslyn (`$(MSBuildSDKsPath)/../Roslyn/bincore/Microsoft.CodeAnalysis.CSharp.dll`),
and before this task started a spike confirmed that reference works (`ROSLYN_SPIKE_OK`, the 335 existing tests passing as usual, spike rolled back).

## Goals

CI parses the syntax of every tracked `.cs` at least once; a new file must not bypass both the "compile" and "parse" paths.

## Acceptance criteria

- [x] `STS2AIAgent.Tests.csproj` references the Roslyn shipped with the SDK (zero NuGet), and `dotnet run` works on a clean machine.
- [x] New syntax test: for every file of `AgentSourceFixture.SourceFiles()`,
      `CSharpSyntaxTree.ParseText(...)` asserts 0 errors; falsifiable (inject a syntax error into any uncompiled file -> red -> restore).
- [x] `AgentSourceFixture.SourceFiles()` filters `obj/` and `bin/` (otherwise extra generated files appear locally).
- [x] Coverage assertion: the set of files the csproj leaves uncompiled == the explicit whitelist (the 16 in the table above); the test goes red when an unregistered file is added.
- [x] Full C# tests / gates / preflight green.

## Out of scope

- Do not actually compile the 16 files into the test project (that needs the game DLLs).
- Do not do cross-file semantic/type checking (single-file Roslyn parsing does not resolve references).

