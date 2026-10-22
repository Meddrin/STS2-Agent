# Design: two guards in opposite directions

## 1. Pure policy: `STS2AIAgent/Agent/NoProgressPolicy.cs` (new, pure BCL)

```csharp
internal static class NoProgressPolicy
{
    public const int RepeatThreshold = 3;   // how many consecutive occurrences of the same (action, fingerprint) count as spinning

    // The fingerprint is updated only on "successful execution"; changing the action or a state change both count as progress
    public static bool IsRepeat(string? previousAction, string? previousFingerprint,
                                string? action, string? fingerprint);

    // Upper bound on consecutive unconfirmed results (pending timeouts)
    public const int UnsettledLimit = 5;
}
```

Thresholds must be constants and asserted by tests (avoid magic numbers in the implementation).

## 2. State machine of `AutoPlayRecovery`

```csharp
private int _failures;          // existing: hard failures
private string? _lastAction;    // new
private string? _lastFingerprint;
private int _repeats;           // new
private int _unsettled;         // new
```

`Observe` branch table (to be landed one by one during implementation):

| Input | Behavior |
|---|---|
| `RequiresConfiguration` | Unchanged: stop immediately (Configuration) |
| `WaitingForGame` | Unchanged: 1s delay, consumes no failure |
| Success with progress (action changed or fingerprint changed) | Reset `_failures`/`_repeats`/`_unsettled`, record the new action + fingerprint |
| Success but the same (action, fingerprint) repeats < threshold | Delay (reuse the backoff or 1s), `_repeats++` |
| Success but repeats >= threshold | Halt (pick an existing kind in `StopKindPolicy` or reuse the existing "consecutive non-success" text), and give a readable reason |
| Executed but unconfirmed (new field) | `_unsettled++`, does not consume `_failures`; halt on reaching `UnsettledLimit`; delay reuses the backoff |
| `Error != null` | Unchanged: `_failures++`, halt at >= 3 |

## 3. `AgentTurnResult` (`STS2AIAgent/Agent/AgentLoop.cs` or wherever it is defined)

Add a boolean field expressing "executed but not confirmed settled", set by `AgentLoop` in the `!settled` branch, leaving
`Error` empty (the third returned value is null, or keeps the text but is not used as the basis for failure; decide at implementation time based on "who reads the third value",
and the consumers of that value must be listed in the evidence).

The return at `AgentLoop.cs:633-651` needs a matching adjustment: the status is still `pending` (JSON unchanged),
but upstream must **no longer** count it as a failure.

## 4. Fingerprint source

The fingerprint must be cheap and stable: hashing the string from `GetCompactStateJsonAsync` once is enough
(for example the first 16 bytes of `System.Security.Cryptography.SHA256` as hex, or directly compare length + string equality).
**Do not** introduce new game APIs; do not do per-token serialization comparison (compact is already a string).
Note at implementation time: `AgentLoop` already fetches the compact state on several paths (`checkState?.Invoke(...)`);
reuse that as much as possible instead of requesting again.

## 5. Tests

- `NoProgressPolicyTests` (behavioral, offline): `IsRepeat` truth table (null handling, same action same fingerprint,
  same action different fingerprint, different action same fingerprint), and the two constants asserted explicitly.
- `AutoPlayRecoveryTests` (behavioral, offline): feed a sequence of `AgentTurnResult` and assert
  (1) repeating the same fingerprint to the threshold -> halt; (2) changing the action -> no halt; (3) N consecutive unconfirmed states -> halt without affecting
  `_failures` (assert indirectly by whether a subsequent hard failure still needs 3 occurrences); (4) the existing 3-consecutive-failure semantics unchanged.

`AutoPlayRecovery` is already in the test project's compile list (`..\\STS2AIAgent\\Agent\\AutoPlayRecovery.cs`),
so these must be **real behavioral tests**, not source contract assertions.
