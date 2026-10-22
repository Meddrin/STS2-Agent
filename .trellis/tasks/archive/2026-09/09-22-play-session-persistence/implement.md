# Implementation plan: play chat session persistence

## Prerequisites

- Depends on subtask 1 (chat merged into the play page) and subtask 2 (PlayStrategy/StrategyStore) landing.
- Branch: `feature/play-session-persistence`.

## Steps

### 1. Persistence layer
- [ ] `Agent/PlaySessionRecord.cs` DTO.
- [ ] `Agent/PlaySessionStore.cs`: directory resolution, atomic write, .bak, corruption recovery, redaction, capacity trimming.
- [ ] Unit tests: serialization/restore/isolation/corruption/run_unknown/redaction/trimming.

### 2. Wire AgentRuntime
- [ ] Hold the store; dirty marking + flush timing (run switch/pause/hide/exit).
- [ ] Restore: when entering a run with a session, Load -> chat/decisions/strategy each to its own place.
- [ ] Unit tests: model switch, run switch isolation.

### 3. Overlay restore display
- [ ] Rebuild the chat area from the record.

## Verification

```powershell
dotnet run --project STS2AIAgent.Tests/STS2AIAgent.Tests.csproj
```

- [ ] Unit tests all green.
- [ ] Live: open a run and chat a few lines -> exit -> continue that run -> chat/decisions/strategy restored; switching models keeps context; a new run gets a fresh session.

## Rollback points

- Purely additive; just revert.
