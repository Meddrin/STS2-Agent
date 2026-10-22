# Proactive Speech Gate and Endpoint Contract Acceptance (2026-09-11)

Record date: 2026-09-11. Scope: live acceptance, on an isolated game copy, of the unreleased mainline changes after the v0.10.6 tag: two fixes (the proactive speech volume cap and the run boundary), the endpoint additions to `docs/api.md`, and the correction of stale wording on the status page. Evidence directory: `build/validation-2026-09-11/` (gitignored).

## 1. Fixes in This Round

| Commit | Content | Offline evidence |
| --- | --- | --- |
| deafa23 | The proactive speech cap of 6 is now reissued each time an auto-play session starts (`ProactiveChatSession.BeginSession`, called by `AgentRuntime.StartAutoPlay`); the 75-second interval remains a global constraint across sessions | `ProactiveChat.NewPlaySessionResetsCap` |
| deafa23 | `StartAutoPlay` resets `CurrentRunBoundary`, so "a new run started while paused" belongs to the new session instead of being judged a run identity change | `CurrentRun.FreshSessionAcceptsNewRun` |
| e4c5797 | `docs/api.md` gains `/session/control`, `/events/stream`, `/companion/control`, `/companion/message`, and the `/health` fields and the `stop_kind` value table are filled in | `scripts/check_verification_gates.py` four gates |

C# core tests: **217 PASS / 0 FAIL** (2 added this round).

## 2. Why These Two Things Were Changed

- **The cap was never reset**: `AgentRuntime` holds only one `ProactiveChatSession` for the whole process, and the only call site of `Reset()` is the settings page's "Reset this session's stats". That is, after 6 messages proactive speech stayed permanently silent for the rest of the process, which does not match the literal meaning of "6 per session". It now returns the quota each time an auto-play session starts.
- **The run boundary leaked across sessions**: the class comment of `CurrentRunBoundary` says "scoped to one automatic session", but it is actually a runtime field that is reset only when the auto-play loop observes the main menu. If the previous run ended while paused (for example auto-play stopped at GAME_OVER and the player went back to the main menu by hand), the old boundary stayed behind with the previous run's run_id; starting a new run and then pressing "Start auto-play" made the loop's first iteration throw "run identity change detected", and it kept failing for as long as the player stayed in the run.

Live reproduction (build before the fix, evidence in the first segment of `gate-timeline.jsonl`):

```
{"at": "2026-09-11T20:19:11", "kind": "setup_run", "screen": "EVENT"}
{"at": "2026-09-11T20:19:14", "kind": "setup_autoplay", "play_running": false, "phase": "paused"}
```

`GET /health` reading in the same period: `play_running=false, stop_kind=run_end`. Because `StopKindPolicy` now classifies only the two run-boundary messages as `run_end`, that reading proves it stopped on the boundary check.

The same sequence after the fix (new build):

```
{"at": "2026-09-11T20:21:38", "kind": "setup_autoplay", "play_running": true, "phase": "running"}
```

## 3. Live Acceptance of the Proactive Speech Gate

Method: isolated copy + a local zero-cost OpenAI-compatible stub (`build/validation-2026-09-10/stub-model-server.py`, accounting per request), configuration `settings.proactive-on.json` / `settings.proactive-off.json` (`supportsTools=false`, taking the JSON fallback path); auto-play was started with `POST /session/control {"running":true}`, and the console `fight NIBBITS_WEAK` / `win` produced COMBAT<->MAP transitions. Driver: `build/validation-2026-09-11/proactive-gate-driver.py`.

| Boundary | Result | Evidence |
| --- | --- | --- |
| Negative case with the switch off | Auto-play ran to 50 requests, entering and finishing a combat in the meantime, with **0** proactive messages | `off-ledger.jsonl` (20:14:09-20:14:58), with `/state` being COMBAT and `play_running=true` in the same period |
| Combat start trigger | Triggered, with the prompt `A fight just started…` | `on-ledger2.jsonl` 20:21:40, 20:34:37 |
| Combat end trigger | Triggered 6 times, with the prompt `The fight just ended…` | `on-ledger2.jsonl` 20:23:09 / 20:24:38 / 20:26:06 / 20:27:34 / 20:29:04 / 20:31:21 |
| 75-second minimum interval | 9 send intervals of 88-137 seconds; plus a blocking test: **12.2 seconds** after a send, a forced COMBAT->MAP and MAP->COMBAT transition, followed by 0 sends within 63.4 seconds | the `block_*` segment of `gate-timeline.jsonl` (`refused=true`) |
| Cap of 6 per session | After 6 were full, 0 were sent at the 7th moment (a real transition 84 seconds later, with auto-play still running) | `cap_result` of `gate-timeline.jsonl` (`refused=true`) |
| Pause/resume returns the quota (this round's fix) | After pausing and resuming auto-play, the 7th was accepted | `reset_result` of `gate-timeline.jsonl` (`accepted=true`, 20:31:21) |
| Read-only path | Proactive speech requests go only to the chat model, and `act` is not in its tool set | Offline `ProactiveChat.ReadOnlyCannotAct`; 641 requests in total up to 20:34:37 this round, and proactive speech requests can be told apart from play requests directly by `last_user_head` |

Observation: the loop observes the situation at the start of each iteration, so a send can lag the screen transition by ten to twenty-odd seconds (the `sends_after` read by `cycle` is sometimes one beat earlier than reality). The interval conclusions in the table above are computed from the accounting timestamps and are unaffected by that lag.

## 4. Endpoint Contract and Documentation

`docs/api.md` previously registered only `/health`, `/state`, `/actions/available`, and `/action`, while production code also implemented `POST /session/control` (the same entry point as the floating window's "Start auto-play", and the only programmatic way to start in this round's acceptance), `POST /companion/control`, `POST /companion/message`, and `GET /events/stream`. This round filled them in, and wrote the `/health` field descriptions and the `stop_kind` value table into the contract; example values were synced to the current `0.10.6` / `v0.111.0`.

Status-page wording corrected in the same batch: the card selection screen row after the #82 fix, the pets and orb slots row, and the stop reason classification row; the "Not verified: real in-game behaviour" in `docs/proactive-chat-review.md` was out of date and was changed to the two rounds of live conclusions plus the parts still unverified; the Deck selection row of `docs/mechanic-coverage-matrix.md` gained a #82 correction note (keeping the historical snapshot marker).

## 5. Environment and Real Saves

- Isolated copy: `build/validation-2026-09-08/game/`, deployed DLL SHA256 first 16 digits for this round `1276E878E525D6BE`; profile `--clientId 2026091012`; `STS2_AGENT_SETTINGS_PATH` pointed at this round's `settings.proactive-*.json`.
- Re-check of the real-save protection snapshot (`build/validation-2026-09-10/protected-snapshot.json`, 197 entries): **0 changed, 0 missing**. The real Steam install, the real saves, and the `%APPDATA%/STS2AIAgent` configuration were not touched by this round's isolated copy.
- The official install directory is still v0.10.6's `47CE0F90…`, and this round did not redeploy to the official install.

## 6. Not Covered
## 7. Second Round: Defects Found by the Audit and Their Fixes (Later the Same Day)

Four read-only audits of the HTTP service layer, auto-play stop/resume, overlay text, and the MCP contract found and fixed the following real defects.

| Defect | Trigger and consequence | Fix | Verification |
| --- | --- | --- | --- |
| `POST /action` has no body limit and no JSON tolerance | A non-JSON body or a wrongly typed field (such as passing a string for `card_index`) throws `JsonException`, which the generic catch turned into a **500 `internal_error`**, while the contract requires 400 `invalid_request`; a chunked body would also be read without bound | `Router.RequireBoundedBody` + `ReadJsonBodyAsync`, with both routes going through these two helpers | Live probe: not-json / wrong-type / empty / missing all return HTTP 400 `invalid_request` (previously 500) |
| `POST /session/control` with a non-JSON body -> 500 | Same as above | Same as above | Same as above |
| The stop reason was decided by Chinese keyword matching | The wrapper message for a decision failure was concatenated with the game's raw error string; when the error string contained the Chinese word for "limit", it was reported as `budget`, and the UI suggested "Reset this session's stats" instead of checking the game state | `AutoPlayStoppedException` carries an optional `Kind`; the budget guard and run boundary declare their type explicitly; `StopKindPolicy.Resolve` lets the explicit type win | Offline `StopKind.ExplicitKindWins`; `StopKind.RetryIsNotRunEnd` |
| Budget keyword misjudgment | `Classify` did `Contains("上限")` on the whole string, so an ordinary failure message could hit it | Bare-word matching was removed: a budget stop is produced only by the explicit type from `SessionBudgetGuard` | Same as above (`Classify("手牌已达上限")` is now `failed`) |
| Run boundary reset inside the loop | `AutoPlayLoopAsync` swapped in a fresh `CurrentRunBoundary` every round it saw a menu screen and then immediately called `Check`, with `_enteredRun` false, so **leaving a run never triggered a stop** | Removed the reset inside the loop; the boundary is installed only in `StartAutoPlay` | Live: after `save_and_quit` back to the main menu, `play_running=false`, `stop_kind=run_end` |
| Overlay disk write exceptions escaped | `PersistOverlayVisible` / `PersistOverlayPlacement` / `PersistChatAttachFlags` / `SetMcpEnabled` called `SettingsStore.Save` directly, which rethrows, so on a full or locked disk the exception was thrown into a Godot signal callback and the switch display disagreed with reality | Added `SaveSettingsQuietly`, which catches `IOException` / `UnauthorizedAccessException` and writes to the status line | Code path + existing settings page text |
| "Teammate is acting" was a dead branch | `Phase` has only paused/running/stopping, and running was hit first by `PlayPhase == "running"` in the previous branch, so players only ever saw "requesting model" | That branch was narrowed to `s.RequestingModel` | Offline `Session.RunningBranchReachable` |
| Solo auto-play showed "you can invite an AI teammate" | The ready_to_invite branch was ordered before all running branches and did not check `PlayRunning` | Added `!s.PlayRunning` to the condition | Same as above |
| MCP action timeout shorter than the Mod's wait | `_DEFAULT_ACTION_TIMEOUT = 30s`, while `continue_game_over` waits up to 60s for a native save -> a normal slow path was treated as a lost response | The default was raised to 75s | Offline `test_action_timeout_covers_the_longest_server_wait` |
| A refused connection was reported as outcome_unknown | A refused connection means the request never reached the Mod, yet it was reported as "may have completed, do not replay" | Only `ConnectionRefusedError` / `gaierror` are classed as `connection_error` (retryable); a loss after the request was sent stays uncertain | Offline `test_refused_connection_is_retryable_connection_error`, `test_lost_response_after_send_stays_uncertain` |
| `status="failed"` was treated as success | The contract allows that value, and clients looked only at `ok` | `_decode_action_success` raises `action_failed` for that value | Offline `test_failed_status_is_not_reported_as_success` |
| Documentation gaps | `/data/{collection}` and `/mcp` were never registered; 7 error codes were unregistered; the README's timeout environment variable names did not exist | Added the `GET /data/{collection}` and `POST /mcp` sections and the error code table; the README now says `STS2_API_READ_TIMEOUT` / `STS2_API_ACTION_TIMEOUT` / `STS2_API_MAX_RETRIES` | Four gates passed |
| Tool text pointed at fields that do not exist in the compact view | The `act` description told readers to read `requires_target` / `target_index_space` / `valid_target_indices`, but `agent_view` has only `target` / `targets` (only `rest.options` has the trio) | The text now describes the compact view and states the corresponding names in the full state | Checked against the compact-build code of `GameStateService` |
| The `full` profile lacked 9 action tools | The documentation says full exposes "an independent tool for every action", but of 55 actions only 45 had legacy tools, and the rest could only go through `act` | Added `switch_profile`, `dismiss_game_over_wait`, `confirm_unlock`, `close_cards_view`, four multiplayer-lobby actions, and `invite_ai_teammate`, plus the matching client methods | Offline `test_legacy_action_coverage` (asserting both coverage and client methods) |
| "Single step" did not count toward the session budget | `StepOnceCoreAsync` -> `ApplyPlayResult` only accumulated the display-only `_sessionRequests` and did not feed `SessionBudgetGuard`, so the guard's request count never grew and repeated single steps could exceed `MaxSessionRequests`; the pre-check in `AgentLoop` was ineffective | The single-step path records the turn and checks the budget, stopping with the `budget` type when over the limit and explaining in the status line | Source contract test `Session.StepOnceRecordsBudget` |

Live validation (isolated copy, DLL `315ED550…`, then rebuilt as the final build):

```
action: not json         HTTP 400 ok=False code=invalid_request
action: wrong type       HTTP 400 ok=False code=invalid_request
action: empty            HTTP 400 ok=False code=invalid_request
action: missing field    HTTP 400 ok=False code=invalid_request
action: unknown verb     HTTP 409 ok=False code=invalid_action
session: not json        HTTP 400 ok=False code=invalid_request
session: wrong type      HTTP 400 ok=False code=invalid_request
session: missing field   HTTP 400 ok=False code=invalid_request
data: unknown            HTTP 404 ok=False code=collection_not_found
```

Live run-boundary sequence (the core evidence for this round's fix):

1. `continue_run` -> REWARD (inside a run)
2. `POST /session/control {"running":true}` -> `play_running=true`, `stop_kind=null` (this scenario used to stop immediately with `run_end`)
3. Run `save_and_quit` while auto-play is in progress -> back to MAIN_MENU
4. After 8 seconds: `play_running=false`, `stop_kind=run_end`: the stop check for leaving a run is still effective

Test counts: C# core **219 PASS / 0 FAIL** (3 added this round); MCP **53** passed (4 added this round).

Evidence files: `build/validation-2026-09-11/http-contract-probe.py`, `boundary-ledger.jsonl`, this round's `/health` readings.


- Real model connectivity and speech quality: this round still used the local stub, called no real upstream, and consumed no budget.
- The cross-session semantics of the cap of 6 were verified only within a single process; the actual feel of "does it still give 6 after long multi-run play" was not tested.
- The blocking test relies on the console `fight` / `win` to produce transitions, and does not cover interval behavior under a player's normal pacing.
- `/companion/control` and `/companion/message` were registered this round only from source and existing contract tests, with no new live call evidence (the live frame capture of `/events/stream` is in Section 4).
- Among the second-round fixes, "an error message containing a keyword causing misclassification" was constructed only in offline tests, and such an error message was not produced in a real run.
- The `/mcp` native MCP endpoint was not called live this round; the Origin contract and live probe conclusions of 2026-09-08 stand.
- The audit noted that the `NativeMcpServer` body limit only checks `> 1_000_000` and does not check `ContentLength64 == -1` (chunked): no change was made. The reason is that rejecting `-1` could directly break normal MCP clients that use chunked transfer, while the current exposure is limited to the local loopback; if it is to be tightened, the right way is a bounded read (reject once 1 MB + 1 byte has been read) rather than simply adding a comparison. Recorded for later judgment.
- The audit noted that the `McpPort` / `McpServerPath` settings have no controls in the overlay (written to disk but not shown), and that on save `ThinkingIntensity` is overwritten by the chat model's value: both are low-severity usability issues and were not changed this round.
