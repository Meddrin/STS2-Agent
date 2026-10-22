# Implementation plan: the Jev two-layer decision engine

## Prerequisites

- Branch: `feature/jev-two-layer-engine` (can run in parallel with the UI branch of subtask 1, and merge on dev at the end).
- Read `.trellis/spec/mod/agent-and-ui.md`, `architecture.md`, `game-actions.md`.
- Depends on the settings fields of subtask 1 (JevBaseUrl/JevApiKey/JevModel/JevConfidenceThreshold/DualLayer*Enabled). If developing in parallel, first land these settings fields within this task and align with subtask 1 to avoid conflicts. Agreed: **the settings fields are landed by subtask 2 (this task)**, and subtask 1 only does UI binding.

## Steps

### 1. Llm layer: JevClient
- [ ] `Llm/JevTypes.cs`: request/response DTOs + serialization.
- [ ] `Llm/IJevClient.cs` + `Llm/JevClient.cs`: the systemone call, timeout, 429 backoff (retry-after), error classification, PingAsync.
- [ ] Unit tests: a fake HttpMessageHandler covers request shape/backoff/cancellation/errors.

### 2. Landing the settings fields
- [ ] `AgentSettings` gains the Jev and two-layer switch fields + `EnsureValidShape` + `SettingsClone`.
- [ ] `ModelRoleProbe.ModelRoleNames` adds `jev`; the role array of `AgentLoop.TestConfiguredRolesAsync` adds jev.
- [ ] Unit tests: round-trip / clone / normalization.

### 3. Agent layer: decider and strategy
- [ ] `Agent/ExecutionDecision.cs`, `Agent/IActionDecider.cs`.
- [ ] `Agent/JevOptionEnumerator.cs` + unit tests (snapshot fixtures per screen).
- [ ] `Agent/PlayStrategy.cs`, `Agent/StrategyStore.cs` + unit tests.
- [ ] `Agent/JevExecutionDecider.cs` + unit tests (fake IJevClient).
- [ ] `Agent/StrategyPlanner.cs` + unit tests (fake LLM).

### 4. Wiring into AgentLoop
- [ ] The `AgentLoop` constructor gains the optional decider/store/threshold; `PlayOnceAsync` gains a branch.
- [ ] `AgentTurnResult` gains `Confidence`; `AgentRuntime` injects and passes through; `DecisionLog(Entry)` gains `confidence`.
- [ ] Unit tests: confidence gating (fake bridge), fingerprint filling, the fallback path.

### 5. HTTP routes
- [ ] `GET /strategy`, `POST /strategy` (IsLocal).
- [ ] Add rows to `docs/api.md` (the api-facts gate).

### 6. MCP parity
- [ ] `AgentTools.Mcp` + `NativeMcpServer.Tools.cs` add two tools.
- [ ] `mcp_server` client.py + server.py add the counterparts.
- [ ] `test_native_tool_alignment.py` + Python tool unit tests.

### 7. CHANGELOG
- [ ] `CHANGELOG.md` `## Unreleased` entry.

## Verification

```powershell
dotnet run --project STS2AIAgent.Tests/STS2AIAgent.Tests.csproj
cd mcp_server; uv run --locked python -m unittest discover -s tests -v
powershell -ExecutionPolicy Bypass -File "scripts/preflight-release.ps1"
```

- [ ] All C# tests green (including GameTaskBounding with no bare await).
- [ ] All Python tests green, alignment test passes.
- [ ] preflight passes.
- [ ] Live: configure a Jev key -> turn on two-layer -> single-player and companion auto-play are driven by Jev, low confidence falls back to the LLM, the decision log carries confidence; an external MCP agent can read the briefing/adjust the strategy.

## Rollback points

- Steps 1-3 are purely additive and independently revertible.
- Step 4 wires into the main loop and is the key review point (self-test gating and fallback first, then continue).
- Steps 5-7 are surface extensions, and rolling them back does not affect the engine.

## Review gates

- After step 4: self-test of gating/fallback/fingerprint/no-progress.
- After step 7: trellis-check full + preflight.
