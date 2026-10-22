# Add offline tests for zero-coverage C# core pieces, and complete the screen list in the docs

## Background

`STS2AIAgent.Tests` is a pure BCL project (no `<Reference>`/`<PackageReference>`) and can only compile source that does not depend on
Godot / MegaCrit. This round's probes checked the dependencies file by file and reached clear conclusions:

| File | Offline testable? | Basis |
|---|---|---|
| `Server/ApiException.cs` | **Yes** (zero dependencies) | Uses only BCL, `internal` is no obstacle (the source is compiled directly into the test assembly) |
| `Server/JsonHelper.cs` | **Yes** (zero dependencies) | Uses only `System.Text.Json` |
| `Server/HttpServer.cs` | Cannot compile | `MegaCrit...Log` + depends on `Router.HandleAsync/WriteErrorAsync` -> `Router` drags in the whole game dependency closure |
| `Server/GameEventService.cs` | Cannot compile | `Log` + `GameThread`/`GameStatePayload` |
| `Ui/UiFactory.cs` | Cannot compile | The whole file instantiates Godot nodes |
| `Game/GameThread.cs` | Only the `InvokeAsync` contract has no Godot | But that part still calls `MegaCrit...Log` (:27/31/54/61/112); splitting the file requires replacing the log calls first |

So the offline coverage that can really land this round is two **behavioral tests** for `ApiException` and `JsonHelper`,
plus a **source contract test** for the `HttpServer` port policy (a weak assertion, but it can prevent regressions like "an explicit port silently drifting").

Also: per the audit the screen table in `docs/api.md` lacks `BUNDLE_SELECTION`/`CAPSTONE_SELECTION`/`TIMELINE`
(a historical leftover; the previous round already added these screen names to `/state`).

## Goals

1. `ApiException`: assert the propagation of `StatusCode`/`Code`/`Details`/`Retryable` and `Message`.
2. `JsonHelper`: assert that serialization keeps PascalCase, indentation is on, and deserialization is case-insensitive.
3. `HttpServer`: source contract assertions that "an explicit port does not drift, a marker is set when auto-incrementing, and the branch allowing fallback exists".
4. `docs/api.md`: add the missing screen names (verify first, change only if really missing), and keep the action contract block (55 action names) untouched.
5. Write "which files cannot be tested offline and why" into `research` and the evidence to avoid re-evaluating next round.

## Acceptance criteria

- [x] Two new test files + the `<Compile Include>` of `STS2AIAgent.Tests.csproj` + registration in `TestRunner.cs`.
- [x] Assertions are falsifiable: temporarily breaking the behavior under test (for example turning off `WriteIndented` of `JsonHelper`) must turn the test red.
- [x] The `docs/api.md` screen list is completed, and the action contract block is verbatim untouched (`check_verification_gates.py` checks it).
- [x] Full gates pass (C# unit tests / MCP unit tests / gates / package / preflight).

## Out of scope

- **Do not** refactor the split of `GameThread`: it would first need to replace the `MegaCrit...Log` calls on the `InvokeAsync` path with an injectable
  log delegate, which is a behavior-visible change, and the benefit (4 contracts) is not enough to take that risk this round; recorded as known debt.
- Do not write end-to-end tests for the `Router` error envelope (`Router` drags in the game dependency closure; the loopback
  service started by `McpServiceTests` drives only `NativeMcpServer`).
- Do not change any production behavior; only add tests and docs.
