# Evidence

## Deliverables

| File | Change |
|---|---|
| `STS2AIAgent.Tests/ApiExceptionTests.cs` (new) | 3 behavioral tests |
| `STS2AIAgent.Tests/JsonHelperTests.cs` (new) | 2 behavioral tests |
| `STS2AIAgent.Tests/HttpServerPortPolicyTests.cs` (new) | 3 source contract tests |
| `docs/api.md` | The screen enum table gains 6 rows: `MULTIPLAYER_LOBBY`, `BUNDLE_SELECTION`, `CAPSTONE_SELECTION`, `CARDS_VIEW`, `UNLOCK`, `TIMELINE` (measured to be more missing than the 3 listed in the audit report) |
| csproj + TestRunner | Registered by the main agent (compile entries + 8 test names) |

Zero production code changes.

## Acceptance

| Criterion | Evidence |
|---|---|
| The 8 tests pass by registered name | `PASS ApiException.CarriesStatusAndCode / RetryableDefaultsToFalse / DetailsAreOptional`, `PASS JsonHelper.PascalCaseIndented / CaseInsensitiveRead`, `PASS HttpServerPort.ExplicitNeverDrifts / AutoIncrementFlagged / FallbackGate` |
| Falsifiable | Changing `WriteIndented` of `JsonHelper` to false -> `JsonHelper.PascalCaseIndented` red; three mutations on a **copy** of `HttpServer.cs` (`PortWasAutoIncremented=false`, `allowIncrement=true`, a wrong environment variable name) -> the matching contract tests each turn red; zero residue in the repo production files |
| Docs | `git diff -- docs/api.md` shows only 6 new screen table rows; the `<!-- BEGIN ACTION CONTRACT -->` block has zero diff, and the gate reports `55 actions match` |

## Gates

C# **PASS=311 FAIL=0**; `check_verification_gates.py` passed; `check_release_package.py --source-root .` passed; preflight passed.

## Coverage boundaries (written down explicitly to avoid re-evaluating next round)

- The three `HttpServer` tests are **source text contracts**, not runtime behavior: `HttpServer.Start()` depends on `MegaCrit...Log` and `Router` and cannot compile offline. Runtime assurance still comes from the indirect coverage of `LoopbackListenerTests` (7 tests).
- Screen names come from the static mapping of `GameStateService.ResolveNonModalScreen` and were not triggered one by one in the real game to check.
- The screen list is not a gate-checked item, so it may still drift in the future (the action contract is the hard check).
