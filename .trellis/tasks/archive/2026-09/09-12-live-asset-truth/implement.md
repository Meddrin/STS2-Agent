# Execution checklist

1. Read: `scripts/test-main-menu-active-run.ps1` (the whole file), `scripts/run_sts2_validation.py`
   (`settle_game_over`/`settle_main_menu`/`collect_rewards_if_needed`/`suite_new_run_lifecycle`/
   `suite_main_menu_active_run`), `scripts/test-multiplayer-lobby-flow.ps1:630-745`,
   `STS2AIAgent.Tests/TimelineIndexContractTests.cs`, `STS2AIAgent/Game/GameStateService.cs:6795-6866`
   (the full set of screens) and `:4768-4800` (the timeline payload).
2. Change four places per the design.
3. Falsifiability self-check:
   - `python -c "import py_compile; py_compile.compile('scripts/run_sts2_validation.py', doraise=True)"`;
   - `python scripts/check_verification_gates.py --only script-encoding`;
   - PowerShell syntax: `[System.Management.Automation.Language.Parser]::ParseFile('<script>', [ref]$null, [ref]$errors)` reports 0 errors;
   - verify the timeline index logic with a fake payload (`slots=[{index:0,is_actionable:false},{index:1,is_actionable:true}]`) and confirm it takes 1.
4. Verify:
   ```powershell
   python scripts/check_verification_gates.py
   ```
5. Do not commit or push; report the changed files, the syntax/encoding evidence, and the list of "live-only verification".

Note: this round needs no new C# tests, and do not change `STS2AIAgent.Tests/TestRunner.cs`.

