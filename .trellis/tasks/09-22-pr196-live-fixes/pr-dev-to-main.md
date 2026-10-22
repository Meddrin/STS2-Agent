## Two-mode overlay + Jev two-layer decisions + session persistence + live fix batch

This PR merges a batch of work verified on dev into main; the core is PR #196 (already merged into dev) and the live-test fix batch after it.

### New features
- **Two-tab overlay reorg**: Play/Settings tabs, a single-player/multiplayer mode slider, and section jumps on the settings page.
- **Jev two-layer decision engine**: TypeSafe Jev (System One) executes actions step by step while the LLM only does low-frequency strategic planning; confidence gating, falling back to LLM per-move decisions on low confidence/failure; `GET`/`POST /strategy` + MCP `get_planner_briefing`/`update_play_strategy` with parity across both surfaces.
- **Run session persistence**: each run's chat history, decision memory, and play strategy are persisted per run and restored after "continue game".

### Live-test fix batch (the focus of this batch)
- **Two-layer decisions really take effect**: the Jev decider used to be built once at construction (entering the Key after launch never took effect) and is now rebuilt lazily every turn by configuration fingerprint; "Test Jev connection" changed from a placeholder stub to a real ping; the decision log records `source="jev"`; the Jev panel is wired to real values; StrategyPlanner is wired in (it was dead code).
- **Loot no longer instantly picks the first card**: when `resolve_rewards`/`collect_rewards_and_proceed` carry no choice they stop at the card choice screen and wait for an explicit decision instead of silently taking the first card; the Jev enumerator expands into one option per card + an explicit skip.
- **Auto-play hangs are visible and bounded**: all game thread calls are cancellable with a deadline (previously they could hang forever and pause had no effect); the status bar shows the current phase + seconds waited; waiting 2 minutes in a row halts and gives a visible reason; the LLM/Jev per-request timeout is configurable; exceptions keep their type.
- **Chat is the decision stream**: thinking content (toggleable) and every action decision enter the chat as bubbles; fixed the chat freeze (RefreshDynamic exceptions were swallowed); cap of 200 entries + omitted hint + auto-scroll to bottom; removed the redundant "allow auto-play".
- **Model settings refactor**: a "Test" button on every model card (real ping + tool-calling probe) + a check badge; main/play models merged into a single main model; vision model moved into advanced options; the "tool calling" checkbox removed (the probe decides); reasoning effort and reply language (Chinese/English) moved to the play page's chat card; first-run setup guidance rewritten for the two-tab UI.

### Verification
- All 737 C# core tests green; all 416 Python MCP tests green; preflight-release fully passed.
- The mod is built and deployed; see `.trellis/tasks/09-22-pr196-live-fixes/plan.md` for the live retest checklist.
- Note: release builds of v0.15.0 and earlier contain no two-layer decision code, so the retest must use the DLL built from this branch.
