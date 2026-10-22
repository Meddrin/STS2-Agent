# Implementation plan: UI reorg (Play/Settings tabs and mode slider)

## Prerequisites

- Branch: cut the feature branch `feature/overlay-two-tab-reorg` from `dev`.
- Read the UI/settings sections of `.trellis/spec/mod/agent-and-ui.md` and `architecture.md` (done).
- Close the game before building (DLL lock).

## Steps

### 1. Settings and persistence (Config layer, first)
- [x] `AgentSettings.cs` adds 8 fields (OverlayPlayMode/ShowThinkingInChat/DualLayerSoloEnabled/DualLayerCoopEnabled/JevBaseUrl/JevApiKey/JevModel/JevConfidenceThreshold).
- [x] `SettingsClone.cs` copy list synced.
- [x] `AgentSettings.EnsureValidShape` normalizes the new fields.
- [x] Unit tests: new field round-trip, clone coverage, threshold clamping (`Settings.DualLayerFieldsNormalized` + extended `SettingsClone.CarriesEditableFields`).

### 2. Tab catalog shrink
- [x] `OverlayTabCatalog.cs`: delete dual/chat/connect/decisions, keep play (Play, RefreshOnShow=true)/settings (Settings).
- [x] `AgentOverlayHost.Tabs.cs`: `BuildPageForTab` shrinks to two arms; remove the `_chatFooter.Visible` control.
- [x] `AgentOverlayHost.cs`: default tab logic changed to first run -> settings, otherwise -> play; the `ShowTab` call sites stay in the base file; the tick is merged under Play visibility.

### 3. New UiFactory control
- [x] `SegmentedSwitch` (mode slider, `Ui/SegmentedSwitch.cs`): mutually exclusive pills, the selected one solid, callback onSelect(index), `SetSelected` for refresh repaint.
- [x] Confirm `RepaintForPalette` covers the new control type (segment buttons reuse TabButton=Button and go through the existing Button arm; the container itself has no style).

### 4. Play page rewrite (BuildPlayPage)
- [x] Mode slider on top + content area containers (`_soloSection`/`_coopSection` mutually exclusive visibility).
- [x] Single-player area: migrate the current play page control bar + metrics; merge in the chat card (chat log + input + checkboxes + send/clear) + the "show thinking content" checkbox.
- [x] Multiplayer area: migrate the entire current dual page content (`BuildCoopSection`, no longer carrying its own scroll).
- [x] Jev panel card: container + empty state (the data hook is left to subtask 2).
- [x] Decision log card: migrate the decisions page content (`BuildDecisionCard`), with the RefreshDecisionPage logic staying in RefreshDynamic.
- [x] Two-layer switches: the single-player area binds `DualLayerSoloEnabled`, the multiplayer area binds `DualLayerCoopEnabled`.
- [x] The page builder stays in Pages.cs (after moving BuildConnectSection to Settings.cs it drops back to 861 lines, within budget).

### 5. Settings page extension
- [x] Add Jev and Connection anchors to the jump bar.
- [x] Jev section form (BaseUrl/ApiKey/Model/threshold/test button -> `AgentRuntime.TestJevConnectionAsync` configuration validation stub, with the real ping left to subtask 2).
- [x] Connection subpage: migrate the connect page content (`BuildConnectSection` moved into Settings.cs).
- [x] The harvest chain writes back the new fields (the four Jev fields; mode/thinking/two-layer switches are saved immediately through their own Toggled handlers).

### 6. Localization
- [x] All new Chinese text gets `Loc.T` + English entries (UI strings go into `Loc.Strings.Ui.cs`, AgentRuntime strings into `Loc.Strings.Runtime.cs`).

### 7. Contract/test updates
- [x] `OverlayTabContractTests` rewritten (tab count = 2, order, ownership, RefreshOnShow, the decision log tick changed to Play).
- [x] The tick assertion of `CoopRouteTests.cs:168` synced (Dual->Play).
- [x] `CatalogLabelsResolveToEnglish` checked (both tab labels have English entries).
- [x] `OverlayLayoutContractTests` synced (BuildDualPage->BuildCoopSection, BuildChatFooter->BuildChatCard, BuildDecisionPage->BuildDecisionCard, _mcpStatus ownership Pages->Settings, CoopChatBox changed to "no longer carries its own scroll").
- [x] Settings-related tests cover the new fields (see step 1).
- [x] The "Connect tab" error text in `Router.cs`/`NativeMcpServer.cs` changed to point to the Connect section of the settings page.

## Verification

```powershell
dotnet run --project STS2AIAgent.Tests/STS2AIAgent.Tests.csproj   # C# contract/unit tests
powershell -ExecutionPolicy Bypass -File "scripts/build-mod.ps1" -Configuration Release   # build (close the game first)
```

- [x] C# tests all green (0 failures).
- [x] `dotnet build STS2AIAgent.csproj` 0 warnings 0 errors.
- [x] MCP Python tests all green (416 passed).
- [ ] Release build + DLL/PCK deployment (the game must be closed first).
- [ ] Live: the overlay has only two tabs; the mode slider switches; single-player/multiplayer features do not regress; the Jev section and Connection subpage of the settings page work.

## Rollback points

- Step 1 is independently revertible (pure settings fields, backward compatible).
- Steps 2-7 are one whole UI change, and rollback = revert the matching commits.

## Review gates

- After step 1: continue only after the settings layer self-test passes.
- After step 7: trellis-check full.
