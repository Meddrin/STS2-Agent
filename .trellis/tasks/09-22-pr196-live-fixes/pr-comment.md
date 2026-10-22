## Live-test fix batch (fix/pr196-live-issues merged)

All issues found in live testing are fixed and merged into this PR. Static verification: all 737 C# tests green, all 416 Python tests green, preflight fully passed, mod built and deployed.

### Confirmed and fixed root causes

**Two-layer decisions not taking effect (severe)**
- Root cause: the Jev decider was built once when AgentRuntime was constructed, so entering the API Key after launch left it null forever and the two-layer branch was never entered. It is now rebuilt lazily every turn by configuration fingerprint.
- "Test Jev connection" was a placeholder stub that sent no request; it now verifies with a real GET /v1/models.
- The decision log now records actions executed by Jev as source=jev (with confidence), and the Jev panel shows the latest choices and probability distribution (previously dead UI).
- StrategyPlanner was dead code (never instantiated); it is now wired in: it refreshes the strategy in the background on a screen/phase change or on consecutive low confidence.

**Loot instantly picks the first card (severe)**
- Root cause: when resolve_rewards / collect_rewards_and_proceed carried no option_index, the game side silently took the first card, and the playbook explicitly said to prefer the macro action. Now: when no choice is carried, the flow stops at the card choice screen (pending_card_choice=true, returns pending), and the model decides explicitly via choose_reward_card / skip_reward_cards. The Jev enumerator likewise expands resolve_rewards into one option per card plus an explicit skip. ActIndexValidator now validates resolve_rewards indices against reward.cards and lets the -1 skip sentinel through.

**Hangs after a few turns with no message (severe)**
- Root cause A: GameThread.InvokeAsync had no timeout/cancellation, so when the game thread did not pump, the turn hung forever and even pausing could not interrupt it. Added a timeout/cancellation overload and wired it into state reads and actionable waits.
- Root cause B: the LLM request had a default 10-minute hard timeout and the status bar froze in the meantime. The status bar now shows the current phase (reading state / waiting for actionable / requesting Jev / requesting model) + seconds waited; the per-request LLM/Jev timeout can be tuned in settings.
- Root cause C: WaitingForGame spun endlessly at 1 second and the UI called it "normal waiting". It now halts after 120 consecutive waits (about 2 minutes) and gives a visible reason.
- Exceptions no longer reduce to just a message: the exception type is kept.

**UI batch**
- Thinking content (tick "show thinking content") and every action decision enter the chat flow as bubbles; the old "Thinking" card is removed. The real reasoning_content is no longer overwritten by the one-line sentence in the act parameters.
- Chat freeze fix: RefreshDynamic is fully wrapped in try/catch (previously any exception made the chat stop updating forever with the exception swallowed), the 800ms tick also refreshes the chat, the cap is unified at 200 entries with an "N omitted" notice, and it auto-scrolls to the bottom.
- The "allow auto-play" option is removed (chat is read-only; saying "play for me" still triggers a single-shot play; use auto-play/single step for continuous play).
- Model settings: every model card has a "Test" button (real ping + a tool-calling probe with tool_choice=required), and a pass shows a check badge; the main model is merged into one (shared by chat and play), the vision model moves to advanced options; the "tool calling" checkbox is removed (the probe result decides); reasoning effort and reply language (Chinese/English/follow the player) move to the play page's chat card.
- First-run setup guidance is rewritten for the two-tab UI (it no longer points to the removed "AI teammate" tab).

### pi agent evaluation conclusion
pi is a TypeScript/Node toolkit and cannot be embedded in the game process; introducing it as a sidecar violates the zero-player-dependency goal. This round adopts the key points of its event stream design (phased heartbeat, explicit error surfacing, every wait cancellable with a deadline), and the full event stream refactor is a separate task. See .trellis/tasks/09-22-pr196-live-fixes/plan.md for details.

### Live retest note
**You must use the DLL rebuilt from this branch and restart the game**: release builds of v0.15.0 and earlier do not contain the two-layer decision code. See the end of plan.md for the retest checklist.
