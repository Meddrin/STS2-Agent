# Jev two-layer decisions: execution layer + LLM planning layer + MCP parity

## Goal

Introduce a two-layer decision mode for both play modes: when on, Jev (a TypeSafe System One model) directly chooses each concrete operation through the Choice primitive of `POST /v1/systemone`, while the LLM only adjusts Jev's questions and strategy at a low-frequency strategic planning layer from the game state. An external MCP coding agent acts as the same planning layer through new planner tools, ensuring the experience matches the in-game one. The first version supports only Jev.

## Requirements

### Execution layer (Jev)
- Add the `IActionDecider` abstraction: input is the "same-frame state + available_actions snapshot" and the current strategy, output is one concrete action decision (action name + card/target/option/x/y/tool indices + confidence + probabilities + usage).
- `JevExecutionDecider` is the default implementation: it expands the snapshot into an **enumeration of concrete selectable actions** (such as "play Strike -> enemy 1", "end turn", "choose map node 2"), used as the criteria of the Jev Choice (<=255 items); a single call can mix in one Score (danger level, for UI display).
- Add `JevClient` (`IJevClient`): `POST {BaseUrl}/v1/systemone`, Bearer auth, explicit timeout, 429 backoff respecting `retry-after`, error classification aligned with `ModelRoleProbe.FailureKind` (4xx -> config, 408/429/5xx/timeout -> network).
- Jev HTTP calls happen outside the game thread; only the chosen action is sent back to the game thread via `IGameBridge.ActAsync`.

### Planning layer (LLM)
- Add `PlayStrategy` (play posture + instruction text + option hints) and a thread-safe `StrategyStore` (with a built-in default strategy).
- Add `StrategyPlanner`: low-frequency triggers (screen change / Act change / Boss / consecutive low confidence / once every N moves), which uses a read-only LLM to produce an updated `PlayStrategy` from the run summary + current screen + recent decisions + current strategy, written into `StrategyStore`.
- The strategy text is injected into the Choice instructions of Jev (in facets: goal / current screen playbook / strategy / legality reminders), copying the instructions structure of typesafe-mario.

### Confidence gating
- `confidence >= JevConfidenceThreshold` -> execute directly (reusing the full `ExecuteActAsync` pipeline: legality, `ActIndexValidator`, game thread, unsettled re-wait, fingerprint).
- Below the threshold or a failed Jev call -> fall back to the existing LLM per-move decision path (without interrupting the whole run), and trigger a strategy refresh.

### Integration point
- Branch after `AgentLoop.PlayOnceAsync` gets the state and before the LLM prompt is assembled (finding B1 of the baseline report). When two-layer is on and the current mode is enabled it takes the Jev path, otherwise the existing LLM path.
- Single-player (`/session/control`) and the multiplayer companion (`/teammate/control` -> `/companion/control`) share the same `AutoPlayLoopAsync`, and the two-layer logic takes effect identically for both (per each mode's switch).

### MCP parity (both surfaces)
- Add planner tools, once each on the C# native surface (`AgentTools.Mcp` + `NativeMcpServer.Tools.cs`) and the Python sidecar (`server.py` + `client.py`):
  - `get_planner_briefing`: run summary + current screen + current strategy + recent Jev decisions and confidence trend.
  - `update_play_strategy`: write a new strategy (posture / instructions / hints).
- Add the HTTP routes `GET /strategy`, `POST /strategy` (IsLocal restricted, modeled on `/session/control`).
- `test_native_tool_alignment.py` two-way comparison passes (the exemption table stays empty).

### Decision log and budget
- `DecisionLogEntry` gains a nullable `confidence` field (`WhenWritingNull` is naturally backward compatible); Jev decisions have `source="jev"`.
- Every Jev call does `RequestsSpent += 1`; unknown usage stays null.
- `StateFingerprint` must be filled (the no-progress guard depends on it).

## Acceptance Criteria

- [ ] After two-layer is on, single-player and companion auto-play are decided and executed step by step by Jev, and the decision log shows `source="jev"` with confidence.
- [ ] When confidence is below the threshold it falls back to LLM per-move decisions, and the whole run is not interrupted.
- [ ] The LLM planning layer updates the strategy by trigger conditions, and the strategy text enters the Jev question.
- [ ] An external MCP agent can read the briefing and adjust the strategy through `get_planner_briefing` / `update_play_strategy`; the two-surface tools are aligned.
- [ ] Jev calls have explicit timeouts, 429 backoff, and cancellation propagation; no HTTP is awaited inside the game thread.
- [ ] The budget/log/no-progress guards take effect on the Jev path as well.
- [ ] C# unit tests cover: JevTypes serialization, JevClient (fake handler), option enumeration, confidence gating, the strategy store, the DecisionLog confidence field.
- [ ] Python unit tests cover the new tools (DummyClient) + the alignment test.
- [ ] `docs/api.md` adds the new route descriptions; `CHANGELOG.md` Unreleased records them.

## Notes

- Shape template: `Multiplayer/CompanionPlayPolicy.DecideImmediate` (state+actions+parameters -> action+indices).
- Reference typesafe-mario `policy.py`: Choice criteria = action -> description, instructions in facets, mixing multiple questions in one call.
- For constraints see the decision-chain baseline report C1-C5 (game thread, cancellation, budget, logging, no-progress).
- Online multiplayer is currently only the local two-instance form; this task adds no cross-network AI protocol.
