# STS2 AI Agent Product Status and Development Plan

> **Historical; does not represent the current state.** Archived on 2026-09-09. The single entry point for current status is [PRODUCT_PLAN_CURRENT.md](../PRODUCT_PLAN_CURRENT.md); [COOP_DELIVERY.md](../COOP_DELIVERY.md) is now only an index of historical evidence. The commit hashes, test counts, and "to be implemented" items below belong to an old point in time; do not use them as a task board.

Assessment date: 2026-09-05. This document is a development proposal; it does not mean any feature has been delivered or constitute a release commitment.

## 1. Conclusion and Product Positioning

The project already has the foundations of a player product: an in-game window, model configuration, auto-play, optional vision, external MCP access, and fairly broad coverage of game state and actions. It is currently best positioned as a "fairly complete Beta that still needs stability work". This judgment is based on the code, public issues, and the validation system, not on user scale or retention data.

The proposed product promise for the next phase: **invite an AI teammate who can chat, cooperate, and has a personality, and explore the Spire together with the player. The player controls their own character and the AI controls the companion character; the two can discuss routes, fight in coordination, share close calls, and review the run afterward.**

The primary users are players who want to play together with an AI; external Agent developers are the second user group. **Local dual-instance launch and human-AI co-op are the Mod's main selling point and the main-flow acceptance that 1.0 must pass.** Solo auto-play stays as an auxiliary mode, and MCP stays as an advanced entry point. Passing solo mode cannot substitute for delivering dual-instance play.

1.0 must first prove that "this is playing together, not watching a bot in another window". It measures team formation success, whether player suggestions enter the teammate's decisions, whether the AI's responses are based on the real situation, co-op experience, pause and takeover, full-run completion, and cost visibility; strategy win rate is evaluated separately.

The product direction was adjusted on 2026-09-05 at the user's request. The priorities below follow this section and the human-AI co-op acceptance in [COOP_DELIVERY.md](../COOP_DELIVERY.md); the original research data keeps its historical time boundary.

## 2. Assessment Baseline and Evidence Boundary

| Item | Actual situation |
| --- | --- |
| Current working directory | `C:/Users/chart/Documents/project/sp`, branch `dev`, commit `f362f73`, v0.9.0 |
| Latest release | GitHub v0.9.2, released 2026-08-31 |
| Planning code baseline | `main`, `71c3e41c0735fae91b6349495941268ed8fa3909`; confirmed via the GitHub API to match the remote main |
| Where main was inspected | The existing `C:/Users/chart/Documents/project/sp-main-workshop` worktree, which was clean at inspection time |
| Game compatibility | The main manifest declares a minimum of v0.111.0, and the Changelog records validation on that version; compatibility with later game versions is not inferred from this |
| Research scope | README, Changelog, existing roadmap and release checklist, core runtime loop, LLM, settings, UI, state/action entry points, MCP retries, tests, packaging scripts, open issues/PRs |
| Not done | Launching or operating the game, real paid model calls, full-run live regression, user interviews, a complete dependency security audit, Workshop subscription install validation |

This pass did not switch branches, merge PRs, modify business code, deploy, or release. Existing untracked files in the working directory were left untouched. The semantic search tool `fast_context_search` was unavailable in this session; the research used file listings and exact searches.

### Validation actually run this time

The following commands were run in the main worktree above:

| Check | Command | Result |
| --- | --- | --- |
| C# standalone core tests | `dotnet run --project STS2AIAgent.Tests/STS2AIAgent.Tests.csproj --configuration Release` | 48 passed, exit code 0 |
| Python unit tests | Run in `mcp_server`: `uv run --locked python -m unittest discover -s tests -v` | 29 passed, exit code 0 |
| MCP profile contract validation | Run in `mcp_server`: `uv run --locked python ../scripts/run_sts2_validation.py mcp-tool-profile` | Failed, exit code 1; the guided expected set omits four existing tools |

The C# test project links some game-independent sources and does not cover the whole Mod, the Godot UI, or `AgentRuntime`. These passing results cannot prove full-run stability and are not a full Mod build result. The tests and live evidence in PR descriptions are contributor reports and were not re-verified this time.

## 3. Existing Capabilities and Product Gaps

| Area | Existing foundation | Next gap |
| --- | --- | --- |
| Player entry | F8/AI tab, settings/chat/play/dual-instance/connect pages | First-use path, capability detection, actionable error explanations, UI language and scaling acceptance |
| Model access | Multiple endpoints, multiple models, tool calls, JSON action fallback, per-model thinking intensity, optional vision | The connectivity test only proves a simple request succeeds; it needs to distinguish chat-capable, tool-capable, and screenshot-capable |
| Auto-play | State first, index checks, one action per decision, pending wait, cancellation token, in-process turn mutex | A clear lifecycle, error classification, bounded retries, no-progress detection, human takeover boundary, and a stop policy after the run ends |
| Human-AI co-op | Dual-instance launch, auto join/ready, local player actions and multiplayer state | Two-way communication between the main window and the AI teammate, shared goals, teammate pause/resume, disconnect recovery, full co-op flow acceptance |
| Game coverage | Combat, map, rewards, shop, events, rest, multiplayer, and more; v0.9.1 added the crystal sphere and unlocks | Native settlement/save correctness, dynamic event descriptions, and complex-scenario regression still have open PRs |
| External access | HTTP, SSE, guided/layered/full MCP; responses already carry request_id | Replay protection when a write result is uncertain, cross-client control ownership, a consistency contract between state and actions |
| Cost and experience | Recent actions, thinking, chat history; parsable SSE responses | No usage field or per-run budget yet; the SSE content is read in full before parsing, with no end-to-end incremental display |
| Settings | Local JSON, multi-model roles, basic migration | The API key is serialized in plaintext with the settings object; a fixed `.tmp` plus an in-process lock cannot coordinate writes from two instances; a load error falls back to defaults and needs explicit notice and a recovery mechanism |
| Delivery | GitHub Release, build scripts, Workshop packaging/VDF/bilingual copy | Not equal to a validated public subscription path; clean-machine install, upgrade/rollback, and channel consistency validation are still needed |
| Engineering assurance | 48 core tests, 29 Python tests, several live validation scripts | No `.github/workflows` seen on main; profile validation has drifted; the old roadmap and release checklist still sit at an early version |

### Key code references

The links below are pinned to the assessed commit, to avoid confusing the current dev with the latest main:

- [AgentRuntime](https://github.com/CharTyr/STS2-Agent/blob/71c3e41c0735fae91b6349495941268ed8fa3909/STS2AIAgent/Agent/AgentRuntime.cs): the auto loop continues after a delay on error; no consecutive-failure threshold or session budget seen.
- [AgentLoop](https://github.com/CharTyr/STS2-Agent/blob/71c3e41c0735fae91b6349495941268ed8fa3909/STS2AIAgent/Agent/AgentLoop.cs): already has at most 8 tool rounds per turn, legal-action checks, and latest-index validation; each PlayOnce rebuilds the messages and lacks an explicit cross-step strategic context.
- [LLM types](https://github.com/CharTyr/STS2-Agent/blob/71c3e41c0735fae91b6349495941268ed8fa3909/STS2AIAgent/Llm/LlmTypes.cs) and [client](https://github.com/CharTyr/STS2-Agent/blob/71c3e41c0735fae91b6349495941268ed8fa3909/STS2AIAgent/Llm/OpenAiCompatibleClient.cs): Completion has no token usage, and streaming responses are parsed after `ReadAsStringAsync`.
- [SettingsStore](https://github.com/CharTyr/STS2-Agent/blob/71c3e41c0735fae91b6349495941268ed8fa3909/STS2AIAgent/Config/SettingsStore.cs): the fixed temp path, copy-overwrite, and error fallback are the grounds for concurrency and recovery work; no configuration corruption was reproduced this time.
- [MCP client](https://github.com/CharTyr/STS2-Agent/blob/71c3e41c0735fae91b6349495941268ed8fa3909/mcp_server/src/sts2_mcp/client.py): the shared request retry loop does not vary the retry count between reads and writes; action parameters only affect the timeout choice, and a connection error has a path that resends a write request.
- [Release preflight](https://github.com/CharTyr/STS2-Agent/blob/71c3e41c0735fae91b6349495941268ed8fa3909/scripts/preflight-release.ps1): already has a multi-file version consistency check; no explicit non-zero exit code check after external commands, so failure injection is needed to verify that errors propagate to the outermost level; success cannot be judged from an "OK" in the output alone.

## 4. Handling Order for Existing Issues/PRs

| Priority | Work | Judgment and acceptance requirements |
| --- | --- | --- |
| P0 | [#55 reserved port range causes initialization failure](https://github.com/CharTyr/STS2-Agent/issues/55) | Directly blocks use. Verify default port in use, consecutive ports reserved, explicit port failure, and multi-instance contention; auto mode should find a bindable port, and explicit mode should give an accurate error |
| P0 | [PR #53 native settlement, unlocks, and saves](https://github.com/CharTyr/STS2-Agent/pull/53) | Touches player progress and write-action retries. Review first, then decide on merging; re-verify normal settlement, crossing an unlock threshold, a full unlock queue, save persistence, and lost action responses, and do not substitute the PR's own description for mainline acceptance |
| P0 | [PR #54 event description localization variables](https://github.com/CharTyr/STS2-Agent/pull/54) | Affects decision information completeness. Review the generic variable injection, and verify the reported events, Chinese and English, and empty fields; an empty description caused by a read failure must not be treated as complete state |
| P0 | MCP profile validation drift, preflight exit codes, and a minimal CI | Restore a trustworthy release gate; failure must return non-zero. The contract tests must catch both omitted and extra or over-privileged tools |
| P1, promoted to P0 if exploitable | Dependency reports [#50](https://github.com/CharTyr/STS2-Agent/issues/50) and [#51](https://github.com/CharTyr/STS2-Agent/issues/51) | Verify the locked versions, upstream advisories, whether they enter the release package, and the actual call paths, then upgrade and regress. This pass only confirmed that the reports exist and did not verify their vulnerability counts, severity, or exploitability; do not directly apply the example patches in the reports |

This work should reuse existing contributions and avoid rewriting the same features. Version numbers are decided at release time; the numbers below are only suggested milestones.

## 5. Phased Roadmap

Scheduling assumptions: one main developer familiar with the repository, with roughly half-time live QA/maintenance help throughout, and access to a legitimate game installation and a limited model test budget. Estimated 8-12 weeks; game version changes, PR rework, and staffing shortfalls will extend it. Phases advance on acceptance results, and gates are not skipped to meet dates.

### M0: Restore a Trustworthy Baseline, suggested v0.9.3, 1-2 weeks

Delivery goal: the game launches reliably, settlement completes fully, and release failures are not hidden.

1. Establish a clear baseline of developing from the latest main, schedule a dev sync, and keep the existing worktrees and user changes. This pass does not run the sync.
2. Handle #55, and decide on merging #53/#54 after review and re-verification.
3. Fix profile validation; add an exit code propagation check under Windows PowerShell.
4. Add a game-independent CI: C# core tests, Python unittest, MCP profile, version/manifest consistency; the full Mod build and live acceptance go on a controlled runner with game dependencies or on the maintainer's machine, to avoid committing game DLLs into the repository.
5. Check #50/#51; update stale version notes, test commands, and acceptance checklists. Key product plan and release documents should be under version control, so new documents are not lost to the `docs/` ignore rule.

Exit criteria: the blocking issues above are closed with evidence or have a clear scope note; CI is all green; preflight exits non-zero when a test failure is deliberately injected; at least one live evidence chain for normal settlement and one for crossing an unlock threshold is complete.

### M1: An AI Teammate You Can Talk To and Control, suggested v0.10.0, 2-3 weeks

Delivery goal: the player invites, contacts, and pauses the AI teammate from their own game window; suggestions enter the teammate's next decision, the teammate can explain choices and respond to the player, and never takes over the player's own character.

Core interaction: an "AI Teammate" page in the main window, teammate connection status, tactical chat, quick suggestions, brief proactive teammate remarks, and pause/resume. Communication binds to the real teammate instance of this launch, avoiding sending commands to another run. Chat still works while paused; a waiting state is shown while a model request is in flight.

1. Add explicit auto-play states: idle, reading state, requesting model, executing action, waiting for game, paused, needs manual handling, run ended.
2. Build an error classification: credential/model configuration errors stop immediately; transient network/rate-limit errors back off a bounded number of times; stop once UNKNOWN or no-progress reaches a threshold; when an action result is uncertain, read the state first and do not resend blindly.
3. Add run control: current character, current run/new run choice, run the current run only, pause, single step, manual takeover. Do not by default endlessly start new runs after settlement.
4. Extend the existing in-process mutex into server-side unified action coordination: when the built-in Agent, MCP, and other API clients compete for control, return a clear status; manual operation invalidates old decisions.
5. Add a minimal session record: session/decision/request IDs, screen, model, action, before/after state summary, duration, result, and pause reason; reuse the existing request_id instead of building another unrelated log.

Exit criteria: network loss, 401/429/5xx, no-tool actions, consecutive UNKNOWN, pending timeout, lost response, pause/resume, and external client contention each have an automated scenario; no new write action is dispatched after a pause. Actions already submitted to the game are allowed to settle, and the UI must show this honestly. Failures in long live sessions can be exported and traced to a specific decision.

### M2: First Use and Cost Control, suggested v0.11.0, 2-3 weeks

Delivery goal: a non-developer can complete their first play session independently and knows who the data is sent to and how much was used.

1. Add a first-run guide: check Mod/game version -> enter endpoint -> choose model -> test tool/vision capability as needed -> perform one controlled single step -> start the current run.
2. Upgrade the simple connectivity test into itemized diagnostics; keep the already implemented multi-endpoint support, cover the tested compatible types first, and do not promise that every OpenAI-compatible implementation behaves identically.
3. Add token usage, request counts, duration, and a session budget. Cost is estimated from the user-configured unit price and clearly marked as an estimate; show unknown when the server returns no usage. When cost cannot be computed reliably, use request count, token cap, and run duration as actionable stop conditions.
4. Implement end-to-end incremental state/text display and cancellation; send screenshots on demand or on scene change, and record the extra vision call cost.
5. Settings schema version, backup, and recoverable migration; use cross-process coordination and unique temp files/reliable replacement. Separate credentials from ordinary settings, preferring the platform's credential store; diagnostic export removes keys, authorization headers, and sensitive input by default.
6. Make "AI Teammate" the main page, showing the team formation stage, teammate role, current intent, team chat, and recovery button; put solo play-for-me and MCP in auxiliary/advanced entry points. Add communication styles such as relaxed partner and steady advisor, plus a proactive speech frequency, avoiding chatter on every card. Move the Chinese UI strings into resources, and validate Chinese and English, common resolutions, and scaling.

Exit criteria: invite 5-8 players who did not take part in development to test with their own model credentials; the goal is at least 80% completing the first legal action within 10 minutes. Endpoint failures are recorded separately with reasons, and failed samples are not quietly excluded. No new requests are sent once the budget is reached; settings upgrade, corruption recovery, two-instance saving, and redacted export all pass scenario validation.

### M3: Build a Strategy Quality Loop, 1.0 Beta/RC, 2-3 weeks

Delivery goal: be able to explain with evidence whether the strategy improved and whether the cost is worth it, instead of relying on a few demo recordings.

1. Fixed evaluation records: game/Mod version, character, difficulty, seed, model identifier, prompt version, parameters, and vision switch. Model backends may still be non-deterministic, so compare results by group and across multiple runs.
2. Build two evaluation layers: state fixture replay validates legality, contracts, and recovery; live fixed scenarios and run tests validate real actions, flow, and strategy. Offline replay does not claim to fully simulate the game.
3. First build 10-20 high-frequency/known-failure scenarios; accumulate at least 30 end-to-end sessions covering combat, rewards, events, shop, special choices, death settlement, unlocks, and continuing a save. 30 is only the RC sample threshold and gives no statistical guarantee of reliability.
4. Add a lightweight cross-step team summary: the player's current suggestions, both decks' direction, shared route goals, resource budget, and key observations; action legality is always judged against the latest state. Have the AI give brief, grounded remarks at route disagreements, when support is needed, and after key fights, without inventing teammate behavior.
5. Revise prompts and metadata based on failure classification, prioritizing fewer missing-information and wrong-decision cases; decide whether a combat planner is needed only after evaluation. Do not introduce multi-Agent orchestration, vector stores, or training pipelines for now.

Exit criteria: the new strategy shows no significant legality/flow regression against the baseline; win rate, floors, duration, request count, and tokens are recorded under the same experimental conditions; every quality improvement has a re-runnable sample. Model selection does not rely on untested rankings.

### M4: Stable Delivery, v1.0.0, 1-2 weeks

Delivery goal: users can install, upgrade, and recover from errors, and maintainers can reproduce problems and ship fixes.

1. Align GitHub and Workshop on version, features, and dependency notes; complete private subscription, clean install, upgrade, and rollback drills, and check for duplicate-install conflicts. Reuse the existing packaging capability directly.
2. Define a support matrix: officially support the validated Windows/game version/local dual-instance co-op flow first; list macOS/Linux as supported, experimental, or unverified based on real testing. A Windows dual-instance failure is a main-flow defect and cannot be downgraded to an experimental label to bypass acceptance.
3. Provide compatibility status, release notes, an issue report template, and a redacted diagnostic bundle; if update checks need the network they should be configurable, and no account system needs to be built.
4. Put recovery steps for common faults inside the product; keep user-controlled entry points for screenshot sending, pausing, and run budget.
5. Observe one RC cycle: suggested 7 days with no unresolved P0 before publishing the stable release; re-validate the relevant scenarios after a breaking game update.

Exit criteria: meets the release thresholds in Section 6, and problems can be located from a user's diagnostic bundle, so ordinary players no longer need to install development tools to report an error.

## 6. 1.0 Acceptance Metrics

The following are suggested targets; there is currently not enough data to show they have been reached.

| Metric | Definition and suggested threshold |
| --- | --- |
| First success | At least 80% of 5-8 new users form a team with the AI within 10 minutes and complete the first co-op fight; record every failure reason |
| Flow reliability | At least 30 end-to-end human-AI co-op sessions, with a target of >=95% reaching a natural end and completing both sides' settlement; a normal loss counts as flow completion, while a software hang, erroneous stop, or missing settlement counts as failure |
| Real and effective interaction | Evidence that player messages reach the right teammate and enter later decisions, and that AI responses return to the main window; tactical suggestions must not turn into automatic operation of the human's character; chat works during a pause |
| Fun of playing together | New-user test interviews record "teammate or tool", and concrete moments of fun or interruption; the goal is at least 80% willing to team up with this AI teammate again, with the sample size recorded; model win rate is not a substitute for fun |
| Takeover | In every pause/cancel scenario, confirm that no new write action is dispatched after the pause; actions already submitted settle normally and are announced |
| Write safety | Lost-response/timeout cases do not execute twice; stale decisions are rejected or refreshed; no known data corruption in save/unlock regression |
| Cost | Every model call records usage or explicitly "unknown"; no new requests are sent after the budget triggers; cost estimates are not confused with actual bills |
| Diagnostics | Every anomaly in testing can be linked to a session and decision; diagnostic export contains no keys or authorization headers |
| Strategy quality | Report win rate, average floors, requests/tokens per run, and duration under a fixed model/character/difficulty/seed set; build a baseline first and do not preset an unjustified absolute win rate |
| Delivery | Clean install, upgrade, and rollback drills complete; CI all green; the declared supported platforms have live evidence; no unresolved P0 |

Do not advertise a "95% product reliability rate" on a small sample; the targets above are for internal RC decisions, and any public description must carry the sample size and test conditions. Fault injection tests and normal run samples are counted separately to avoid mixing up the metrics.

## 7. First Batch of Tasks to Split Directly into Issues

| ID | Priority | Deliverable | Main location | Dependencies and acceptance |
| --- | --- | --- | --- | --- |
| R01 | P0 | Fix the reserved port range and automatic port selection | `Server/HttpServer.cs`, startup tests | Corresponds to #55; includes bind contention and explicit port failure |
| R02 | P0 | Native settlement/save and write-request uncertainty | `GameActionService`, MCP client | Review #53; re-verify complete progress saving and no replay |
| R03 | P0 | Event option dynamic variables | `GameStateService` | Review #54; multi-event and Chinese/English regression |
| R04 | P0 | Profile contract, failure propagation, and a minimal CI | `scripts/`, `.github/workflows/` | No prerequisite; failure injection can block a release |
| R05 | P1 | Lifecycle, error classification, and circuit-break pause | `AgentRuntime`, `AgentLoop` | After R02, clarify execution result semantics; the failure matrix passes |
| R06 | P1 | Control ownership and state consistency | Server, Game bridge, action entry points | R02/R05; cross-client contention and manual intervention tests |
| R07 | P1 | Session records and redacted diagnostic export | Agent, LLM, UI | Same period as R05; linked to the existing request_id |
| R08 | P1 | Configuration recovery and credential storage | `Config/`, settings UI | Two-instance saving, corruption recovery, and old-version migration |
| R09 | P1 | Model capability detection and first-run co-op guide | LLM, UI, Multiplayer | R05/R08; acceptance by new-user dual-instance testing |
| C01 | P0 | Two-way communication from the main window to the AI teammate, and pause/resume | Multiplayer, Router, Agent, UI | R01; matches this launch's teammate process, with messages isolated from game write actions |
| C02 | P1 | Player suggestions enter decisions, AI proactive responses | AgentLoop, team chat context | C01; live proof that suggestions take effect and that only the AI's own character is controlled |
| C03 | P1 | Teammate style, shared goals, post-fight interaction | Settings, prompts, UI | C02/R12; fun interviews and low-interruption acceptance |
| C04 | P0 | Full dual-instance acceptance and disconnect recovery | Multiplayer, live validation | C01/C02; cannot be replaced by solo testing |
| R10 | P1 | Tokens, budget, and incremental display | LLM, Agent, UI | R07; tests for missing usage, cancellation, and budget boundaries |
| R11 | P1 | Dependency report verification and fixes | Lock files, packaging flow | #50/#51; set severity after confirming the real release surface |
| R12 | P1 | Scenario fixtures and live quality baseline | Tests, validation scripts, evaluation docs | R02/R03/R07; re-runnable and records version conditions |
| R13 | P2 | Run summaries and strategy improvement | Agent, prompts | After R12, decide strategy complexity from data |
| R14 | P1 | Dual-channel install/upgrade/rollback acceptance | Packaging scripts, Workshop, docs | R04 and RC; the support matrix has evidence |

For the first two iterations, put roughly 60% into stability and validation, 25% into user operation and diagnostics, and 15% into quality evaluation preparation; this does not mean starting all tasks at once in those proportions.

## 8. Architecture Evolution Principles

On current main, `GameStateService.cs` is about 7004 lines, `GameActionService.cs` about 4976 lines, and `AgentOverlayHost.cs` about 1156 lines. The size shows that maintenance cost deserves attention, but this is not grounds to start a full rewrite.

- While fixing and adding contract tests, gradually extract screen handling logic, covering the real change hotspots first: settlement/unlocks, events, and combat; keep the existing HTTP interface as a compatibility entry point.
- Extract the lifecycle/recovery policy that can be separated from Godot into independent units, so the existing core tests can cover run management; the Godot adapter remains responsible for game-thread calls.
- Gradually unify the contract across action parameters, availability, state metadata, and tool descriptions, first using cross-layer tests to prevent drift, then deciding whether to auto-generate some descriptions.
- Design backward-compatible fields for state version/signature and action correlation information; the existing request_id is a tracing identifier and is not used directly as a deduplication key. Keep old clients working and enable stronger validation only for new clients.
- Do not change the existing boundaries such as "actions must run on the GameThread", "state transitions wait with a timeout", and "new fields are backward compatible".

## 9. Deferred Items and Follow-up Decisions

1. Defer cloud hosting, account/subscription billing, a community model marketplace, and a complex plugin platform; current user value does not depend on them.
2. Defer large-scale multi-Agent orchestration, training/fine-tuning, and complex long-term memory; first prove the effect of basic state quality, recovery mechanisms, and lightweight summaries.
3. Dual-instance co-op is the main line; first solve process launch, settings contention, control ownership, and the full co-op flow. What is deferred is cross-internet hosting and complex voice systems, not the experience of a human and an AI playing together.
4. Defer large numbers of low-frequency action extensions; prioritize by user failure records and scenario coverage gaps.

Staffing, test budget, and the target model will affect the schedule; the main-line order is reliable team formation -> two-way communication and coordination -> richer teammate personality -> stable end-to-end dual-instance delivery. The original 8-12 week estimate needs to be re-estimated against the dual-instance live results, and the human-AI co-op goal is not reduced to keep the original schedule.
