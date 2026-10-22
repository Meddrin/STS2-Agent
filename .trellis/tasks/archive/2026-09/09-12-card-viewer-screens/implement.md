# Execution checklist

1. Read these (not just summaries):
   - `STS2AIAgent/Game/GameStateService.cs`: `ResolveNonModalScreen` (about 6815-6845),
     `CanCloseCardsView` (942-946), `CanCloseMainMenuSubmenu` (1433-1447),
     the comment of `GetDeckSelectionOptions` (1725-1731), `GetCardsViewBackButton` (2119-2133),
     `GetMainMenuSubmenuStack` (6563-6577);
   - `STS2AIAgent/Game/GameActionService.cs`: `ExecuteCloseMainMenuSubmenuAsync` (600-660),
     `ExecuteCloseCardsViewAsync` (2221-2265), `WaitForCardsViewCloseAsync` (2305-2325),
     `IsCardsViewClosed` (2325-2334);
   - `STS2AIAgent.Tests/RewardScreenContractTests.cs` (the existing pattern for early-return contracts),
     `STS2AIAgent.Tests/DeckSelectionAvailabilityTests.cs` (the "available means executable" pattern),
     `.trellis/tasks/09-12-card-viewer-screens/research/viewer-screens.md`;
   - `docs/api.md:80-100` and `:1645-1660`; `skills/sts2-mcp-player/SKILL.md:99-120`.
2. Change the C# per the design (two early returns, stack lookup moved up, card viewer set collapsed).
3. Update `docs/api.md` and `SKILL.md`.
4. Create `STS2AIAgent.Tests/CardViewerScreenContractTests.cs` (3 groups of assertions + existing regressions),
   and **append** the registration lines at the **end** of `AllTests` in `TestRunner.cs`.
5. Falsifiability self-check (each one needs "break -> red -> restore -> green"):
   - move the two early-return branches after E4 -> the order assertion turns red;
   - change `IsClosableCardViewer` back to recognize only `NCardsViewScreen` -> the same-source assertion turns red;
   - change `GetSubmenuStack` back to `is NMainMenuSubmenuStack` -> the stack assertion turns red.
6. Verify:
   ```powershell
   dotnet build STS2AIAgent/STS2AIAgent.csproj -c Release --no-incremental
   dotnet run --project STS2AIAgent.Tests/STS2AIAgent.Tests.csproj -c Release
   python scripts/check_verification_gates.py
   cd mcp_server; uv run --locked python -m unittest discover -s tests
   ```
7. Do not commit or push; report the changed files, the red/green evidence, and the list of "live-only verification".

