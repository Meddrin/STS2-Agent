# Play chat session persistence (save/restore context per run)

## Goal

Make one game run save correspond to one chat session: the player can save and exit midway, and the next "continue game" restores that run's chat and decision context; switching models does not lose context. The "save" here means the in-game "continue game" run save, not the whole game save.

## Requirements

### Current state (from code survey)
- The play loop `AgentLoop.PlayOnceAsync` is **stateless per move**: every turn it rebuilds system+playbook+strategy+team context+vision+compacted memory+latest state. The so-called "memory" comes from the summary of `DecisionLog` made by `ContextCompaction.Format` (the `_recentDecisions` provider), not from a chat transcript.
- Chat (`ChatAsync`) is another independent session, and its history lives only in overlay memory.
- `DecisionLog` is already persisted best-effort (200 in memory + 2MB file rotation), but it is **not restored into the loop memory per run**.

### Requirements
- Persist a run's session keyed by `run_id`:
  - Chat history (user messages + model replies + decision/tool call entries).
  - Decision context (the recent decision sequence for `ContextCompaction`).
  - The current `PlayStrategy` in two-layer mode.
- When "continue game" enters the same run, restore the above context: the chat area reproduces the history, decision memory continues, and strategy continues.
- When switching models (conversation/play model), keep and reuse that run's context (context is bound to the run and independent of the model).
- Exiting midway (closing the overlay/closing the game) loses nothing: persistence is immediate or flushed on exit.
- A new run (a new save) starts a new session and does not mix contexts.

## Acceptance Criteria

- [ ] Each run's chat history is persisted to disk by `run_id`, and when re-entering that run the chat area shows it again.
- [ ] Decision context (the recent decision sequence) is restored per run, and `ContextCompaction` can work from historical decisions when play continues.
- [ ] `PlayStrategy` is saved and restored per run in two-layer mode.
- [ ] After switching models and continuing the same run, the context is kept.
- [ ] After exiting midway and re-entering, the context is not lost.
- [ ] A new run starts a fresh session.
- [ ] Persistence failure does not affect play (best-effort, aligned with the DecisionLog principle that "diagnostics must never block play").
- [ ] Sensitive information (API key) is not written into session files; redaction via `DiagnosticExport.Redact` is reused.
- [ ] Unit tests cover: session serialization/deserialization, restore by run key, the model switch scenario, corrupted file recovery.

## Notes

- Depends on subtask 1 (the shape of chat merged into the play page) landing first.
- The storage location is suggested to be a `sessions/` subdirectory next to `SettingsStore`, one file per run (`{run_id}.json`), with atomic write + .bak.
- Note that the `run_unknown` placeholder run is not written to disk (aligned with how DecisionLog normalizes `run_unknown`).
