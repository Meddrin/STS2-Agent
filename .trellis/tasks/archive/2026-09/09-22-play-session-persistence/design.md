# Design: play chat session persistence

## Core judgment

"Context" in the current architecture is not a single transcript but a combination of three things. Persistence must cover all three, each restored to its own place:

1. **Chat history**: user messages + model replies + decision/tool call entries (after subtask 1 renders decisions into the chat stream, the chat area is the decision stream).
2. **Decision memory**: the recent decision sequence consumed by `ContextCompaction` (the `AgentLoop._recentDecisions` provider currently reads `AgentRuntime.RecentDecisions`, the in-memory snapshot of DecisionLog).
3. **Strategy state**: the `PlayStrategy` in two-layer mode (the StrategyStore of subtask 2).

## New types

- `Agent/PlaySessionRecord.cs`: DTO `{run_id, character, updated_at, chat: ChatTurn[], decisions: DecisionLogEntry[], strategy: PlayStrategy?}`. System.Text.Json, nullable fields are backward compatible.
- `Agent/PlaySessionStore.cs`: the persistence service.
  - Directory: a `sessions/` sibling of `SettingsStore`, one `{run_id}.json` per run.
  - Atomic write (temp file + move) + `.bak`; on corruption restore from the backup and raise a notice (reusing the SettingsStore pattern).
  - `Load(runId)` / `Save(record)` / `Delete(runId)`; `run_unknown` is not written to disk.
  - Best-effort: any IO failure is not thrown to the play path.
  - Redaction: pass through `DiagnosticExport.Redact` before writing; never write the API key.

## Wiring

- `AgentRuntime` holds `PlaySessionStore`; `CurrentRunBoundary` can already provide the run_id / run switch signal.
- **Save**: mark dirty on every chat entry appended, every decision recorded, and every strategy update; flush on run switch, pause, overlay hide, and game exit (aligned with the timing pattern of the existing `FlushSettingsIfDirty`).
- **Restore**: when `AutoPlayLoopAsync` detects entering a run that has a session (`boundary.Check` gives the run_id), `Load(runId)` -> restore chat to the overlay, feed decisions to the `_recentDecisions` provider, and write the strategy back to `StrategyStore`.
- **Model switch**: context is model-independent and is naturally kept; `ContextCompaction.ResolveWindow` just recomputes the compaction threshold by the new model's window (existing logic).

## Boundaries

- Capacity: set caps on chat and decisions each (for example decisions keep 200, chat 200 entries), trimming the oldest beyond that; a file size cap + rotation (aligned with the 2MB of DecisionLog).
- Concurrency: all reads and writes go through the store's internal lock; flush runs in the background and does not block the game thread.
- Multiple instances: the companion process has its own run and session file (a different run_id) and does not share.

## Tests

- Unit tests: record serialization/deserialization, store atomic write and corruption recovery, isolation by run key, run_unknown not written to disk, redaction, capacity trimming, the model switch scenario (context kept).

## Compatibility and rollback

- A purely additive persistence layer; no existing format change. Rollback = revert (leftover session files are harmless).
