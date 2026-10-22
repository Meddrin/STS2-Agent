# Evidence

## Deliverables

| File | Change |
|---|---|
| `STS2AIAgent/Agent/AgentErrorEnvelope.cs` (new) | Pure-BCL "exception -> error object" mapping; field names match the HTTP envelope |
| `STS2AIAgent/Agent/AgentLoop.cs` | The catch blocks of `ExecuteActAsync` / `ExecuteReadToolAsync` now use the shared envelope; added a check that "a bridge error envelope must not be treated as success" |
| `STS2AIAgent/Agent/GameBridge.cs` | `ActAsync` catches `ApiException` and reuses the same envelope; the success payload and the semantics of other exceptions are unchanged |
| `STS2AIAgent.Tests/AgentErrorEnvelopeTests.cs` (new) | 8 tests (truth table + field name comparison + source contract) |
| `skills/sts2-mcp-player/SKILL.md` | The shared contract block gains `## Action Failure Rules` (marker untouched) |

## Acceptance

| Criterion | Evidence |
|---|---|
| Failure JSON carries code/message/retryable (ApiException also carries details/status_code) | `PASS AgentErrorEnvelope.ApiExceptionKeepsItsHttpMetadata / ApiExceptionDefaultsArePreserved` |
| Non-ApiException classification is not lost | `PASS UnexpectedFailureIsClassifiedNotDropped` (`internal_error` / null / null / false) |
| Pure policy testable offline | `PASS CancellationIsClassifiedWithoutRetry / EnvelopeReadsBackAsTheFailureText` |
| Field names match the HTTP envelope | `PASS EnvelopeFieldNamesMatchTheHttpRouter`: parsing `Router.WriteErrorAsync` gives `[code,details,message,retryable]`, runtime serialization gives `[code,details,message,retryable,status_code]`, and the sets are equal |
| Source contract | `PASS GameBridgeActFailureCarriesTheEnvelope / AgentLoopFailureCarriesTheEnvelope` (catch no longer serializes only `ex.Message`) |
| Falsifiable | `CodeFor` always returning `internal_error` -> 1 red; `IsRetryable` always true -> 3 red; both restored |

### Consumers of the error field (compatibility conclusion after exhaustive search)

`NativeMcpServer.LooksLikeError` only checks "whether there is an error key"; `AgentTurnResult.Error` (string) still keeps
`ex.Message`, so the behavior of `AutoPlayRecovery` / `StopKindPolicy` is unchanged; the three argument pre-validations of `ExecuteActAsync`
still use a string error (they are not exceptions, carry no code semantics, and the PRD adds no error codes). No consumer parses `error` as a string.

## Gates

C# 335 PASS / 0 FAIL; `dotnet build` 0 warnings 0 errors; MCP 167 OK; gates / package / preflight all green.

## Live-only verification

The end-to-end shape of a real `state_unavailable (retryable:true)` after passing through `GameBridge -> AgentLoop`;
the readability of the "error object envelope" to native MCP clients; the actual effect of the new SKILL.md rules on the model's retry decisions.
