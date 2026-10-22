# Live Acceptance Record (Five-Goal Candidate Version)

> Historical snapshot: this file is the isolated live acceptance record of 2026-09-10 and does not represent the current state; see [PRODUCT_PLAN_CURRENT.md](../PRODUCT_PLAN_CURRENT.md) for the current status.

Record date: 2026-09-10. Scope: the Release candidate for this cycle's five goals (dependency cleanup, proactive speech and tone, the docs/api.md action contract, archiving stale documents, verification gates), accepted on an isolated game copy with offline gates + real-game end-to-end runs, closing out the last live validation item for proactive speech.

## Candidate and Environment

| Item | Value |
| --- | --- |
| Mod version | 0.10.5 |
| Candidate DLL SHA256 | 4C2092EFC771BF435440B426989B5E07D8CB5689156CEFA7A06F971B8109D420 |
| Candidate PCK SHA256 | 73AA0233254B7B79B494E20606A82DA126A3DACED331C1D9B1323A032F90DD38 |
| Game version | v0.111.0 |
| Isolated copy | build/validation-2026-09-08/game/SlayTheSpire2.exe (exe SHA256 8602C26BFFD2937E3841835FD8360EF8E974624A543E05977229FD3D062BE231) |
| Launch arguments | --windowed --force-steam off --clientId 2026091001 (offline, does not write the Steam profile) |
| API | http://127.0.0.1:8080, debug actions enabled |
| settings during acceptance | build/validation-2026-09-10/settings.json (the endpoint points at the dead port 18099; no real model was configured throughout) |
| Service log | %APPDATA%/SlayTheSpire2/logs/godot.log (later launches overwrite it; the key conclusions are excerpted in this document) |
| Raw evidence directory | build/validation-2026-09-10/ (build is gitignored; the suite logs are in the suites/ subdirectory) |

## Offline Gates

| Check | Command | Result |
| --- | --- | --- |
| Verification gates | python scripts/check_verification_gates.py | exit 0; api-doc 55 actions consistent, doc-marks 7 entries, lockfile 5 entries |
| Gate self-test | powershell -File scripts/test-verification-gates.ps1 | exit 0; all drift cases PASS |
| Release preflight | powershell -File scripts/preflight-release.ps1 | exit 0 |
| C# core unit tests | dotnet run --project STS2AIAgent.Tests | 210 PASS / 0 FAIL (re-verified this cycle) |
| MCP unit tests | uv run --with pytest python -m pytest tests/ -q | 49 passed, 61 subtests passed (re-verified this cycle) |

This round also verified and committed the uncommitted gate enhancement in the working tree (the version-constraint solving in scripts/check_verification_gates.py and scripts/test-verification-gates.ps1: supports >=, <, ~=, ^, and so on, with real comparison against uv.lock / package-lock.json / pyproject.toml).

Additional finding: the test command uv run pytest tests/ -v recorded in AGENTS.md and mcp_server/README cannot be executed on a clean checkout: pytest is not declared in mcp_server/pyproject.toml, so uv run pytest fails directly with Failed to spawn: pytest / program not found (uv run python -m pytest likewise gives No module named pytest). This time uv run --with pytest python -m pytest tests/ -q was used, giving 49 passed. This is a gap in goal five, "reproducible verification".

## Live Suite Results

An isolated instance on port 8080; the raw suite output is saved in build/validation-2026-09-10/suites/. 13 were run in total: 10 passed, 3 failed, 1 not run.

| Suite | Result | Notes |
| --- | --- | --- |
| mod-load | PASS | mod_version 0.10.5, status ready |
| state-summary | PASS | MAIN_MENU, actions readable |
| state-invariants | PASS | 4 actions, 0 failure / 0 warning |
| debug-console-gating --enable-debug-actions | PASS | help returns the full command table; tool registration gating is correct |
| mcp-tool-profile | PASS | guided 10 tools |
| new-run-lifecycle | PASS | IRONCLAD -> MAP -> die -> GAME_OVER -> MAIN_MENU, 11 seconds |
| bootstrap-active-run | PASS | Run set up to MAP |
| deferred-potion-flow | PASS | LIQUID_MEMORIES selection flow; use_potion returns pending, and after card selection it returns to COMBAT |
| target-index-contract | PASS | target_index_space / requires_target contract correct |
| enemy-intents-payload | PASS | BYRDONIS SWOOP_MOVE, damage 17, single hit |
| combat-hand-confirm-flow | FAIL | See finding 1 |
| assert-active-run-main-menu | FAIL | See finding 2 (open_timeline unavailable) |
| main-menu-active-run | FAIL | See finding 2 (same breakpoint) |
| multiplayer-lobby-flow | Not run | See "Not Covered" |

Outside the suites, this round also walked through these action chains by hand (all on the isolated instance): abandon_run + confirm_modal, save_and_quit (succeeded both in combat and on the map), continue_run, choose_map_node -> COMBAT, confirm_selection (back to COMBAT after confirming a card selection), and the timeline triple open_timeline -> confirm_timeline_overlay -> close_main_menu_submenu. The console commands help, room, card, potion, die, win all worked.

## Live Findings

### 1. combat-hand-confirm-flow Failed (a Pre-existing Regression within the Baseline, Not Introduced This Cycle)

Symptom: after injecting and playing PURITY it enters CARD_SELECTION, and when one card is selected the interface returns status=completed, stable=true while the UI is still waiting for confirmation (selection.requires_confirmation=true, can_confirm=true, selected_count=1). The suite expects pending to be kept after selecting a card, so the assertion failed.

Location: in WaitForCombatHandSelectionStepAsync in STS2AIAgent/Game/GameActionService.cs, when the selected count changes it directly does return true. In commit 52c3467 that introduced this function (the same commit as the suite) this spot was return false; the baseline cdd820a (the last accepted build) and the current HEAD are both return true, so the change happened before the start of this cycle. Comparison: the similar multi-select use_potion still returns pending (deferred-potion-flow passes), so the two semantics are inconsistent.

Impact: callers cannot use stable to tell that "the selection is still waiting for confirmation" and can only read the selection field again. The fix requires rerunning a full multiplayer run (the auto-play flow since dd1cfe6 depends on this function), so this round only recorded it and did not change the code.

### 2. The Main Menu Does Not Expose open_timeline When a Run Save Exists (Difference Between the Suite and the Game's Behavior)

Symptom: when the main menu has continue_run/abandon_run, the action set is switch_profile, continue_run, abandon_run, invite_ai_teammate; with no save it is switch_profile, open_character_select, open_timeline, invite_ai_teammate. Both assert-active-run-main-menu and main-menu-active-run assert that open_timeline must be available when a save exists, so they cannot pass.

Location: CanOpenTimeline requires the timeline button to be visible and enabled. The same lookup logic returns normally when there is no save (open_timeline is available), showing that the node lookup is not broken, but that game v0.111.0 simply does not expose the timeline entry when an in-progress save exists.

Substitute coverage: with no save in the main menu, the three actions open_timeline, confirm_timeline_overlay, and close_main_menu_submenu were manually verified to all complete. choose_timeline_epoch is unavailable on a brand-new profile in any case (the timeline goes straight to the confirmation overlay), which is a profile state limitation.

### 3. The One-Time Tutorial Popups of a Brand-New Profile Make the Suite Fail on Its First Run (Environment Precondition)

Two measured cases: on first entering the map, NMapSelectFtue made run_console_command die get rejected (invalid_action: A run does not appear to be in progress); on first obtaining a potion, NObtainPotionFtue stays up permanently, making use_potion unavailable for the whole wait. Both stop appearing once that profile has consumed them once, and a rerun then PASSes (as new-run-lifecycle and deferred-potion-flow both did). It is advisable to do a warm-up before the acceptance flow, or to resolve the FTUE once uniformly before waiting for actions to become available.

### 4. An In-Progress Run Is Written to Disk Only on Naturally Entering a Node (Process Knowledge, Affects Existing Scripts)

Measured: when a bootstrap-built run stops on the MAP, neither killing the process nor save_and_quit writes current_run.save, and the main menu has no continue_run; after first entering a node naturally with choose_map_node, current_run.save (41801 bytes) and the progress update appear immediately, and after that save_and_quit gives a main menu with continue_run. The debug command room Monster jumping rooms does not trigger that save hook.

Therefore Ensure-ActiveRunMainMenu in scripts/test-full-regression.ps1, with its "build run -> kill -> restart and expect continue_run", does not hold in this environment and should become "build run -> pick one map node naturally -> save_and_quit".

### 5. Observation: The Proactive Speech Situation Key Is Not Reset with the Play Session

AgentRuntime's _proactiveSituationKey is written only on observation and is not cleared when the auto-play session ends. So pausing and then resuming auto-play in the middle of a combat will not trigger CombatStart again; one has to leave the combat and enter again. This is a behavioral trade-off rather than a fault, recorded for later judgment.

### 6. Observation: The Stop Reason Classification Is Inaccurate

When auto-play stopped because "the model gave no executable action", stop_kind in /health reported run_end, which does not match the actual cause (not a run boundary). It reproduces reliably when the stub model returns an illegal action.

## Live Validation of Proactive Speech (Closing Out Goal Two)

Method: a local zero-cost OpenAI-compatible stub (build/validation-2026-09-10/stub-model-server.py, supports SSE, accounting per request), with the configuration proactiveChatEnabled=true, proactiveChatTone=friendly, supportsTools=false (so the JSON fallback path takes effect), touching neither the real model endpoint nor the budget.

Evidence (build/validation-2026-09-10/proactive-chat-live-ledger.jsonl, with the /health readings of the same period):

- Auto-play advanced on its own driven by the stub: collect_rewards_and_proceed -> choose_map_node option_index=0 -> entered COMBAT -> consecutive end_turn, with play_running=true and no interruption.
- A proactive speech request appeared within the same session: stream=true, message_count=8, tool_count=0 (read-only chat path), and the user message was exactly A fight just started. Say one short line to your teammate about how you want to handle it.
- The tail of that system prompt contains Voice: warm co-op partner. Encouraging, first person, and still concrete. plus the shared rules, i.e. the friendly tone in settings was indeed injected into the request.
- Right after that request there were still normal end_turn requests, showing the session was not interrupted by an error from the proactive speech.

Conclusion: the COMBAT-transition trigger, prompt construction, tone injection, read-only chat path, and reply consumption all hold in the real game, at zero model cost.

## Not Covered

- multiplayer-lobby-flow: the Python entry depends on scripts/start-game-session.sh (unavailable on Windows), and the PowerShell version scripts/test-multiplayer-lobby-flow.ps1 has no isolated copy parameter and by default points at the official Steam install. To protect the player's real saves, it was not run this round; it is advisable to add --exe-path/--game-root parameters to that script first and then fill this in.
- choose_timeline_epoch: a brand-new profile has no selectable epoch, which is a profile state limitation.
- Real model connectivity and the cost path: this round used the stub, called no real upstream, and consumed no budget.

## Real Saves and Deployment State

- Re-check of the protection snapshot (steam/76561198420578597, default/1, default/1001, %APPDATA%/STS2AIAgent, 197 files in total): 0 changed, 0 missing. The real saves and model configuration were not modified by this acceptance.
- The isolated instance wrote only default/2026091001 and the game log.
- The official install directory's mods/STS2AIAgent.dll currently has SHA256 47CE0F90... (rebuilt and deployed from the merged main on 2026-09-11, see the next section); the previous version 4C2092EF... that it overwrote is backed up in build/backup-steam-mods-2026-09-11/ and can be swapped back directly if a rollback is needed.

## Conclusion and Recommendations

The results of this round's five goals introduced no regression: the changed files (ProactiveChatPolicy, SettingsClone, docs/api.md, scripts and gates) are none of them on the three failing paths above; all three failures can be reproduced on the baseline build or in the game's own behavior.

Recommended follow-up actions, by priority:

1. Fix the return true in WaitForCombatHandSelectionStepAsync (change it back to pending semantics), then rerun the multiplayer full-run acceptance to confirm that the dd1cfe6 flow is unaffected.
2. Align the open_timeline assertion in assert-active-run-main-menu and main-menu-active-run, or cover the timeline flow separately on the no-save main menu.
3. Change the active-run bootstrap in test-full-regression.ps1 to "natural node + save_and_quit", and add an FTUE warm-up step.
4. Add the isolated copy parameters to the multiplayer lobby PowerShell script to restore the suite's runnability.
5. Declare pytest in mcp_server/pyproject.toml (a dependency group or optional dependency), or change the documented command to uv run --with pytest, so a clean checkout can run the tests as documented.

## Follow-up Fixes and Re-verification (2026-09-11)

The fixable items ranked first in the conclusions above have landed; the change and the evidence for each are listed below.

| # | Fix | Evidence |
| --- | --- | --- |
| 1 | Stop reason classification: added the pure policy class STS2AIAgent/Agent/StopKindPolicy.cs, where run_end matches only the two real messages of CurrentRunBoundary | 3 new unit tests (StopKind.RetryIsNotRunEnd / BoundaryIsRunEnd / OtherKindsSurvive), C# up from 210 to 213 PASS / 0 FAIL; live reproduction: after the stub model gave illegal actions three times in a row, stop_kind in /health changed from run_end to failed |
| 2 | Verification gates: the frozen versions in uv.lock / package-lock.json are really solved against constraints such as >=, <, ~=, ^ | Gate exit 0, self-test exit 0, release preflight exit 0 |
| 3 | Documented command: changed the MCP unit test command from uv run pytest to the one CI actually uses, uv run --locked python -m unittest discover -s tests -v | The same CI command gives 49 passed locally; pytest is neither declared nor supported, so no dependency was added |
| 4 | test-full-regression.ps1: the run bootstrap now "naturally advances one map node + save_and_quit", and Close-MainMenuOverlay was added to handle the timeline overlay that pops up automatically after the first death | Ensure-ActiveRunMainMenu was extracted and run against a live instance, and both branches passed (popup + unsaved run 7.1 seconds; new run from an empty main menu 10.2 seconds), both reaching a main menu with continue_run |
| 5 | One-time tutorial popups: the suite can now automatically dismiss the FTUE while waiting for actions, console commands support bounded retry, and settle_game_over / settle_main_menu were added | On a brand-new profile (clientId 2026091007) the five suites new-run-lifecycle, bootstrap-active-run, deferred-potion-flow, target-index-contract, enemy-intents-payload all exit 0 |
| 6 | test-multiplayer-lobby-flow.ps1: added -ExePath / -HostExtraArguments / -ClientExtraArguments, which can point at an isolated copy with each of the two instances having its own clientId | Run against the isolated copy: the two instances came up on 8080/8081, created a lobby, and entered the same run (same run_id), advanced to floor 2 and finished the reward flow |

A key mechanism confirmed during the live re-verification (worth writing into the suite design): on a brand-new profile, **issuing die right after entering the map makes the game accept the command but not open the settlement screen**, leaving a dead run stopped on the MAP; waiting until the map is interactive and has been stable for about 3 seconds before issuing die makes the settlement appear normally right away. This has been fixed in as RUN_SETTLE_SECONDS and the corresponding stability wait.

Item 6 still has one uncovered spot: the multiplayer lobby suite did not finish the "both reach the rest site together" stage. In MP the two players share the map position; after room RestSite is sent to the client alone, the client goes back to MAP (the host stays at REST), and the suite then waits for the client to enter REST. This is unrelated to the launch parameters added this time and belongs to the suite's own MP advancement logic; it is advisable to handle it separately later.

## select_deck_card Semantics Settled (2026-09-11)

The issue in "Live Finding 1", where the combat multi-select returned completed, was settled and verified as "change the code back to pending".

Change: WaitForCombatHandSelectionStepAsync in STS2AIAgent/Game/GameActionService.cs now returns false (pending) when "the selected count has changed but the UI still needs confirmation", aligning with use_potion and the semantics the skill documentation describes; the select_deck_card paragraph of docs/api.md was split by selection.kind into two descriptions: deck single selection (auto-confirm, completed) and combat hand multi-select (step-by-step pending, needs confirm_selection).

Regression location: from its introduction (52c3467) to the merge of PR #56 (9295f35) the function was return false, and 93e5ab5 (#68) changed it to return true; the only selection-related sentence in that commit's message is "stop confirming deck picks at min_select", which refers to the deck path, and the flip for the combat hand this time was a side effect.

Live evidence (isolated copy, candidate DLL SHA256 43100E7621E1F962D2A1F7B1719339E2EEF8607735AF39B16B9AA232A6FF1A39):

| Verification | Result |
| --- | --- |
| combat-hand-confirm-flow | exit 0; post_select_status=pending (it was completed before the fix, exactly the original failing assertion), confirm_status=completed, final_screen=COMBAT |
| state-invariants | exit 0, 0 failure / 0 warning |
| deferred-potion-flow | exit 0 (the deck single-selection auto-confirm path is unaffected) |
| target-index-contract / enemy-intents-payload | exit 0 |
| C# core unit tests | 213 PASS / 0 FAIL |
| Auto-play smoke (stub model, zero cost) | The stub first played PURITY (play_card card_index 5), then called select_deck_card three times in a row; "Timed out waiting for a stable state" appeared 0 times in the log, and the session kept advancing after every pending |

The stub used for the smoke test (build/validation-2026-09-10/stub-model-server.py) gained the ability to pick cards from the compact state: the compact hand array has only i/line/playable/targets and no card_id, and a card that needs a target must carry target_index, otherwise it is rejected by the agent's own index validation. Both points apply equally to writing later auto-play validation scripts.

## open_timeline Assertion Settled (2026-09-11)

The issue in "Live Finding 2", "the main menu does not expose open_timeline when a save exists", was settled and fixed as "the suite's expectation was wrong".

Root cause (the game's source, not a mod defect): NMainMenu.UpdateTimelineButtonBehavior under extraction/decompiled requires !SaveManager.Instance.HasRunSave to enable the timeline button; with a save it falls into the else branch and is Disabled, and the second branch also explicitly Disables it when "discovered epochs > 0". The mod's CanOpenTimeline requires the button to be IsEnabled, so not exposing open_timeline when a save exists is correct behavior. The same file also shows the state depends on profile progress (it Enables when no epoch has been discovered), so asserting either "must have" or "must not have" does not hold.

There is another layer of profile dependence: choose_timeline_epoch is exposed only when an "epoch that is revealed but not unlocked" exists. On the measured profile 2026091007, opening the timeline gave only close_main_menu_submenu and confirm_timeline_overlay.

Changes:

| Suite | Change |
| --- | --- |
| assert-active-run-main-menu | Dropped the open_timeline assertion and instead reports open_timeline_available honestly in the result |
| main-menu-active-run | Dropped the precondition "open_timeline should be present when a save exists" and the later timeline stage; instead asserts that continue_run is still kept after withdrawing the abandon dialog, and continues to verify that continue_run can return to the run |
| new-run-lifecycle | Added timeline coverage on the no-save main menu at the end: open_timeline -> (when the profile supports it) choose_timeline_epoch -> confirm_timeline_overlay -> close_main_menu_submenu; when the profile does not support epoch selection this is recorded honestly rather than failing. The final wait now uses settle_main_menu, to absorb the timeline overlay the game pops up automatically after death |

Live verification (isolated copy, run consecutively on the same instance):

| Suite | Result |
| --- | --- |
| assert-active-run-main-menu | exit 0; open_timeline_available=false (the game correctly does not expose it when a save exists) |
| main-menu-active-run | exit 0; menu_after_dismiss_actions kept continue_run, continue_run_destination=COMBAT |
| new-run-lifecycle | exit 0; timeline={opened: true, screen: TIMELINE, epochs_selectable: false, overlay_confirmed: true} |
| The other 8 suites (mod-load, state-summary, state-invariants, bootstrap-active-run, combat-hand-confirm-flow, deferred-potion-flow, target-index-contract, enemy-intents-payload) | All exit 0 |
| Offline | Gates exit 0, self-test exit 0, C# 213 PASS / 0 FAIL |

## CI Failure After the First Push and Its Fix (2026-09-11)

After pushing 23 commits to origin/main, the "Verification gate self-test" step of CI failed, while the same command locally always exited 0.

Root cause: `scripts/test-verification-gates.ps1` contains non-ASCII text and has no UTF-8 BOM header. Windows PowerShell 5.1 reads a BOM-less file with the **system ANSI code page**, and CI's Windows runner is CP1252 while the dev machine is GBK, so the same bytes decode to different text. The em-dash (U+2014, UTF-8 bytes E2 80 94) decodes to `â€”` under CP1252, and its 0x94 is exactly the right double quote U+201D, which PowerShell treats as a string terminator, so the string closed early and the whole script failed to parse (CI reported `Unexpected token 'not' in expression or statement`). Locally it passed only by luck because GBK decodes the same bytes into harmless Chinese characters, a locale-dependent portability defect introduced by this work (the script and the CI step were added in the same batch and had never been executed on CI before).

Reproduction (no CI needed): decode the file as CP1252 and count the quote-like characters; exactly 1 U+201D is found.

Fix:

1. The phantom fixture string in that self-test now uses an ASCII hyphen, removing the highest-risk character (the kind that decodes to a string terminator under CP1252).
2. Added a UTF-8 BOM to `scripts/test-verification-gates.ps1` and to `scripts/sts2-coop-full-run-acceptance.ps1`, which likewise contains non-ASCII, so PowerShell reads it as UTF-8 under any locale.
3. Added the `script-encoding` gate: `scripts/*.ps1` must carry a UTF-8 BOM when it contains non-ASCII, fundamentally preventing the same class of problem from recurring.
4. The self-test gained two cases: a non-ASCII script with a BOM must pass, and with the BOM removed it must be rejected.

Re-verification: gates exit 0 (listing the two scripts with a BOM), self-test exit 0 (9 drift cases + baseline + restored), preflight exit 0, C# 213 PASS / 0 FAIL, MCP 49 tests OK; parse errors 0; quote-like characters under CP1252 dropped from 1 to 0.

## PR #81 Review, Merge, and Live Re-verification (2026-09-11)

External contribution PR #81 (XenoAmess, branch fix/simple-card-multiselect-acknowledgements, 2 commits, 4 files +75/-46) generalizes the deck selection metadata from the concrete type `NDeckCardSelectScreen` to the base class `NCardGridSelectionScreen` and adds `NSimpleCardSelectScreen` to the allowlist; it also renames `TryGetCardGridSelectionMetadata` / `CardGridSelectionMetadata` / `SettleCardGridSelectionClickAsync`.

Premise check (decompiled source, extraction/decompiled/MegaCrit.Sts2.Core.Nodes.Screens.CardSelection/):

| Premise | Conclusion |
| --- | --- |
| `NDeckCardSelectScreen` / `NSimpleCardSelectScreen` are sealed subclasses of `NCardGridSelectionScreen` | Holds |
| `_prefs` / `_selectedCards` are declared by each subclass itself (not in the base class) | Holds; reflecting on the concrete instance is the only correct way |
| The body of `SettleCardGridSelectionClickAsync` is not modified by the PR | Holds; only the signature is generalized, and the existing "return completed after a click" semantics are kept |
| `NChooseACardSelectionScreen` does not inherit the base class | Holds; the branches do not swallow each other |
| Sourcery's blocking finding (`confirm_selection` gives 409 on the simple screen) | Already fixed by the second commit 27fa223 |

Local real merge verification (worktree detached from 2bb1f34, git merge without conflict): C# 214 PASS / 0 FAIL (213 + the PR's new CardGridSelection.ConfirmDispatch), MCP 49 tests OK, the four gates including the new script-encoding exit 0; the previous step's select_deck_card combat-hand fix and the PR's renames both survived, with no old names left over.

Conclusion and action: merged (merge commit ee308ff, consistent with the repository's existing `Merge pull request #NN` convention; main protection requires 0 approvals). The review comments were posted to the PR (#issuecomment-5622477667, including the 214 baseline and two non-blocking suggestions), and two points, the generalized allowlist being too narrow and the renaming being incomplete, are tracked in a separate issue #82.

### Live Re-verification After the Merge

| Item | Value |
| --- | --- |
| Post-merge DLL SHA256 | CC6D616978F726E1BAAFD31732A54AF727A6EB80E23CF4D7B3E00576B9F82C7F |
| Isolated copy | build/validation-2026-09-08/game/SlayTheSpire2.exe, `--windowed --force-steam off --clientId 2026091012` |
| Run bootstrap method | Debug console (`room EVENT` / `event <id>` / `relic add <id>`), `STS2_ENABLE_DEBUG_ACTIONS=1` |

A pitfall: in the `settings.save` of a newly created clientId profile `mod_settings` is null, and the game pops up a "mods warning" and **skips loading mods** (log `Skipping loading mod STS2AIAgent, user has not yet seen the mods warning`, gated at `NMainMenu.cs:306`). Using the already-authorized profile 2026091012 avoids it, with no save needing to be changed.

Results (all measured):

| Scenario | Result |
| --- | --- |
| `event ROOM_FULL_OF_CHEESE` -> Gorge (`NSimpleCardSelectScreen`, min=max=2) | Before input `min_select=2 max_select=2 selected_count=0` (1/1/0 before the fix); the first card click took 151 ms with `selected_count=1`, `cards[0].selected=true`; the second click, 347 ms later, closed the screen, the deck went 10 -> 12, and the native log showed `Player 1 chose cards [ANGER,THUNDERCLAP]` |
| `relic add SEA_GLASS` (min=0/max=15, needs manual confirmation) | `confirm_selection` correctly appeared in available_actions; clicking 1 card took 143 ms; `confirm_selection` completed in 235 ms, the deck went 12 -> 13, and the native log showed `Player 1 chose cards [THUNDERCLAP]` |
| `relic add GNARLED_HAMMER` (enchant screen, within the scope of issue #82) | The native prompt said "Choose 3 cards to enchant", while the API still reported `1/1/0` and did not expose `confirm_selection`; the first click took 10036 ms (a full 10 s timeout) and returned pending |
| `event SAPPHIRE_SEED` -> Eat (upgrade screen, single selection) | Reports `1/1/0`; because for a single selection the native value is 1/1 anyway, the misreport is invisible here, and the problem only appears when min != max or for multi-select |

Offline re-verification: C# 214 PASS / 0 FAIL, MCP 49 tests OK (post-merge main).

### Deployed to the Official Install and Re-verified (2026-09-11)

The official install had long stayed on an old DLL (4C2092EF..., missing all the recent fixes before the merge), so the merged main was rebuilt and deployed to the real Steam install: `scripts/build-mod.ps1 -Configuration Release` (without -GameRoot, which defaults to the Steam install). Before deploying, a backup was made to build/backup-steam-mods-2026-09-11/.

Build nondeterminism investigation: the deployed DLL (47CE0F90...) and the one measured in the isolated instance (CC6D6169...) have the same byte count but different hashes. A byte-by-byte comparison shows only 72 bytes differ, all in the PE timestamp (offset 0x88) and the assembly/PDB identity GUID regions (the 16-byte MVID from 0xE6E64, plus those from 0xFE4E4, 0xFE538, and 0xFE5A6), with no difference in the code section; repeated builds reproduce 47CE0F90 stably. A further check: the two `data_sts2_windows_x86_64/sts2.dll` hashes are identical (0861BFA1...), showing it is not caused by referencing different assemblies. The conclusion is a compile-identity difference rather than a code difference, backed by a symbol search: the deployed DLL contains `TryGetCardGridSelectionMetadata` / `SettleCardGridSelectionClickAsync`, and the old names `TryGetDeckCardSelectionMetadata` / `SettleDeckCardSelectionClickAsync` have disappeared completely.

To dispel the doubt about an "untested binary", the artifact with exactly the same bytes as the Steam one was put back in the isolated instance, and the two scenarios were rerun with the same bootstrap methods:

| Scenario | Result |
| --- | --- |
| `event ROOM_FULL_OF_CHEESE` -> Gorge (simple screen, min=max=2) | Before input `min=2 max=2 count=0`; the two clicks took 30 ms / 131 ms (both completed); deck 10 -> 12; native `Player 1 chose cards [SETUP_STRIKE,TAUNT]` |
| `relic add SEA_GLASS` (min=0/max=15, manual confirmation) | `confirm_selection` correctly exposed; click 34 ms; `confirm_selection` completed in 168 ms; deck 12 -> 13 |

After the re-verification the official install DLL was still 47CE0F90. Nothing else about the official install was changed (the rest of the mods directory and the real saves were untouched).
