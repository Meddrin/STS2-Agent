# Make in-game action failures keep the error code and the retryable signal

## Background

The in-game Agent goes through an in-process bridge, not HTTP:

`ActAsync` at `STS2AIAgent/Agent/GameBridge.cs:73-99` calls `GameActionService.ExecuteAsync`
and serializes only `{action, status, stable, message, state}`.
Exceptions are squashed into `{ error = ex.Message }` at `STS2AIAgent/Agent/AgentLoop.cs:665-667`:

```csharp
catch (Exception ex)
{
    return (null, JsonSerializer.Serialize(new { error = ex.Message }, JsonOptions), ex.Message);
}
```

So the `StatusCode` / `Code` / `Details` / `Retryable` of `ApiException`
(`STS2AIAgent/Server/ApiException.cs:5-20`) are **all lost**:

- An MCP client going over HTTP gets `error.code = invalid_action / invalid_target / state_unavailable`
  and `retryable` (the envelope at `STS2AIAgent/Server/Router.cs:259-263`);
- The in-game model gets only a human sentence, **cannot tell 400 / 403 / 409 / 503 apart**, and does not know whether a given 503 is
  `retryable: true` (many places in the code explicitly mark retryable).

With the same state and the same actions, the two paths differ in error fidelity: a typical case of "one contract at two precisions" drift.

## Goals

1. In-game action failure error objects carry fields with the **same names and meanings** as the HTTP envelope:
   `code` / `message` / `details` / `retryable` (`status_code` optional, for the model's reference).
2. Unexpected exceptions that are not `ApiException` must also be classified (for example `code = "internal_error"`, `retryable = false`),
   and must not degrade into just a message.
3. Add guidance on "decide whether to retry by code" to the shared contract block of `skills/sts2-mcp-player/SKILL.md`
   (marker boundaries untouched; that file is embedded at mod build time).

## Acceptance criteria

- [x] The JSON produced by the failure branch of `AgentLoop` contains `error.code`, `error.message`, `error.retryable`,
      and for `ApiException` also `error.details` and a top-level `status_code` next to `error`.
- [x] The existing key `error` (string) is **no longer** the only form; if kept for compatibility, it must coexist with the structured fields
      (up to the implementation, but the compatibility trade-off must be written in the evidence).
- [x] Pure policy: extract the "exception -> error object" mapping into a type that compiles offline (no Godot / no MegaCrit references),
      with truth table tests (the four elements of `ApiException`, an ordinary exception, classification of a cancellation exception).
- [x] Source contract test: the serialized object of `GameBridge.ActAsync` contains `status_code`/`code`/`retryable`
      or equivalent field names (per the implementation), and the catch of `AgentLoop` no longer serializes only `ex.Message`.
- [x] Field names match the HTTP side's envelope (a test asserts both sides use the same set of field names, preventing another fork).
- [x] Full C# / MCP tests, `check_verification_gates.py`, and preflight pass.

## Out of scope

- Do not change the existing shape of the HTTP envelope (`Router.WriteErrorAsync`).
- Do not change the action contract block of `docs/api.md`.
- Do not add error codes (keep using `invalid_action` / `invalid_request` / `invalid_target` /
  `state_unavailable` / `invite_failed`; the only addition is the "non-ApiException fallback" classification).
