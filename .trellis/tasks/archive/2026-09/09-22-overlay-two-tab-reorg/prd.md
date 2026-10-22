# UI reorg: Play/Settings tabs and mode slider

## Goal

Reorganize the in-game overlay from 6 scattered tabs (dual/chat/play/settings/connect/decisions) into two top-level tabs, **Play / Settings**. The Play page is chat-centered and embeds the mode slider (single-player <-> multiplayer), play controls, the decision log, and the Jev panel; the Settings page holds model/endpoint/budget/appearance and takes connection (MCP) as its subpage.

## Requirements

### Top-level structure
- Only two top-level tabs remain: `play` (Play) and `settings` (Settings). `OverlayTabCatalog` shrinks to two entries, and `BuildPageForTab` of `AgentOverlayHost.Tabs.cs` has two matching arms.
- Default landing tab: first run -> settings; otherwise -> play (the companion instance keeps its current logic).

### Play page (play)
- A **mode slider** (segmented control) at the top: `Single-player` <-> `Multiplayer`, only one active at a time; switching swaps the whole content area. The mode choice is persisted (a new setting).
- **Single-player mode content**:
  - Play control bar: start/pause auto-play, single step, current status (the three metrics screen/last action/Token are kept).
  - **Chat area** (merged from the chat tab): the chat stream directly renders model decision information and operation results (LLM tool calls, act results, reply text); provides a "show thinking content" switch (off by default).
  - Chat input box + send + clear + two checkboxes "attach current state" and "attach screenshot". On 2026-09-23 the user confirmed that the "allow play on behalf" checkbox is not restored: idle chat may play one move on behalf only when the intent is stated explicitly, and during auto-play chat serves only as a later decision instruction and does not play cards concurrently. See [the archived review-fix requirements](../09-23-audit-remediation/prd.md).
  - You can send messages in the chat directly to influence decisions and pause/resume play.
- **Multiplayer mode content** (merged from the dual tab):
  - Invite AI teammate / continue last co-op run / pause teammate / continue play / disable-auto-character-select switch.
  - Team status card (session headline/detail/next, teammate live chip).
  - Team chat (teammate chat log + input + send).
- **Decision log area** (merged from the decisions tab): usage tiles (this session/this run) + a decision record list (newest first). It can be a collapsible section inside the Play page.
- **Jev panel**: when the two-layer decision switch of the current mode is on, show the Jev output (chosen action, confidence, probability distribution, latency) below the chat area; hidden when the switch is off. The data hook of the panel is provided by subtask 2; this task only builds the UI container and the empty state.
- **Two-layer decision switch**: the Play page shows the matching switch for the current mode (single-player -> `DualLayerSoloEnabled`, multiplayer -> `DualLayerCoopEnabled`), with the switch state persisted; when Jev is not configured a "configure Jev in Settings" hint appears next to the switch.

### Settings page (settings)
- Keep the existing: first-run setup guidance, appearance (theme), endpoint card, model card, model role settings, advanced options (hotkeys/budget/proactive speech/window position). Afterwards `.trellis/tasks/09-22-pr196-live-fixes/plan.md:30-35` explicitly merged chat and play into a single main model; on 2026-09-23 the user confirmed keeping that newer decision and not restoring a separate PlayModelId selector.
- Add a **Jev section**: BaseUrl (default `https://api.typesafe.ai`), API Key (secret), Model (default `jev-latest`), confidence threshold, and a "Test connection" button. Section anchors are added to the jump bar.
- Add a **Connection subpage** (merged from the connect tab): the MCP service switch, status, address/config copy, and explanatory text. Presented as a subsection or subpage within the settings page.

### Removed
- The four top-level tabs `chat` / `dual` / `connect` / `decisions` and their catalog entries and page builder arms are removed; their content is merged into play / settings as described above.

## Acceptance Criteria

- [ ] The overlay has only the two tabs Play / Settings; the length of `OverlayTabCatalog.Tabs` is 2.
- [ ] The mode slider at the top of the Play page switches the single-player/multiplayer content area, the choice is persisted and kept after the overlay restarts.
- [ ] In single-player mode: start/pause/single step work; the chat area can send and receive messages, show decisions and operation results, and toggle the thinking content display.
- [ ] In multiplayer mode: invite/continue/pause/resume teammate and team chat all work (no feature regression).
- [ ] The decision log and usage are visible inside the Play page and refresh with the tick.
- [ ] The two-layer switches are persisted independently per mode; there is clear guidance when Jev is not configured.
- [ ] The Settings page contains the Jev section and the Connection subpage; the MCP switch/address copy features do not regress.
- [ ] `OverlayTabContractTests`, `LocalizationTests` (including `CatalogLabelsResolveToEnglish`), and `SourceShapeContractTests` all pass; the contract assertions of the deleted tabs are updated in sync.
- [ ] All new UI text goes through `Loc.T` and has English entries in `Loc.Strings.Ui.cs`.
- [ ] The new settings enter `AgentSettings` + `SettingsClone` + `EnsureValidShape` + the harvest chain.

## Notes

- For technical constraints see the UI baseline report: three-place tab sync, 440px panel width, dynamic text must use `UiFactory.Wrapped`, a single `RefreshDynamic` + tick backstop, the file line count ratchet (new page builders go in Pages.cs or a new partial, not back into the base file).
- `CoopRouteTests.cs:168` pins `IsTabVisible(OverlayTabCatalog.Dual)` in the tick, so a tab id change must be synced.
- This task only does the UI and settings hooks; the Jev engine and planner tools are in subtask 2.
