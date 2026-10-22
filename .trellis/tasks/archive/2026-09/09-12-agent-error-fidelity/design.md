# Design: Structuring the error object

## 1. New file `STS2AIAgent/Agent/AgentErrorEnvelope.cs` (pure BCL, compilable offline)

```csharp
internal static class AgentErrorEnvelope
{
    public const string InternalErrorCode = "internal_error";

    // Every exception other than ApiException falls into this bucket; cancellation is handled by the caller first and should not reach here.
    public static bool IsRetryable(Exception exception);          // ApiException.Retryable, otherwise false
    public static string CodeFor(Exception exception);            // ApiException.Code, otherwise InternalErrorCode
    public static int? StatusCodeFor(Exception exception);        // ApiException.StatusCode, otherwise null
    public static object? DetailsFor(Exception exception);        // ApiException.Details, otherwise null
    public static object ToPayload(Exception exception);          // an anonymous object made of the four above
}
```

**Can it not** reference `ApiException`? It can: `ApiException` itself is pure BCL (`Server/ApiException.cs`),
and is already compiled into the test project (`ApiExceptionTests` from the previous round). So this file can also go into the test project,
registered by the main agent.

If during implementation putting `StatusCode` in the payload conflicts with an existing consumer, it may be emitted only for `ApiException`,
but the reason and measurements must be given in the evidence.

## 2. `AgentLoop.cs:665-667`

```csharp
catch (Exception ex)
{
    var payload = AgentErrorEnvelope.ToPayload(ex);
    var json = JsonSerializer.Serialize(new { error = payload }, JsonOptions);
    return (null, json, AgentErrorEnvelope.CodeFor(ex));
}
```

Note the third returned value (the error text) is used as the "failure reason" by places like `AutoPlayRecovery`,
so during implementation confirm it is still a non-empty string (use `ex.Message`, do not change it to the code).

## 3. `GameBridge.ActAsync`

The success path is unchanged. If the implementation chooses to also catch exceptions at the bridge layer, it must use the **same** `AgentErrorEnvelope` as `AgentLoop`,
and two places must not each carry their own copy of the field names.

## 4. Tests

- `AgentErrorEnvelopeTests` (behavior tests, offline):
  `ApiException(409, "invalid_action", "m", details, retryable: true)` →
  assert each of the four elements of `CodeFor`/`StatusCodeFor`/`DetailsFor`/`IsRetryable` one by one;
  `new InvalidOperationException("x")` -> `internal_error` / `null` / `null` / `false`;
  the truth table must be asserted value by value (changing one branch turns it red).
- Source contract: the catch block of `AgentLoop` contains `AgentErrorEnvelope`; the
  serialized object of `GameBridge.ActAsync` has the same field names as the HTTP envelope (compare the field names of `Router` and the bridge side as sets,
  reading the text via `AgentSourceFixture`).

## Compatibility

- The only consumer on the agent side is the LLM (which reads JSON); extra fields only add and never remove, so it is safe.
- Changing `error` from a string to an object is a **breaking** change; if anywhere in `skills/` or `PlayPrompt` reads `error` as a string,
  it must be fixed together during implementation (list all consumers in the evidence).
