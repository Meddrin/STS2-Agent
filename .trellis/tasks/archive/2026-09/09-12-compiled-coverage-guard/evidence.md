# Evidence

## Deliverables

| File | Change |
|---|---|
| `STS2AIAgent.Tests/STS2AIAgent.Tests.csproj` | New ItemGroup: two `<Reference>` entries for `Microsoft.CodeAnalysis` / `Microsoft.CodeAnalysis.CSharp`, with HintPath pointing to `$(MSBuildSDKsPath)/../Roslyn/bincore/*.dll` (**zero NuGet**; diff `12 0`) |
| `STS2AIAgent.Tests/AgentSourceFixture.cs` | `SourceFiles()` filters `obj/` and `bin/` (new `IsBuildOutputPath`, judged by path segments relative to the mod root) |
| `STS2AIAgent.Tests/SourceCoverageTests.cs` | New. `EveryModSourceParsesWithoutSyntaxErrors` (`CSharpSyntaxTree.ParseText` on every source file, collecting error diagnostics and asserting empty, with `file:line` in the failure message) + `UncompiledSourcesMatchTheDeclaredWhitelist` (after expanding wildcards in the csproj compile set, take the set difference against tracked sources, which must equal the 16 explicit whitelist entries; asserts both unexpected and missing) |
| `STS2AIAgent.Tests/TestRunner.cs` | Appended 2 `SourceCoverage.*` registration lines |

Zero residual changes to production code (during falsifiability checks `ScreenshotService.cs` was temporarily modified and restored by its original SHA256).

## Acceptance

| Criterion | Evidence |
|---|---|
| Roslyn reference usable, zero NuGet | `Microsoft.CodeAnalysis.dll` and `Microsoft.CodeAnalysis.CSharp.dll` really exist under `$(MSBuildSDKsPath)/../Roslyn/bincore`; no packages added or lock changes; build 0 warnings. The main agent ran a spike before starting (`ROSLYN_SPIKE_OK` + 335 tests passing as usual, change rolled back) |
| Syntax test is falsifiable | Injecting invalid syntax at the end of `STS2AIAgent/Vision/ScreenshotService.cs` -> `FAIL SourceCoverage.ModSourcesParse ... ScreenshotService.cs:59 CS1026: ) expected`, exit 1; after restoring (SHA256 verified back to the original bytes) PASS |
| `obj/`/`bin/` filtering | See the fixture diff above; the tests stay green on this machine even with `obj/bin` present |
| Coverage assertion is falsifiable | (1) remove `ScreenshotService.cs` from the whitelist -> `Unexpected uncompiled source(s)` red; (2) add `STS2AIAgent/DoesNotExist.cs` to the whitelist -> `no longer uncompiled tracked sources` red; both green after restoring |
| Full gates | C# **347 PASS / 0 FAIL** (including `SourceCoverage.ModSourcesParse` and `SourceCoverage.UncompiledWhitelist`); all 6 gates green; preflight exit 0 |

**Replacement for "temporarily delete a `<Compile Include>` in the csproj" in implement.md**: that route is unusable (measured):
after deleting any entry `dotnet run` exits 1 right away with **no test output at all** (compilation fails first: every explicit include is referenced by at least 2 tests).
Instead the whitelist data was disturbed directly ((1)(2) above), hitting exactly the same two branches as "adding an unregistered file".

## Design boundaries (must know)

- Per the design the coverage assertion **compares only tracked sources** (`git ls-files -z -- STS2AIAgent`), falling back to the filesystem scan set when git is unavailable.
  This restriction is in effect: this machine has a gitignored `STS2AIAgent/_scratch/Class1.cs` (80 `.cs` on disk vs 79 tracked),
  and it stays green after filtering; otherwise it would falsely go red locally.
- Direct consequence: **a new file not yet `git add`ed does not immediately turn the coverage assertion red** (in a CI/PR checkout it is tracked -> it goes red).
  The syntax test uses the filesystem scan, so untracked files are parsed as well. This is the trade-off stated explicitly in the design.
- The syntax test is **single-file parsing**; it does not resolve references/types; cross-file semantic errors still rely on compilation (which needs the game DLLs).

## CI-only verification

1. Resolvability of `$(MSBuildSDKsPath)/../Roslyn/bincore/*.dll` on `windows-latest` + `setup-dotnet@v4 9.0.x` (locally proven only on SDK 9.0.311).
2. Availability of `git ls-files` in the CI container (`actions/checkout@v4` leaves `.git`, expected to be available; when unavailable it takes the fallback branch, equivalent on a clean checkout).
3. This round has no push, so GitHub Actions was not actually triggered.

