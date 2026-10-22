# Agent Turn Integrity Fix Record

Historical snapshot: 2026-09-21. Device CharBook, working branch dev. Baseline before the fixes: `00aad146b8bafb27a64aabb806f823f412251998`.

The user asked for all 5 issues confirmed by the second review round to be fixed. Following the earlier instruction, this round did not launch the game, deploy into the game directory, call a real paid model, or push or release. The existing `cstest.log` and `v.log` were kept.

## The Five Fixes

| Issue | Behavior after the change | Main code and tests |
| --- | --- | --- |
| F01: a failed state read after an accepted action caused a resend | The action is recorded as soon as the bridge success receipt arrives; a later state-read failure reports pending / stable=false; this decision no longer submits a second action. A genuine action rejection can still be corrected. | `AgentLoop.ExecuteActAsync` / `CompleteWithToolsAsync`; comparisons for tool calls, chat, JSON fallback, and explicit rejection. |
| F02: the tokens already known in the current turn did not enter the next check | Before each request, history usage plus the current turn's known usage is checked, including the external vision call; the check does not write to the ledger, and the final result is booked only once. | `SessionBudgetGuard.CheckBudget`; 95 + 10 exceeding the limit of 100 blocks the next request; the same block applies after vision. |
| F03: cancellation lost the completed action and usage | The cancellation exception carries the partial receipt, and the recovery code records what already happened before stopping; the run boundary likewise keeps the receipt; provider tokens that were not returned stay unknown. | `AgentTurnCanceledException` / `AutoPlayRecovery` / `AgentRuntime.Accounting`; cancellation after completion, after playing a card, during a later model request, read tools, vision, and the run boundary. |
| F04: proactive speech ran ahead of play accounting and crossed the request limit | The play result and budget are committed first, then proactive speech begins; if the budget is full it stops directly; the shared turn gate is held until accounting completes, so queued requests do not read a stale ledger. | Recovery's `afterTurn` / `turnGate` and the runtime wiring; a limit of 1 request does not start proactive speech, a limit of 2 books one each, proactive speech cancellation, and queued request tests. |
| F05: a non-object action argument got past error handling | The root node is validated as an Object first; `[]` / `null` / numbers / booleans / strings / corrupt JSON return invalid_request, keep the request usage, and let the model correct itself. | `AgentLoop.ParseArgs` / `TryReadActReason`; two test groups for recovery and structured errors. |

## Accounting Boundary

A normal turn is booked from its final result. An interrupted turn is booked from the result carried by the exception. The two paths are mutually exclusive. Auto-play uses the recovery code's unified commit step; the single-step, chat, and teammate paths consume the result before releasing the shared turn lock. An interrupted auto-play only updates the fact record and does not show the old turn as still running.

Proactive speech runs after play is booked, and its own result or cancellation receipt is booked separately once. The current turn's budget check only reads cumulative values and does not write ahead; therefore repeated calls to the check do not double count.

This round added `AgentRuntime.Accounting.cs`, putting the existing session accounting and the UI result projection in one partial; the main file's size budget was lowered from 1400 to 1350. No public HTTP/MCP request fields, release version, or third-party dependencies were changed.

## Regression and Validation

The baseline was 589 C# tests and 315 Python tests. First only 10 permanent regressions were added: 9 failed on the original code, and the normal single-action control passed. Evidence is in `build/agent-turn-integrity-2026-09-21/red.log`.

The tests for the first round of fixes were blocked only by the source manifest check, because the new partial had not been staged yet; after adding it to the index they passed. The extended validation reached 609 and then 612 C# tests, all passing. The final full `preflight-release.ps1` exited 0: C# **615 passed / 0 failed**, Python **315 passed**, Release build **0 warnings / 0 errors**, and all 16 preflight steps and the 12 validation checks within them passed.

This round added **26 C# regression and wiring checks**. Logs are at `build/agent-turn-integrity-2026-09-21/preflight.log`; the machine-readable summary is `verification-summary.json` in the same directory. `tested-source-hashes.json` records the source digest that passed the final validation, checked file by file at wrap-up and found consistent.

The initial failing cases and the final tests together show: a read failure after an action went from executing twice to executing only once; the 95 + 10 token scenario exceeding 100 stops later model requests; cancellation after completion keeps 1 request / 25 tokens; a 1-request limit blocks proactive speech; a non-object argument returns a structured error and keeps the known usage.

Permanent regression entry point: `dotnet run --project STS2AIAgent.Tests/STS2AIAgent.Tests.csproj -c Release`. The specific cases are in `STS2AIAgent.Tests/AgentTurnIntegrityTests.cs`, and each one is registered in TestRunner.

## Limits of the Evidence

The model and game interfaces use controlled fakes, and the queueing/cancellation tests use real Task, CancellationToken, and SemaphoreSlim. The runtime parts that touch Godot are covered by source wiring checks and a real Mod build; this round had no live thread scheduling, visuals, real model usage, or full-run results. Future live validation should focus on the UI and decision log when a pause lands exactly at the end of a card play, the end of a request, or during proactive speech.
