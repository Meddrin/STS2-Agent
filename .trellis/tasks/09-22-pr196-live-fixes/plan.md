# PR #196 Live Issue Fixes: Development Plan

Source: feedback after the user's live testing of PR #196 (two-tab overlay + Jev two-layer decisions + session persistence).
Working branch: open `fix/pr196-live-issues` on top of `feature/play-session-persistence`, and push to that PR branch when done (or go through dev per repo rules).

## Issue list and root causes (confirmed)

### P0: Two-layer decision mode not taking effect (most severe)
- **Root cause A (confirmed)**: `decider: BuildJevDecider()` in the `AgentRuntime` constructor is called only once, at **first construction**. If the player enters the Jev API Key in settings after launching the game, `_decider` stays null forever, the two-layer branch of `AgentLoop.PlayOnceAsync` (`dualLayerOn && _decider is { } decider`) never holds, and the LLM path is always taken. **Fix**: rebuild the decider lazily from settings (check for a fingerprint change on every PlayOnce or when settings are saved).
- **Root cause B (confirmed)**: `TestJevConnectionAsync` is a placeholder stub that never actually pings (it returns "the connection test will ship together with the two-layer decision engine"). **Fix**: implement a real `IJevClient.PingAsync` (GET /v1/models).
- **To confirm C**: in the loot phase, "instantly picks the first card": need to confirm whether JevOptionEnumerator yields only one option for the REWARD screen, whether it is the fallback of the LLM path, or CompanionPlayPolicy. Sub-agent report pending.

### P0: Auto-play hangs after a few turns with no error message
- Symptom: the loop stops; the UI shows no error, no next step, and the thinking does not update.
- Candidate root causes:
  - The LLM request defaults to a 10-minute timeout, during which the UI only shows "requesting model..." and looks hung.
  - `WaitUntilActionableAsync` returns `WaitingForGame=true` after a 20s timeout and recovery retries every 1s in an endless loop; if the game sits in a non-actionable state it waits forever with "waiting for the game to be actionable", no error, no escalation.
  - Paths where exceptions are swallowed are pending sub-agent confirmation.
- **Fix direction**: (1) the status bar shows the current phase + elapsed wait time; (2) escalate to a visible warning after WaitingForGame persists for N minutes; (3) add a configurable timeout + progress heartbeat to LLM requests; (4) make every catch path guarantee `_status` is updated.

### P1: Thinking content not shown / should go to the chat area
- Current state: thinking (Reasoning) is shown in the "Thinking" card (`_playThought`, truncated to 400 characters); the chat area shows only `AssistantText`, and during auto-play the model usually calls only tools and produces no text, so the chat area is empty.
- **Fix**: append each turn's reasoning + action decision to the chat area as chat messages (collapsible/truncated), and remove the "Thinking" card or change it to show the current status.

### P1: Chat display cap bug
- Current state: `_history` is capped at 80 entries (the oldest are dropped when exceeded), and the UI draws only the last 60 (`ChatTailLimit`).
- The user reports "it stops displaying after that", which needs confirming as either a perception issue from the 80-entry cap or rendering really stopping.
- **Fix**: make the chat area a scrollable full history (RichTextLabel itself scrolls), remove the 60-entry draw cap or raise it substantially; raise the history cap and add an "N entries omitted" hint.

### P1: Model settings refactor
- (7) Test connection becomes **per model**: one test button on each model card, and the test is a real call (ping + one simple decision request with tool definitions, verifying tool-calling ability).
- (8) After a successful test, show a prominent marker next to that model card (a "verified" badge with a check mark), not only text in the first-run setup bar.
- (9) Merge the main chat model and the play model into a single "main model"; the vision helper model stays under advanced settings.
- (10) Remove the "tool calling" checkbox (a model that does not support tool calling cannot be used for play; this is detected and marked automatically during the test).
- (11) Move reasoning effort to the chat/play side (add a reasoning effort selector to the play page, effective immediately).

### P1: Remove the "allow auto-play" option
- The "allow auto-play" checkbox in the chat area is redundant (the auto-play button already exists). Remove it and keep chat read-only; use auto-play/single step to take action.

### P2: Model reply language setting (Chinese/English)
- Add a "reply language" option in settings or on the chat side, injected into the system prompt (the system message of PlayPrompt / ChatAsync).

### P2: First-run setup guidance text update
- Rewrite the text of `FirstRunSetup` and the "first-run setup" section of the settings page for the new two-tab UI (Play/Settings tabs, single main model, per-model testing, Jev configuration location).

### P3: agent loop architecture evaluation (pi agent), conclusion (2026-09-22)

- pi (earendil-works/pi) is a **TypeScript/Node** toolkit (`pi-agent-core` / `pi-ai` / `pi-coding-agent`, distributed via npm). This Mod is a C#/.NET 9 Godot plugin inside the game process and **cannot embed** pi directly; introducing it as a sidecar would require players to install Node, which violates the "zero player dependencies" product goal (the native MCP surface exists precisely so Python need not be installed).
- **Adopt its design, not its code**: the event stream model of pi-agent-core (agent_start/turn_start/message_update/tool_execution_end/agent_end), the transformContext/convertToLlm separation, the beforeToolCall/afterToolCall hooks, and the explicit finishTurn end state. Mapped onto this round's fixes: phased heartbeat (PlayPhases + StatusWithElapsed) is roughly the minimum viable version of the event stream; exception type preservation + NoteEvent is roughly explicit error surfacing; GameThread timeout/cancellation is roughly "every wait has a deadline and is cancellable".
- The full event stream refactor (replacing the implicit `(StopReason, Delay)` tuple of `AutoPlayRecovery` with an explicit state machine, extracting `CompleteWithToolsAsync` into an event stream kernel) is a separate task and out of scope for this round.

## Live retest points (for the user)

1. **You must use the DLL rebuilt from this branch** (release builds of v0.15.0 and earlier contain no two-layer decision code at all). Build: `scripts/build-mod.ps1 -Configuration Release` (already run and deployed to the game mods directory), then restart the game.
2. Two-layer decisions: enter the Jev Base URL + API Key on the settings page -> click "Test Jev connection" (now a real ping) -> turn on "two-layer decision mode" on the play page -> start auto-play. In the decision log, actions executed by Jev read `source: jev` and carry a confidence; the Jev panel shows the latest choices and probabilities.
3. Loot card choice: when the reward screen has a card reward it now **stops and waits for a decision** (it no longer auto-takes the first card); it continues only after the LLM/Jev explicitly picks or skips.
4. Hang diagnosis: the status bar now shows the current phase + seconds waited; waiting for more than 2 minutes in a row halts and gives a visible reason.
5. Chat area: thinking content (tick "show thinking content") and every action decision enter the chat flow as bubbles; the chat cap is 200 entries, and when exceeded the top shows "N earlier entries omitted".
6. Model settings: every model card has a "Test" button (real call + tool-calling detection), and a pass shows a check badge; one main model is enough (shared by chat and play); the vision model is under "show advanced options"; reasoning effort and reply language are adjusted directly on the play page's chat card.


## Execution order

1. P0 two-layer decision fix (lazy decider rebuild + real Jev ping + loot enumeration check)
2. P0 hang fix (status visibility + wait escalation + error surfacing)
3. P1 thinking into chat + chat cap fix
4. P1 model settings refactor (per-model test + badge, single main model, remove the tool-calling checkbox, move reasoning effort)
5. P1 remove allow auto-play
6. P2 language setting
7. P2 first-run setup text
8. Build + unit tests + preflight, update PR #196

## Verification

- `dotnet run --project STS2AIAgent.Tests` (new/updated unit tests: decider rebuild, Jev ping, chat cap, settings migration)
- `powershell scripts/build-mod.ps1 -Configuration Release` (close the game first)
- preflight-release.ps1
- Live verification is done by the user
