# Make invite_ai_teammate report the real launch outcome

## Background

`POST /action {"action":"invite_ai_teammate"}` uses a **Chinese substring** to judge whether the invite succeeded
(`STS2AIAgent/Game/GameActionService.cs:5102-5106`):

```csharp
var message = AgentRuntime.Instance.DualStatus ?? string.Empty;
var failed = message.Contains("失败") || message.Contains("请先") || message.Contains("找不到");
```

`DualStatus` is display text produced by `Loc.T(...)`, which turns entirely English on an English client => the check is always false.
Even on a Chinese client, most failure messages of `LocalDualInstanceLauncher` ("cannot compute teammate launch arguments",
"the AI teammate process has exited", "the AI teammate process is still running ... but the connection could not be confirmed", "inviting the AI teammate, please wait for the connection result")
also do not contain these three keywords (the keywords mean "failed", "please first", "cannot find"). The result is that **a failure is returned as success** with `status:"completed"`, and callers (the in-game Agent loop,
MCP clients) conclude from it that the teammate is in place.

Another silent path: when `_dualLaunchGate.WaitAsync(0)` fails to preempt at `AgentRuntime.cs:715-718` it returns right away,
updating neither `_dualStatus` nor reporting anything, so the caller reads the **previous** stale text and judges from that.

## Goals

1. `AgentRuntime` exposes a structured launch outcome, and callers no longer parse display text.
2. `invite_ai_teammate` returns `completed` only when the launch really succeeded; rejected/failed/canceled return 409
   `invite_failed`; still in progress returns `status:"pending"` (`stable:false`).
3. The display text `DualStatus` and all its consumers (UI, `/health.dual_status`, `PlayerFacingSession`,
   `DiagnosticExport`) behave unchanged.

## Acceptance criteria

- [x] The outcome enum and pure policy live in a file that **compiles offline** and are added to the `STS2AIAgent.Tests` compile list.
- [x] Every branch of `LaunchDualInstanceCoreAsync` writes the outcome explicitly; a gate preemption failure no longer reuses the old outcome.
- [x] `GameActionService.cs` **no longer contains** substring matching on `DualStatus` (with a falsifiable source contract assertion).
- [x] New behavioral tests cover: the outcome classification truth table (every enum value asserted), language independence of the text, classification of cancellation and exception paths.
- [x] `DualInstanceCoordinator` exposes a structured outcome; the signature and returned text of the original `HostLocalCoopAsync` are unchanged verbatim.
- [x] Full gates pass: C# unit tests, MCP unit tests, `check_verification_gates.py`,
      `check_release_package.py`, `preflight-release.ps1`.

## Out of scope

- Do not change any `Loc.T` text, nor the `/health` field names and UI presentation.
- Do not change MCP-side tool names/parameters (the 409 semantics already express failure sufficiently).
- Do not touch the precheck logic of `CoopLaunchPolicy.GetError` (it is already structured).
