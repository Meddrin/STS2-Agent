# Execution checklist

1. Read `docs/api.md:1003-1061` (the contract block), `mcp_server/tests/test_native_tool_alignment.py:240-300 / :380-436`,
   `STS2AIAgent/Game/GameStateService.cs:1688-1726`, `STS2AIAgent/Game/GameActionService.cs:2064-2120`.
2. Change that one line of `docs/api.md` (keep the shape of action name + dash + description, with parameters and values wrapped in backticks).
3. Delete the exemption entry and its comment; rewrite the comment as a note that "both sides are now consistent and no exemption is needed".
4. Delete the generic fallback branch of `GetDeckSelectionOptions` (delete only that branch, keeping the three parts before it and the trailing
   `return Array.Empty<NCardHolder>();`).
5. Add source contract assertions in `STS2AIAgent.Tests/` (create a new test class `DeckSelectionAvailabilityTests` with two static methods;
   **do not change TestRunner.cs or the csproj yourself**, the main agent registers them):
   - `AvailabilityMatchesTheExecutableSet`: assert that the body of `GetDeckSelectionOptions` **does not contain**
     `currentScreen is Node rootNode`, and still contains the identifiers of the three-part criteria
     (`NCardGridSelectionScreen` / `NChooseACardSelectionScreen` / `TryGetCombatHandSelection`).
   - `ExecutorKeepsItsGuard`: assert that the body of `ExecuteSelectDeckCardAsync` still contains those three criteria and the 409 branch
     (preventing someone from deleting only the guard but not the fallback, causing yet another false success).
   After writing, self-check falsifiability (temporarily add the fallback back -> red -> restore).
6. Verify:
   ```powershell
   cd mcp_server; uv run --locked python -m unittest discover -s tests
   dotnet run --project STS2AIAgent.Tests/STS2AIAgent.Tests.csproj -c Release
   dotnet build STS2AIAgent/STS2AIAgent.csproj -c Release --no-incremental
   python scripts/check_verification_gates.py
   ```
7. Do not commit or push; report the changed files, the evidence for each acceptance criterion, and the "live-only verification" items.
