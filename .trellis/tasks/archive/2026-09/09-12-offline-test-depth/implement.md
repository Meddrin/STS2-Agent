# Execution checklist

1. Read `STS2AIAgent/Server/ApiException.cs`, `Server/JsonHelper.cs`,
   `Server/HttpServer.cs:30-60 / :160-221`, `STS2AIAgent.Tests/AgentSourceFixture.cs`.
2. Add two `<Compile Include>` entries, write two test files, and register them in `AllTests` of `TestRunner.cs`.
3. Write `HttpServerPortPolicyTests.cs` (source contract).
4. Add screen names to `docs/api.md` (verify the missing ones first).
5. Verify:
   ```powershell
   dotnet run --project STS2AIAgent.Tests/STS2AIAgent.Tests.csproj -c Release
   python scripts/check_verification_gates.py
   python scripts/check_release_package.py --source-root .
   powershell -ExecutionPolicy Bypass -File scripts/preflight-release.ps1
   ```
6. Counter-check self-test: temporarily break the behavior under test, confirm the new tests turn red, then restore.
7. Do not commit or push; report the new/modified files and the test counts.
