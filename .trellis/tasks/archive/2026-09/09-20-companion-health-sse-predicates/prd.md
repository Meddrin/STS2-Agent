# Fix companion health, the SSE lifecycle, and the predicate split

## Goal

Fix the misjudged companion health identity and the role-error observability, eliminate the periodic full state build when there are zero SSE subscribers and the silent event loss on a full queue, and continue splitting the `GameStateService` predicate layer with zero behavior change.

## Background

- Since v0.13.0 `/health.data.status` comes from a compatibility probe, and the legal live statuses are `ready` and `degraded`; the current companion identity check and the POSIX startup wait still accept only `ready`.
- `Router.BuildHealthData()` currently also returns the host two-instance state machine's default status text for the companion, misleading the role semantics.
- `GameEventService.Start()` currently unconditionally starts a full state build every 120ms; it occupies the game thread even when there are no `/events/stream` subscribers.
- The subscriber channel currently uses `DropOldest`, while the publisher relies on `TryWrite == false` to identify slow clients, so semantic events may be silently lost.
- The architecture spec designates the next mechanical split as the `Can*` / `Is*` predicate layer of `GameStateService`.

## Requirements

### R1 — Companion liveness/identity

- `CompanionHealth.IsExpectedProcess` must treat both `ready` and `degraded` as live compatible statuses.
- Keep strictly validating `ok == true`, `service == "sts2-ai-agent"`, `instance_role == "companion"`, the exact `api_port`, and the exact `process_id`.
- Missing, unknown, non-string status, wrong types, wrong envelope, and malformed JSON must be rejected.
- The startup health wait of `scripts/lib-sts2.sh` adopts the same live status set, without weakening its process and port ownership checks.

### R2 — Role-correct `/health`

- Review and record the applicable fields for common, host-only, and companion.
- Keep the existing key shape; the companion returns a stable `null` for host-only fields (not the host default text or a faked status).
- The host keeps returning the real `companion_process_alive`, `companion_process_exited`, `companion`, `dual_status`, `dual_launch_outcome`, `team_control_status`.
- Both roles keep returning their own common fields, including `instance_role`, `status`, `compatibility`, `api_port`, `process_id`, `play_running`, `play_phase`, `session_requests`, `state_build`, etc.
- Update `docs/api.md` and the gates/contract tests that depend on the `/health` facts.

### R3 — Demand-driven event polling

- Service startup is no longer equivalent to periodically building state right away; with 0 subscribers `BuildStatePayload` is not called periodically.
- The 0->1 transition reliably wakes the single shared poll loop; multiple concurrent subscribers must not create multiple pollers.
- After 1->0 periodic building stops; a later new subscriber can resume it.
- When one subscriber leaves but others remain, polling continues.
- `Stop()` terminates cleanly, an old unsubscribe must not affect a new lifecycle, it must not be implicitly restarted after shutdown, and a disposed token/source must not be reused.
- Keep the existing event types and first-state semantics; the first subscriber's first state still produces `session_started`, and later subscribers with an existing snapshot receive `stream_ready`.

### R4 — Explicit slow-subscriber failure

- The bounded queue capacity stays finite and the producer does not wait for slow HTTP clients.
- On a full queue there must be no silent DropOldest; a non-blocking write fails explicitly, the slow subscriber is removed from service and its channel completed, so that the SSE client is explicitly disconnected and re-aligns through reconnection/the current state.
- Normal consumer event order is preserved, no false disconnect when not full, and one slow subscriber does not affect other subscribers.
- `event_id` stays a globally monotonic sequence number; after disconnect/reconnect the numbering may show a gap in between, and replay is not promised.
- Review Python `iter_events` / `wait_for_event`; change only if a server-initiated close would break the existing fallback/reconnect contract.

### R5 — Predicate partial relocation

- According to actual dependencies, move the `Can*`, `Is*` clearly belonging to the availability/predicate layer and helpers serving only that layer into one or a few `GameStateService.*.cs` partial files.
- This is a mechanical relocation: signatures, method bodies, method order, decision order, exceptions/fallbacks, and return values stay as they are.
- Helpers used by both the raw builder and the predicates stay in the base file.
- Do not change `EnumerateAvailableActions`, action availability, reflection logic, payload, or compact agent view behavior.
- Prove with line-by-line/member evidence that the non-empty logic lines deleted from the base appear unchanged in the new partial; lower the named budget and the actual architecture line count in sync, without raising the total budget.

## Constraints

- Strict execution order: R1 -> R2 -> R3 -> R4 -> R5 -> full verification.
- For each item first run the relevant small-scope tests; for failures record the real error, attribute it, and fix this round's regression.
- Do not modify the existing untracked files `cstest.log` and `v.log`.
- Do not fake live validation; if this round is only offline verification, write plainly in the docs and report `offline verified / live validation pending`.
- For external behavior changes R1-R4 check `docs/api.md`, `docs/live-validation-checklist.md`, `.trellis/spec`, and the `Unreleased` section of `CHANGELOG.md`.
- R5 must not create product changelog noise.

## Acceptance Criteria

- [ ] ready companion accepted; degraded companion accepted.
- [ ] wrong pid/port/role/service, malformed/missing/unknown/non-live status all rejected.
- [ ] POSIX health wait is consistent for ready/degraded, with an offline regression test.
- [ ] host health still gives host-only status; the companion returns `null` for these stable keys; common fields all stay correct.
- [ ] With 0 subscribers 0 periodic state builds; 1 subscriber starts; 2 subscribers still a single loop; one leaves continues; the last leaves idles; a later subscription resumes; Stop is clean.
- [ ] Normal event order is preserved; a full queue explicitly closes/removes the slow subscriber; no silent DropOldest; other subscribers are unaffected.
- [ ] Python SSE wait/fallback tests prove the server-close contract is acceptable, or fix within the necessary scope.
- [ ] Predicate relocation passes through pure-move proof, C# tests, source-shape, and arch-facts, and the budget only goes down.
- [ ] `dotnet run --project STS2AIAgent.Tests\STS2AIAgent.Tests.csproj` passes and the total test count is reported.
- [ ] `uv run --locked python -m unittest discover -s tests -v` under `mcp_server` passes and the total test count is reported.
- [ ] `python scripts\check_verification_gates.py` passes.
- [ ] `powershell -ExecutionPolicy Bypass -File scripts\preflight-release.ps1` passes.
- [ ] The final report lists, per Task 1-5, the root cause, files, implementation, tests, API/behavior, verification, live to-dos, and commit hash, and lists the complete command results, git status, leftover issues, and live-run suggestions.

## Out of Scope

- Do not change `degraded` back to `ready`, and do not delete compatibility information.
- Do not rewrite the whole Router or event system, do not introduce an unbounded queue, and do not make the producer wait for consumers.
- Do not redesign predicates, merge/rename methods, or change LINQ/null/reflection/action/payload behavior.
- Do not run verification that needs starting/controlling the real game unless explicitly authorized later; offline results do not pass as live results.
