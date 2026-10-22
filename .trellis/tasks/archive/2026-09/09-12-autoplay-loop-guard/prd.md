# Make auto-play neither spin in place nor treat "executed successfully but slow" as a failure

## Background

Two defects in opposite directions, both from the same retry policy `STS2AIAgent/Agent/AutoPlayRecovery.cs`:

### A. Spinning: there is no "no progress" detection at all

As long as `Error == null && Acted != null`, `AutoPlayRecovery.Observe` (`:20-24`) resets `_failures` to zero,
**without comparing whether the state changed before and after the action**:

```csharp
if (result.Error == null && result.Acted != null)
{
    _failures = 0;
    return (null, null, TimeSpan.Zero);
}
```

Combined with the default of **no request cap** (`MaxSessionRequests` at `AgentSettings.cs:39-41` defaults to null,
UI text "Unlimited"), the case of "action is legal but produces no state change" is forever treated as success,
and the failure count never grows. The whole repo also has no state fingerprint: `rg 'state_hash|no_progress|unchanged' STS2AIAgent/Agent` has no hits.

### B. False halt: a `pending` timeout is counted as a failure

`STS2AIAgent/Agent/AgentLoop.cs:633-651`: when an action returns `pending`/`stable=false` it waits
`WaitUntilActionableAsync(20s)`, and if it still has not settled it does `return (action, result, "Timed out waiting for a stable state after act.")`
and the third value being non-null => `Error != null` => counted toward the 3-consecutive-failure halt.

But the contract at `SKILL.md:84` says: "Treat `pending` responses as an instruction to stay inside the returned screen flow",
and the action **really was executed** (the returned JSON carries `previous` and the latest state).
A slow animation (for example a long settlement) makes auto-play halt on its own after 3 times.

## Goals

1. **Spinning is detectable**: `AutoPlayRecovery` records the "action + state fingerprint" of each successful action, and when the same fingerprint repeats consecutively to a threshold
   it escalates as "no progress" (reusing the existing 3-consecutive-failure halt semantics, or adding a dedicated halt reason, up to the implementation, but it must not spin forever).
2. **Slow does not mean failed**: a `pending` timeout no longer counts as a hard failure; it goes through the "waiting" channel (consuming no failure quota),
   but **must have an upper bound** (N consecutive unsettled results still halt), otherwise a false halt is merely swapped for silent infinite waiting.
3. Both rules go into an **offline-compilable pure policy**, with falsifiable tests.

## Acceptance criteria

- [x] The success branch of `AutoPlayRecovery` no longer resets unconditionally: repeating the same (action, fingerprint) to the threshold escalates to a halt/counts a failure.
- [x] The fingerprint does not falsely flag "real progress": changing the action, or the same action with a state change, both reset the count (tests assert each scenario).
- [x] `AgentTurnResult` gains a state that distinguishes "executed but not confirmed settled"; `AutoPlayRecovery` does
      not increase the failure count for it, but must halt once it accumulates to the cap.
- [x] Both rules have truth table tests (every branch asserted; breaking it turns it red).
- [x] The cancellation/budget/configuration paths of `AutoPlayRecovery.RunAsync` behave unchanged (existing tests stay green).
- [x] Full C# / MCP tests, `check_verification_gates.py`, and preflight pass.

## Out of scope

- Do not change the defaults of `AgentSettings` (whether to give users a default request cap is a product decision the user must make).
- Do not change the semantics of `WaitingForGame` (it already correctly consumes no failure quota).
- Do not change UI text.
