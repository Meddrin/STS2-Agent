# Evidence: spinning and false halts

## Spinning side

- `AutoPlayRecovery.Observe` (`AutoPlayRecovery.cs:20-24`): `Error == null && Acted != null` -> `_failures = 0`.
  **It does not compare state changes**.
- No state fingerprint: `rg 'state_hash|stateHash|no_progress|unchanged|SameState|duplicate' STS2AIAgent/Agent` -> no hits.
- No budget backstop by default: `AgentSettings.cs:39-41` `MaxSessionTokens/MaxSessionRequests` default to null;
  `SessionBudgetGuard.CheckBudget` (`:26-41`) stops only when the values are finite.
- On the `AgentRuntime` side `_lastAction` is used only for UI display (`AgentRuntime.cs:42/182/602/1053`).

## False halt side

- `AgentLoop.cs:633-651`: `IsUnsettled(result)` (`ActIndexValidator.cs:137-166`:
  `status=="pending"` or `stable==false`) -> `WaitUntilActionableAsync(20s)`;
  if it still has not settled it returns the third value `"Timed out waiting for a stable state after act."` => `Error != null` => counted as a failure.
- Contract reference: `skills/sts2-mcp-player/SKILL.md:84` "Treat `pending` responses as an instruction to
  stay inside the returned screen flow": pending means "stay in that flow", not "failure".

## Source of the criteria (existing implementations to reference)

- `AutoPlayRecovery.RunAsync` already distinguishes `AutoPlayStoppedException` / `OperationCanceledException` /
  other exceptions (`AutoPlayRecovery.cs:52-58`).
- `StopKindPolicy` provides the stop kinds; when implementing a new halt reason, prefer reusing an existing kind to avoid adding public enum values.

## To confirm at implementation time

- Who consumes the third value returned by `AgentLoop` (`AccountTurn`/`ApplyPlayResult` of `AgentRuntime`,
  logs, UI text), confirming that "pending no longer counts as failure" will not make anything display as success.
- The existing semantics of `WaitingForGame` (1s delay, consumes no failure, does not reset existing failures) must remain unchanged.
