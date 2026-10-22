# Fix review issues since v0.12.4

## Goal

Fix the verifiable deviations found in the review from `v0.12.4` to the `dev` HEAD; keep the existing 21 uncommitted changes, keep parallel workers from overwriting each other, and restore the product contracts of dual-mode mutual exclusion, chat guidance, Jev planning, and session memory.

## Background / evidence

- As of this round's baseline: `dev` HEAD `e11e265`, 21 uncommitted changes, including `AgentLoop.Decision.cs`, `PlaySessionMemory.cs`, and the new audit tests; these files are existing work and must not be reset or overwritten.
- The Mod Release build has 0 warnings; the Python MCP run with the existing `.venv` passed 416 with 8 skipped. Earlier in the restricted environment the C# offline tests had 7 failures involving `File.Replace` and `HttpListener`; after the permission change on 2026-09-23, rerunning the same `dotnet run --project STS2AIAgent.Tests/STS2AIAgent.Tests.csproj -c Release --no-restore` gave exit code 0 and no FAIL lines. That result is the pre-fix offline baseline and does not mean a live pass. All 12 offline gates of `python scripts/check_verification_gates.py` also passed (2026-09-23, pre-fix baseline).
- Requirements baseline: `.trellis/tasks/09-22-dual-mode-ui-jev-two-layer/prd.md` and the three subtask PRDs and designs; this task is fix and integration and does not replace the original requirements.

## Requirements

### R1 Mode, chat, and decision stream (P1/P2)
- The mode slider is not just show/hide: single-player auto-play and local teammate auto-play cannot be active at the same time; on switching, first wait for the current mode's pause confirmation, then update the display and settings, and if the pause fails keep the original mode and do not automatically start the other mode; do not break the read-only/takeover ability of the external MCP. Basis: parent PRD:20,40; code `AgentOverlayHost.Pages.cs:90-99`.
- During single-player auto-play the player can send an instruction to influence the next decision, and can also pause/resume from the chat entry; the instruction does not call `PlayIntent` to play a card immediately, is consumed only once, has capacity and length limits, is isolated per the same run, is not reused after a run switch, an unconsumed instruction is not lost after cancel/pause, an accepted action is not replayed, and an action in progress is not interrupted; original PRD:21; currently `AgentRuntime.cs:619-623` rejects it and `AgentLoop.Decision.cs:92-148` does not consume history.
- **Do not restore a separate `PlayModelId`.** On 2026-09-23 the user confirmed that the newer `.trellis/tasks/09-22-pr196-live-fixes/plan.md:30-35` prevails: chat/play share one main model; clearing the old binding at `AgentOverlayHost.Settings.cs:761-768` is an expected migration, not a defect of this round. The "role binding" of old UI PRD:29 must be interpreted per that later decision; verify that old configuration migration causes no invisible wrong references.
- The chat stream shows operation/tool results, not just the action name and reason; original UI PRD:17; currently `AgentRuntime.Accounting.cs:78-92` does not write `ActResultJson`. The user chose to keep "play one move on behalf only on an explicit phrase" and not to restore the "allow play on behalf" checkbox; chat during auto-play serves only as a next-move strategy instruction and does not run a play on behalf concurrently, and old UI PRD:18 and its design.md:31 must be revised in sync.

### R2 Jev execution and planning (P1/P2)
- An already-started companion can receive multiplayer two-layer switch changes immediately, consistent with the main instance's Jev configuration; it cannot just update the main instance settings. Parent PRD:43; `AgentOverlayHost.Pages.cs:340-345`, `CoopLaunchPolicy.cs:80-94`.
- Both MCP surfaces of `get_planner_briefing` and the underlying HTTP should provide a same-source run summary, current screen, current strategy, recent Jev decisions, and confidence trend; only nullable fields are added, with no existing fields removed. Engine PRD:30; `NativeMcpServer.Tools.cs:125-142`, `Router.cs:246-258`.
- The planner uses recent decisions and the current strategy, with periodic triggers on context switch/consecutive low confidence/every N moves; it stays read-only, with budget ownership, cancellation, and version protection. Engine PRD:17; `StrategyPlanner.cs:45-56,85-95`.
- Align the Choice + optional Score danger level display; the Jev UI is visible in both modes and shows action, confidence, probabilities, and latency, and must not show fake data from a stale instance. Engine PRD:11, UI PRD:25.
- A Jev 429 retries at most once on the same move per a bounded `retry-after`, **each actual network attempt is counted separately against the request budget**, and the whole Jev turn shares a total timeout; with insufficient budget it must not retry, and failure/cancellation keeps the attempt receipt. Ordinary 4xx/408/429/5xx and timeout classification is explicit; currently `AgentRuntime.Jev.cs:159-163`, `JevClient.cs:186-218,284-289`. This decision supersedes the "single transport attempt" wording in the old Unreleased and must be synced to the CHANGELOG.

### R3 Per-run session persistence (P1/P2)
- External HTTP `/action` can attribute decisions to the real game run even without starting auto-play, for use by recent memory and the on-disk session; do not use only `_runBoundary.RunId`. `Router.cs:461-467`, `AgentRuntime.Session.cs:22-29`.
- Old-run logs arriving late must not switch the current session back to the old run; switch by the real current run, and record and isolate by the run the log belongs to. `AgentRuntime.Session.cs:22-29,77-102`.
- Session save backup/old file migration and deletion stay recoverable and must not mask real code problems behind environment-limited `File.Replace` test failures; recheck the new `Audit.StoreSavePreservesCollidingLegacyRun` in a normal environment.

## Constraints / out of scope

- Do not reset the user's dirty working tree, do not commit, release, or deploy over the game DLL; before the task starts, save a timestamped/checksummed snapshot of the original 21 items (17 modified, 4 untracked) of the working tree in the task directory or an isolated backup. Workers exclusively own production files and their own new test files, and the integrator is responsible for cross-domain code, shared registration, and targeted rollback; a whole-file undo that overwrites the original dirty changes is not allowed.
- HTTP/MCP state and fields are backward compatible; the two MCP surfaces stay aligned in both directions; keep the game thread boundary, bounded waits, same-turn request/Token accounting, and cancellation receipts.
- Do not call offline compilation/unit tests live acceptance; if live runs and the real Jev service cannot be executed safely, list them as a manual recheck list.

## Acceptance Criteria

- [ ] R1 Single-player/multiplayer selection is mutually exclusive at runtime, in-run instructions are queued per run and take part in the next decision, pause/resume, main model migration, and chat results have compilable pure-logic/source regressions; Godot UI and two-instance behavior are listed separately as live acceptance.
- [ ] R2 The companion two-layer settings take effect immediately; native and Python MCP briefing fields agree, the risk score/latency UI is visible; planning refresh/accounting and the 429 policy have deterministic tests.
- [ ] R3 Run ownership is consistent across the HTTP/native MCP/auto-play entry points, late old-run records do not pollute the new run, and continuing a game can restore decisions/chat/strategy.
- [ ] C# unit tests, the Mod Release build, Python MCP unit tests, and the matching static gates pass; environment-limited items are listed separately; the Unreleased section of `CHANGELOG.md` and the API docs are in sync.

## Product decisions

- The mode slider pauses and confirms before switching, and does not automatically start the other mode.
- Keep play-one-move-on-behalf by explicit phrase, do not restore the "allow play on behalf" checkbox; in-run chat only affects later decisions.
- A Jev 429 retries at most once, billed per attempt, with the budget checked first and a single-turn total timeout kept.
- Continue the newer 09-22 live requirement: a single main model for chat/play, and do not restore a separate play model selector.
