# Evidence: the fidelity gap of in-game errors

## HTTP side (faithful)

`Router.WriteErrorAsync` outputs `{ok:false, request_id, error:{code,message,details,retryable}}`,
and the status code comes from `ApiException.StatusCode`. The MCP client (`client.py`) and the docs (the error code table at `docs/api.md:46`)
both consume this set of fields.

## In-game side (lossy)

- On success `GameBridge.ActAsync` (`GameBridge.cs:73-99`) serializes
  `{action, status, stable, message, state}`; exceptions are not handled here.
- `RunActAsync` of `AgentLoop` (`AgentLoop.cs` about :600-670) in `catch (Exception ex)`
  serializes **only** `new { error = ex.Message }` (`:665-667`), and the returned error text is likewise `ex.Message`.
- So the `StatusCode`/`Code`/`Details`/`Retryable` of `ApiException` (`ApiException.cs:5-20`) are dropped on
  the in-game path; the model cannot see `invalid_action` vs `state_unavailable` vs `invalid_target`,
  and does not know `retryable`.

## Why it matters

- Many places in the code explicitly mark `retryable: true` (for example the several `503 state_unavailable` in `GameActionService.cs`),
  and this signal is completely ineffective on the in-game path.
- The decision rules of `skills/sts2-mcp-player/SKILL.md` depend on distinguishing "whether the action is legal / whether it is retryable";
  with only a message the model can only guess.
- The two paths (HTTP and in-process) should express the same contract; this is contract precision drift.

## To verify at implementation time

- Consumers of the `error` field: exhaustively search with `rg` at implementation time (`STS2AIAgent/Agent`, `skills/`, `docs/api.md`),
  and confirm whether changing `error` from a string to an object breaks any reader.
- How the third value returned by `AgentLoop` (the error text) is used in `AutoPlayRecovery` / UI logs.
