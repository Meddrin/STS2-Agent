# Evidence: success/failure judgment of invite_ai_teammate

## Current state

- `GameActionService.cs:5102-5106`: `message.Contains("失败")||("请先")||("找不到")` (the keywords mean "failed", "please first", "cannot find").
- `AgentRuntime.LaunchDualInstanceAsync(AgentSettings, CancellationToken)` (:519-522) returns a non-generic
  `Task`, with **no result value**; only 3 hits in the whole repo (the definition + two call sites).
- `_dualStatus` assignment points (`AgentRuntime.cs`): :722 in progress, :728 rejected, :735 rejected,
  :742 in progress, :744 `await HostLocalCoopAsync` (success/failure mixed in the same string), :748 canceled, :752 exception.
- `:715-718` gate preemption failure -> silent return, **does not update `_dualStatus`**, and the caller reads the stale text.

## Actual failure messages judged as "success" (`LocalDualInstanceLauncher.cs`)

| Line | Message | Can the existing substring hit it? |
|---|---|---|
| :155 | Inviting the AI teammate, please wait for the connection result. | No ("please wait" is not "please first") |
| :166 | The AI teammate window is already running. ... | No |
| :187 | Cannot find the game executable. | Yes |
| :212 | Cannot compute teammate launch arguments: {0} | No |
| :241 | Cannot configure the teammate settings file path: {0} | No |
| :289 | Failed to start the second instance. Steam may have blocked running two instances: {0} | Yes |
| :306 | The AI teammate process has exited (exit code {0}). | No |
| :307 | The AI teammate process is still running (PID {0}), but the connection could not be confirmed. | No |

25: On an English client `Loc.T` outputs English => none of the three keywords hits => **every failure is reported as success**.

## Coordinator return surface (`DualInstanceCoordinator.cs:14-45`)

- `:16-19` non-MAIN_MENU -> failure text; `:29-33` lobby creation exception -> failure text;
  `:25-28` cancellation -> **rethrow**; `:35-39` `!launch.Ok` -> `launch.Message`;
  `:41` success -> success text (`LocalDualInstanceLauncher.cs:317`).
- A structured type already exists: `DualLaunchResult { bool Ok; string Message; int CompanionPort; int? CompanionPid; }`
  (`LocalDualInstanceLauncher.cs:11-20`), but :38 flattens it into a string.

## Existing test coverage

- `LaunchDualInstance` has 0 hits inside `STS2AIAgent.Tests`; `DualStatus` appears only as fixture text
  (`PlayerExperienceTests.cs:111/306`, `RuntimeExperienceRegressionTests.cs:34`).
- The success/failure judgment of `ExecuteInviteAiTeammateAsync` has **no tests at all**.

## Consumers (their behavior must not be changed this time)

`AgentOverlayHost.cs:1265`; `Router.cs:299` (`dual_status` of /health);
`PlayerFacingSession.cs:19/96/106/260`; `DiagnosticExport.cs:19/92`;
`AgentRuntime.cs:1076/1110`; `docs/api.md:134/156`.
