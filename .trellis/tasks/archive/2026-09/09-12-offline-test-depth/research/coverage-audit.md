# Evidence: compile boundary of the test project

## Facts

- `STS2AIAgent.Tests.csproj` has **no** `<Reference>`/`<PackageReference>`: a pure BCL project,
  all `Godot`/`MegaCrit.*` symbols are unavailable; `internal` types can be tested directly (the source is compiled into the same assembly).
- Existing capabilities: `AgentSourceFixture` (reads source for contract assertions, :7-40);
  `McpServiceTests` can start a real loopback service with `LoopbackListener.Start(49170, true)`
  (:213 / `:272-322`), but it drives `NativeMcpServer`, not `Router`.

## Per-file conclusions

| File | Lines | Godot | Game assembly | Conclusion |
|---|---|---|---|---|
| `Server/ApiException.cs` | 21 | None | None | Compilable and testable |
| `Server/JsonHelper.cs` | 24 | None | None | Compilable and testable |
| `Server/HttpServer.cs` | 223 | No direct | `Log` (:40/55/94/106/115/124/156/195) | Not compilable (also references `Router`) |
| `Server/GameEventService.cs` | 417 | Indirect | `Log` + `GameThread`/`GameStatePayload` | Not compilable |
| `Ui/UiFactory.cs` | 179 | Direct (factory methods new nodes + `Godot.Key`) | None | Not compilable |
| `Game/GameThread.cs` | 171 | Only `WaitForNextFrame*` (:139-170) | `Log` (:27/31/54/61/112) | The `InvokeAsync` contract itself has no Godot, but is blocked by `Log` |
| `Server/Router.cs` | 466 | - | `MegaCrit...Debug/Logging` + the whole Game/Agent/Multiplayer stack | Not compilable |

The four contracts of `GameThread.InvokeAsync` (throws `InvalidOperationException` when uninitialized;
returns an already-completed Task inline when the current thread == the captured thread; goes through `_syncContext.Post` across threads; exceptions propagate back)
can be tested offline only after replacing `MegaCrit...Log` with an injectable delegate; not done this round, recorded as debt.

## Responsibility boundary between `HttpServer` and `LoopbackListener`

`LoopbackListener` does only pure port selection/binding/error classification (the file header comment :6-8 says plainly "port selection independent
of Godot"), and is covered by 7 tests; `HttpServer` handles the environment-variable preferred port, whether fallback is allowed, lifecycle,
the auto-increment marker, and companion port publication, with binding delegated to `LoopbackListener.Start` (:44-51).
So this round makes only source contract assertions on `HttpServer`.

## The screen list in `docs/api.md`

Audit report: the screen table lacks `BUNDLE_SELECTION`/`CAPSTONE_SELECTION`/`TIMELINE` (never registered historically).
Verify before implementing; the action contract block (55 action names) is checked by `check_verification_gates.py` and must not be changed.
