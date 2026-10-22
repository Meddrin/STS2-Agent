# Execution checklist

1. Read: `STS2AIAgent.Tests/STS2AIAgent.Tests.csproj`, `STS2AIAgent.Tests/AgentSourceFixture.cs`,
   `STS2AIAgent.Tests/UnlockScreenContractTests.cs` (test style), `.github/workflows/validate.yml`,
   the end of `STS2AIAgent.Tests/TestRunner.cs` (registration shape).
2. Per the design, change the csproj (add the Reference), fix the fixture (filter obj/bin), add `SourceCoverageTests.cs`.
3. Register: append your `yield return` at the end of `AllTests` in `TestRunner.cs`
   (**other workers are also appending at the same spot in parallel**: if the patch fails, re-read and retry; never overwrite whole files or delete others' lines).
4. Falsifiability self-check:
   - temporarily insert invalid syntax at the end of any uncompiled file (such as `Server/Router.cs`) -> the syntax test must go red -> restore;
   - temporarily delete a `<Compile Include>` in the csproj -> the coverage assertion must go red -> restore.
5. Verify:
   ```powershell
   dotnet run --project STS2AIAgent.Tests/STS2AIAgent.Tests.csproj -c Release
   ```
6. Do not commit or push; report the changed files, the red/green evidence, and the list of "can only be confirmed on CI".

This task may change `STS2AIAgent.Tests.csproj` and `AgentSourceFixture.cs` (these are the core of the task), but only those two
test project files; do not touch the production code under `STS2AIAgent/`.

