# STS2 AI Agent: Pre-Test Delivery Plan (Current at the Time)

> **Historical; does not represent the current state.** This document is the original pre-test plan of the old worktree around 2026-09-07. For the current state see [PRODUCT_PLAN_CURRENT.md](../PRODUCT_PLAN_CURRENT.md).

This document is the only pre-test execution entry point for this worktree. It describes how the fixed code is accepted in layers, and does not write static reading, historical records, or old output on a branch up as currently passing. For the old plan see [PRODUCT_ROADMAP.md](../PRODUCT_ROADMAP.md) (historical); for the delivery evidence record see [COOP_DELIVERY.md](../COOP_DELIVERY.md).

## Current Boundary and Baseline

- Worktree: C:\Users\chart\Documents\project\sp-player-first-experience
- Branch: codex/player-first-experience
- Exact baseline: HEAD 1be8e83 (tag v0.10.2) plus all current uncommitted changes in this worktree. The uncommitted changes include code, tests, scripts, and documents; for the full file list, go by git status --short in that worktree.
- Status this round: offline A1-A6, B, C/G0 passed. After the user's permission, staging was installed into Steam `mods/` (old files backed up in `build/live-test-backup-mods`). Live checks verified health/version/unverified-invite blocking/native MCP; the dual-instance lobby timed out entering combat. The real model F was not called. The everyday profile slot 1 did not start a run, and the test lobby used slot 3 and then switched back to 1.
- Ownership this round: only maintain PRODUCT_PLAN_CURRENT.md, COOP_DELIVERY.md, CHANGELOG.md; do not undo the changes of other agents or the user, and do not commit, merge, release, or upload to the Workshop.
- Evidence labels: code changed (unverified), static check (not run), automated pass, live pass, historical record, published. The last two current-status labels may be used only after the corresponding new evidence is obtained.

v0.10.2 is used only as a baseline tag and a historical version name. It does not mean the uncommitted changes layered on this worktree have entered a release package.

## One Confirmation for the User

The user only needs to confirm the scope below once, and the agent can then execute by phase, continuing to the next phase after each phase produces evidence:

> Agreed: on C:\Users\chart\Documents\project\sp-player-first-experience, baseline 1be8e83 + the current uncommitted changes, execute offline gates, isolated builds, ZIP checks, and the designated live manual acceptance per phases A-G of this file; writing is allowed to the worktree's build/, bin/obj, .uv-cache, and temporary directories; the live phases use only a dedicated test game directory, test profile slots, and isolated dual-instance settings files; by default, do not access real saves, call a real model, or run release/upload operations. Real model tests may begin only after the model, endpoint, unit price source, maximum requests/tokens, and total budget are separately confirmed. On any failure, fix first, then retest the failed item and the full affected matrix; items without evidence stay "not run".

This confirmation does not automatically authorize the following actions: overwriting the everyday mods/, opening or modifying the everyday saves, sending requests to a model provider, pushing to GitHub/Workshop or creating a Release. Even though the current packaging script already passes -SkipInstall to the build step, phases C/G may still write only to dedicated test directories.

## Phases, Commands, and Isolation Matrix

All code blocks are candidate commands that run "only after confirmation"; none of them was executed while writing this document. Each command is annotated with its own working directory, to avoid continuing to use repository-root relative paths after a cd inside mcp_server.

| Phase | Purpose and entry point | Explicit CWD / environment | Game, save, model side effects | Rough duration / exit gate |
| --- | --- | --- | --- | --- |
| A1 | C# core tests: the full set registered in TestRunner, covering the first-run guide, character probing, settings save, delete binding, pause/resume, recovery policy, budget, teammate, run boundary, settlement/save contracts, Agent loop, native MCP, and the existing game contracts | CWD=<repo>; Windows PowerShell; .NET 9 SDK; no real model credentials set | Does not launch the game; loopback/temp files in the tests may stay only in the test temp directory | About 2-8 minutes; exit code 0, all tests PASS |
| A2 | Python MCP sidecar unit tests | CWD=<repo>\mcp_server; uv per uv.lock; UV_CACHE_DIR=<repo>\.uv-cache recommended | Does not launch the game, does not access the Mod, does not call a model; the uv cache writes only into the worktree | About 1-5 minutes; exit code 0 |
| A3 | Python profile matrix and native tool alignment: guided / layered / full / debug gating, plus test_native_tool_alignment.py | CWD=<repo>; PowerShell; RepoRoot=<repo>; STS2_ENABLE_DEBUG_ACTIONS set only temporarily by the script | Only imports Python and reads source/offline data; does not launch the game | About 1-3 minutes; the tool sets and debug switch match the matrix |
| A4 | Static source check of the version and README install rules | CWD=<repo>; Python 3.11+; no game or model needed | Reads files only; generates no ZIP, installs no Mod | About 1 minute; version fields, links, and mod/ install instructions are consistent |
| A5 | Native command failure propagation check | CWD=<repo>; Windows PowerShell; the script uses temporary fixtures | Does not launch the game, does not access saves or models; writes only the system temp directory and cleans up | About 1 minute; fault injection returns non-zero and must not print success |
| A6 | The offline overall gate coordinated with fix_delivery: preflight-release.ps1 | CWD=<repo>; PowerShell, .NET 9, Python, uv; -ProjectRoot <repo> -Configuration Release | Builds and runs offline tests; does not install into the game mods/, does not launch the game; uv may access the dependency index | About 5-15 minutes; all offline categories and the version and document thresholds pass |
| B | Safe build of Mod staging, explicitly using -SkipInstall | CWD=<repo>; needs the Godot console executable, preferably an explicit -GodotExe; -GameRoot may point at a dedicated test directory | Writes the worktree bin/obj and build/mods/STS2AIAgent; -SkipInstall does not copy into the game mods/ | About 5-15 minutes; DLL, PCK, and mod_id.json are complete and the game directory is unchanged |
| C | Generate the Windows ZIP; package-release.ps1 currently passes -SkipInstall to the internal build-mod and calls the artifact checker before and after compression | CWD=<repo>; same as B; -OutputRoot points at a dedicated worktree directory | Writes only the worktree release directory and ZIP; the internal build will not write into the game mods/ | About 2-8 minutes; the source check, directory check, and ZIP check all pass |
| D | Script smoke / API state of an installed Mod | CWD=<repo>; dedicated game directory, dedicated API ports (for example 18080/18081); settings files at absolute temporary paths | Launches/stops the game and reads and writes test profile slots, may write logs and steam_appid.txt; does not touch everyday saves | About 10-20 minutes; run only after a dedicated install |
| E | Live manual product journey: settings, pause, budget, disconnect, dual-instance, full settlement and save | CWD=<repo> used only for records; the game in a dedicated directory/account; dual-instance uses two isolated STS2_AGENT_SETTINGS_PATH | Launches two windows, occupies test ports, and changes the test save; real models not allowed by default | 5-90 minutes per item; gather evidence item by item per the feature matrix |
| F | Real model cost and strategy acceptance (confirmed separately) | Provider, model, endpoint, price, and limits confirmed separately by the user | Incurs provider charges and sends state/prompts; must not inherit the everyday Key or real saves | Depends on model response and run length; no cost is estimated without a price source |
| G | Install, upgrade, rollback, and the final delivery decision | CWD=<repo>; operate only on the dedicated test install directory | Writes the test mods/, backup directory, and test settings; no push, no upload, unless separately authorized | About 15-40 minutes; still a candidate package after passing |

### Offline Commands That Can Be Re-checked Directly

Each block of commands below starts from a fresh PowerShell session, and CWD is not shared:

    # A1; CWD: C:\Users\chart\Documents\project\sp-player-first-experience
    Set-Location -LiteralPath 'C:\Users\chart\Documents\project\sp-player-first-experience'
    dotnet run --project '.\STS2AIAgent.Tests\STS2AIAgent.Tests.csproj' -c Release

    # A2; CWD: C:\Users\chart\Documents\project\sp-player-first-experience\mcp_server
    Set-Location -LiteralPath 'C:\Users\chart\Documents\project\sp-player-first-experience\mcp_server'
    $env:UV_CACHE_DIR = 'C:\Users\chart\Documents\project\sp-player-first-experience\.uv-cache'
    uv run --locked python -m unittest discover -s tests -v

    # A3; CWD: C:\Users\chart\Documents\project\sp-player-first-experience
    Set-Location -LiteralPath 'C:\Users\chart\Documents\project\sp-player-first-experience'
    $env:UV_CACHE_DIR = 'C:\Users\chart\Documents\project\sp-player-first-experience\.uv-cache'
    powershell -ExecutionPolicy Bypass -File '.\scripts\test-mcp-tool-profile.ps1' -RepoRoot 'C:\Users\chart\Documents\project\sp-player-first-experience'

    # A4; CWD: C:\Users\chart\Documents\project\sp-player-first-experience
    Set-Location -LiteralPath 'C:\Users\chart\Documents\project\sp-player-first-experience'
    python '.\scripts\check_release_metadata.py'
    python '.\scripts\check_release_package.py' --source-root 'C:\Users\chart\Documents\project\sp-player-first-experience'

    # A5; CWD: C:\Users\chart\Documents\project\sp-player-first-experience
    Set-Location -LiteralPath 'C:\Users\chart\Documents\project\sp-player-first-experience'
    powershell -ExecutionPolicy Bypass -File '.\scripts\test-native-exit-propagation.ps1'

    # A6; CWD: C:\Users\chart\Documents\project\sp-player-first-experience
    Set-Location -LiteralPath 'C:\Users\chart\Documents\project\sp-player-first-experience'
    powershell -ExecutionPolicy Bypass -File '.\scripts\preflight-release.ps1' -ProjectRoot 'C:\Users\chart\Documents\project\sp-player-first-experience' -Configuration Release

A6 is the candidate for the offline overall gate that can be confirmed statically in the current repository. It is not evidence run this round. Before final delivery, fix_delivery must confirm it is still the offline overall gate and keep its source-check call using --source-root; the actual ZIP check is done by the --artifact input of G0.

### Build, Packaging, and Final ZIP Check

    # B; CWD: C:\Users\chart\Documents\project\sp-player-first-experience
    Set-Location -LiteralPath 'C:\Users\chart\Documents\project\sp-player-first-experience'
    powershell -ExecutionPolicy Bypass -File '.\scripts\build-mod.ps1' -ProjectRoot 'C:\Users\chart\Documents\project\sp-player-first-experience' -Configuration Release -SkipInstall -GodotExe '<absolute-path-to-Godot-console.exe>'

B's -SkipInstall is a required parameter. B writes worktree build artifacts but does not copy the DLL/PCK/manifest into the game mods/.

    # C; CWD: C:\Users\chart\Documents\project\sp-player-first-experience
    Set-Location -LiteralPath 'C:\Users\chart\Documents\project\sp-player-first-experience'
    powershell -ExecutionPolicy Bypass -File '.\scripts\package-release.ps1' -ProjectRoot 'C:\Users\chart\Documents\project\sp-player-first-experience' -Configuration Release -OutputRoot 'C:\Users\chart\Documents\project\sp-player-first-experience\build\release-confirmation' -GodotExe '<absolute-path-to-Godot-console.exe>'

C currently passes -SkipInstall in its internal build step, checks the release directory before compression, and checks the actual ZIP after compression. The generated directory name and ZIP name are determined by the mod_manifest.json version and any collision suffix; there is currently no real artifact, so no name, hash, or pass status can be written in advance.

The checker can run in only one mode at a time: `--source-root` or `--artifact`, which cannot be written in the same command.

    # G0 source contract; CWD: C:\Users\chart\Documents\project\sp-player-first-experience
    Set-Location -LiteralPath 'C:\Users\chart\Documents\project\sp-player-first-experience'
    python '.\scripts\check_release_package.py' --source-root 'C:\Users\chart\Documents\project\sp-player-first-experience'

    # G0 artifact; CWD: C:\Users\chart\Documents\project\sp-player-first-experience
    Set-Location -LiteralPath 'C:\Users\chart\Documents\project\sp-player-first-experience'
    python '.\scripts\check_release_package.py' --artifact '<absolute-path-to-generated-release.zip>'

G0 source mode checks the install instructions and the packaging script wiring; artifact mode checks `mod/` and the in-package Markdown links of a real directory or ZIP. Neither has been run, and there is no real ZIP, hash, or pass record. package-release.ps1 also runs the artifact check internally once each for the directory and the ZIP.

## Full Feature Acceptance Matrix

| ID | Capability and coverage | Automated evidence (once approved) | Live/manual evidence and acceptance criteria | Default resources / status |
| --- | --- | --- | --- | --- |
| M1 | C# / Python basic contracts | A1, A2; C# TestRunner and mcp_server/tests/ | No live play needed; all commands exit 0, and a failing item blocks the later ones | No model, no game; not run |
| M2 | Native MCP and Python cross-surface contract | McpServiceTests: disabled 403, initialize/session, tools/list, tools/call, notification 202, client config; PlayerExperienceTests.NativeMcpToolsMatchGuidedActContract; test_native_tool_alignment.py; A3 profile matrix | After the dedicated Mod starts, discover the health/state/data/wait/act tools from an available MCP client; confirm the option_index, target_index, and card_index fields are consistent, notifications get no fake response, and when disabled an understandable 403 is returned | Offline contracts first; external client connection not run |
| M3 | First-run guide and per-purpose Test Connection | PlayerExperienceTests default unverified, play verified, Key change, chat success does not mask play 401, fresh fingerprint; CompanionStartupTests.FirstRunProvider | Test the chat/play/vision roles in turn; the result shows the correct role, success/failure, and the next step; when play is unverified an invite is not allowed; when vision is unused it shows unused/unverified rather than faking success | No real Key; blocked without a safe fixture; not run |
| M4 | Settings save, reload, migration, and delete protection | SettingsStoreTests round-trip/missing/migration; PlayerExperienceTests endpoint/model removal; A1 | Bind chat, play, and vision separately and save; they remain after switching pages/restarting the overlay; deleting a referenced endpoint/model first shows the role and replacement steps; canceling does not delete; deletion is allowed only after rebinding and saving; unreferenced objects can be deleted | Writes only test settings; not run |
| M5 | Error paths per role | C# fakes cover play 401, per-purpose state, and error next steps; OpenAiCompatibleClientTests covers protocol parsing | Observe 401, 429, 5xx, timeouts, and bad addresses separately for chat/play/vision; the error is attributed to the purpose that triggered it and cannot be overwritten by another purpose's success; no infinite retry; can be corrected and retested on its own | Real model confirmed separately; not run |
| M6 | Pause / resume / chat takeover | AutoPlaySessionTests, AgentLoopTests, TeamConversationTests, PlayerExperienceTests.PlayerFacingMapsPauseAndConfigError | Pause while a model request is waiting; an already-submitted action may complete and no new action is dispatched afterwards; the pause reason is shown; chat works during the pause and executes no act; resume starts only one new turn, with no overlap or duplication | Prefer cost-free responses; not run |
| M7 | Budget, usage known and unknown | SessionBudgetGuardTests covers token/request limits, in-flight, recovery accumulation, and immediate stop on exceeding; OpenAiCompatibleClientTests covers JSON/SSE usage; PlayerExperienceTests.MissingUsageIsNotDisplayedAsZero | A response without usage shows "unknown/none yet" instead of 0; a tiny request/token limit stops before the next turn; pause/resume does not zero it; chat/teammate replies share the budget; no new requests after exceeding | No model cost; not run |
| M8 | Network disconnect, backoff, recovery, and non-retryable errors | AutoPlayRecoveryTests covers 401/403/429/5xx, no action, waiting, and canceling backoff; AgentLoopTests covers cancel/failed action | Only in a cost-free mock or a separately authorized environment: temporarily cut the network while a request is waiting and observe bounded backoff and a visible error; after recovery continue manually and read the latest state; configuration errors are not retried infinitely; an uncertain action is not blindly replayed | Blocked without a safe mock; a real network failure needs separate confirmation; not run |
| M9 | Dual-instance launch, account/settings/port/character isolation | CompanionStartupTests, LoopbackListenerTests, TeamConversationTests; the existing test-multiplayer-lobby-flow.ps1 covers lobby/port/vote preconditions | The host invites the companion; health matches service/ready/role/port/PID together; repeated clicks do not start duplicate processes; offline dual-instance uses different clientIds; the main/secondary settings files do not overwrite each other; the companion operates only its own character | Changes test processes, ports, and saves; not run |
| M10 | Dual-instance shared journey and cooperative combat | The existing test-multiplayer-lobby-flow.ps1 and test-coop-play-together.ps1 (with real parameters only); A1 teammate/boundary tests | Create room -> join -> character select -> ready -> opening cutscene -> same map node -> at least one combat -> rewards/rest -> back to the map; the main window can send messages; pause/resume controls only the companion; votes, characters, and state are consistent | No real model by default; strategy effect by manual testing; not run |
| M11 | Full settlement, unlocks, return to main menu, and save | GameOverContractTests and ProgressSaveVerificationTests cover summary/continue/return and physical save missing/corrupt/mismatched/read failure; CurrentRunBoundaryTests covers leaving a run | On a dedicated save, finish a run normally or lose normally, wait for the summary animation, and go through the native continue/unlock/return flow; both windows confirm save verified; progress persists after restart; after returning to the main menu the teammate stops and does not start a new run automatically. With no script, operate manually only | Changes the test save; not run |
| M12 | Install, upgrade, rollback | A4, C, G0 ZIP structure/link checks; B staging | Copy only mod/ from the ZIP into the dedicated mods/, and start via the Steam entry in the README; confirm overlay/health/version/settings; back up the old DLL/PCK/manifest before upgrading, and after overwriting check for leftover old manifests; rollback restores the backup | Operates only on the dedicated install and backups; not run |
| M13 | Release candidate and support boundary | A6, C, G0; the fix_delivery final checker confirms the record | Record the version, ZIP path/hash, install/upgrade, failures/fixes/retests, and the support matrix; without the user's release authorization, stop at the candidate package and create no Release/Workshop | Not released; not run |

## Live Commands and Manual Boundaries

The project's existing live scripts may only be called with their real parameters:

    # M9 lobby and port preconditions; CWD: C:\Users\chart\Documents\project\sp-player-first-experience
    Set-Location -LiteralPath 'C:\Users\chart\Documents\project\sp-player-first-experience'
    powershell -ExecutionPolicy Bypass -File '.\scripts\test-multiplayer-lobby-flow.ps1' -ProjectRoot 'C:\Users\chart\Documents\project\sp-player-first-experience' -HostApiPort 18080 -ClientApiPort 18081

    # M10 existing dual-instance flow; CWD: C:\Users\chart\Documents\project\sp-player-first-experience
    Set-Location -LiteralPath 'C:\Users\chart\Documents\project\sp-player-first-experience'
    powershell -ExecutionPolicy Bypass -File '.\scripts\test-coop-play-together.ps1' -HostApiPort 18080 -CompanionApiPort 18081 -Minutes 25

These scripts launch the game and read and write test state, and must not be run on the default user install or everyday saves. test-full-regression.ps1 also launches/stops the game; use it only in a dedicated game directory and after the user confirms the side effects on the test install. It does not replace the manual acceptance of M3-M8 and M11-M12.

Where settings save/delete, pause/resume, network disconnect, real model cost, full settlement save, and install/upgrade have no single safe script that can be confirmed statically, use the manual steps and acceptance criteria in the matrix. Without a usable cost-free network fault fixture, do not fill the gap with real model requests.

## Failure, Fix, and Retest Rules

1. Any non-zero command, assertion failure, UI state mismatch, inconsistent state across the two windows, unverified save, or ZIP structure mismatch is immediately marked "failed / needs fixing", and later phases are not claimed to pass.
2. Fixes are done by the agent that owns the relevant code files, with the cause, modified files, and new evidence written in the record; do not rerun without fixing, and do not fix without retesting.
3. Retest order: the failing case/manual scenario -> the related matrix of the same file or module -> A6 or the corresponding live regression. The affected C#, Python, native MCP, ZIP, or dual-instance contracts must all be brought back into scope.
4. For failure output, request IDs, versions, ports, PIDs, settings paths, and save types, record only redacted summaries; diagnostic exports by default contain no chat text, API Key, Authorization, or session token.
5. Until new evidence is obtained, keep "not run"; a PASS in historical logs may only go in the historical area and cannot be promoted to the current baseline or current release status.

## Cost and Authorization Boundary

The offline C#/Python/MCP contracts and the ZIP documentation check need no real model cost; the game's own test time and local resources are recorded separately. Real model cost cannot be reliably derived from the repository, request counts, or old logs, and must be confirmed separately by the user before phase F starts: provider, model, billing unit/price source, maximum requests, maximum tokens, maximum duration, and total budget. If the service returns no usage, only "unknown" can be shown, relying on hard caps of request count/duration, and unknown must not be converted to 0 or a made-up amount.

## Currently Incomplete

- A1-A6 and B-G have all not been run on this worktree; there is no current automation, build, ZIP, API, game, real model, or release evidence.
- The G0 --source-root / --artifact checker interface has been coordinated into the documents by fix_delivery, and the actual scripts and parameters still need a static re-check before execution; currently not run, no ZIP generated, not passed.
- Do not access real saves, do not call a real model, do not clean other worktrees, do not commit, do not publish.

