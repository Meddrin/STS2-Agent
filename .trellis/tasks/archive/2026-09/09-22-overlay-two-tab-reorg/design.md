# Design: UI reorg (Play/Settings tabs and mode slider)

## Current state (from the UI baseline report, line numbers match the current workspace)

- 6 tabs: `dual` (AI teammate, RefreshOnShow) / `chat` (chat) / `play` (play) / `settings` (settings) / `connect` (connection) / `decisions` (decision log, RefreshOnShow), defined at `Ui/OverlayTabCatalog.cs:39-47`, dispatched at `Ui/AgentOverlayHost.Tabs.cs:51-64`.
- Page builders are in `Ui/AgentOverlayHost.Pages.cs` (chat:43 / play:99 / dual:167 / connect:282 / decisions:323) and `Ui/AgentOverlayHost.Settings.cs:62`.
- Global chrome: the header (status chip, API row, hide button, drag), the edge "AI" button, the chat footer (Pages.cs:66-88, visible only on the chat tab, controlled at Tabs.cs:88).
- Refresh: a single `RefreshDynamic` (Pages.cs:487-724) + an 800ms tick that refreshes the currently visible tab in a targeted way (AgentOverlayHost.cs:824-888) + `RefreshesOnShow`.
- Contracts: `OverlayTabContractTests` pins tab count = 6, order, catalog <-> builder correspondence, builder owner file, and the `ShowTab("dual"/"play"/"settings")` call sites in the base file; `CoopRouteTests.cs:168` pins `IsTabVisible(OverlayTabCatalog.Dual)` in the tick; `CatalogLabelsResolveToEnglish` pins English entries for the tab labels.

## Target structure

### OverlayTabCatalog (changed to 2 entries)

| id | Label | RefreshOnShow | Notes |
|---|---|---|---|
| `play` | Play | true | Mode slider + chat + decision log + Jev panel (content changes with state that has no event source, so it must be re-read on entry) |
| `settings` | Settings | false | Existing settings + Jev section + connection subpage |

- Delete the four id constants and catalog entries `dual` / `chat` / `connect` / `decisions`.
- `BuildPageForTab` shrinks to two arms: `play => BuildPlayPage()`, `settings => BuildSettingsPage()`.
- Contract tests are rewritten in sync: tab count = 2, new order, `ShowTab` call sites, builder ownership.

### Play page (BuildPlayPage rewritten)

Layout (440px panel, vertical Scroll):

1. **Mode slider**: `UiFactory` gains `SegmentedSwitch(options, selectedIndex, onSelect)` (two mutually exclusive pill buttons, the selected state solid, reusing the TabButton style). The selection is persisted to `AgentSettings.OverlayPlayMode` (`"solo"`/`"coop"`, default `"solo"`).
2. **Single-player area** (visible when `OverlayPlayMode == "solo"`):
   - Control bar card: start/pause `_playToggle`, single step `_stepButton`, three metric tiles (screen/last action/Token), reusing the current play page implementation (Pages.cs:99-133).
   - Chat card: `_chatLog` (Rich, ScrollFollowing) + input + send/clear + two checkboxes (attach state/screenshot); on 2026-09-23 the user confirmed that play-on-behalf relies only on explicit phrases and the authorization checkbox is not restored; in-run instructions affect only later turns; merged from the chat page and the chat footer; adds a "show thinking content" checkbox `_showThinking` (persisted as `AgentSettings.ShowThinkingInChat`, default false), which when on renders `AgentTurnResult.Reasoning` into the chat stream.
   - Decision rendering: append the tool call and result of each act into `_chatLog` as chat bubbles (source tag `agent_loop`/`jev`), making "chat is the decision stream".
3. **Multiplayer area** (visible when `OverlayPlayMode == "coop"`): the whole content of the current dual page moves in (invite/continue/pause/resume/auto character select/team status/team chat, Pages.cs:167-261).
4. **Jev panel card** (visible when the two-layer switch of the current mode is on): `_jevStatus` (connection/configuration status), `_jevLastChoice` (chosen action + confidence + latency), `_jevProbabilities` (probability distribution, Wrapped/Rich). Data comes from the latest Jev decision snapshot exposed by `AgentRuntime` (subtask 2 provides read-only properties such as `AgentRuntime.LastJevDecision`; this task first builds the container and the "two-layer decisions not enabled" empty state).
5. **Decision log card** (collapsible): two usage tiles + the `_decisionLog` list, moved in from the decisions page (the RefreshDecisionPage logic of Pages.cs:323-412 merges into RefreshDynamic).

### Settings page (BuildSettingsPage extended)

- Add section anchors to the jump bar: `Jev`, `Connection`.
- **Jev section**: `JevBaseUrl` (Line, default `https://api.typesafe.ai`), `JevApiKey` (Line secret), `JevModel` (Line, default `jev-latest`), `JevConfidenceThreshold` (Line, double 0-1, default 0.35, range validated on harvest), a "Test Jev connection" button (calls `AgentRuntime.TestJevConnectionAsync`, provided by subtask 2; this task may first hang a disabled placeholder).
- **Connection subpage**: the current connect page content (Pages.cs:282-318) moves in as a card group inside the settings page: MCP switch `_mcpToggle`, status `_mcpStatus`, address/config copy (visible only when McpRunning).

### Handling of the chat footer

The current chat footer is global chrome visible only on the chat tab (Tabs.cs:88). After the chat tab is deleted, the footer's input area merges into the chat card of the play page (the global footer is no longer used), and the `_chatFooter.Visible` logic is removed.

## New settings (AgentSettings)

| Field | Type | Default | Purpose |
|---|---|---|---|
| `OverlayPlayMode` | string | `"solo"` | Mode slider of the play page |
| `ShowThinkingInChat` | bool | false | Whether chat renders thinking content |
| `DualLayerSoloEnabled` | bool | false | Single-player two-layer switch (consumed by subtask 2) |
| `DualLayerCoopEnabled` | bool | false | Multiplayer two-layer switch (consumed by subtask 2) |
| `JevBaseUrl` | string | `https://api.typesafe.ai` | Jev endpoint |
| `JevApiKey` | string | `""` | Jev key (secret) |
| `JevModel` | string | `jev-latest` | Jev model |
| `JevConfidenceThreshold` | double | 0.35 | Confidence gating threshold |

Each field needs: copy in `SettingsClone.Clone` + normalization in `EnsureValidShape` (threshold clamped to 0..1, model non-empty, url trailing slash stripped) + harvest write-back + Watch marking dirty.

## Contract/gate sync checklist

- `OverlayTabContractTests`: tab count 6->2, order, catalog <-> builder, `ShowTab` call sites, builder owner file.
- `CatalogLabelsResolveToEnglish`: English entries for the new labels "Play" and "Settings" (reusing existing ones is fine, confirm they exist).
- `CoopRouteTests.cs:168`: change `IsTabVisible(OverlayTabCatalog.Dual)` in the tick to the equivalent check on the multiplayer area of the play page (or per the new id).
- `LocalizationTests.EveryCallSiteHasAnEnglishEntry`: add English for all new `Loc.T` Chinese text.
- `SourceShapeContractTests`: new files get the default 1000-line budget; if the play page builder exceeds it, split off a new partial `AgentOverlayHost.PlayPage.cs` (the settings page is the precedent).
- `SettingsExperienceRegressionTests` / `PlayerExperienceTests`: round-trip and clone coverage for the new settings.

## Compatibility and rollback

- Pure UI/settings change, touching neither the API nor the decision chain; when an old settings file lacks the new fields `EnsureValidShape` fills defaults, so it is backward compatible.
- Rollback = revert this task's commit; no destructive persistence format change (new fields are nullable/have defaults).

## Explicitly not done (to avoid scope creep)

- Do not implement the Jev engine and planner (subtask 2).
- Do not implement persisting chat sessions to disk (subtask 3); this task only merges the chat UI into the play page.
- Do not change the MCP tool surface.
