# Evidence

## Deliverables

| File | Change |
|---|---|
| `STS2AIAgent/Agent/NoProgressPolicy.cs` (new) | `RepeatThreshold=3`, `UnsettledLimit=5`, `IsRepeat(...)`, `Fingerprint(...)` (SHA-256 truncated to 16 digits, pure BCL) |
| `STS2AIAgent/Agent/AutoPlayRecovery.cs` | Added `_lastAction/_lastFingerprint/_repeats/_unsettled`; `Observe` gains the "repeat without progress" and "unconfirmed" branches; `RunAsync` untouched |
| `STS2AIAgent/Agent/AgentLoop.cs` | `ExecuteActAsync` return adds the fingerprint and the unconfirmed flag; a pending result that has not settled **no longer** returns an error |
| `STS2AIAgent/Agent/IGameBridge.cs` | `AgentTurnResult` gains `ExecutedUnsettled` and `StateFingerprint` |
| `STS2AIAgent/Localization/Loc.Strings.Support.cs` | English entries for the 2 new halt messages (otherwise `Loc.Coverage` goes red) |
| Tests | `NoProgressPolicyTests.cs` (new, 3 tests) + `AutoPlayRecoveryTests.cs` (7 behavioral tests + 1 shape contract appended) |

## State machine (`AutoPlayRecovery.Observe`, top to bottom)

| Input | Behavior |
|---|---|
| `RequiresConfiguration` | Unchanged: stop immediately (config) |
| `WaitingForGame` | Unchanged: 1s delay, touches no counter |
| Success but unconfirmed (`ExecutedUnsettled`) | `_unsettled++`, does **not** touch `_failures`; halt at >= 5; otherwise back off |
| Success and settled | Reset `_unsettled`/`_failures`; the same (action, fingerprint) repeating -> `_repeats++`, halt at >= 3; otherwise 1s delay; real progress has zero delay |
| `Error != null` / no action | Unchanged: `_failures++`, halt at >= 3 |

## Acceptance

| Criterion | Evidence |
|---|---|
| Spinning is detectable | `PASS NoProgressPolicy.IsRepeatTruthTable / Thresholds / Fingerprint`; `PASS Recovery.UnchangedActionStops` |
| Real progress is not falsely flagged | `PASS Recovery.ProgressResetsRepeat` |
| Slow != failure | `PASS Recovery.UnsettledBudget / UnsettledRunCleared / PendingReturnShape` |
| Unconfirmed is bounded | `PASS Recovery.UnsettledLimit` |
| Existing semantics unchanged | `PASS Recovery.UnsettledKeepsFailures` and the 4 existing `Recovery.*`, `Loc.*` stay green |
| Falsifiable | Short-circuiting `_repeats >= RepeatThreshold` -> only `Recovery.UnchangedActionStops` red; routing the unconfirmed branch to the failure channel -> 4 red; both restored |

## Gates

C# 335 PASS / 0 FAIL; `dotnet build` 0 warnings 0 errors; MCP 167 OK; gates / package / preflight all green.

## Behavior change (good to know)

A pending result is now treated as "executed", so `PlayOnceAsync(stopAfterAct)` ends right away in that turn and no longer lets the model `act` a second time in the same turn.

## Live-only verification

Whether a real slow settlement really triggers `ExecutedUnsettled`; whether the compact fingerprint is stable in a real run (if `agent_view` contains volatile fields it will miss detections);
the feel of the 5-attempt cap and the `2^n` backoff; the end-to-end chain of the pending branch.
