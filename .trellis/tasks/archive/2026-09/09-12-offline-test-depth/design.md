# Design: what can be tested and how

## 1. `STS2AIAgent.Tests/ApiExceptionTests.cs`

Compile `..\STS2AIAgent\Server\ApiException.cs` into the test project and assert (against `ApiException.cs:5-20`):

- `new ApiException(409, "invalid_action", "msg", details)` -> `StatusCode==409`, `Code=="invalid_action"`, `Message=="msg"`;
- `Retryable` defaults to `false` and is `true` when `retryable: true` is explicit;
- `Details` allows `null` (the actual signature of the constructor overload is per the source);
- it is a subclass of `Exception` (`Assert.IsAssignableFrom<Exception>`).

## 2. `STS2AIAgent.Tests/JsonHelperTests.cs`

Compile `..\STS2AIAgent\Server\JsonHelper.cs` into the test project:

- Build your own POCO (`PascalCase` properties); the serialized text contains `"PascalName"` and the prefix `{\n  `, proving
  `PropertyNamingPolicy = null` + `WriteIndented = true`;
- deserializing `{"pascalname":...}` (all-lowercase key) can fetch the value, proving case insensitivity;
- assertions must be falsifiable: change `WriteIndented` to `false` or use a camelCase policy, and the test must go red.

If the method signatures of `JsonHelper` are generic `Serialize<T>`/`Deserialize<T>`, call them per the actual signature;
any assumption that does not match the source is fixed by changing the test, not the production code.

## 3. `STS2AIAgent.Tests/HttpServerPortPolicyTests.cs` (source contract)

Use `AgentSourceFixture.Read("Server/HttpServer.cs")` + `MethodBody(...)`:

- the `Start`/`StartAsync` path has a write of `PortWasAutoIncremented`, and the write condition includes a port comparison;
- `IsExplicitPortConfigured` (:173-179) reads the `STS2_API_PORT` environment variable;
- `AllowFallback` (:166-171) and `ResolvePreferredPort` (:210-221) exist and are called by `Start`;
- write the assertions in the "delete this line and it goes red" form (for example assert that the fragment after `WithoutWhitespace` contains
  `PortWasAutoIncremented=started.Port!=preferredPort`).

The real port behavior of `LoopbackListener` is already covered by `LoopbackListenerTests` (7 tests) and is not repeated.

## 4. `docs/api.md`

- First verify with `rg -n 'BUNDLE_SELECTION|CAPSTONE_SELECTION|TIMELINE' docs/api.md`;
- if missing, add them to the screen list in the repo's existing format (keeping the original table style and field order);
- the 55 action names inside the `<!-- BEGIN ACTION CONTRACT -->` block stay verbatim unchanged (a gate checks this); run the gate after changing to confirm.

## Debt record (write into research and evidence)

`HttpServer.cs` / `GameEventService.cs` / `UiFactory.cs` / `Router.cs` currently **cannot** be compiled offline,
because of Godot node instantiation, `MegaCrit...Log`, and the game dependency closure that `Router` drags in, respectively;
`GameThread.cs` must first decouple logging before the `InvokeAsync` contract can be split out.
