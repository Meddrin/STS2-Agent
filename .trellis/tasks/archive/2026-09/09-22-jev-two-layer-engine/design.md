# Design: the Jev two-layer decision engine

## Architecture overview

```
┌────────────────────────────────────────────────────────┐
│ Planning layer (LLM, low frequency)                    │
│  In-game: StrategyPlanner (read-only ILlmClient)       │
│  External: coding agent via MCP get_planner_briefing / │
│          update_play_strategy                          │
│  Both write the same StrategyStore                     │
└──────────────────────┬─────────────────────────────────┘
                       │ PlayStrategy (posture/instructions/hints)
                       ▼
┌────────────────────────────────────────────────────────┐
│ Execution layer (Jev, every move)                      │
│  AgentLoop.PlayOnceAsync branch:                       │
│   GetActionSnapshotJsonAsync (same-frame state+actions)│
│   -> JevOptionEnumerator expands concrete options      │
│   -> JevExecutionDecider.DecideAsync (IJevClient)      │
│   -> conf>=threshold ? ExecuteActAsync : LLM fallback  │
└────────────────────────────────────────────────────────┘
```

## New types (all new files, respecting the line count ratchet)

### Llm layer
- `Llm/JevTypes.cs`: DTOs. Request `{state, model, questions{id:{type,instructions,criteria}}}`; response `{model, answers{id:{type, choice|score|noul, probabilities, confidence}}, usage{input_tokens, output_tokens}}`. System.Text.Json serialization, pure DTOs that can be unit tested.
- `Llm/IJevClient.cs`: `Task<JevResponse> SystemOneAsync(JevRequest, CancellationToken)` + `Task<string> PingAsync(CancellationToken)` (lists models via `/v1/models` to verify the key).
- `Llm/JevClient.cs`: the implementation. Owns its `HttpClient` (`DefaultLlmClientFactory.SharedHttp` is private static, so reuse is not forced); explicit `Timeout`; on 429 reads `retry-after` and backs off and retries (bounded count); errors map to `JevException{Kind}` (config/network), aligned with `ModelRoleProbe.FailureKind`.

### Agent layer
- `Agent/IActionDecider.cs`: `Task<ExecutionDecision> DecideAsync(string snapshotJson, PlayStrategy strategy, CancellationToken ct)`.
- `Agent/ExecutionDecision.cs`: record `{Action, CardIndex, TargetIndex, OptionIndex, X, Y, Tool, Reason, Confidence, Probabilities, Usage, RequestsSpent, Error}`, with fields aligned to `AgentTurnResult` for easy backfill.
- `Agent/JevOptionEnumerator.cs`: pure logic. Parse the action snapshot JSON -> `List<JevOption>{Id, Description, Action, CardIndex, TargetIndex, OptionIndex, X, Y, Tool}`. Expanded per screen:
  - COMBAT: `play_card` x hand x legal targets, `end_turn`, potions;
  - MAP: `choose_map_node` x `map.options`;
  - REWARD/CARD_SELECTION/SHOP/REST/CHEST/EVENT/CAPSTONE/BUNDLE etc.: expanded by the `requires_index/requires_target` of `available_actions` and the matching list in state;
  - Fallback: one item for each parameterless action. Beyond 255 items truncate by priority (combat card plays first).
- `Agent/JevExecutionDecider.cs`: the default `IActionDecider` implementation. Builds a Choice (criteria = option Id -> Description, instructions in facets: goal / current screen playbook `PlayPrompt.PlaybookGuidance(screen)` / strategy / legality reminders) + an optional Score (danger). Calls `IJevClient` and maps the choice back to an `ExecutionDecision`.
- `Agent/PlayStrategy.cs`: record `{Posture, Instructions, OptionHints, UpdatedAt, Source}`; `PlayStrategy.Default` is built in.
- `Agent/StrategyStore.cs`: thread-safe holder of the current strategy (`Current`/`Update`).
- `Agent/StrategyPlanner.cs`: trigger decision (screen/Act/Boss change, consecutive low confidence >= N, every M moves) + calls the read-only LLM to produce strategy JSON -> parse -> `StrategyStore.Update`. Reuses the `AgentLoop.ChatAsync` read-only mode (`ChatOptions{ReadOnly=true}`).

## Integration point (AgentLoop.PlayOnceAsync branch)

After `stateJson` is obtained at `AgentLoop.cs:120` and before the prompt is assembled at `:129`:

```csharp
if (_decider is { } decider && _strategyStore is { } store)
{
    var snapshotJson = await _bridge.GetActionSnapshotJsonAsync(ct);   // same-frame state+actions
    var decision = await decider.DecideAsync(snapshotJson, store.Current, ct);
    if (decision.Error == null && decision.Confidence >= threshold)
    {
        var actArgs = JsonSerializer.Serialize(decision.ToActArguments());
        var outcome = await ExecuteActAsync(actArgs, ct, checkState, onAccepted);
        return new AgentTurnResult { Acted=..., ActResultJson=..., StateFingerprint=..., // must be filled
                                     Reasoning=decision.Reason, Usage=decision.Usage,
                                     RequestsSpent=decision.RequestsSpent, Confidence=decision.Confidence };
    }
    // Low confidence or failure: trigger a StrategyPlanner refresh and fall into the existing LLM path
}
```

- The `AgentLoop` constructor gains an optional `IActionDecider` + `StrategyStore` + a threshold provider (there are already 4 optional `Func<>`s, so the style is consistent).
- `AgentRuntime` injects them at construction; whether they are enabled is decided by `DualLayerSoloEnabled`/`DualLayerCoopEnabled` of `AgentSettings` and the current instance role.
- `AgentTurnResult` gains a nullable `Confidence` (init-only, backward compatible).
- `AgentRuntime.RecordDecision` passes confidence through; `DecisionLogEntry` gains a nullable `confidence`.

## MCP parity

- HTTP: `GET /strategy` (current strategy + briefing), `POST /strategy` (write strategy), both `IsLocal`.
- Native surface: `AgentTools.Mcp` adds the schemas of two tools; `NativeMcpServer.Tools.cs` adds the dispatch.
- Python: `client.py` adds `get_strategy`/`update_strategy`; `server.py` registers the two tools in guided.
- `test_native_tool_alignment.py` passes (the exemption table is empty).

## Errors and boundaries

- Jev failure/timeout/low confidence -> fall back to the LLM path, without stopping the whole run; consecutive failures are backstopped by the existing policy of `AutoPlayRecovery`.
- Cancellation: pass `CancellationToken` throughout; `AgentTurnCanceledException` is handed back to the receipt.
- Budget: every Jev call does `RequestsSpent+=1`; unknown usage stays null.

## Verified API contract (2026-09-22, double-confirmed with a real key + the official docs)

> The following comes from real calls to `https://api.typesafe.ai` + the official `docs.typesafe.ai` documentation, and the two agree. The implementation follows this.

- **Request** `POST /v1/systemone`, `Authorization: Bearer <key>`, body `{state, model, questions:{id:{type, instructions, criteria}}}`.
  - `state` can be a string or an object (we pass an object: the state part of the same-frame `{state, available_actions}`).
  - `criteria`: choice = map (optionId -> description, <=255 items); score = an ordered array of level descriptions (2-10 levels); none for noul.
  - Both `instructions` and `criteria` accept JSON structures (the faceted instructions object of typesafe-mario is legal).
- **Response** `{model, answers, usage:{input_tokens, output_tokens}}`. `model` is the resolved version number (such as `jev-1.13.0`), not equal to the requested alias `jev-latest`.
- **choice answer** `{type:"choice", choice, confidence, probabilities:{optionId->p}}`.
- **score answer** `{type:"score", score:<float>, confidence, legend:{index->level label}, probabilities:{index->p}}`. Note that score is a **float** (such as 1.82), not an int.
- **noul answer** `{type:"noul", noul:<0-1>}`, **no confidence**.
- **confidence** is derived from the probability distribution (the more concentrated, the higher), present for both Choice/Score; the officially recommended pattern is exactly **confidence-gated routing** (execute on high confidence, escalate on low), consistent with this design.
- **Errors**: an invalid key -> HTTP 401 (empty body) -> mapped to Config. 429 -> read `retry-after` and back off.
- **`GET /v1/models`** -> `{models:[{name, description, release_date}]}`, used by PingAsync to verify the key. Currently `jev-latest` and `jev-preview` exist.
- no-progress: the Jev path must fill `StateFingerprint`.

## Tests

- C# (`STS2AIAgent.Tests`, `dotnet run`): JevTypes serialization, JevClient (injected fake HttpMessageHandler: URL/request body/429 backoff/cancellation/error classification), JevOptionEnumerator (snapshot fixtures per screen), JevExecutionDecider (fake IJevClient), the confidence gating branch (fake bridge), StrategyStore/PlayStrategy parsing, the DecisionLog confidence field, the new SettingsClone fields.
- Python (`mcp_server/tests`, `uv run --locked python -m unittest discover -s tests -v`): DummyClient tests of the new tools + `test_native_tool_alignment.py`.

## Compatibility and rollback

- Everything new is an optional path; the two-layer switch is off by default and existing behavior is unchanged.
- The API only adds and never deletes; `confidence` is nullable.
- Rollback = revert; no destructive persistence change.
