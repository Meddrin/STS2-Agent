# Design

## Mechanism (spike-verified, not speculative)

Add to the csproj:

```xml
<ItemGroup>
  <Reference Include="Microsoft.CodeAnalysis">
    <HintPath>$(MSBuildSDKsPath)/../Roslyn/bincore/Microsoft.CodeAnalysis.dll</HintPath>
  </Reference>
  <Reference Include="Microsoft.CodeAnalysis.CSharp">
    <HintPath>$(MSBuildSDKsPath)/../Roslyn/bincore/Microsoft.CodeAnalysis.CSharp.dll</HintPath>
  </Reference>
</ItemGroup>
```

Spike evidence: temporarily adding the references above + a `[ModuleInitializer]` that calls `CSharpSyntaxTree.ParseText`,
`dotnet run` printed `ROSLYN_SPIKE_OK` and 335 tests passed as usual; the change was rolled back.
Zero NuGet: the DLLs ship with the SDK, and the `setup-dotnet@v4` in CI installs the same SDK.

## Test shape (`SourceCoverageTests.cs`)

1. `EveryModSourceParsesWithoutSyntaxErrors`: iterate `AgentSourceFixture.SourceFiles()`,
   `CSharpSyntaxTree.ParseText(File.ReadAllText(path))`, collect error diagnostics and assert they are empty
   (the failure message carries the first `file:line`).
2. `UncompiledSourcesMatchTheDeclaredWhitelist`: parse the `<Compile Include>` entries of
   `STS2AIAgent.Tests/STS2AIAgent.Tests.csproj` (expanding wildcards),
   take the set difference with `SourceFiles()`, and assert it equals the explicit whitelist (16 relative paths, compared after normalizing separators).
   Adding an uncompiled file turns this assertion red: the author must explicitly decide to "register it for compilation" or "confirm it is covered by the syntax test".

## Pitfalls

- `AgentSourceFixture.SourceFiles()` currently does not filter `obj`/`bin`: locally there are extra generated files
  (`.NETCoreApp,Version=v9.0.AssemblyAttributes.cs`, `*.AssemblyInfo.cs`, etc.), clean on CI but red locally.
  Fix the fixture first; the whitelist comparison covers only tracked source files.
- Path normalization: the csproj uses `\` and starts with `..\`; the expanded relative paths must be unified to `/` before comparing.
- Do not change how existing source contract tests (`TimelineIndexContractTests` etc.) read files.
- New tests must be registered in `TestRunner.cs`.

