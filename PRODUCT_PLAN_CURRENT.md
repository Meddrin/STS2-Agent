# STS2 AI Agent: Current Status Page
> **Current release: v0.16.1 (2026-09-24, patch)**. Tag `v0.16.1` @ `a64e052` (the Release commit on dev, CI passed after the merge of PR #200); GitHub Release asset 779153 bytes / SHA256 `835E22E8…7551`; Workshop item 3796486050 was updated at 11:54:48 (public, `file_size` 1646567 = local byte sum). One fix: inviting a teammate in solo mode / during auto-play **no longer hangs silently at pending**; it records `Rejected` and states the cause. The fix was proven live; offline 866 + 423 all green. See the [v0.16.1 release record](history/release-v0.16.1_2026-09-24.md). For the previous release v0.16.0 see the [v0.16.0 release record](history/release-v0.16.0_2026-09-24.md) (thinking streaming / dual-layer / screenshot layout all green and re-verified live; on 2026-09-24 the dual-layer and thinking-display retests passed; the AI teammate is marked BLOCKED in the Steam-off environment, which restricts it). **The Workshop Simplified Chinese listing still needs a manual paste on the web page**.
> The release, Workshop, and live data of the versions below keep their original dates as historical evidence.

<details>
<summary>Release and channel records of 2026-09-19</summary>

> This page is the repository's only current-status entry point. Updated: 2026-09-19 (**v0.13.0 released**, GitHub and the Steam Workshop on the same day, from the same source, once each, with no re-cut). Historical snapshot marker: this page is the current status page, not a historical snapshot.
> Release code baseline: tag `v0.13.0` @ `78b085f` (the `dev → main` merge of PR #161). GitHub Release asset `sts2-ai-agent-v0.13.0-windows.zip` 576033 bytes, SHA256 `8CFC0F45…8733`.
> Latest on the Workshop: item 3796486050 was updated to `0.13.0` at 2026-09-19 11:32:25 (public, `file_size` 1252357 equals the local content byte sum, content id `3840768014407368680`, DLL SHA256 `6B9D90C8…FBE42`). The final release DLL separately passed a short isolated live smoke run. See the [v0.13.0 release record](history/release-v0.13.0_2026-09-19.md).
> Build identification: the cross-version fingerprint index is at [history/build-fingerprints.md](history/build-fingerprints.md); from this version on these numbers are written by the packaging script into `build-fingerprint.json` next to the artifact and are no longer collected by hand afterward.
> For the previous release v0.12.5 see the [v0.12.5 release record](history/release-v0.12.5_2026-09-17.md). **The Workshop Simplified Chinese listing is still the old version** (since v0.11.0), awaiting a manual paste of `steam-workshop/description.zh-CN.txt`: `ModUploader` has no language parameter, so it can only be done on the Workshop web page.

</details>

For the old roadmap see [PRODUCT_ROADMAP.md](PRODUCT_ROADMAP.md) (historical), and for the original old delivery text see [history/PRODUCT_PLAN_CURRENT_2026-09-07.md](history/PRODUCT_PLAN_CURRENT_2026-09-07.md) and [history/COOP_DELIVERY_2026-09-07.md](history/COOP_DELIVERY_2026-09-07.md). [COOP_DELIVERY.md](COOP_DELIVERY.md) is now only a historical evidence index. This page does not inherit the approvals, worktrees, or pre-test execution constraints of the historical documents.

## 1. Current Baseline

- **Second round of 5 behavior fixes**: duplicate actions, the current turn's token check, the receipt after cancellation, the proactive speech budget order, and non-object action arguments have entered the local Unreleased. See the [turn integrity fix record](history/agent-turn-integrity-2026-09-21.md) for details. This round keeps the offline validation scope.

- **v0.16.0 (current release baseline)**: released 2026-09-24, with live display of thinking content, the macro goal and hint alignment in dual-layer mode, the root-cause fix for overlay overflow, and screenshots that do not include the overlay; see the [v0.16.0 release record](history/release-v0.16.0_2026-09-24.md). The next two entries were the local state at that time and have now shipped with this version.

- **(Shipped with v0.16.0) Local dev: v0.15.0 + Unreleased audit fixes**. `628ba75` handles solo/multiplayer mode exclusion, next-hand instructions within the same run, Jev 429 budget and retry, session ownership, and the HTTP/dual-MCP briefing; `455641f` adds the spec for the game bridge and session observation boundary. The task is archived at the [audit remediation record](.trellis/tasks/archive/2026-09/09-23-audit-remediation/prd.md). These are offline validation results; this round's live dual-instance and real Jev acceptance has not yet been done.

- **Historical local dev of 2026-09-21: v0.14.5 + the Unreleased fixes of that time**. The starting point was `65c9ad9`; the work then covered theme saving, teammate identity, two-sided state diffs, the event stream lifecycle, and model cancellation handling. For the validation of the old version see the [audit record of that time](history/dev-audit-2026-09-21.md).

- **v0.14.0 (historical release baseline)**: released 2026-09-21. Release commit `5f9ea72` (the `dev → main` merge of PR #174;
  the feature work was merged into `dev` via PR #173, CI `contracts` passed), and the tag `v0.14.0` points at it. GitHub Release asset
  `sts2-ai-agent-v0.14.0-windows.zip` 637550 bytes / SHA256 `37F95EED…2A70`. See the
  [v0.14.0 release record](history/release-v0.14.0_2026-09-21.md).
  For the contents see the `## v0.14.0` section of [CHANGELOG.md](CHANGELOG.md): the decision explanation chain and decision log (overlay /
  `GET /decisions` / both MCP surfaces / SSE `decision_made`), the per-screen injected strategy layer, structured teammate signals and the live teammate panel,
  **the UI redesign and four color themes**, the generated OpenAPI contract, two typed payloads in the Python client,
  the per-screen split of `GameStateService`, and the v0.13.1 event stream reliability batch that was originally planned as its own release (v0.13.1 has a release commit but
  was never published, and was folded into this version).
  **Live acceptance status** (2026-09-20, isolated profile `default\2026092014`, game v0.111.0, the player's real profile was not written):
  `/health` ready with 27/27 reflected members present, `mod-load --deep-check`, `state-summary`,
  `state-invariants` (0 failures 0 warnings), and `patch-check` (including the action-surface baseline replay, `mismatches: []`) all passed;
  **a real-model run went through**: CommandCode / `deepseek/deepseek-v4.1-flash` connected successfully for both roles via the overlay's "Test connection",
  unattended auto-play went from floor 1 to floor 3, all 45 decisions belong to the same run id,
  22 of 26 sampled entries carry the model's own stated reasons, the SSE `decision_made` was visible live, and it finally stopped automatically at the session token limit
  (52 requests / 622,864 tokens in fact); **local dual-instance co-op also went through**: the second instance, the shared run,
  the cooperation rule "follow your map choice." and the live teammate panel are all recorded. For details see
  [live-validation-checklist.md](docs/live-validation-checklist.md).
  **Still not done**: the Steam dual-instance path, a complete natural run, Vision live, the online path of typed intent,
  5.4/5.6, 7.2 BESTIARY, and 7.3 mechanic matrix sample top-ups. For the Workshop channel status see the "Workshop Upload Process" section of the release record.

- **v0.13.0 (historical release baseline)**: released 2026-09-19. The tag `v0.13.0` points at the
  `dev → main` merge commit `78b085f` of PR #161; GitHub Release asset 576033 bytes / SHA256 `8CFC0F45…8733`;
  the Workshop is public, `file_size` 1252357 equals the local content byte sum, content id `3840768014407368680`.
  The release candidate passed full offline and live acceptance; because the rebuilt final DLL differs in bytes from the candidate DLL, the `6B9D90C8…FBE42` DLL shared by GitHub and
  the Workshop also separately completed a short smoke run of a new-run combat, monster data, save and quit, and continuing the run.
  See the [v0.13.0 release record](history/release-v0.13.0_2026-09-19.md).

- **v0.12.5 (historical release baseline)**: released 2026-09-17, GitHub and the Steam Workshop on the same day, from the same source, **uploaded once each**.
  Release commit `f361bdb` (the `dev → main` merge of PR #145), and the tag `v0.12.5` points at it; Release asset
  `sts2-ai-agent-v0.12.5-windows.zip` 562212 bytes / SHA256 `0E4A1518…CFED`; Workshop `file_size` 1238533
  equals the local content byte sum, content id `2962740913650521121`, visibility 0 (public).
  See the [v0.12.5 release record](history/release-v0.12.5_2026-09-17.md).

  Two runtime fixes that players / agents can perceive, **both found by driving the real game and re-verified on the patched build**:
  `/state.screen` reports `GAME_OVER` again after death (PR #142; previously the whole settlement phase reported `COMBAT`, and that name never appeared once in 2346 live samples);
  failed game actions state the reason the game gave (PR #143; previously 12 paths shared one
  "the game task faulted"). The rest is the contract and tooling cleanup of PR #141.

  This version is the **first that needed no same-number re-release**: these two fixes were likewise found after the tag, but waited in the newly created
  `## Unreleased` section of `CHANGELOG.md` until this release finalized them; v0.12.3 / v0.12.4 each had
  three same-number builds precisely because there was no such section to write into at the time. The build fingerprint was also produced automatically by the packaging script for the first time,
  and the hashes in the release record are copied directly from `build-fingerprint.json`, no longer collected by hand afterward.

- **v0.12.4 (Workshop first, GitHub release added afterward, two same-number re-releases the same day)**: updated on the Steam Workshop on 2026-09-15 (item 3796486050, public, `file_size` 1233413, `time_updated` 2026-09-15 19:38:00, source commit `4b8e7c5`). The contents are the reliability fixes of PR #132: inviting / continuing the teammate returns `pending` immediately and a concurrent second request no longer misreports, request accounting covers the paths previously missed, `state-invariants` requires `play_card` only when the executor is ready, and the isolated-profile seed for a first `--clientId` launch is written, on both platforms, into the directory the game actually reads. The GitHub side was completed together with the 2026-09-16 `v0.12.4` tag and Release (asset `sts2-ai-agent-v0.12.4-windows.zip` 557906 bytes / SHA256 `AD970DB1…`, from the same source as the Workshop's third build). See the [Workshop upload record](history/workshop-upload-v0.12.4_2026-09-15.md).
(**Same-number re-release**: fixes a one-frame race in which `available_actions` and `combat.action_readiness` contradicted each other within one `/state` response; the combat gate is now evaluated once per payload and shared along the call chain, and readiness is its pure projection. After the re-release `file_size` was 1236997 and `time_updated` 2026-09-15 23:34:57.)

- **v0.12.3 (current release baseline)**: first released 2026-09-13, **re-released twice under the same number on 2026-09-14**: the version number never changed and only the build did, so the Workshop and GitHub each published three times. The first release commit was `b0217b0` (`Release v0.12.3 for the Steam Workshop`, merged via PR #108, which the tag pointed at then; the Workshop went first at 22:31 and the tag and Release were added afterward); the first re-release re-cut the tag and Release to `0f60ec4` (the merge commit of PR #113, contents = #105 + #106 + #111) and re-uploaded the Workshop at 00:09 the same day; the second re-release re-cut the tag and Release to `c2630a8` (the merge commit of PR #123, contents = #116 + #117 + #118 + #119 + #120 + #121 + #122) and re-uploaded the Workshop at 02:17 the same day. The GitHub Release asset `sts2-ai-agent-v0.12.3-windows.zip` is now 555061 bytes, SHA256 `E882B653B48B15EF278CFD9E20CC443FDCA3E5D69A51E362A40B934E29E167DF` (the second one 552014 bytes / `B7684C9F…`, the first release 549933 bytes / `B9DC1A07…`); the Workshop `file_size` is now 1232901 (the first two 1229829 / 1227269). The three builds have the **same version string** and can only be told apart by size or hash: this is the cost of repeated same-number re-releases, and the top of the v0.12.3 section of the CHANGELOG states this. See the [v0.12.3 release record](history/release-v0.12.3_2026-09-13.md). Relative to the previous one, the third build has **only one player-visible change** (the Continue button not refreshing while idle), and the rest is logging, release tooling, and offline contract tests; **this version did not get a separate full live acceptance**: the live evidence for #106 comes with it (a real click launched the teammate with no model configured, and `/health` reported `companion.auto_play: false`), and #111's two UI entry points had their live click acceptance on 2026-09-14 (see the `[coop]` section of `docs/live-validation-checklist.md`), which is exactly the run that found the Continue defect fixed in this version.
- **v0.12.2 (previous release)**: released 2026-09-13, release commit `40d1464` (`Release v0.12.2`) merged via PR #104 as `72b2a81` (the tag points at the merge commit, and the two have the same tree), GitHub Release asset `sts2-ai-agent-v0.12.2-windows.zip` (549033 bytes, SHA256 `35C1F0FC0ED3C664C0F74A73B5759486E4CA2BE92295CC47062FCC65552B5D9D`), and both the push and pull_request Validate runs of CI on `40d1464` succeeded. Workshop item 3796486050 was updated: public, `file_size` 1225733 equals the local content byte sum, `time_updated` 2026-09-13 16:51:04, content id `8439947284938535648`. See the [v0.12.2 release record](history/release-v0.12.2_2026-09-13.md). The contents are the co-op relay: the external takeover route and the main-window `POST /teammate/control` from #85, the shared model gate from #99, and the three live wrap-ups of #101. **The code is byte-for-byte identical to the copy that was run live** (the live evidence is the isolated acceptances of #85 / #99 / #101), and after the tag only the README link fix, the new `packaged-links` gate, and one overlay invite route fix were added (the latter two are listed under "Unreleased mainline changes after the v0.12.2 tag"). **No separate full live acceptance was run again for this version**; for the to-do items see `docs/live-validation-checklist.md`.
- **v0.12.1**: released 2026-09-13, release commit `f0f3b2a` (`Release v0.12.1`) merged via PR #95 as `640c343`. The contents are the pause boundary (#88 / #89 / #92 / #93), and the only live evidence is the two isolated acceptance rounds of #93 (`verify-capstone-pages.log`, FAILURES: 0). See the [v0.12.1 release record](history/release-v0.12.1_2026-09-13.md). The Workshop at that time had `file_size` 1213444, `time_updated` 2026-09-13 12:41:42.
- **v0.12.0**: released 2026-09-13, release commit `69602c9` (`Release v0.12.0`) merged via PR #86 as `69887a3` (the tag points at the merge commit, and the two have the same tree), GitHub Release asset `sts2-ai-agent-v0.12.0-windows.zip` (533493 bytes, SHA256 `C2B1F3229D6CF8E7D757D571AAF717004DDBCAFD4F8AAE1276A86A54A3AFA685`), and both the push and pull_request Validate runs of CI on `69602c9` succeeded. Workshop item 3796486050 was updated: public, `file_size` 1202181 equals the local content byte sum, `time_updated` 2026-09-13 02:25:02, manifest `3382317012139913714`. See the [v0.12.0 release record](history/release-v0.12.0_2026-09-13.md). The new `continue_ai_teammate` / `CompanionAutoSelectCharacter` (#83 / #84) and the state trustworthiness cleanup (`72c96fd`, `12c35b3`, `492722a`, `ca12a4f`, and others) shipped with this version.
- **v0.11.0**: released 2026-09-12, release commit `84631b9` (`feat(i18n): follow the game language in the overlay and the state payload`), GitHub Release asset `sts2-ai-agent-v0.11.0-windows.zip` (677015 bytes), CI Validate `34629717120` success. Workshop item 3796486050 was then updated to `file_size` 1135844, `time_updated` 2026-09-12 02:33:39. See the [v0.11.0 release record](history/release-v0.11.0_2026-09-12.md) and the [localization acceptance](history/localization-2026-09-12.md). The v0.10.7 release commit `f9330ba`, which updated only the Workshop, is already included in `v0.11.0`.
- **Shipped with v0.11.0** (`git log 2f75e4a..84631b9`, 13 commits in total):
  - `d77982a` the card grid selection metadata now reads the base class `NCardGridSelectionScreen`, fixing the upgrade/transform/enchant card selection screens of #82 (measured on the isolated copy: enchant 1/1/0 -> 0/3/0, first click 10s timeout -> 151ms; transform 36ms/187ms; upgrade 173ms with no regression; event multi-select 2/2/0 with no regression).
  - `7b02168` combat state exposes the player's own pets (`pets[]` / `pet_missing`), with the compact view synced; Necrobinder measured with Osty 1/1 and `DIE_FOR_YOU_POWER`. The same batch also tested the Defect orb slots (`orbs[]`/`orb_capacity`/`empty_orb_slots`, cleared after DUALCAST).
  - `deafa23` the proactive speech quota and run boundary converge per session (the cap of 6 per session is reissued with each auto-play session, the 75-second interval remains a global constraint across sessions; the run boundary is reset when auto-play starts); `2526832` restores "stop on leaving a run" and hardens the request contract of `POST /action` / `/session/control`; `d6ede1f` single-step turns also count toward the session budget; `ccf093e` every action in the full profile has a legacy tool; `03507dc` / `e4c5797` / `c3e654c` correct the stale wording of the status page and `docs/api.md`.
- **Shipped with v0.10.5** (2026-09-08, tag `04d2466`): product commits merged after v0.10.4: 19710ad (#78 an empty reward overlay no longer gets stuck at pending), 4b4da6e (#79 continue_game_over waits for the native settlement write), 22907b0 (#80 session budget / corrupt configuration recovery / MCP Origin / stream-stop timeout / play_card cancellation); the v0.10.5 GitHub ZIP was published and installed into Steam mods/, and for the Workshop subscription load acceptance see the [acceptance record](history/workshop-load-acceptance_2026-09-09.md).
- **Shipped with v0.10.6** (2026-09-11, tag `2f75e4a`): `bd93662` stop reason classification; `a9d4478` proactive speech and optional tone (off by default); dependency security #50/#51 (`cd55fe1`: `fastmcp` -> 3.4.7, `fast-uri` -> 3.1.7, `npm audit` 0 items); the offline verification gates `scripts/check_verification_gates.py` and the self-test (`c212594`, `6aabb4f`); `34c6e91` archives `docs/sts2-coverage-gaps.md` to `history/sts2-coverage-gaps_2026-03-10.md` and adds historical snapshot markers to the dated validation records. Commits that had not made it into the v0.10.5 tag (96bd410 Workshop update default public, 15e483c/2838250 release and P3 records, 4f1ec66 wrap-up log) also shipped with v0.10.6.
- **Repository hygiene (checked in, not a release)**: the 90s continue timeout and no duplicate Continue; Trellis spec/skills/platform put under version control, with 00-bootstrap-guidelines archived; stash@{0} (the old dirty tree of ai-companion) dropped; `.trellis/.template-hashes.json` kept local and not checked in; `c69d3b3` only ignores the local mod-uploader.log, and `374d7fd` only records the Trellis journal.
- **Shipped with v0.12.0: the five goals of 2026-09-12** (task tree `.trellis/tasks/09-12-agent-trust-hardening`, with each subtask archived in `.trellis/tasks/archive/2026-09/`):
  - `72c96fd` (action-trust) action trust cleanup: an explicit out-of-range index in `resolve_rewards` now reports 409 `invalid_target` and no longer clicks the wrong card; `continue_run` / `embark` / `open_character_select` return `pending` when blocked by a modal popup instead of counting "there is a popup" as success; a purchase failure in `remove_card_at_shop` is no longer swallowed into `pending`; bundle actions are no longer paired with an empty state and reported `completed`; `play_card` rolls back the turn count when it has not left the hand.
  - `12c35b3` (bounded-game-waits) all nine game-side `await`s now have deadlines: a timeout returns `pending` and names the action and the timeout; when the task ends with false or an exception it returns 409 `invalid_action`; bare `await`s in `GameActionService.cs` are blocked by a source contract test.
  - `33b137e` (agent-contract-compact) the skill now reads the compact state field names (`selection.min|max|selected|confirm`, `shop.open`, `chest.claimed`, `character_select.embark`, `timeline.slots[].i|line|actionable`); `get_game_state` gains `compact_agent_view`; `wait_until_actionable` gains `actionable`; `resolve_rewards` in the full profile allows no index.
  - `5457e0d` (screen-index-contract) `choose_timeline_epoch` takes the slot by `timeline.slots[].index`, and an inoperable slot returns 409 `invalid_target`; the descriptors of `crystal_clear_cell` / `crystal_set_tool` expose `requires_coordinates` / `requires_tool` respectively; five new screen names: `FAKE_MERCHANT` (`open_shop_inventory` available), `PATCH_NOTES` (`close_main_menu_submenu` can close it), `CARD_INSPECT` / `RELIC_INSPECT` (`close_cards_view` can close them), and `FEEDBACK`.
  - This documentation cleanup (docs-release-baseline): this file was changed to the v0.11.0 baseline; the `AGENTS.md` version number lists all five files and was changed to the PR release flow consistent with `.github/CONTRIBUTING.md`; `docs/api.md` gains the `failed` status, `requires_coordinates` / `requires_tool`, the five screen names, the timeline index contract, and the widened scope of the two close actions; the two READMEs gain routing and a symmetric paragraph; the changeNote of `steam-workshop/workshop.json` was updated to 0.11.0; `scripts/preflight-release.ps1` now also runs the CI-only `test-verification-gates.ps1` and `test-native-exit-propagation.ps1`.
  - The five items above have only offline evidence: after this cleanup on 2026-09-12, `dotnet run --project STS2AIAgent.Tests -c Release` gave 272 PASS / 0 FAIL, the `mcp_server` unit tests 76 OK, the four gates of `scripts/check_verification_gates.py` all green, and `preflight-release.ps1` exit 0; there was no live re-verification. For item-by-item records see the `prd.md` / `evidence.md` archived with each subtask.

- **Contents of v0.12.2 (shipped with that version)**:
  - `d80a19d` (PR #97, [issue #85](https://github.com/CharTyr/STS2-Agent/issues/85)) splits the prerequisites of "Invite AI teammate" by route: when the play model is verified the teammate plays itself as before; when it is unconfigured / unverified / failed verification the teammate is still launched and still enters the map, but the child process gets `STS2_AGENT_AUTOPLAY=0` and stops in place waiting for external takeover, and this route does not call a model at all. The structural conditions shared by both routes (not a teammate instance, that role has no auto-play running, must be in the main menu) were extracted into `CoopLaunchPolicy.GetStructuralError`, and the split applies only to the model tier. The same PR adds the main window's `POST /teammate/control` (the start / pause entry point for external agents, needing no teammate session token) and the `companion` block of `GET /health` (`api_host` / `api_port` / `process_id` / `auto_play`, without the token).
  - Live evidence: on 2026-09-13 an isolated host (`--clientId 2026091001`, API `18080`) was driven entirely over HTTP, with the model endpoint pointing at the **dead port** `127.0.0.1:18199` and `roleTests` empty. One full run went through: `invite_ai_teammate` returned 200 `completed` on the first call; the teammate came up as `role=companion` on `18081`; 20 seconds after entering the map `play_running=false`, `play_phase=paused`, `session_requests=0`, `stop_kind=null`; the teammate API exposed `choose_map_node` as externally drivable; `/teammate/control` pause returned 200, start while unverified returned 409, and a non-boolean returned 400. The 183 files of the player's real Steam save hashed identically before and after. Script and evidence: `build/validation-2026-09-13/verify-takeover.ps1`, `takeover-evidence.jsonl` (gitignored). The record is in "External-takeover route (issue #85)" of `docs/live-validation-checklist.md`.
  - That PR also fixes three live findings: the route marker used to be written **before** startup validation, so a rejected retry would mislabel a teammate that was already playing itself as `auto_play:false`; `/health` kept reporting the teammate's port after the teammate process exited; `teammate_control_failed` was documented as retryable while the implementation returned `retryable:false`.
  - `7a371c7` (PR #99) completes the launch gate and records a real external agent acceptance: the teammate instance's own `POST /session/control {"running":true}` used to bypass model validation and start the in-process loop directly (the very request that the host's "Continue play", `/companion/control`, and `/teammate/control` all reject, it accepted): `SetCompanionRunningAsync` now also goes through `FirstRunSetup.ReadyToInvite`, and pausing deliberately does not go through the gate; confirmed live that `running:true` on the teammate side returns 409 `session_not_ready` and `running:false` returns 200.
  - **The real external agent end-to-end takeover acceptance passed**: an external agent (Grok 4.6, bringing its own model and context, given only the repository's own MCP tool surface and the play skill, not the in-process loop) on a teammate window with **no model configured at all** finished a full combat by itself: the teammate seat made 16 `play_card` + 6 `end_turn` + 4 `confirm_modal` plus a map vote, `FUZZY_WURM_CRAWLER` went from `121/121` to dead (7 turns), took the reward `REWARD` -> `MAP`, gold `99` -> `110`, and it stopped while the second combat (`SHRINKER_BEETLE`) was already underway; throughout, the teammate stayed at `play_running=false`, `play_phase=paused`, `session_requests=0`, and a mechanical check of the 84 log lines found 0 hits on `/teammate/control`, `/session/control`, and `run_console_command`. Evidence `build/validation-2026-09-13/external-agent-log.jsonl` + `external-agent-final-state.json`, driver `mcp-call.py`; the 183 files of the player's real save hashed unchanged.
  - The three "recorded, unchanged" findings from that round above were all wrapped up by `d826935` (PR #101); see the next entry.
  - `d826935` (PR #101) wraps up three live findings and fixes two defects newly exposed while checking the derived paths:
    - **The three `confirm_modal` calls for `NCombatRulesFtue` were right all along** (`NCombatRulesFtue.cs:165` `_totalPages = 3`, with `CloseFtue` only after the third page); what was missing was that "returning `pending` on a non-final page still used the generic transition text", which read as if stuck. Now a non-final page returns `Tutorial page advanced; the modal is still open. Call confirm_modal again.` (`FtueModalPolicy.IsMultiPageFtue`), and other popup text is unchanged. Live: the three clicks gave `pending` / `pending` / `completed` in turn, and after the third `modal` was cleared and `screen` returned to `COMBAT`.
    - **`item_ids` of `get_relevant_game_data` is now optional**: when omitted, the ids to look up are derived from the live state for the current screen (`GameDataFilter.SceneItemSources` <-> `_SCENE_ITEM_SOURCES` on the MCP side, with the keys and paths on both sides pinned equal by `test_scene_field_alignment.py`), and when the scene has no matching content it falls back to role-level ids such as the deck / relics / potions. Live through the MCP tool surface: `{"collection":"monsters"}` returned exactly the three enemies of that room, `{"collection":"cards"}` returned the hand, and `relics` fell back to `BURNING_BLOOD`; passing `item_ids` explicitly behaves as before, and an empty potion slot returns `{}` instead of inventing something.
    - **`combat.enemies[].base_max_hp` added**: carries `Creature.MonsterMaxHpBeforeModification` from before scaling, on the same scale as `monsters.min_hp` / `max_hp`; `max_hp` is still the scaled live value. Live in a two-player run (both instances agreeing): TWIG_SLIME_S metadata 7-11 / `base=9` / live 19, LEAF_SLIME_M 32-35 / 33 / 72, LEAF_SLIME_S 11-15 / 13 / 28, i.e. `base x player count x act0 factor 1.1` rounded. A solo run is the degenerate case (`playerCount == 1` skips scaling directly), so the original symptom can be reproduced only in a multiplayer run.
    - Also fixed in passing: when the scene a screen was classified into has a `null` payload on that screen (`FAKE_MERCHANT` is classified as the shop but has no `shop` payload), walking the JSON path segment by segment stepped into null and threw: the path traversal gains a kind guard, and when the scene-side derivation is empty it falls back to role-level ids; the two mirrors in C# and Python also now treat empty-string ids the same way (previously C# accepted them and Python skipped them).
    - Evidence: `build/validation-2026-09-13/verify-round-3.jsonl`, `verify-fixes-3.log`, `verify-base-hp-mp.log` (gitignored); the record is written into "Those three findings, fixed and re-verified in the game" in `docs/live-validation-checklist.md`; `docs/api.md` gains `base_max_hp` and the optional `item_ids`. C# 382 PASS / 0 FAIL, Python 167 OK, all seven gates green, mod build 0 warnings; the 183 files of the player's real save hashed identically before and after.
  - Shipped with v0.12.2; the teammate window itself has no overlay (`ModEntry` skips it for the companion), which is now written into the documentation.

- **Contents of v0.12.3 (shipped with that version)**:
  - `b64e7e7` (PR #105) when packaging v0.12.2, the artifact check stopped it: the relative link newly added to the README by #97 did not make it into the packaging rewrite table, leaving in the artifact a link pointing at an unpackaged file. The links in both READMEs were changed back to the `./` form, and an eighth offline gate `packaged-links` was added: it parses the rewrite table from `package-release.ps1`, imports the artifact manifest and link rules directly from `check_release_package.py` (no copy-paste, so the gate cannot lie after the manifests drift), replays the packaging rewrite on the source documents, then verifies the remaining local links are all in the artifact. Destructive verification: writing a bare link back errors and names the target, and restoring it byte for byte turns it green. **This one touched only documentation and scripts and does not affect the published DLL/PCK.**
  - `0f63d6d` (PR #106, author sachi4clover) brings the route split of #85 into the in-game UI: the F8 window's "Invite AI teammate" button used to call the overload that only goes through auto-play, so with the model unverified clicking it was rejected by the old gate, while the same request through `POST /action` could bring up the teammate to wait for external takeover: **the route split reached the API but not the one button players actually click**. Now the button picks its route by `FirstRunSetup.Evaluate(settings).ReadyToInvite`, the tab's two explanatory lines switch with the route, and it is pinned by the source contract test `CoopRoute.OverlayInviteRoute` (restore the old form and that test turns red immediately). **This one changed mod code**, so it did not go into the v0.12.2 GitHub artifact; it reached subscribers with the v0.12.3 Workshop update below.
  - **`b0217b0` (PR #108) shipped these two as the first release of v0.12.3**: the version number was bumped to `0.12.3` in five places, `uv lock` regenerated, the `Unreleased` section of the CHANGELOG finalized, and the changeNote of `workshop.json` updated. The Workshop was uploaded first on 2026-09-13 at 22:31 (item 3796486050, `file_size` 1227269 equals the local content byte sum, content id `5284257893537057643`), and the tag `v0.12.3` and the GitHub Release were added the same day (asset 549933 bytes, SHA256 `B9DC1A07…`). See the [v0.12.3 release record](history/release-v0.12.3_2026-09-13.md).
- **v0.12.3 same-number re-release (2026-09-14)**:
  - `6a327d3` (PR #111, author sachi4clover, continuing the UI part of #83 / #84 / #106) brings two co-op entry points that previously existed only in the API into the "AI Teammate" page of the F8 window: **above** the invite button a new **"Disable automatic character pick"** checkbox (checked = `CompanionAutoSelectCharacter: false`; the toggle writes the settings immediately, because this tab has no separate Save and the teammate reads the settings at launch), with the explanatory line below it switching between the two states: unchecked it is still the original automatic character pick text, and checked it becomes "the second window stays at character select, letting the AI decide, or you switch over, finish picking for it, and press Embark"; **below** the invite button a new **"Continue the saved co-op run"**, which takes the same runtime entry as `continue_ai_teammate` (`ContinueDualInstanceAsync(settings, companionAutoPlay, CancellationToken.None)`) and is available only in the host main menu with a co-op save present: the game's own load button goes through Steam networking and does not recognize the NetId of a local direct connection, and would rename that save to `.VAL.corrupt`. The busy text applies only to the button that was pressed, and the other is merely grayed out; when the tab comes into view the availability of Continue is re-read. A new source contract test `CoopRoute.OverlayEntries` pins the wiring, and the two READMEs and the `## Unreleased` section of the CHANGELOG are synced.
  - **Bypassing the HTTP route does not mean bypassing the protection** (Sourcery gave one blocking finding on #111, judged a false positive and answered point by point under the PR): `CoopLaunchPolicy.GetError` already has the same check at the runtime entry `AgentRuntime.cs:804-815` (the same call and the same `requireVerifiedPlayModel` derivation as `GameActionService.cs:5224-5237`), and the two save NetId pre-checks are redone in `DualInstanceCoordinator.ContinueLocalCoopResultAsync` (`DualInstanceCoordinator.cs:74-94`, whose comment says explicitly it is a backstop for callers that do not go through the HTTP executor), and the coordinator additionally re-checks `MAIN_MENU` itself (`DualInstanceCoordinator.cs:69`).
  - Offline evidence (head `b42bc5a`): C# 384 PASS / 0 FAIL; `scripts/preflight-release.ps1` all green, with the eight gates (api-doc / api-facts / doc-marks / docs-tracked / lockfile / packaged-links / ps1-syntax / script-encoding) each OK; CI contracts and Sourcery both green; the destructive verification turned red all three times (removing the checkbox write-back / switching Continue to a non-route overload / inverting the checkbox semantics) and was then restored byte for byte. **When it was finished this one had not yet gone into any artifact, and it was the 2026-09-14 same-number re-release that carried it into `0.12.3`**: after the re-release both the Workshop and GitHub contain both items; the live click for this item was added the same day (see the next entry).

- **v0.12.3 second same-number re-release (2026-09-14, third build)**:
  - `61176f4` (PR #116) fixes the defect found by the live acceptance: the availability of the Continue button was refreshed only when the tab came into view, so when the panel sat on the "AI Teammate" page, the button that was correctly grayed out during the launch popup **stayed gray** after the popup disappeared (the API offered that action throughout), and it recovered only after switching away and back. Now `RefreshContinueAvailability()` re-reads within the panel's 800ms tick while that page is visible, pinned by the source contract test `CoopRoute.OverlayEntries` (removing that call -> 383 PASS / 1 FAIL, destructively verified). The fix was re-verified live on the same machine: brightness 364 (gray) while the popup was up and 660 (lit) 4 seconds after it disappeared, without switching tabs throughout.
  - `6a4b3a0` (PR #117) adds MCP error envelope and event stream diagnostics: `get_game_data_*` failures now keep `code` / `status_code` / `retryable` (previously dropped), `wait_until_actionable` no longer uses a broad `except Exception` to swallow a broken event stream into an ordinary timeout (it now catches `Sts2ApiError` / `OSError` / `TimeoutError` and returns `event_stream_error`), the skill adds that `AnyPlayer` in multiplayer needs `target_index`, and the debug reference adds `wait_for_event`.
  - `3d2eefb` (PR #118) and `9cc7114` (PR #119) are release tooling: the `packaged-links` gate gains a destructive case (of the eight gates it was the only one without one), preflight now runs the shared `check_release_metadata.py` instead of its own drifted copy (which had no version format check and read `pyproject.toml` by "the first `version =`"), and the artifact check now reads the three version sources from inside the artifact (`mod/mod_id.json` / `mcp_server/pyproject.toml` / `mcp_server/uv.lock`) and rejects inconsistency (new `test_release_artifact_versions.py`, with two destructive cases).
  - `7875c7e` (PR #120) and `a42bb60` (PR #121) are diagnostics: `CanChooseEventOption` / `CanChooseRestOption` for events / rest sites used to return false from an empty catch, so a probe that threw could not be told apart from "this room has no selectable option", and the action would silently vanish from `available_actions`: they still fail closed now, but the exception goes into the log; twelve other empty catches in `GameActionService.cs` (the cancellation chain, the three reward drain fallbacks, the two key fallbacks of `confirm_bundle`, the three injection points of `continue_game_over`, the Godot environment variable read) likewise gain a `Log.Warn` naming the chain, with return values and control flow unchanged. Two new source contracts, `SurfacedAction.RoomProbesDoNotSwallowFailures` and `ActionDiagnostics.NoWordlessRecoveryCatch`, were both destructively verified.
  - `cd5683b` (PR #122) changes the bounded-wait contract from the "normalized line shape" to reading the syntax tree: previously `await entry.OnTryPurchaseWrapper(...)` (where the argument list position demands a semicolon) and `await completedTask!` (the null-forgiving operator) could both slip through, and it scanned only the single file `GameActionService.cs`; now it walks four game-driving files (adding `DualInstanceCoordinator.cs` / `LocalDualInstanceLauncher.cs` / `AgentOverlayHost.cs`), and accepts only three kinds: "points at a member of this mod", "a `Task.` race", and "a call carrying a live `CancellationToken`", with the task value required to be produced by a bounded call. Destructive verification twice: adding a bare await in `GameActionService.cs` turns red, and putting the same shape into `DualInstanceCoordinator.cs` also turns red and names that file.
  - `c2630a8` (PR #123) shipped the seven items above as the third build: **none** of the five version numbers was touched, the v0.12.3 section of the CHANGELOG now reads "republished twice on 2026-09-14" and gained the diagnostics section, the changeNote of `workshop.json` leads with the Continue fix, and the release notes were newly written as `notes-v0.12.3-recut-2.md`.
  - Offline evidence (head `c2630a8`): `scripts/preflight-release.ps1` exit 0; C# 386 PASS / 0 FAIL; MCP unit tests passed; all eight gates and the gate self-test green; release metadata consistent. Three destructive verifications (the Continue tick / empty catch / bare await) each turned red first and were then restored byte for byte.


## 2. Existing Acceptance Evidence and Boundaries

The following results come from the actual execution against an isolated candidate build on 2026-09-08. Evidence directory: build/validation-2026-09-08/ (gitignored, not a release artifact).

| Check | Result | Evidence boundary |
| --- | --- | --- |
| C# core tests | 180 PASS, 0 FAIL | Offline test result, not equal to the full Mod or a complete run to its natural end passing |
| Stream-stall timeout regression | loopback replies the header first and then stalls the body, classified per contract (timeout vs user cancel) | Offline regression LlmClientTests; production default request timeout 10 minutes, HTTP client 11 minutes (OpenAiCompatibleClient.DefaultRequestTimeout, since 22907b0/#80). End-to-end for a real upstream's very long stall is still deferred |
| MCP Origin | 7 offline tests passed; live probe on an isolated Host | Not a full browser-page exploit |
| Mod Release build | 0 warnings 0 errors; SkipInstall generated the DLL/PCK | This row records the isolated candidate build; later official packaging and installation are in the release acceptance below |
| Isolated dual-instance 13 floors | 2026-09-08 08:35 reached native GAME_OVER on floor 13 | full-run-result.json. The Host progress aggregate had not been updated at the time; it cannot stand in for the short path below |
| P1.3 over-limit UI | 2026-09-08 request limit=1, stop_kind=budget, the overlay showed the limit text and next step | p13-overlimit-result.json; reset button screenshot ui-budget-scrolled3.jpg. Not a real MiniMax very long stall. |
| Isolated dual-instance GAME_OVER save | 2026-09-08 20:24-20:28, current main isolated DLL 72C72F02. In the first map combat, an empty-pass wipe, with continue_game_over once on each side (4.22s / 2.42s), save_verified=true, both progress.save mtimes updated after continue, and back to the main menu. forced_return_suspected=false | gameover-save-result.json outcome=gameover_save_ok. The console fight/die causes a multiplayer data desync disconnect, so the short path was changed to naturally entering combat + end_turn wipe. total_losses did not increase this run (host still 2), score was updated. Shipped with v0.10.5. |

Historical candidate binding: isolated DLL SHA256 72C72F022F7EC32563BCDFA5F3269449DFFF0956A764441E8D7565A8D8AED920; isolated exe SHA256 8602C26BFFD2937E3841835FD8360EF8E974624A543E05977229FD3D062BE231. That isolated acceptance did not modify the real-save snapshot; the Steam manual install was later updated to v0.10.5, see P2.4.

Later release acceptance: on 2026-09-08 preflight-release and the release directory/ZIP artifact check passed, recorded in commit 15e483c. On 2026-09-09 the GitHub asset `sts2-ai-agent-v0.10.5-windows.zip` (SHA256 `27da01401714347143244badfb9674e0e31932d03f1e5bebe1e4c67705fb12b8`), the Workshop public status, and the local subscription manifest were re-checked. The user later enabled the mod and restarted the game, and this Workshop load's log, floating window, and runtime interface were checked; see the [subscription load acceptance](history/workshop-load-acceptance_2026-09-09.md). The historical game tests were not rerun.

## 3. Capability Status Matrix

| Capability | Implementation commit / version | Release attribution | Acceptance evidence | Remaining gap |
| --- | --- | --- | --- | --- |
| In-game UI, chat, auto-play | v0.9.0 | Released v0.9.0 | Release record | Completing a run to its natural end still awaits live testing; the short-path settlement is done |
| AI teammate launch isolation and map voting | v0.10.0-v0.10.1 | Released | Isolated dual-instance | The console fight/die desyncs and disconnects in dual-instance |
| Native MCP | v0.10.2 | Released | Origin contract; on 2026-09-08 an external client's initialize/list_tools/ping/close succeeded | A streamable-http client was verified, which does not mean every desktop client was tested |
| Shared skill and voting | v0.10.3 | Released | Offline tests; isolated live | A complete 13-floor settlement is not the current bar |
| FTUE and timeline unlock | v0.10.4 | Released | Offline tests; isolated FTUE | None |
| First-run configuration and diagnostics | v0.10.4 | Released | Isolated invite succeeded | None |
| Recovery, budget, and pause | v0.10.4 + #80 | Released v0.10.5 | Pause probe ok; the P1.3 over-limit UI was seen | A real upstream's very long stall is still deferred |
| Empty reward overlay | 19710ad | Released v0.10.5 | 180 PASS; release preflight and ZIP check passed | The Workshop subscription load smoke run has passed; does not mean full-run acceptance |
| GAME_OVER waiting for native settlement | 4b4da6e (#79) | Released v0.10.5 | 2026-09-08 20:28 isolated dual-instance continue 4.22s/2.42s, save_verified, progress mtime updated | total_losses was not observed to go +1 |
| Simple card selection screen state and confirmation (#81 / #82) | 1b7236a, 27fa223; #82 fix d77982a | v0.10.6 already includes #81; the #82 fix shipped with v0.11.0 | Merged build on the isolated live copy: event multi-select reported 2/2/0, two clicks completed in 30ms/131ms, native chose cards records; Sea Glass (0/15 manual confirmation) exposed confirm_selection and completed in 168ms. #82: enchant screen 1/1/0 -> 0/3/0, first click 10s timeout -> 151ms, single confirm 166ms; transform screen 36ms/187ms; upgrade screen 173ms with no regression | Only isolated-copy evidence from the same machine; not rerun on the official install or real saves |
| Combat own-pet and orb slot state | 7b02168 | Shipped with v0.11.0 | Isolated live: Necrobinder `pets[]` reported Osty 1/1 and `DIE_FOR_YOU_POWER`, `pet_missing=false`, compact view synced; Defect `orbs[]`/`orb_capacity=3`/`empty_orb_slots`, cleared after DUALCAST | Only the two samples Necrobinder / Defect; other summons and orb types were not sampled one by one |
| Auto-play stop reason classification | bd93662 (shipped with v0.10.6) | Released v0.10.6 | Offline regression: 3 consecutive empty decisions no longer report `run_end`; the two real boundary messages still classify as `run_end` | The classification is based on message matching, not a structured error code |
| Proactive speech and communication style | a9d4478 (v0.10.6); session quota and run boundary fix deafa23 (v0.11.0) | v0.10.6 includes the base implementation; session convergence shipped with v0.11.0 | 2026-09-10 isolated live + local stub verified trigger, prompt, tone injection, read-only chat path, and reply consumption (five items); 2026-09-11 gate re-verified: with the switch off 50 requests gave 0 proactive messages, both combat start and end triggered, 9 send intervals of 88-137 seconds, a forced transition 12 seconds after a send stayed silent, after 6 the 7th moment was refused, and the quota recovered after pause and resume. See the [09-10 acceptance](history/validation-acceptance_2026-09-10.md) and the [09-11 acceptance](history/validation-acceptance_2026-09-11.md) | An optional feature off by default; triggered only at combat start/end, at most 6 lines (per auto-play session), interval >= 75 seconds (across sessions). The speech quality of a real model in a real run is still not accepted |
| Dependency security (#50 / #51) | cd55fe1 | Released v0.10.6 | fastmcp 3.4.7, fast-uri 3.1.7; npm audit total 0; MCP 49 unit tests passed | Covers only these two reports and the npm tree, not a full third-party audit |
| Documentation contract and verification gates | c212594, 6aabb4f | Released v0.10.6 | `check_verification_gates.py` four gates all green; self-test drift scenarios all rejected; preflight end-to-end exit 0 | Static checks, which cannot replace live behavior verification |
| Screen names of the pause and in-run menu pages (#88 / #93) | `31296bd` (#88/#89), `3cf347a` (#92), `04748f6` (#93) | Shipped with v0.12.1 | 2026-09-13 isolated live, two rounds of screen-by-screen checks: `PAUSE_MENU` / `SETTINGS` / `COMPENDIUM` / `CARD_LIBRARY` / `RELIC_COLLECTION` / `POTION_LAB` / `STATS` / `RUN_HISTORY` each report their name, the action surface is left with only `close_main_menu_submenu`, `choose_capstone_option` is 409 everywhere, FAILURES: 0 | `BESTIARY` was not opened live (that profile's hub does not draw the tile); the 409 of `save_and_quit` was added after a live finding |
| External agent takeover of the teammate window (#85) | `d80a19d` (PR #97) + `7a371c7` (PR #99) | **Shipped with v0.12.2** | 2026-09-13 isolated live: with a dead model endpoint + empty `roleTests`, `invite_ai_teammate` still returned 200 `completed`, the teammate was `auto_play:false`, 20 seconds after entering the map `session_requests=0`, `/teammate/control` pause 200 / start while unverified 409, and `/health.companion` lets the teammate's API be discovered; the teammate's own `/session/control` goes through the same gate (`running:true` 409 / `false` 200); an external agent (Grok 4.6, given only the MCP tool surface) finished a whole combat on the model-less teammate window; the 183 files of the real save hashed unchanged | Not rerun on the Steam dual-instance path (verified only on an isolated offline host) |
| Three live findings wrapped up: paginated FTUE text / scene-derived metadata ids / multiplayer base HP | `d826935` (PR #101) | **Shipped with v0.12.2** | 2026-09-13 isolated live: the FTUE three clicks gave `pending`/`pending`/`completed` and then back to `COMBAT`; on the MCP surface omitting `item_ids` returns the hand / enemies / relics per screen, with explicit ids and the empty set behaving as before; in a two-player run `base_max_hp` 9/33/13 against live 19/72/28 (`x2x1.1`), the two instances agreeing; the 183 files of the real save hashed unchanged | The FTUE reproduces only on a solo profile (it is already completed on the co-op profile); the `FAKE_MERCHANT` fallback has only offline unit tests (the screen was not reached live) |
| Live acceptance of the co-op UI entry points and the Continue fix (#111 / #116) | `6a327d3` (#111), `61176f4` (#116) | **Shipped with the v0.12.3 third build** | 2026-09-14 isolated live (`--windowed --force-steam off --clientId 1`, API 18080, co-op test profile): a mouse click on the checkbox immediately wrote `companionAutoSelectCharacter: false`, the explanatory line switched, the teammate stayed at `CHARACTER_SELECT` and was still `is_ready=false` after 70 seconds; a mouse click on Continue launched the teammate (PID 71620 / API 18081), both sides went to `MULTIPLAYER_LOAD` -> `embark` -> `COMBAT`, with HP 80/80, gold 113/112, and floor 2 matching the save; **found and fixed** the defect where Continue stayed gray for a long time when the panel sat on the "AI Teammate" page, re-verified after the fix with brightness 364 (gray) -> 660 (lit) without switching tabs | The live run was on an isolated offline host (the local direct-connection path, which is exactly why the feature exists), and was not rerun on the Steam networked save-loading path; an unrelated old 2026-05 mod DamageMeter in the environment throws `MissingMethodException` in `OnRunStarted`, and was moved out during testing and restored afterward |

## 4. To-Do Tasks

Priority from high to low. Where there is no clear owner, mark "unassigned". Do not turn the inventory or wrap-up into a complete natural 13-floor clear.

Milestones completed and moved out of the to-do list (no longer listed separately): the release, installation, and Workshop updates of v0.10.5 / v0.10.6 / v0.10.7 / v0.11.0; the 90s continue timeout and clicking only once; Trellis put under version control and the bootstrap archived; the old stash dropped; dependency security #50/#51; verification gates and self-test; the model compatibility matrix; the external streamable-http client check; the illegal-budget safety cap, corrupt configuration backup recovery, the MCP Origin contract, the stream-stall timeout contract, play_card cancellation, the empty reward overlay, continue_game_over waiting for native settlement; external agent takeover of the teammate window ([issue #85](https://github.com/CharTyr/STS2-Agent/issues/85), PR #97 merged into main as `d80a19d`, isolated live acceptance passed on 2026-09-13). The original records are kept in the commit history and under `history/` ([v0.11.0 release record](history/release-v0.11.0_2026-09-12.md), [v0.10.7 Workshop upload](history/workshop-upload-v0.10.7_2026-09-12.md), [v0.10.5 subscription load acceptance](history/workshop-load-acceptance_2026-09-09.md)).

### Shipped with v0.13.0: Second Batch After the v0.12.5 Tag (2026-09-18)

Listed separately per §5 rule 3. **It changes no runtime behavior shipped with the mod**: the only runtime-related code touched is
the descriptions of three error codes in `docs/api.md`, plus a pure relocation inside the Python sidecar.

The last round filled in "where the code is" and "where the links point". This round asks: **which parts of the contract surface that agents depend on is nobody watching.**
`docs/api.md` has four kinds of things a client branches on: which routes it can call, which fields it will read, which error codes it must handle, and
which events it can wait for. **Only the field kind was being checked.**

1. **Three error codes had dropped out of the contract table**, and none of them is obscure: `listener_error` (500, the HTTP listener loop itself threw),
   `method_not_allowed` (405, a non-POST request to `/mcp`), `payload_too_large` (413, an `/mcp` request body over 1 MB).
   An agent receiving one of these three finds nothing in the documentation, and cannot tell "fix the request" from "retry". All three are now documented.
2. **`api-facts` gains three bidirectional checks**: error codes (including a comparison of HTTP status codes, with the code side captured from three mechanisms: `ApiException` /
   `WriteErrorAsync` / `RestError`), event types, and routes. Events and routes were accurate already,
   but they are checked anyway, for the same reason as `doc-links` last round: **being accurate through good maintenance and being accurate through a mechanism are not the same thing.**
3. **Six line-number anchors pointed past the end of the file** (`client.py#L691`, `server.py#L1082`, and so on), all caused by the splits of the last round and this one.
   A link with a line number **still opens**, it just lands somewhere else: this is the quietest half of link rot. They were re-pointed,
   and `doc-links` now checks that line numbers are within the file's range.
4. **`server.py` 1,103 -> 707 lines**: the loading / caching / indexing / scene derivation of game data moved into `game_data.py`,
   with the budget 1,150 -> 750. A Python-specific pitfall was hit: `from X import f` binds the **function object**,
   so patching a same-named attribute of another module neither errors nor takes effect: the patch targets of four tests had to follow the function.
5. **`AgentOverlayHost.cs` (1,829 lines) was decided not to be split**, with the reasoning written into the architecture page along with measurements: of 85 instance fields,
   **78 are touched by more than one method**, 2.6 on average. Forcing a split into partials would only scatter shared mutable state across several files,
   trading a smaller file for something harder to read and easier to get wrong. The real change would be to extract a tab together with its state into an independent class, which is design work rather than a mechanical split.

Offline evidence (full rerun after this round's wrap-up): C# **415 PASS / 0 FAIL**; `mcp_server` **224 OK**;
**all eleven gates green**; **all 41 gate self-test cases pass** (six new this round); mod build 0 warnings 0 errors.

**No live re-verification**: this batch contains no runtime behavior change. Every new check was destructively verified: deleting one error code row,
changing an error code's status to 400, deleting one event type row, writing a nonexistent endpoint in the documentation, adding a route in code with no documentation,
and an anchor pointing at line 99999: each turned red naming the culprit, and was then restored byte for byte.

### Shipped with v0.13.0: Mainline Changes After the v0.12.5 Tag (2026-09-17)

Listed separately per §5 rule 3. **This batch changes no runtime behavior shipped with the mod**: both large-file splits are member relocations inside the same `partial`
class, the diff of each base file has only one truly new line (the `partial` keyword), and the semantics of the compiled output are unchanged.
The 0.12.5 on the Workshop and the GitHub Release asset are both unaffected.

The starting point of this round was not finding bugs but one specific discovery: **`.trellis/spec/mod/architecture.md` was lying.**
It carried metric numbers from before ADR 0001, and in the section "where to write a new action" it still said
"add it to both action surfaces", while that duplication had been merged into a single traversal a month earlier. When ADR 0001 landed, `AGENTS.md` and the game-actions spec were changed in sync,
**but this one page was missed, and no mechanism would ever notice**.

So the main line of this round is: **put a watcher where "nobody is watching".**

1. **Two monolith files were split** (`GameStateService.cs` 8,295 lines, `GameActionService.cs` 7,061 lines, together
   half of the mod). The state service gave up the compact `agent_view` rewrite and 60 payload type declarations; the action service was split by room into eight
   partials. **Ownership is computed, not assigned**: a member is put into a group only if "all references come from this group",
   and one reachable from both groups stays in the base file. Of the action service's 6,756 lines of members, 5,697 lines fell into a unique room and 1,059 lines are truly shared.
   The largest file went from 8,295 down to 5,872, and six of the eight room files fall within the default 1,000-line budget and need no entry at all.
2. **The `arch-facts` gate** (the tenth): the file table of the architecture page must match the real files: each file exists, the line count is within
   5%, the total matches, **and any source file over 1,000 lines must appear in the table**. That last one is the key: that table is the only place
   that says "where it is heavy", and a monolith it misses is exactly a monolith nobody is watching. Changing a heading makes the gate go red rather than quietly stop working.
3. **The `doc-links` gate** (the eleventh): all 411 relative links in the 403 checked-in pages must resolve. Previously only
   the three documents that go into the release package were checked. All its value is in what it blocks, and this round happened to move four files.
4. **Four things that "were checking but not really checking" were caught**, the first three predating this round's splits and the fourth being a flaw of this round's new gate itself:
   - `AgentSourceFixture.MethodBody` located a method by its "last occurrence", and across partial classes it would return the body of the **method containing the call
     site**: the contract asserted as usual, just against the wrong object, and without an error.
   - `GameTaskBounding` scanned for deadline-less `await` with a hard-coded file table, so the first bare `await` in a new room file would be invisible.
   - Three cases in the gate self-test reported PASS while the actual gate failed with "missing required file": `Assert-Case` looked only at the
     exit code. Now every case must declare the error text it expects to see, and a case that does not is judged a failure.
   - **The first version of `doc-links` and `arch-facts` asked the file system**: all green locally, red on CI. The working tree has files a clone does not:
     `AGENTS.md` is deliberately not checked in and `extraction/decompiled/` is a local decompilation product, and 36 links pointed at them.
     That is, the conclusions of these two gates depended on **who was running them**, which is exactly what the layer above them is meant to prevent. Both now ask git.
     Those 36 were real broken links, not false positives: two bodies already said "does not exist in a fresh checkout". They are now written as code spans,
     since a Markdown link is a promise that it "can be opened".
5. **ADR 0002** records why two MCP tool surfaces are kept (the in-process native surface + the Python sidecar), and under what conditions to reassess.
   A new MCP tool has to be added on both sides: **this is exactly the opposite of ADR 0001's "add it in one place"**, so it was written into the steps of `AGENTS.md`.
6. The 58 per-action wrappers of `client.py` on the Python side moved into `client_actions.py`, and both halves are back within the 700-line default budget,
   with the budget entry withdrawn as required by the ratchet itself.

Offline evidence (full rerun after this round's wrap-up): C# **415 PASS / 0 FAIL**; `mcp_server` **224 OK**;
`check_verification_gates.py` **all eleven gates green**; `test-verification-gates.ps1` **all 35 gate self-test cases pass**
(five new this round: three arch-facts, two doc-links); mod build 0 warnings 0 errors.

**No live re-verification**: this batch contains no runtime behavior change. Every new guard was destructively verified: breaking one compact view rename,
moving away a newly split file, writing a bare `await` in a new room file, writing the old numbers back into the architecture table, deleting one row of the table, changing a section heading,
giving a self-test case an expectation the gate would never state, adding a broken link in a checked-in page, linking to a
file and a directory that exist on disk but that git does not track: each turned red naming the culprit, and was then restored byte for byte.

### Unreleased Mainline Changes After the v0.12.4 Tag (2026-09-16)

Listed separately per §5 rule 3. **This batch touches only documentation, tests, and scripts, and changes no runtime code shipped with the mod**, so the third build of
0.12.4 on the Workshop and the GitHub Release asset are both unaffected; the published DLL/PCK is unchanged.

The starting point of this round is the same thing: among **four consecutive same-number re-releases** (v0.12.3 twice, v0.12.4 twice), the second v0.12.4 re-release fixed
a regression introduced by the first re-release: the combat gate read `RunManager.ActionExecutor` in the main menu (where there is no executor),
making every `/state` return 500 and leaving the mod completely unusable outside combat; and at the time the 408 C# tests and nine gates **stopped none of it**.
So what this round does is add regression-prevention mechanisms to the three places that "nobody was watching at the time", rather than adding features.

1. **The gate's in-combat guard now has a source contract** (`CombatGate.QueueReadIsCombatOnly`): the reads of `RunManager.ActionExecutor` / `ActionQueueSet` by `EvaluateCombatActionGate`
   must stay inside
   `if (combatState != null && CombatManager.Instance.IsInProgress)`, and within the whole state builder only the gate
   touches these two members; the second condition guards against "making the same mistake again from another call site". Two destructive verifications: lifting the read outside the guard turned red,
   and adding a second unguarded read elsewhere in the file also turned red, then restored byte for byte. C# tests 408 -> **409 PASS / 0 FAIL**.
2. **`docs/api.md` writes down the `combat` top-level fields of `/state` for the first time**. The four fields `action_readiness`, `players[]`,
   `end_turn_will_kill_player`, and `lethal_risks[]` had long been shipped and were sent to
   every external agent through the compact `agent_view`, and the documentation had not a word about them; among them `action_readiness` is exactly the subject of this version's headline fix, and is
   also the basis for the `state-invariants` judgment. Three new tables (three payload records) and all 19 values of `reason` were added, stating for each value
   whether it means "wait" or "close the popup first" to the agent.
3. **The `api-facts` gate pins these three tables to the records that produce them**: `CombatPayload` / `CombatActionReadinessPayload`
   / `CombatLethalRiskPayload` field by field, plus every `reason` that `EvaluateCombatActionGate` can give. Missing a
   field, adding a field the code does not have, or missing a reason each errors naming the culprit. `test-verification-gates.ps1` gains three
   destructive cases (previously api-facts had only three, covering the version number / screen names / port).
4. **Packaging produces a build fingerprint** (`scripts/lib-build-fingerprint.ps1`, called once by each of the two packaging scripts): the per-file SHA256,
   the byte sum Steam uses as `file_size`, and the commit used for the build and whether the working tree was clean, written as
   `build-fingerprint.json` next to the artifact. In a same-number re-release this is the only thing that can tell builds apart, and before this it was always captured by hand after the upload.
   The cross-version index was newly created at [history/build-fingerprints.md](history/build-fingerprints.md), with a four-step procedure for "when a player reports a bug,
   how to confirm which build he is running".
5. **`CHANGELOG.md` establishes the convention of an `## Unreleased` section**, written into `.github/CONTRIBUTING.md`, `AGENTS.md`, and
   `.trellis/spec/operations/validation-and-release.md`. All four re-releases began with "a small fix came after the tag and
   the changelog had nowhere to write it": a change that has somewhere to be written can wait for the next version number.
6. **The play skill gains how to read `action_readiness`**: no `play_card` on `COMBAT` is some `reason` of the gate,
   not a lost turn; the reason says whether to close a popup, to wait, or to stay out of a run someone has paused. The in-game agent reads the same contract
   (SKILL.md is an embedded resource, and `Skill.McpPlayerContract` pins the two equal).

7. **`docs/api.md` describes `/state` completely for the first time**. The inventory found **seven substructures with no documentation at all**: `session`,
   `multiplayer`, `multiplayer_lobby`, `character_select`, `timeline`, `modal`, `game_over`;
   the top-level field table was also missing 9 fields, for a total of **91 fields with no trace anywhere in the documentation**. Three of these blocks are screens the agent has to drive in person (character select /
   multiplayer lobby / settlement), and `session` is exactly the block the play skill calls "the first basis for routing".
   All are now filled in, and the inventory script's recheck went 91 -> 0.
8. **The rename table of the compact `agent_view`**. MCP `get_game_state` returns the compact one by default, and it **renames 43 keys**
   (`can_embark`->`embark`, `enough_gold`->`affordable`, every `index`->`i`, ...). This mapping previously existed only in the
   `BuildAgent*Payload` methods, with zero words in the documentation: an external client fetching by the `/state` names would read `undefined` instead of getting an error.
9. **One documentation error fixed** (not a gap, an error): `shop.cards[]` / `relics[]` / `potions[]` were written as having an `available`
   field, which these three records never had; only `shop.card_removal` does. An agent following the old documentation could not tell "sold out"
   from "affordable". It now records `is_stocked` and `enough_gold`, and states which one answers "can it be bought right now".
   Also fixed the rendering defect where the `run` field table was cut into two tables by a blank line.
10. **The `api-facts` gate expanded accordingly**: 16 payload records checked field by field against tables, the 21 keys of `GET /health` against a table,
    the rename table against the actual renames of `BuildAgent*Payload`, plus a catch-all net: **every field of every `/state` payload record
    must be mentioned in `docs/api.md`** (now covering 488 fields of 56 records). The gate self-test grew from 22 to 28 cases.
    Also fixed in passing a flaw in the gate's own extraction: C# keyword identifiers escaped with `@` (`public EventPayload? @event`)
    used to be misjudged as "in the documentation but not in the code".

11. **Global architecture check-up and anti-spaghetti mechanisms**. The measured facts: 82 C# source files with 31,632 lines in total, of which
    `GameStateService.cs` (8,559 lines) and `GameActionService.cs` (7,008 lines) **two files account for 49%**.
    The two are not the same kind of "big": the former is 318 methods crowded into one 7,290-line class, with three concerns (raw payload building /
    compact rewriting / action availability) tangled together; the latter is one pattern (`Execute*` + `WaitFor*`) repeated 60 times,
    clearly structured and merely all in one file, so splitting it by room is a mechanical action that needs no design.
12. **Found the project's biggest structural debt**: `BuildAvailableActionNames` (301 lines) and
    `BuildAvailableActionsPayload` (609 lines) are **two hand-written implementations of the same judgment**: a one-by-one comparison confirmed they query
    **the same 50 predicates** and produce **the same 55 action names** (only the emission order differs from item 28 on). Adding one action
    means changing two places, and the project documentation has always written "change both" as a normal step; a local contract was already written once for it
    (the only reason `CombatGate.ActionsAskTheSharedGate` exists is to stop them from diverging on three combat predicates).
    **This round does not touch it**: these 910 lines decide what an agent is allowed to do, and rewriting them with only offline evidence is exactly the cause of the v0.12.4
    regression. Instead two source contracts were added to pin the set (adding an action to only one surface turns red and names it), and
    [ADR 0001](docs/adr/0001-single-action-surface.md) was written to record the convergence plan and the live-validation bar it waits for.
13. **Size ratchet** (both C# and Python): every file has a line budget, the largest four C# files and two Python modules
    are each named, and the rest follow the default cap; **budgets only go down, never up**. A second check turns red when a budget is far above the actual size: when a file slims down
    the ratchet tightens with it, leaving no room to grow back. Destructive verification was done on both sides.
14. **Architecture debt written into the spec**: `.trellis/spec/mod/architecture.md` gains a "Code shape and its known debts"
    section, with the measurements above, the difference between the two giant files, and "where new code should go". Also fixed in passing
    the "add an action" steps in `AGENTS.md` pointing at a **nonexistent method** (`BuildAvailableActionDescriptors`).

**CI caught a real defect introduced by this round, now fixed**: the gate's hints and error messages quote the Chinese section headings of `docs/api.md`,
and the Windows CI stdout is cp1252, so `print` threw `UnicodeEncodeError` directly, and therefore **a gate that judged a pass still exited 1**;
worse, when a gate really failed, the real cause would be pushed aside by the encoding traceback. The gates now uniformly output UTF-8 (`errors="replace"`),
and the self-test gains a case that runs the whole suite under `PYTHONIOENCODING=cp1252`, which turns red if the fix is withdrawn.

**The gate self-test really did stop something this round**: the newly added `/health` check has to read `Router.cs`, and the fixture CI uses
does not have it, so the baseline case turned red at once, exposing "runs locally, will break on CI" before the merge.

Offline evidence (full rerun after this round's wrap-up): C# **409 PASS / 0 FAIL**; `mcp_server` **222 OK**;
`check_verification_gates.py` **all nine gates green**; `test-verification-gates.ps1` **all 29 gate self-test cases pass**
(seven new this round, 22 before); `check_release_metadata.py` all five version numbers agree; `preflight-release.ps1` exit 0.
**No live re-verification**: this batch contains no runtime code changes, and the live conclusions stand from the 0.12.4 third build run.

### Conclusions of the Gap-Filling Scan (2026-09-18, Briefing Digested and Deleted)

The results of a systematic scan (reflection dependencies, action contract coverage, dependency security, documentation reference style, observability).
**A and B1 were implemented**, and the rest were merged into the deferred list below.

**Done:**

1. **Upstream breakage used to be a blind spot.** The mod references `sts2.dll` at compile time, so a renamed public API turns the build red at once;
   the exception is **23 game-private members reflected by string** (4 more were later found hiding behind a helper that "takes the method name as a parameter", now 27), and when they break `GetField` returns null,
   the call site falls back to the default value, and the agent gets `max_players: 0`, unable to tell "zero people in the lobby" from "can no longer be read".
   Now they are resolved one by one at startup, the `compatibility` block of `/health` names the missing items and the affected features,
   and `status` is derived from it instead of being hard-coded as `"ready"`.
2. **`abandon_run` gains a behavior contract**: it is the only action that destroys player progress, and was one of the 12 actions without a contract.
   The pinned point is **where it stops**: it only opens the confirmation popup, and the destruction relies on the `confirm_modal` that follows.

**Verifying the registry took three rounds, worth recording**: `extraction/decompiled/` is from 2026-03-10,
and the installed `sts2.dll` is from 2026-08-16. Reading the stale decompilation, `StartRunLobby._maxPlayers` looked gone,
replaced by the public property `MaxPlayers`: **that would have been a confident report about a bug that does not exist**.
Only reading the ECMA-335 metadata of the assembly actually installed made it clear: it is there; while the six fields of `NUnlockScreen` are indeed not on that type,
because each lives on a concrete subclass (`_relics` -> `NUnlockRelicsScreen`, and so on), and the reader walks the six names and gets only one hit per screen:
**the code is right, and registering them under `NUnlockScreen` would make the probe raise six false alarms each time**. A gate that cries wolf gets switched off.

**Deliberately not done**: the briefing proposed changing `ReflectionMemberAccessor.TryGetValue` to distinguish "member does not exist" from "value is null".
Its four call sites already handle absence correctly, the startup probe already answers the real question,
and adding one more overload nobody calls is speculative code, and this repository has a size ratchet.

**Verified live (2026-09-18, game v0.111.0, isolated instance)**: the probe **caught a real bug on its first live run**:
`NEndTurnLongPressBar._longPressDuration` is a **static** field, while the code reading it used instance binding flags,
so since that line was written it never read a real value, and always used the hard-coded `0.45`, while the game actually uses `0.5`.
The "all 22 exist" obtained from reading the metadata offline was right about the names and irrelevant to the question: **whether a name exists says nothing about whether the binding flags can reach it**.
After the fix, re-verified `status: ready` / 22 checks / 0 missing; after injecting a fault `/health` names it precisely.
Also the two large refactors (PR #148 / #150) **for the first time have live evidence**: the main menu `/state` is 200 (the v0.12.4 regression did not recur),
the full chain of entering the map and starting combat, and `end_turn` advancing the turn. See the 2026-09-18 entry of the [live checklist](docs/live-validation-checklist.md) for details.

**One more structural defect was added after the live run**: the first version of the probe and the read sites **each checked on their own**, each with its own binding flags,
so the probe could report `ready` while a read site was broken: putting the `_longPressDuration` bug back with the registry kept correct,
**the offline 420 tests and 11 gates were all green**. That the live run caught it was half luck: the two places made the same mistake.
Now a call site can only ask the registry for a member, with one lookup and one set of flags, so what the probe reports is what the mod actually gets.
The contract also no longer recognizes only names starting with an underscore: 5 method/property names had slipped past it before,
among them `NEndTurnButton.CanTurnBeEnded` (a private getter that decides whether `end_turn` is ready, and always answers "ready" when missing), which is now registered.

While making the registry the single lookup source, **4 reflection reads that had never succeeded on the installed game were dug up**:
`continue_game_over` looked for three handler names on the button (none of them exist), and the game-over overlay looked on `NGameOverScreen`
for two methods that actually belong to `NCombatRoom` / `NRewardsScreen`. They all stood in front of the code that does the real work and never took effect,
so behavior was unaffected, and they were removed. The 4th is not harmless; see the next entry.

**The `moves` of `GET /data/monsters` was always an empty array, now fixed (separate PR).** `MonsterModel.MoveNames` no longer exists in the installed
`sts2.dll`, so the export had always been `moves: []`. Looking at the old implementation showed it had only ever been a public localization table lookup
(`LocManager.Instance.GetTable("monsters").GetLocStringsWithPrefix(...)`), and these three APIs are all still public in the installed version,
so the fix was not to swap in another set of reflection but to **not use reflection at all**: call directly, checked by the compiler, so an upstream rename turns the build red.
It took three live rounds to get right: round one had moves for 104/107 but 9 monsters had dialogue mixed in and duplicates appeared (dialogue keys also hang under the same prefix) -> take only
`.title`; round two brought duplicates to zero, but the three segments of the millipede were still empty (they share one set of text, and the key is not their own id) -> the prefix is now derived from the monster's own
Title key; round three gave 107/107 with zero duplicates, and the other 104 matched the previous round one by one.

**The other 11 handlers without a behavior contract are now covered (`HandlerContract.*`, 10 tests).** Three apply to all and are checked by table:
the predicate a refusal is based on is consistent with the predicate on which the action surface offers that action (`crystal_set_tool` originally inlined a copy of
`CanPlayCrystalSphere`, and now calls it directly); a refusal is a 409 that names the action; `completed` can only come from an observed change.
For the rest, each handler is pinned on the one or two spots most likely to be "casually changed", such as an area potion not being allowed to specify a target, and the chest relic's
index check and pick having to come from the same synchronizer. While adding the contracts, one missing null check in `open_character_select` was fixed in passing (it used to be a 500).

**`/state` build timing is now added.** The timing is placed inside `BuildStatePayload` rather than outside the `/state` route: action responses and
SSE refreshes build the same payload and run on the same game thread, and route timing would never see them even once. The 100 ms threshold is derived
(6 frames at 60 fps), not measured: the logs at hand have only 9 `/state` samples, too few to set a percentile; the `state_build` summary of `/health`
is the data source for setting the threshold in future.

**Line-number anchors in the specs were changed to symbol references.** The real number was 41 (across three specs), and the target of each was checked on replacement:
the anchor of `build_parser` had drifted into the middle of another function, three test links landed on blank lines, and "client definition" pointed at
`Sts2ApiError`. The `doc-links` gate changed from "reject only out-of-range line numbers" to rejecting every line-number anchor, and the self-test gained in-range cases.

**Reads that guessed members by name were all changed to go by type.** Each "candidate list" style read was checked against the installed `sts2.dll`:
most of the names did not exist at all. Two were real defects: the relic `stack` read a nonexistent `Amount` and was always null; of the seven candidate names for card modifiers
only `Keywords` exists, and its enum values were extracted as empty, so no card had keyword/enchantment tags (the real enchantment
property is the singular `Enchantment`, which was never on the list). The rest were dead fallbacks. `DuckTypedNames` now also requires every entry to still be used.

### Deferred (Not Done This Round, to Re-check Before the Next Release)

1. **End-to-end for a real upstream's very long stall**: the evidence stops at the production timeout contract (production request timeout 10 minutes, HTTP client 11 minutes, 22907b0/#80) and the local loopback regression; a long stall was not reproduced against a real upstream.
2. **Origin scenario with a real browser page load**: the evidence stops at the offline MCP Origin contract and local probing, not a full browser-page exploit.
3. **Native-speaker review of the English text**: the English UI and status text of v0.11.0 is machine-translated and was not reviewed by a native speaker (see "Not covered" in the [localization acceptance](history/localization-2026-09-12.md)).
4. **Whether to add a GitHub tag for v0.10.7**: v0.10.7 (`f9330ba`) updated only the Steam Workshop, with no tag and no Release, while that commit is already included in `v0.11.0`; whether to backfill a tag is for the releaser to decide.
5. **The Workshop Simplified Chinese listing**: the page still has the old text, missing the list items that were written into the repository in v0.11.0 and v0.12.0 (confirmed by scraping the page on 2026-09-13). `ModUploader upload` has only `-w` / `-i` and no language parameter, so `steam-workshop/description.zh-CN.txt` can only be pasted by hand on the Workshop web page.

### Remaining Items and Evidence Boundaries

- Workshop subscription loading is closed out; a second restart round or an upgrade/rollback was not run, and these extensions are not counted as verified.
- The live results cited on this page all come from isolated copies and local stubs (2026-09-08 / 2026-09-10 / 2026-09-11 / 2026-09-12), and do not represent the official install or real saves. A complete long run to its natural end is not a bar for wrapping up the documentation.
- Known observation: the console fight/die in dual-instance causes desync; in the short-path settlement total_losses was not observed to go +1, so save_verified cannot be widened to mean all statistics fields were accepted.
- Product boundary: the model compatibility matrix is mainly source and offline protocol evidence, and does not mean every provider was tested live. Proactive speech and the optional tone have completed isolated live acceptance with a local stub (see history/validation-acceptance_2026-09-10.md, 2026-09-11.md), but **were not accepted with a real model**: whether a real model says the right thing at the right time in a real run is still unverified.
- The five goals of 09-12 this round (the post-tag list in §1) got only offline code and documentation wrap-up: C# / MCP unit tests + verification gates + static preflight, with no live re-verification; the new screen names (`FAKE_MERCHANT` / `PATCH_NOTES` / `CARD_INSPECT` / `RELIC_INSPECT` / `FEEDBACK`) and the timeline index contract are still conclusions at the code contract level.

## 5. Follow-up Maintenance Rules

1. Only PRODUCT_PLAN_CURRENT.md describes the current state; the COOP root file serves only as a historical evidence index and must not grow into a second task board.
2. For each capability, record together the implementation commit/version, release tag, acceptance evidence date and source, and remaining gap. "Released" can only come from verifiable tag/release evidence.
3. Post-tag commits must be listed separately as "mainline unreleased" until an explicit release tag appears.
4. The original plans and the old COOP text go in history/, prominently marked "Historical; does not represent the current state".
5. The README entry points keep pointing at this page and the COOP historical index; the relative targets ./PRODUCT_PLAN_CURRENT.md and ./COOP_DELIVERY.md that the packaging script depends on stay literally unchanged.
6. For the archive directory see [history/README.md](history/README.md). Old documents keep only point-in-time evidence and must not be reused as a task board; the redirect pages at the original paths serve compatibility with references and preflight.
