# Execution checklist

1. Read `GameActionService.cs` :520-620 (open_character_select / open_timeline),
   :700-800 (confirm_timeline_overlay), :1600-1650 (crystal_set_tool),
   :2028-2130 (select_deck_card), :4924-5020 (run_console_command),
   :5600-5900 (the three wait helpers); read the implementations of `TrySetCrystalSphereTool` and
   `GetCrystalSphereMinigame` in `GameStateService`, and confirm what exactly the "observable evidence" is.
2. First extend `MenuTransitionPolicy` (pure policy) and add tests, then change the wait helpers.
3. Tighten the four handlers in turn; leave a comment at each one saying "where the evidence comes from".
4. Add source contract assertions and self-check falsifiability.
5. Verify:
   ```powershell
   dotnet run --project STS2AIAgent.Tests/STS2AIAgent.Tests.csproj -c Release
   dotnet build STS2AIAgent/STS2AIAgent.csproj -c Release --no-incremental
   python scripts/check_verification_gates.py
   ```
6. Do not commit or push; report the final judgment basis of each handler (observation point + return on failure). For parts that cannot be proven offline
   (reachability of real overlays) explicitly mark "needs live verification", and do not write them as verified.
