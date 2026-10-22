# Workshop Subscription Load Acceptance

> Historical snapshot: this file is the acceptance record for loading the Workshop subscription on 2026-09-09. It does not represent the current state; see [PRODUCT_PLAN_CURRENT.md](../PRODUCT_PLAN_CURRENT.md) for the current status.

Record date: 2026-09-09. The user enabled the mod and restarted the game; the main agent checked this process, the log, and the HTTP interface read-only.

- User screenshot 1: in the game's Mods list, STS2 AI Agent shows the Steam icon, confirming the source is the Workshop.
- User screenshot 2: the user confirmed the checkbox was ticked and the game restarted; the main menu shows the STS2 AI Agent floating window as expected.
- Running process: PID 40664, SlayTheSpire2.exe from the Steam install directory.
- This run's godot.log lines 19-20 found item 3796486050 and STS2AIAgent.json; lines 40-41 explicitly load STS2AIAgent.dll and STS2AIAgent.pck from `C:/Program Files (x86)/Steam/steamapps/workshop/content/2868840/3796486050/`; line 46 listens on 8080. The search found no "already loaded" duplicate-load error.
- `GET http://127.0.0.1:8080/health`: ok=true, mod_version=0.10.5, game_version=v0.111.0, status=ready, process_id=40664, instance_role=human, play_running=false, session_requests=0.
- `/state`: MAIN_MENU; `/actions/available`: ok=true, MAIN_MENU, with main-menu actions such as continue_run, abandon_run, and open_timeline visible. Read only; no action was executed.

Conclusion: the P2.5 smoke acceptance for Workshop discovery, load after enabling, window display, and running version passed. No second restart round was run, and this is not extended to model connection, a full dual-instance run, or upgrade/rollback acceptance.

Screenshot source (provided by the user in this session; the temporary paths are not permanent repository attachments):

- `C:/Users/chart/AppData/Local/Temp/codex-clipboard-53f9573e-b4f3-41c0-af4a-4bce62e72ee0.png`
- `C:/Users/chart/AppData/Local/Temp/codex-clipboard-3e16d80c-8bfb-4b82-947a-87369e77f9f9.png`

Log source: `C:/Users/chart/AppData/Roaming/SlayTheSpire2/logs/godot.log`, which later launches may overwrite; the key results above are excerpted. The player's save, model configuration, and Workshop visibility were not modified.
