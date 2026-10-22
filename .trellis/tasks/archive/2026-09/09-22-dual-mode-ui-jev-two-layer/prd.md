# Dual-mode UI reorg and Jev two-layer decision mode

## Goal

Reorganize the in-game overlay from "features scattered across 6 tabs" into a two-mode experience centered on **Play**, and introduce a **two-layer decision mode** for both ways of playing: Jev (a TypeSafe System One model) does the actual play operations, and the LLM only does strategic planning from the game state and tunes the play strategy for Jev. The first version supports only Jev.

## Background and source of requirements

Core asks from the user (2026-09-22 session):

1. The mod essentially provides only two ways to play:
   - **Mode 1 (single-player)**: the in-game overlay lets an LLM operate the game; or any third-party coding agent that supports MCP operates the game through MCP. The two experiences should be fully identical; the overlay just makes it easier for players who do not want to open a separate coding agent.
   - **Mode 2 (multiplayer)**: the in-game overlay, or an MCP third-party coding agent, joins a multiplayer game (local/online) as an **independent player** and plays together with the other players (which may include the local user).
2. UI usability fix: organize directly into two clear modes.
3. Add a **two-layer decision mode switch** to both modes: when on, Jev does the actual play operations, and the LLM is only responsible for making decisions from the current game situation, enemies, deck, state, and all obtainable information, and tuning the play strategy for Jev.
4. Reference case: https://github.com/fhshaik/typesafe-mario (Jev's Policy mode of directly choosing gamepad inputs).

## Key decisions the user settled in review

- **UI structure**: only one of single-player or multiplayer mode can be enabled at a time, built as a **slider switch** toggling single/multiplayer instead of each taking its own tab; **connection (MCP) goes into a subpage of the Settings page**; keep only the **Play page**, with chat inside the page.
- **Chat is the decision stream**: the chat directly outputs the model's decision information and operation results (essentially LLM tool call information and replies), with optional display of model thinking content; the user can talk to the LLM directly in the chat to influence decisions and pause/resume play.
- **A session is a save**: one game run save = one chat session, which can be saved and exited midway, and next time when resuming play or switching models the context is kept (meaning the in-game "continue game" run save, not the whole game save).
- **When two-layer decisions are on**: an extra UI block below the chat UI shows the output of the Jev model.
- **Low-confidence handling**: fall back to LLM per-move decisions (confidence-gated routing, consistent with the official TypeSafe pattern).
- **MCP two-layer form**: the Jev loop always runs inside the mod, and an external coding agent only does the planning layer (adjusting strategy through new MCP tools), ensuring the two ways of connecting have strictly identical experiences.
- **Trellis process**: create tasks + complete planning artifacts (prd + design + implement).

## Task map (subtasks)

| Subtask | Deliverable | Independently verifiable |
|---|---|---|
| `09-22-overlay-two-tab-reorg` | Overlay reorganized into Play/Settings tabs; the Play page embeds the mode slider, chat, decision log, Jev panel; connection merged into a settings subpage | Yes (pure UI + settings, independent of the Jev engine) |
| `09-22-jev-two-layer-engine` | JevClient / IActionDecider / option enumeration / strategy store / confidence gating wired into AgentLoop; planner tools on both MCP surfaces | Yes (engine + tests, with the UI leaving only a switch hook) |
| `09-22-play-session-persistence` | Save/restore chat and decision context per run; keep context when switching models | Yes (persistence layer, depends on the chat shape of subtask 1) |

**Implementation order**: subtask 1 (UI skeleton) and subtask 2 (Jev engine) can run in parallel; subtask 3 depends on the chat shape of subtask 1 landing first. Final integration review is done in the parent task.

## Cross-subtask acceptance criteria

- [ ] The overlay has only two top-level tabs, **Play / Settings**; the top of the Play page has a single-player <-> multiplayer mode slider, and only one mode is active at a time.
- [ ] Single-player mode: LLM play inside the overlay and external MCP coding agent play are equivalent in capability (the same state/action/decision log surface).
- [ ] Multiplayer mode: the AI joins a local/online multiplayer game as an independent player (keeping the local two-instance companion form) and plays in the same game as the other players.
- [ ] Each of the two modes has its own **two-layer decision switch**; when on, Jev performs the actual operations, and the LLM only does strategic planning and tunes Jev's strategy.
- [ ] When two-layer is on, Jev's output (chosen action, confidence, probability distribution) is shown below the chat.
- [ ] When Jev confidence is below the threshold it falls back to LLM per-move decisions without interrupting the whole run.
- [ ] In two-layer mode an external MCP agent acts as the planning layer through the `get_planner_briefing` / `update_play_strategy` tools; the C# native surface and the Python sidecar tool surface are aligned (`test_native_tool_alignment.py` passes).
- [ ] The chat/decision context of a run can be saved, restored after exit, and survives a model switch.
- [ ] All existing contract gates pass (OverlayTabContractTests / LocalizationTests / SourceShapeContractTests / api-facts / GameTaskBounding, etc.), and new code has matching unit tests.
- [ ] The `## Unreleased` section of `CHANGELOG.md` records all player-visible changes.

## Constraints

- Respect all the boundaries of `.trellis/spec/mod/`: the `IGameBridge` seam, the `GameThread.InvokeAsync` game thread boundary, no bare await (GameTaskBounding), the file line count ratchet (new code goes into new files), localization through `Loc.T` with English entries in `Loc.Strings.Ui.cs`, settings persistence through `SettingsStore` + `SettingsClone` + `EnsureValidShape`.
- A Jev HTTP call must never be awaited inside a game thread lambda; it must have an explicit timeout and cancellation propagation.
- Every Jev call counts toward `SessionBudgetGuard`; when usage is unknown keep it null (do not stuff 0).
- API backward compatibility: do not delete existing fields; new fields are nullable.
- The first version supports only Jev (`jev-latest`), but the execution layer abstraction (`IActionDecider`) does not lock in a vendor.

## Notes

- The reference implementation was cloned and analyzed: the `Policy` protocol of `typesafe-mario` (`choose(snapshot, actions) -> Decision`), `TypeSafePolicy` (a single call mixing Choice+Noul+Score), and the runner's async submission and JSONL decision records.
- TypeSafe API: `POST https://api.typesafe.ai/v1/systemone`, Bearer auth; questions support choice (criteria = option -> description, <=255 items) / score (2-10 levels) / noul (yes/no 0-1); Choice/Score return confidence; 429 needs backoff and respecting retry-after; input billed at $0.042/Mtok, output free.
