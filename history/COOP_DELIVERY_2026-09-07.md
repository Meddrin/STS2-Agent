# AI Teammate Delivery Tracking

> **Historical; does not represent the current state.** This file keeps the original delivery record from before the move; for the current status see [PRODUCT_PLAN_CURRENT.md](../PRODUCT_PLAN_CURRENT.md), and for the historical index see [COOP_DELIVERY.md](../COOP_DELIVERY.md).

## 2026-09-06 Player First Success and Run Status (code implemented, validation not run)

Worktree: `C:\Users\chart\Documents\project\sp-player-first-experience`, branch `codex/player-first-experience`, based on `origin/main` `1be8e83` / release v0.10.2.

This batch adds: install instructions and the zip `mod/` layout, the first-run guide, per-purpose connectivity tests, settings save/delete protection, main-window run status and pause wording, unknown usage, redacted diagnostics, and the MCP selection matrix. It does not redo the recovery/budget/native MCP core strategy.

**Evidence status:** the code is written and the test cases are updated, but **no** build or test has been run yet. Live play, real models, and zip artifact checks all await authorization.

Follow-up static rework: all diagnostic fields go through redaction; the pause completion callback is isolated by session identity; a model test failure clears only the transient stop for the matching purpose; deleting a referenced endpoint/model now rebinds first and then deletes; the packaging check is split into `--source-root` / `--artifact`; `RuntimeExperienceRegressionTests` is wired into TestRunner.

**2026-09-06 offline validation (worktree `sp-player-first-experience`, HEAD `1be8e83` + uncommitted changes):** A1 C# TestRunner all green; A2 Python 49 passed; A3 guided 10 / layered 18 / debug 11 / full 63; A4 version and source packaging passed; A5 exit code propagation passed; A6 preflight passed (Mod Release build 0 warnings 0 errors); B SkipInstall build succeeded; C/G0 zip `build/release-confirmation/sts2-ai-agent-v0.10.2-windows.zip` artifact check passed.

**2026-09-06 live (user allowed writing to Steam `mods/`):** the old files were backed up to `build/live-test-backup-mods` and this worktree's staging was installed. `/health` reported `mod_version=0.10.2`, `api_port=18080`, `mcp_enabled=true`, `mcp_url=http://127.0.0.1:18080/mcp`. With an unverified configuration, `invite_ai_teammate` returned 409 "configuration filled in, but the play model has not been verified yet". Native MCP initialize 200, tools/list contained health/state/act and option_index/target_index/card_index. The Agent settings used `build/live-test-settings/settings.json`. The dual-instance lobby script, on profile slot 3, completed room creation/join/character select/ready/map vote, then failed on a combat wait timeout. Afterward it switched back to slot 1 and closed the game. No paid model was called. The everyday slot 1 did not start a run.

Uncommitted changes in the old workspace `sp` @ `24d3244` were not migrated; on checking, the relevant FirstRun/Companion files already exist on main.

---

## PR Series and Branch Capability Progress (2026-09-05)

> **Baseline note**: the remote `main` (`27f2b70`) is currently in a state where `STS2AIAgent.Tests.csproj` references `LoopbackListener.cs` twice, causing a `NETSDK1022` compile failure. The capabilities below were submitted as a chain of candidate PRs split by convention; unit tests and preflight pass inside each branch, but **until all are merged into `main` and integration validation is rerun, they cannot be treated as the delivered baseline of the mainline or an official Release**.

### Candidate PR chain and current status:

1. **[PR #61](https://github.com/CharTyr/STS2-Agent/pull/61) (`codex/integration-validation`, commit `accd605`)** — **Merged into main**
   - Fixes the duplicate compile reference NETSDK1022 in the test project while keeping 100% of the unit tests; adds the `.github/workflows/validate.yml` CI workflow; hardens the preflight exit codes with `Invoke-CheckedNative` in `scripts/lib-checked-native.ps1`.
   - Branch-level validation: 97 C# core tests passed, 48 Python tests passed, Mod Release build 0 warnings 0 errors.
2. **[PR #62](https://github.com/CharTyr/STS2-Agent/pull/62) (`codex/autoplay-recovery`, commits `577953d` + `a280799`)** — **Merged**
   - Implements `AutoPlayRecovery.cs` (stop after 3 consecutive failures, 2/4s exponential backoff, normal waiting does not consume the budget, HTTP status codes preserved); run-boundary protection in `CurrentRunBoundary.cs` (stops immediately on returning to the menu/lobby).
   - Branch validation: 107 C# core tests passed.
3. **[PR #63](https://github.com/CharTyr/STS2-Agent/pull/63) (`feat/coop-offline-and-settings-isolation`, commits `b0d1197` + `f9c9015`)** — **Merged**
   - Offline dual-instance account isolation: the secondary window automatically increments `--clientId {id + 1}`; guard against extreme ClientId values.
   - Configuration isolation: `SettingsStore` supports the `STS2_AGENT_SETTINGS_PATH` environment variable, and the launcher automatically derives `settings.companion.json` and syncs it on first run, preventing concurrent overwrites.
   - Branch validation: 116 C# core tests passed.
4. **[PR #64](https://github.com/CharTyr/STS2-Agent/pull/64) (`feat/agent-token-usage-and-budget`, commits `be3b4d8` + `265953f`)** — **Merged**
   - LLM token usage accounting (JSON / SSE stream_options parsing); multi-round tool and vision model calls measured uniformly; hard session budget guardrail `SessionBudgetGuard.cs` (`MaxSessionTokens` / `MaxSessionRequests`); the budget guard is linked throughout with cumulative session counts and chat interception.
   - Branch validation: 120 C# core tests passed.
5. **[PR #65](https://github.com/CharTyr/STS2-Agent/pull/65) (`docs/roadmap-and-delivery-alignment`)** — **Merged**
   - Consolidates planning documents and delivery status, and spells out the mainline failure baseline and the branch evidence boundary.

### Test evidence boundaries:
- **Remote main (including PR #56, #61)**:
  - PR #61 was merged into main and fixed the NETSDK1022 problem; PR #56 was merged and supports native Profile switching.
- **Full integration-branch validation (all capabilities of #56, #61-#65 together)**:
  - C# core test suite: 121 / 121 passed (100%), including #56 Profile switching and #64 budget protection.
  - Python MCP test suite: 48 / 48 passed (100%).
  - Mod Release build: 0 warnings 0 errors.
  - Pre-release preflight `scripts/preflight-release.ps1`: Exit Code 0, all passed.

---

## Auto-Run Recovery (2026-09-05, follow-up branch)

- Based on the integration fix commit `accd605`, on the independent branch `codex/autoplay-recovery`.
- Auto-run stops after three consecutive errors or no-action decisions, with 2/4 second intervals before the first two retries; a successful action resets the count. Waiting for the player/game does not increase the count and does not clear an existing failure.
- The HTTP status is preserved through to the model exception; non-retryable 4xx configuration errors stop immediately and prompt a check. 401/403/429/5xx no longer trigger an extra non-streaming resend because the error body mentions stream. 408/429/5xx use bounded recovery.
- Pause can cancel the backoff, and the stop reason is shown through the existing session observer; this batch does not change chat control ownership.
- This batch only addresses decision failure recovery and is not equal to full "no-progress detection": an action returning success with no change in the position, a long UNKNOWN wait, the current-run end boundary, budget, and disconnect recovery still need later implementation and live acceptance.

## 2026-09-05 Integration Validation and Release Gate

- Based on the remote main `27f2b70`, fixed NETSDK1022 caused by the test project referencing `LoopbackListener.cs` twice, keeping all tests. The full release preflight passed in an independent worktree: 97 C# core tests, 48 Python tests, guided/layered/debug/full profiles, version metadata and document checks; Mod Release build 0 warnings / 0 errors.
- Added a Windows CI with no game installation dependency. Preflight explicitly checks the exit code of every native command; fault injection actually launches a preflight child process and verifies that when the build exits with 23 the whole preflight fails and prints no success.
- Corrects the historical status below: main-window pause/resume was merged with #59, including the teammate control interface, pause wait, and cancellation boundary; full dual-instance live acceptance is still not complete.
- This batch did not deploy the game, run real-model/live validation, or release a stable version; the remote CI status follows the checks on the corresponding PR. Work continues on run recovery, configuration/budget, and the whole-run experience; completing the gate cannot be counted as completing the whole plan.

Goal: advance a mature product per PRODUCT_ROADMAP.md; dual-instance play is the Mod's main selling point, highlighting the fun interaction of an AI Agent and a human playing Slay the Spire together.

Working branch: `codex/ai-companion-experience`, created from main `71c3e41`. Solo auto-play or marketing copy changes alone cannot substitute for delivery.

## Full User Journey and Completion Evidence

| Journey | Must deliver | How to prove | Status |
| --- | --- | --- | --- |
| Invite a teammate | With a model configured, reliably launch the second instance from the main window, join the same lobby, select a character and ready up; no duplicate processes, no effect on the user's current save | Real dual-instance play; tests for default/reserved ports, exit/timeout, and repeated clicks | In progress |
| Discuss tactics | The player sends a message to the AI teammate in the main window and the AI understands and responds; no need to switch to the second window | Correct instance routing, message return, live chat record | Communication/UI implemented, awaiting live test |
| Fight together | Messages and shared goals enter the teammate's later decisions; the AI controls only itself, explains key intent, and can respond to player suggestions | Request context and game action evidence, both characters' states | Messages entering later requests is tested; the live strategy effect is yet to be verified |
| Take over anytime | Pause/resume the AI from the main window, and chat still works while paused; actions are not executed twice; disconnect is recoverable | Control interface / cancellation boundary / concurrency unit tests cover it (see #59); full two-player live closed loop awaits acceptance | Interface merged into mainline, live end-to-end awaits acceptance |
| A fun companion | Optional communication style, brief proactive remarks at key moments, post-fight feedback, controlled interruption frequency | Responses to the real situation and player experience testing, no faked canned praise | To be implemented |
| A complete adventure | Shared route, events/rewards, shop/rest, combat, settlement/unlocks/save, the whole flow | Full two-player live regression, including losing runs | To be validated |
| Mature delivery | Configuration/budget/diagnostics, CI gate, dual-channel install/upgrade and support matrix | M0-M4 and the 1.0 metrics in PRODUCT_ROADMAP.md | To be completed |

## 2026-09-05 First Batch of Implementation

- Adjusted the product main line and 1.0 acceptance; dual-instance play is no longer an experimental capability that can be skipped.
- Implemented an actual HTTP bind check and a bounded dynamic port fallback, covering Windows reserved ranges, bind contention, an unavailable explicit port, and resource closing; the teammate port probe also uses HTTP rather than TCP only.
- Added a backward-compatible `process_id` to `/health`; teammate launch must now match service, ready, role, port, and this launch's PID together, and no longer judges ready by string containment.
- Waiting now ends early when the teammate process exits, distinguishing process exit from still running but not connected; external cancellation is no longer swallowed.
- Changed the human window's default page to "AI Teammate"; before inviting, it saves the model settings and checks the main menu / auto-play / model endpoint conditions; repeated clicks are disabled during the invite, the actual child process handle is kept underneath, and after a timeout no second teammate is started.
- Automated evidence: 9 new launch/identity/team-formation precondition tests; 57 core tests passed, including real loopback HttpListener requests. The full Mod Release build passed, 0 warnings / 0 errors.
- Not deployed, and live dual-instance acceptance not completed; a simulated Windows reserved range cannot replace a re-check in a real reserved-range environment. A bind race still exists between the teammate's explicit port probe and launch, and must be handled in the session setup/retry design.

## Second Batch: Team Chat

- A separate session token is generated for each new teammate process (passed only through the child process environment). Added `/companion/message`, which requires the companion role, local access, the correct token, and a bounded message body.
- The main window gains a team chat area; messages are delivered to the AI teammate's play model and the reply is shown in the main window; the request first verifies the teammate PID, and a failure is not resent automatically.
- Team chat and play share the turn mutex, so replies can be queued during auto-play and chat also works while paused. Chat is forced read-only; neither an explicit play-for-me phrase nor an act returned by the model can bypass it; chat does not start or resume play.
- The latest 12 team messages are available to later play, explicitly marked as historical context, requiring indices to be re-resolved from state. They are cleared when a new process replaces the session, and repeated invites of the same instance keep the conversation.
- Added 6 tests: the read-only boundary, suggestions entering later requests, history bounds/clearing, session authorization, HTTP transport body/token, and PID replacement rejection. 63 core tests passed; the full Mod Release build 0 warnings / 0 errors.
- Live dual-instance play, real model replies, UI layout, and strategy effect are still unverified. The main-window remote control interface and pause boundary were merged into the mainline with #59; the full two-player live closed loop, AI proactive speech, style, and low-interruption strategy remain to be implemented and accepted.

Next step: main-window teammate control and live dual-instance validation, then extend proactive speech and shared goals. The current communication implementation must not be marked as completing the whole goal.
