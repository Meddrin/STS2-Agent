# Journal - CharTyr (Part 1)

> AI development session journal
> Started: 2026-09-08

---



## Session 1: GAME_OVER save wait and reliability merge

**Date**: 2026-09-08
**Task**: GAME_OVER save wait and reliability merge
**Branch**: `main`

### Summary

Fixed GAME_OVER continue so native summary save runs before Return; merged that plus session reliability work to main via PRs 79 and 80. Isolated floor-0 die verified save_verified. Dual GAME_OVER save on the new DLL was not completed.

### Main Changes

- continue_game_over waits for native Return instead of enabling it after 15s
- Session budget, corrupt settings restore, MCP Origin, LLM stall timeout, and play_card cancel landed on main

### Git Commits

| Hash | Message |
|------|---------|
| `4b4da6e` | (see git log) |
| `22907b0` | (see git log) |

### Testing

- [OK] Contract tests including GameOver.NoForcedReturn
- [OK] Isolated singleplayer floor-0 die: continue 2.47s, save_verified, progress.save mtime after continue

### Status

[OK] **Completed**

### Next Steps

- Finish dual-instance GAME_OVER save verification on current main DLL
- Do not archive 00-bootstrap-guidelines until frontend spec files are filled


## Session 2: v0.10.5 release, Workshop public visibility and P3 closeout

**Date**: 2026-09-09
**Task**: v0.10.5 release, Workshop public visibility and P3 closeout
**Branch**: `main`

### Summary

Closed out the v0.10.5 release, the P3 support scope, and the Workshop default-public fix checks; the workspace is clean and no active task needs archiving.

### Main Changes

- The v0.10.5 GitHub release, Steam install, P3.1-P3.4 checks, and Workshop content upload were completed earlier; evidence is in the work commits and the P2/P3 entries of PRODUCT_PLAN_CURRENT.md.
- 96bd410 sets workshop.json to public by default; the packaging script defaults to public when updating an existing PublishedFileId, a first-time ID=0 stays private, and an explicit Visibility parameter takes precedence.

### Git Commits

| Hash | Message |
|------|---------|
| `e3412eb` | (see git log) |
| `04d2466` | (see git log) |
| `15e483c` | (see git log) |
| `2838250` | (see git log) |
| `96bd410` | (see git log) |

### Testing

- [OK] Queried Steam GetPublishedFileDetails directly this time: item 3796486050, result=1, visibility=0, title=STS2 AI Agent.
- [OK] Read STS2AIAgent.json from the local Steam Workshop subscription directory this time, version=0.10.5; git diff --check passed and the working tree was clean before wrap-up.
- [OK] Earlier acceptance record: C# core tests 180 PASS; preflight-release and release artifact checks passed; two-instance GAME_OVER short path save_verified=true; P1.3 over-limit UI and P3.3 external MCP client passed. These tests were not rerun this time.

### Status

[OK] **Completed**

### Next Steps

- PRODUCT_PLAN_CURRENT.md still has an outdated baseline and a partially-done P2.5 description that need a later docs sync; Workshop public visibility and the local subscribed version are confirmed, but this time the game was not launched from the subscription directory to verify loading.


## Session 3: Documentation archive and Workshop loading acceptance

**Date**: 2026-09-09
**Task**: Documentation archive and Workshop loading acceptance
**Branch**: `main`

### Summary

Archived five outdated documents while keeping compatibility entry points, aligned the current status page, the Chinese and English READMEs, CHANGELOG, and the Workshop release notes, and recorded the acceptance of loading from the subscription after the user enabled it and restarted.

### Main Changes

- P2.5 marked complete within the scope of a subscription-load smoke test: user screenshot and restart notes; this run's log shows the DLL/PCK loaded from Workshop 3796486050, PID 40664, health 0.10.5 ready, state/actions MAIN_MENU.

### Git Commits

| Hash | Message |
|------|---------|
| `5ff7303` | (see git log) |

### Testing

- [OK] 59 local links in 18 changed documents are valid; check_release_package.py --source-root . passed; git diff --cached --check passed. The docs-only commit did not rerun the build or game tests.

### Status

[OK] **Completed**

### Next Steps

- A full run, a second restart round, and upgrade rollback are outside the scope of this subscription-load acceptance; the docs and log commits have not been pushed yet.


## Session 4: Repo hardening: dep security, proactive chat, doc contract, verification gates

**Date**: 2026-09-10
**Task**: Repo hardening: dep security, proactive chat, doc contract, verification gates
**Branch**: `main`

### Summary

Closed issues #50/#51 by refreshing uv.lock (fastmcp 3.4.7) and package-lock.json (fast-uri 3.1.7, npm audit 0). Implemented off-by-default proactive teammate chat with selectable tone behind a read-only chat path, covered by 17 new C# core tests. Documented all 55 mod actions in docs/api.md with a set-equality guard. Archived the stale coverage gap list to history/ with a redirect and marked five dated records. Added check_verification_gates.py plus a negative-path self-test and wired both into preflight and CI. Full offline sweep green; proactive chat remains not live-validated.

### Git Commits

| Hash | Message |
|------|---------|
| `2bc857d` | (see git log) |

### Status

[OK] **Completed**


## Session 5: v0.10.6/v0.10.7/v0.11.0 releases, acceptance hardening, and game-language localization

**Date**: 2026-09-12
**Task**: v0.10.6/v0.10.7/v0.11.0 releases, acceptance hardening, and game-language localization
**Branch**: `main`

### Summary

Continued the thread that started in Session 4: acceptance hardening (card-grid selection metadata, combat-hand pick settling, run-boundary stop, locale-proof gate self-test), then three releases. v0.11.0 makes the mod follow the games language - Chinese stays the source text, every other language reads an English table, and the overlay rebuilds on a mid-session language change. Three places that keyed behaviour off Chinese wording were fixed. All three releases went to GitHub and the Steam Workshop; the v0.11.0 Workshop upload was first misjudged as failed, then confirmed by Steam own workshop_log.

### Main Changes

- v0.11.0 localization: Loc (table, language detection) split from LocSource (Godot side) so the lookup stays testable offline; five shards, ~336 entries; a call-site coverage test fails any Chinese literal with no English entry; a frozen-text test fails any field or computed-once property that resolves text at construction.
- Three behaviour bugs that only surfaced on an English client: the glossary keyword match, the co-op invite result, and the failure classifier all read Chinese wording; they now key off explicit flags and English candidates.
- Acceptance hardening: card-grid selection metadata read from the base screen type (enchant 1/1/0 -> 0/3/0, first pick 10036 ms -> 151 ms); a combat-hand pick reports pending until confirmed; the run-boundary stop restored; the gate self-test made locale-proof; the pytest-style invocation replaced with the runner the project actually uses.
- Three releases shipped to GitHub and the Steam Workshop: v0.10.6, v0.10.7, v0.11.0 (Workshop manifest 3781676487912021003).

### Git Commits

| Hash | Message |
|------|---------|
| `2f75e4a` | (see git log) |
| `f9330ba` | (see git log) |
| `84631b9` | (see git log) |
| `d77982a` | (see git log) |
| `e565073` | (see git log) |
| `bd93662` | (see git log) |
| `1ce254b` | (see git log) |
| `6aabb4f` | (see git log) |
| `a1bd5b5` | (see git log) |
| `06924e4` | (see git log) |
| `ed1c65f` | (see git log) |
| `dd71769` | (see git log) |

### Testing

- [OK] dotnet run --project STS2AIAgent.Tests -c Release: 232 PASS, exit 0
- [OK] uv run --locked python -m unittest discover -s tests: 55 tests OK
- [OK] check_verification_gates.py pass; preflight-release.ps1 exit 0
- [OK] Live on an isolated game copy: Chinese regression reads as before; English cold start and mid-session language switch verified by screenshot; state payload glossary and deck lines read English

### Status

[OK] **Completed**

### Next Steps

- The English wording is machine-translated and was not reviewed by a native speaker; only Chinese and English were exercised.
- v0.10.7 has no GitHub tag - its Workshop upload predates the release, so the Releases list jumps from v0.10.6 to v0.11.0. Backfill the tag if the list should be contiguous.
- Steam Workshop uploads stall on manifest fetches from steampipe-partner.akamaized.net; judge success from Steam workshop_log.txt, never from the uploader stdout.


## Session 6: Agent trust hardening: 5 goals from the 2026-09-12 audit

**Date**: 2026-09-12
**Task**: Agent trust hardening: 5 goals from the 2026-09-12 audit
**Branch**: `main`

### Summary

Ran six parallel scouts over the mod, MCP sidecar, docs, tests, and gameplay skill, then turned the findings into five independently verified deliverables executed in importance order. 1) Action trust: resolve_rewards rejects an explicit out-of-range index instead of quietly taking the first card, an open modal no longer counts as a finished continue_run/embark/open_character_select transition, remove_card_at_shop surfaces a failed purchase, bundle actions fail with 503 instead of a fabricated empty state, and a play_card that never left the hand rolls its counters back (72c96fd). 2) Bounded waits: nine handlers awaited a native task with no deadline while Router waits on the handler; each now goes through WaitForGameTaskAsync with a background observer and an honest pending, plus a source contract proven by mutation probe (12c35b3). 3) Agent contract: get_game_state carries compact_agent_view, wait_until_actionable returns actionable on every path, the full-profile resolve_rewards stops requiring option_index, and the gameplay skill stops naming raw-only fields for the compact view it reads (33b137e). 4) Screens and indexes: choose_timeline_epoch shares the state index space, and FAKE_MERCHANT / PATCH_NOTES / CARD_INSPECT / RELIC_INSPECT / FEEDBACK stop being dead ends (5457e0d). 5) Docs baseline: the state page cites v0.11.0, AGENTS.md lists all five version files, the route lists match the router, and preflight now runs the CI-only gate self-tests (3f55a3f). C# tests 232 -> 272, MCP tests 55 -> 76, gates and a 12-step preflight green. No version bump, tag, or Workshop upload.

### Git Commits

| Hash | Message |
|------|---------|
| `3f55a3f` | (see git log) |

### Status

[OK] **Completed**


## Session 7: Reward choice threading: remove the cross-request static card choice

**Date**: 2026-09-12
**Task**: Reward choice threading: remove the cross-request static card choice
**Branch**: `main`

### Summary

Follow-up to the action-trust child, chosen by the user as option A. resolve_rewards kept its card choice in a process-wide static field that only the card-reward consumer cleared; a drain that never reached a card-reward screen left the value behind, so a later collect_rewards_and_proceed could inherit it and silently skip a card reward or pick a card the caller never asked for. The choice now lives in a per-request RewardFlowChoiceState passed down through DrainRewardFlowAsync, collect_rewards_and_proceed asks for the automatic choice explicitly, and consuming once per drain keeps the within-call behavior identical including the out-of-range 409. _cardRewardSkipped was deliberately left byte-identical because the skip flow needs it to outlive the request; its timeout-exit staleness is recorded as a separate follow-up. A semantic change is documented in docs/api.md: an explicit choice belongs to the call that carries it, so retrying a pending resolve_rewards with collect_rewards_and_proceed resolves automatically. Review fixed two vacuous tests, one of which would have let the explicit index be dropped silently. C# 278 PASS, MCP 76 OK, gates and a 12-step preflight green.

### Git Commits

| Hash | Message |
|------|---------|
| `abc195f` | (see git log) |

### Status

[OK] **Completed**


## Session 8: Reward skip scope: bind the skip intent to the reward set that recorded it

**Date**: 2026-09-12
**Task**: Reward skip scope: bind the skip intent to the reward set that recorded it
**Branch**: `main`

### Summary

Second follow-up to the action-trust work, chosen by the user as option B. The card-reward skip lived in a process-wide bool that only the drain branch observing a screen change cleared; a drain ending through its own proceed click, the empty-reward escape, or the timeout left it set, and the reward-button filter then excluded the CardReward button from the next reward set, silently dropping that set card reward. The intent genuinely must outlive its request (skip_reward_cards is one call and the collect that follows is another), so instead of threading it the fix keys it to the owning NRewardsScreen instance id: recorded on the selection screen by finding the sibling that shares the overlay stack, honored only for that same set, and ignored when the owner cannot be resolved. The fail-safe direction is deliberate: a missing identity re-shows a visible card reward rather than dropping one. Decompiled evidence confirms the selection overlay is pushed onto the same stack while the owning rewards screen stays there as a live sibling, so the recorded and read ids name the same object. Review found no defect and used mutation probes to prove the new tests are not vacuous (renaming the predicate to an unscoped bool still fails; removing the id-zero guard fails). C# 290 PASS, MCP 76 OK, gates and a 12-step preflight green.

### Git Commits

| Hash | Message |
|------|---------|
| `fa7ffbe` | (see git log) |

### Status

[OK] **Completed**


## Session 9: Close seven audit gaps and harden the offline test floor

**Date**: 2026-09-12
**Task**: Close seven audit gaps and harden the offline test floor
**Branch**: `main`

### Summary

Eight tasks from the residual audit: invite_ai_teammate now classifies on a structured DualLaunchOutcome instead of Chinese substrings, three menu waits stop treating a destroyed node as proof of success, the scene field sets match the real export schema, the dead client.py block is gone, the knowledge root no longer guesses outside a checkout, and the previously untested ApiException/JsonHelper/HttpServer-policy/network_server/knowledge/handoff surfaces have tests.

### Main Changes

- AgentRuntime records a per-branch DualLaunchOutcome; the invite handler maps it to completed/pending/409 invite_failed (was: substring match on localized text)
- MenuTransitionPolicy gains IsSubmenuObserved/IsFlagObserved; select_deck_card, confirm_timeline_overlay, crystal_set_tool and run_console_command stop reporting optimistic success
- GameDataFilter scene fields corrected against a new GameDataExportSchema constant, pinned by a bidirectional drift test
- knowledge.py stops guessing the repo root (parents[3]) and degrades reference_files to an empty list outside a checkout
- Removed an unreachable execute_action block in client.py; added an AST guard for all 55 per-action methods

### Git Commits

| Hash | Message |
|------|---------|
| `206a0e8` | (see git log) |
| `adcb49b` | (see git log) |
| `f57cb04` | (see git log) |
| `c08d764` | (see git log) |
| `92f67a2` | (see git log) |
| `52bafd0` | (see git log) |
| `26da6bc` | (see git log) |
| `e373c90` | (see git log) |

### Testing

- [OK] C# offline runner: 311 PASS / 0 FAIL (was 290); each of the eight commits re-verified in a detached worktree (296/300/303/311)
- [OK] MCP unittest: 159 OK (was 76); stage commits verified at 82/121/156/159
- [OK] check_verification_gates.py, check_release_package.py --source-root ., preflight-release.ps1 all pass

### Status

[OK] **Completed**

### Next Steps

- Residual small issues: zero-reference scripts, mcp_server/data/eng packaging, select_deck_card availability asymmetry, crystal_clear_cell doc gap
- Decide whether to push the 26 commits and whether to tag v0.10.7


## Session 10: Give the compact view what it needs, keep errors truthful, and gate the drift

**Date**: 2026-09-12
**Task**: Give the compact view what it needs, keep errors truthful, and gate the drift
**Branch**: `main`

### Summary

Six tasks: two residual audit gaps (select_deck_card availability, the crystal_clear_cell doc exemption), the compact agent view finally carries powers / intent numbers / card and relic ids / overlay context / party, the in-game path keeps ApiException code and retryable, autoplay stops both spinning and false-stopping, three drift-prone facts got gates, and the scene field tables agree across languages.

### Main Changes

- compact agent_view gains powers (both sides), enemy intents with numbers, card_id/relic_ids/card_ids, modal.underlying_screen, unlock, and party summaries
- GetDeckSelectionOptions loses its generic subtree fallback so availability equals executability; the executor guard stays as defence
- AgentErrorEnvelope gives the in-process path the same error fields as the HTTP envelope; skills contract explains how to use them
- NoProgressPolicy + AutoPlayRecovery: repeat threshold on (action, state fingerprint) and a bounded unsettled budget instead of counting pending as failure
- check_verification_gates.py gains api-facts (mod_version, screen enum, default port); README tool list bound to the registry; static packaging check now runs in CI
- Cross-language scene field alignment test closed the C#/Python drift; data/eng README now says it is a snapshot, not a source

### Git Commits

| Hash | Message |
|------|---------|
| `ca12a4f` | (see git log) |
| `79d8747` | (see git log) |
| `f7dccfe` | (see git log) |
| `40735b5` | (see git log) |
| `ed1b810` | (see git log) |
| `98fca75` | (see git log) |

### Testing

- [OK] C# offline runner: 335 PASS / 0 FAIL (was 313); every commit re-verified in a detached worktree (313/317/325/335/335/335)
- [OK] MCP unittest: 167 OK (was 159); stage commits verified at 159/159/159/159/160/167
- [OK] verification gates (now 5), check_release_package, preflight all pass

### Status

[OK] **Completed**

### Next Steps

- Deferred to the user: whether to delete mcp_server/data/eng (1.2 MiB, no reader, now honestly documented), whether to keep the zero-reference scripts, and whether to push the accumulated commits or tag v0.10.7
- ResolveNonModalScreen still shadows NCardRewardSelectionScreen => REWARD behind a generic CARD_SELECTION branch; changing it moves /state.screen semantics and needs live confirmation
- Live-only: compact field values, invite outcome, timeline overlays, and the port/dual-instance paths


## Session 11: Take docs under version control, wire the orphan self-test, guard packaging

**Date**: 2026-09-12
**Task**: Take docs under version control, wire the orphan self-test, guard packaging
**Branch**: `main`

### Summary

Four residuals closed: docs/ became a controlled directory with a docs-tracked gate (two documents had drifted out of version control and the local doc-marks gate was checking a file CI could not see), the operations spec was refreshed against the six gates and five version sources, the zero-reference budget-proxy self-test was wired into preflight and CI, and package-release now refuses to build on a version mismatch. Wiring the self-test surfaced that it was 5-8 percent flaky - the mock upstream spoke HTTP/1.0 and never read the request body, which Windows turns into an RST the proxy reports as a 502.

### Main Changes

- docs-tracked gate plus .gitignore cleanup; the two untracked pages are now committed
- preflight gains a budget-proxy step (12 to 13 OK steps) and CI runs the same command
- selftest harness fixed: MockUpstream protocol_version HTTP/1.1 plus _drain_body; 0 failures in 400 posts and 15/15 full runs, versus 2/40 and 2/150 before
- package-release.ps1 asserts five-way version agreement before building anything
- operations spec and index refreshed: six gates, five version sources, a script inventory naming the deliberately unwired scripts

### Git Commits

| Hash | Message |
|------|---------|
| `6e56e1d` | (see git log) |

### Testing

- [OK] C# 335 PASS / 0 FAIL; MCP 167 OK; six gates pass in a clean checkout; preflight 353 PASS / 0 FAIL with 13 OK steps

### Status

[OK] **Completed**

### Next Steps

- Screen-name truth: ResolveNonModalScreen's generic grid-holder branch returns CARD_SELECTION for the reward-card overlay, shadowing its own REWARD arm and telling the model to use an action that screen no longer offers
- Live-only: compact field values, invite outcome, package-rollout paths


## Session 12: Five new goals: screen-name shadowing, embedded contract, live assets, compile coverage, exposure scope

**Date**: 2026-09-12
**Task**: Five new goals: screen-name shadowing, embedded contract, live assets, compile coverage, exposure scope
**Branch**: `main`

### Summary

Opened a task for each of the 5 findings from the probes, dispatched workers to implement them, split the commits per task, verified in a temporary worktree after each commit that the tree builds, and archived everything.

### Main Changes

- ResolveNonModalScreen: the reward card-pick overlay now reports REWARD (previously shadowed by the generic grid branch as CARD_SELECTION)
- The embedded play contract no longer requires calling health_check inside the game, debug commands are marked external-MCP-only, and both README languages are aligned
- Added Roslyn syntax coverage: 16 uncompiled source files (18k lines) are now protected by CI, and the uncompiled set is pinned by an allowlist assertion
- Three live scripts aligned with the current contract (open_timeline assertion, timeline index, lobby flow screen branch)
- Payload-derived values share a source with the execution scope; skip_reward_cards on both ends now only recognizes an enabled alternative button

### Git Commits

| Hash | Message |
|------|---------|
| `eec80b9` | (see git log) |
| `c060554` | (see git log) |
| `335ba0a` | (see git log) |
| `94765bc` | (see git log) |
| `492722a` | (see git log) |

### Testing

- [OK] Each of the 5 commits verified in a temporary worktree: 336/339/341/341/348 PASS, 0 FAIL; all 6 gates green
- [OK] Final state: C# 348 PASS/0 FAIL, MCP 167 OK, 6 gates, check_release_package passed, preflight exit 0

### Status

[OK] **Completed**

### Next Steps

- User decides: whether to push the accumulated commits, whether to keep the mcp_server/data/eng snapshot, the quasi-orphan scripts, and whether to add screen names for NCardPileScreen/NCardLibrary


## Session 13: Remove the bundled game data snapshot (a v0.5.0 leftover, superseded by the Mod export in v0.6.1)

**Date**: 2026-09-12
**Task**: Remove the bundled game data snapshot (a v0.5.0 leftover, superseded by the Mod export in v0.6.1)
**Branch**: `main`

### Summary

After tracing the design intent of mcp_server/data/, removed it per the user's decision: deleted the directory and the only test that validated it, took out four packaging hooks, corrected stale statements in README and AGENTS.md, and used an unpacked wheel/sdist to assert it is really absent from the artifacts.

### Main Changes

- Deleted mcp_server/data/ (21 files) and tests/test_packaged_game_data.py
- pyproject.toml drops the two hatch force-includes; package-release.ps1 drops the line that copied data
- check_release_package.py deletes ARTIFACT_DIRECTORIES and its traversal loop (has_directory is kept)
- README directory trees in both languages, the data-source note in mcp_server/README.md, and the local guidance in AGENTS.md updated in sync

### Git Commits

| Hash | Message |
|------|---------|
| `5cc314f` | (see git log) |

### Testing

- [OK] Build artifact unpack assertion: wheel 11 entries / sdist 29 entries, zero data/ paths
- [OK] Clean worktree verification: C# 0 FAIL, 6 gates green, package source contract passed, MCP 165 OK

### Status

[OK] **Completed**

### Next Steps

- User decides: whether to push the accumulated commits (local main is ahead of origin/main)


## Session 14: First push and two environment couplings exposed by CI (.NET SDK selection, 8.3 short paths)

**Date**: 2026-09-12
**Task**: First push and two environment couplings exposed by CI (.NET SDK selection, 8.3 short paths)
**Branch**: `main`

### Summary

After pushing 64 commits, CI went red twice in a row: the Roslyn coverage test was broken by CS1705 from the .NET 10 SDK, and the knowledge path test ran into Windows 8.3 short names. Both were exposed only on real CI, both were fixed with falsifiable verification, and CI finally went fully green at 13/13.

### Main Changes

- Added global.json: pins the SDK feature band to the 9.0.x declared by the workflow so Roslyn from .NET 10 is not picked
- The knowledge reference file path test now compares after applying realpath+normcase on both sides (reproduced and verified with a real 8.3 alias)

### Git Commits

| Hash | Message |
|------|---------|
| `82c804f` | (see git log) |
| `61146a5` | (see git log) |
| `746f154` | (see git log) |

### Testing

- [OK] CI 746f154c all green: all 13 steps passed (including 11 steps that had never run in CI before)
- [OK] local: C# 348/0 FAIL, MCP 165 OK, 6 gates, mod build, package source contract all green

### Status

[OK] **Completed**

### Next Steps

- Optional: whether to add a tag for this round / whether to change branch protection to allow direct pushes


## Session 15: Rename and escape for the deck/pile viewer screens + PowerShell syntax gate

**Date**: 2026-09-12
**Task**: Rename and escape for the deck/pile viewer screens + PowerShell syntax gate
**Branch**: `main`

### Summary

The two screens no longer masquerade as CARD_SELECTION and each gets a genuinely usable close action; added a ps1-syntax gate covering all 30 .ps1 files, which on its first run caught a real syntax error in serve-sts2-network-mcp.ps1 that had existed since the commit that introduced it.

### Main Changes

- NCardLibrary/NCardPileScreen early-return CARD_LIBRARY/CARD_PILE instead of being shadowed by the generic grid branch
- The submenu stack lookup moved up to the base class NSubmenuStack, so the in-run card library can now be closed
- Closable card-view screens collapsed into a single predicate, shared by the probe/executor/wait condition, and the pile screen can exit via close_cards_view
- Added the ps1-syntax gate (parse only, never execute; skipped with an explanation when there is no .ps1 or no interpreter) and fixed the genuinely faulty script it caught

### Git Commits

| Hash | Message |
|------|---------|
| `5b3439c` | (see git log) |
| `abb99c5` | (see git log) |

### Testing

- [OK] 5 groups of break-red-restore-green; C# 351 PASS/0 FAIL; MCP 165 OK; all 7 gates green; preflight 372 PASS/0 FAIL
- [OK] Each of the two commits verified in a temporary worktree that the tree builds

### Status

[OK] **Completed**

### Next Steps

- Push the two commits; confirm on live the BackButton behavior of the pile screen and the Pop of the in-run card library, as well as the other in-run submenus that were fixed along the way


## Session 16: First real-machine live session: checklist cleanup + two findings only live runs could expose

**Date**: 2026-09-12
**Task**: First real-machine live session: checklist cleanup + two findings only live runs could expose
**Branch**: `main`

### Summary

Consolidated the live checklist into a document and ran a real live session: verified the REWARD rename on the reward screen, the full timeline path and index contract, and 7 data collections; along the way found and fixed the /data/powers whole-collection 500 and two incorrect timeline comments.

### Main Changes

- Fixed /data/powers 500 (a missing localization key broke the export of the whole collection)
- Fixed the incorrect timeline button rule comment in the script
- Added docs/live-validation-checklist.md to consolidate the live items

### Git Commits

| Hash | Message |
|------|---------|
| `7888566` | (see git log) |
| `6478dae` | (see git log) |
| `4554c8d` | (see git log) |

### Testing

- [OK] Live: REWARD rename on the reward screen, full timeline path and index contract negative cases, 7 collections reachable, two scripts passed
- [OK] Offline: C# 351/0, MCP 165 OK, 7 gates green, preflight 372/0; CI 4554c8d all green

### Status

[OK] **Completed**

### Next Steps

- The live items for card-viewer-screens (pile screen/library) need a player click or a new mod action; the save still needs a reward card picked


## Session 17: Three live leftovers wrapped up: paged FTUE, scene-derived metadata ids, co-op base HP

**Date**: 2026-09-13
**Task**: Three live leftovers wrapped up: paged FTUE, scene-derived metadata ids, co-op base HP
**Branch**: `main`

### Summary

Wrapped up three findings left over from the v0.12.1 live acceptance: the paged FTUE was correct behavior all along (changed into a self-explanatory contract), item_ids of get_relevant_game_data can now be omitted, and a new base_max_hp separates the base HP roll from co-op scaling. Checking the derivation path also caught two defects (a crash on a null scene payload and a disagreement between the two mirrors on empty-string ids), which were fixed together and re-verified live.

### Main Changes

- When NCombatRulesFtue returns pending on a non-final page it now gives a self-explanatory message (three pages are game design, so three clicks were the correct behavior all along)
- item_ids of get_relevant_game_data can be omitted: derived from live state for the current screen, falling back to character-level ids when the scene has no content; the C# and MCP tables are pinned equal by an alignment test
- Added combat.enemies[].base_max_hp: the base HP roll before co-op scaling, on the same scale as monsters.min_hp/max_hp
- Fixed the derivation path stepping on JSON null (FAKE_MERCHANT classified as shop but with no shop payload) and the C#/Python disagreement on empty-string ids
- Evidence and contract written into docs/live-validation-checklist.md and docs/api.md; the status page now lists PR #101/#102

### Git Commits

| Hash | Message |
|------|---------|
| `d826935` | (see git log) |
| `7714f0b` | (see git log) |

### Testing

- [OK] Live (isolated save, clientId 2026091001): after the FTUE three clicks pending/pending/completed the modal cleared and returned to COMBAT
- [OK] Live: the MCP tool surface with item_ids omitted returns hand/enemies/relics; explicit ids and the empty set behave unchanged
- [OK] Live two-player run: base_max_hp 9/33/13 vs live 19/72/28 (base x player count x act0 factor), the numbers match across both instances
- [OK] Offline: C# 382 PASS/0 FAIL, Python 167 OK, all seven gates green, mod build 0 warnings; the player's real save, 183 files, hashed identically before and after
- [OK] CI: contracts + Sourcery all green for #101 and #102

### Status

[OK] **Completed**

### Next Steps

- The FAKE_MERCHANT fallback currently has only an offline unit test; that screen was not reached live
- The paged FTUE reproduces only on a single-player save (the tutorial is already completed on a co-op save)
- The three fixes have not shipped with any tag yet (later than v0.12.1); verify them with the package at the next release


## Session 18: Release v0.12.2: co-op relay (external takeover route) + Workshop upload

**Date**: 2026-09-13
**Task**: Release v0.12.2: co-op relay (external takeover route) + Workshop upload
**Branch**: `main`

### Summary

Shipped the three batches of backlog on main (#85 external takeover route, #99 shared model gate, #101 three live wrap-ups) as v0.12.2 and brought the Workshop to the same version. While packaging, the artifact check blocked a real defect: the new relative links in README were not in the packaging rewrite table, so link fixing was added along with the eighth offline gate (packaged-links). The Workshop upload failed six times because the Steam client lost contact with the UGC backend, and succeeded on the first try after restarting Steam.

### Main Changes

- Bumped the five version files to 0.12.2; the CHANGELOG Unreleased section was finalized as v0.12.2 and entries for #99 / #101 were filled in
- Fixed the README relative links not rewritten by packaging (introduced by #97), without changing the rewrite table so that similar problems are not masked
- Added the packaged-links gate: parses the packaging rewrite table + the imported artifact manifest and link rules, replays the rewrite on the source docs, and checks that all local links are in the artifact
- history/release-v0.12.2_2026-09-13.md records the release, the upload, and how the Steam disconnect incident was handled
- The status page baseline switched to v0.12.2

### Git Commits

| Hash | Message |
|------|---------|
| `40d1464` | (see git log) |
| `72b2a81` | (see git log) |
| `b64e7e7` | (see git log) |

### Testing

- [OK] All preflight steps passed; all eight gates green; gate self-tests passed; packaged artifact checks (directory and zip) passed
- [OK] Gate destructive verification: when README was written back with a bare docs/api.md the gate reported that target with exit code 1; after a byte-for-byte restore it passed again
- [OK] CI: contracts green for #104 and #105 (the first failure of #105 was the runner not being acquired; it was green on rerun)
- [OK] Workshop recheck: file_size 1225733 equals the byte sum of the local content, visibility=0, time_updated 2026-09-13 16:51:04, tags unchanged, English description not overwritten

### Status

[OK] **Completed**

### Next Steps

- The Workshop Simplified Chinese listing has still not been updated (since v0.11.0); steam-workshop/description.zh-CN.txt needs to be pasted manually on the Workshop web page
- The external takeover route has still not been rerun on the Steam two-instance path; the FAKE_MERCHANT fallback has only an offline unit test
- If the Workshop upload reports No Connection again, restart the Steam client first (recorded in the release notes)


## Session 19: Merge sachi4clover's overlay invite route fix (PR #106) + records

**Date**: 2026-09-13
**Task**: Merge sachi4clover's overlay invite route fix (PR #106) + records
**Branch**: `main`

### Summary

Pushed the community PR #106 small fix to the author's branch and merged it: the overlay's invite button had not kept up with the route split in #85, so when the model was unverified clicking it went through the old gate, while the same request through the API could bring up the teammate. Also fixed a misplaced comment and created a new Unreleased section.

### Main Changes

- PR #106: the F8 window's "Invite AI teammate" button now chooses its route by FirstRunSetup.Evaluate(settings).ReadyToInvite, and the two descriptions switch with the route
- Moved the <summary> that the author had placed in the wrong spot back to EveryStartEntryPointSharesTheModelGate (the original implementation left that test without its comment and the new test with two)
- v0.12.2 used up the previous Unreleased section, so a new one was created and this change was written into it
- Status page: corrected the mislabeled "unreleased changes after the v0.12.1 tag" heading and added the two post-tag changes #105 / #106

### Git Commits

| Hash | Message |
|------|---------|
| `0f63d6d` | (see git log) |
| `085c941` | (see git log) |

### Testing

- [OK] Disposable worktree on the author's commit: 383 PASS / 0 FAIL, all eight gates green
- [OK] Guard effectiveness: after reverting the bug CoopRoute.OverlayInviteRoute turned red immediately, and after a byte-for-byte restore of the file it went green again
- [OK] CI: contracts green for #106 and #107

### Status

[OK] **Completed**

### Next Steps

- The v0.12.2 artifact on the Workshop does not include the #106 overlay fix; it needs to be carried by the next release
- The follow-up PR mentioned by sachi4clover (Continue button + character-select checkbox) stacks on top of #106; continue writing the CHANGELOG into the newly created Unreleased section
- The Workshop Simplified Chinese listing is still not updated; the wording "invite after the connection test passes" on the first screen of the Workshop English listing also needs adjusting with the next upload


## Session 20: Workshop update to v0.12.3 (Workshop only, no tag)

**Date**: 2026-09-13
**Task**: Workshop update to v0.12.3 (Workshop only, no tag)
**Branch**: `main`

### Summary

Shipped the #106 overlay invite route fix to the Workshop. Because #106 changed mod code, followed the v0.10.7 precedent: bump the patch number, update only the Workshop, no tag, no Release; the upload succeeded on the first try and was rechecked with the Steam Web API.

### Main Changes

- Bumped the five version files to 0.12.3; the CHANGELOG Unreleased section was finalized as v0.12.3 and states "Workshop only, no tag"
- changeNote in workshop.json updated to v0.12.3
- history/release-v0.12.3_2026-09-13.md records the upload and recheck
- Status page: baseline changed to "GitHub at v0.12.2, Workshop at 0.12.3", and the outdated claim that #106 was "not yet released" corrected

### Git Commits

| Hash | Message |
|------|---------|
| `b0217b0` | (see git log) |
| `00d1789` | (see git log) |

### Testing

- [OK] All preflight steps passed; all eight gates green; check_release_metadata reports 0.12.3
- [OK] Workshop recheck: file_size 1227269 equals the byte sum of the local content, visibility=0, time_updated 22:31:22, tags and English description unchanged
- [OK] Upload succeeded on the first try (with HTTP_PROXY/HTTPS_PROXY set beforehand); the v0.12.2 No Connection did not recur

### Status

[OK] **Completed**

### Next Steps

- Once sachi4clover's follow-up PR (in-game Continue button + character-select checkbox) lands, bundle #106 with it for a GitHub release, or backfill the v0.12.3 tag on its own
- The Workshop Simplified Chinese listing is still not updated; steam-workshop/description.zh-CN.txt needs to be pasted on the Workshop web page


## Session 21: Backfill the GitHub Release for v0.12.3 (completing the Workshop-first release)

**Date**: 2026-09-13
**Task**: Backfill the GitHub Release for v0.12.3 (completing the Workshop-first release)
**Branch**: `main`

### Summary

v0.12.3 was first shipped to the Workshop only; this round adds the tag and the GitHub Release, turning "Workshop first" into one complete release. The release DLL is the same size as the Workshop one and the PCK is byte-identical; only 70 bytes of build metadata differ (PE timestamp / MVID / debug directory), recorded in the release notes so that the hash difference between the two channels is not later misread as different builds.

### Main Changes

- git tag -a v0.12.3 b0217b0 and pushed; package-release packaging (directory and zip artifact checks both passed); gh release create with the zip and Chinese release notes, marked Latest on GitHub
- history/workshop-upload-v0.12.3 merged and rewritten as history/release-v0.12.3, covering both the Workshop and GitHub releases, and recording why the two channels' artifacts differ by 70 bytes
- The v0.12.3 section of the CHANGELOG no longer says "no GitHub tag yet" and now points to the release notes
- Status page: v0.12.3 became the two-channel baseline, v0.12.2 was demoted to the previous version, the original "unreleased after the tag" subsection was retitled "Contents of v0.12.3", and a line for the genuine post-tag changes was added

### Git Commits

| Hash | Message |
|------|---------|
| `b0217b0` | (see git log) |
| `10c9135` | (see git log) |

### Testing

- [OK] Release asset digest matches the local zip (549933 bytes, sha256 b9dc1a07…), asset uploaded, release marked Latest
- [OK] All eight offline gates green, check_release_metadata reports 0.12.3; CI succeeded for both #108 and #110
- [OK] DLL byte-for-byte comparison: only 70 of 1187840 bytes differ, in 7 contiguous ranges, PCK fully identical

### Status

[OK] **Completed**

### Next Steps

- The Workshop Simplified Chinese listing is still not updated; steam-workshop/description.zh-CN.txt needs to be pasted manually on the Workshop web page
- The follow-up PR by sachi4clover stacked on #106 (in-game Continue button + character-select checkbox) is not merged yet and will be the content of the next version
- If the DLL hashes of the two channels should match exactly, enable deterministic builds in STS2AIAgent.csproj and verify


## Session 22: Review and merge sachi4clover's co-op UI PR #111 (resume button + auto character-select switch)

**Date**: 2026-09-13
**Task**: Review and merge sachi4clover's co-op UI PR #111 (resume button + auto character-select switch)
**Branch**: `main`

### Summary

Reviewed #111: the two overlay entry points hook into the same runtime route and the same save protection; Sourcery's blocking finding was judged a false positive and answered point by point; on head 384 PASS / 0 FAIL, all eight gates green, and the destructive verification turned red all three times; squash-merged as 6a327d3, then PR #112 added the status page and the live checklist.

### Main Changes

- Reviewed PR #111 (author sachi4clover): the AI teammate page adds a "Disable auto character select" checkbox above the invite button (checked = CompanionAutoSelectCharacter false, saved on toggle) and a "Continue last co-op run" button below it (goes through AgentRuntime.ContinueDualInstanceAsync, available only on the host main menu with a co-op save)
- Showed that Sourcery's blocking finding is a false positive: CoopLaunchPolicy.GetError at AgentRuntime.cs:804-815 is the same check, the two NetId pre-checks have a backstop at DualInstanceCoordinator.cs:74-94, and the coordinator itself rechecks MAIN_MENU, so the overlay cannot bypass any of them; replied under the PR with the original text attached
- PR #112: the status page adds an "Unreleased mainline changes after the v0.12.3 tag" section (including a reminder that this change touched mod code and is not in the 0.12.3 artifact), and the [coop] section of docs/live-validation-checklist.md registers two items awaiting live clicks
- Verification details: added contract tests CoopRoute.OverlayEntries is registered, every <summary> sits right against its own method, all 5 new Chinese-language keys have English entries, busy applies only to the pressed button

### Git Commits

| Hash | Message |
|------|---------|
| `6a327d3` | (see git log) |
| `044d117` | (see git log) |

### Testing

- [OK] worktree b42bc5a: dotnet run --project STS2AIAgent.Tests = 384 PASS / 0 FAIL, scripts/preflight-release.ps1 all green (eight gates each OK)
- [OK] CI: contracts succeeded for both #111 and #112; Sourcery passed on #111
- [OK] Destructive verification three times (removing the checkbox write-back / switching Continue to a non-route overload / inverting the checkbox semantics) each turned the contract test red, after which the files were restored byte for byte

### Status

[OK] **Completed**

### Next Steps

- #111 changed mod code and is in neither the Workshop nor the GitHub 0.12.3: whether to ship v0.12.4 (or batch more) is undecided
- The Continue button and checkbox need a live click acceptance with a real co-op save: button timing, loading the save reconnects teammates, the checkbox value read by the child process
- The Workshop Simplified Chinese listing is still not updated (since v0.11.0); waiting to be pasted on the Workshop web page, steam-workshop/description.zh-CN.txt


## Session 23: v0.12.3 same-number re-release: bring the two co-op UI entry points of #111 into the published version

**Date**: 2026-09-14
**Task**: v0.12.3 same-number re-release: bring the two co-op UI entry points of #111 into the published version
**Branch**: `codex/record-recut-details`

### Summary

The version number stays 0.12.3 and only the build changes: the Workshop was re-uploaded on 09-14 00:09 (file_size 1229829, content id 6028841468497339213), and the GitHub v0.12.3 tag and Release were re-cut to 0f60ec4 (new asset 552014 bytes, SHA256 B7684C9F…). The five version numbers were not touched, the CHANGELOG Unreleased section was merged into the v0.12.3 section, and it states that the two builds can only be told apart by size/hash.

### Main Changes

- Release prep: CHANGELOG merges Unreleased into the v0.12.3 section (a banner at the top of the section records the 09-14 same-number re-release and how to tell the builds apart), workshop.json changeNote changed to this round's text; the five version numbers were deliberately left alone (check_release_metadata.py only checks that the five agree with each other, not against the tag or the previous version)
- Re-release execution: renamed the two old artifact directories to keep them (both packaging scripts use Get-UniquePath, so not clearing the old directories would produce a -2 suffix) -> package-steam-workshop.ps1 -> detached-process ModUploader upload (succeeded first try) -> Workshop GetPublishedFileDetails recheck (file_size exactly equals the local content byte sum 1229829) -> package-release.ps1 -> delete the old tag/Release and rebuild at 0f60ec4
- Payload cross-check: the two channels' DLLs are the same size (1190400), differing in only 72 bytes in 5 ranges (PE timestamp and MVID/debug directory), PCK and mod_id.json byte-for-byte identical
- Docs: #113 is the release commit and the record was merged by #114; the status page and history/release-v0.12.3 were both changed to a two-part "first (09-13) / second (09-14)" wording, and the original "unreleased changes after the tag" section was changed to the re-release contents

### Git Commits

| Hash | Message |
|------|---------|
| `6a327d3` | (see git log) |
| `0f60ec4` | (see git log) |
| `541734f` | (see git log) |

### Testing

- [OK] preflight-release.ps1 all green (eight gates each OK); all four Validate runs of #113 and #114 (push / pull_request) succeeded
- [OK] Workshop recheck: file_size 1229829 equals the local content byte sum, time_updated 2026-09-14 00:09:26, visibility=0, tags and English description unchanged
- [OK] GitHub recheck: asset 552014 bytes, reported digest sha256:b7684c9f… matches local, Latest points to v0.12.3; the CHANGELOG in the zip already contains the republished 2026-09-14 banner

### Status

[OK] **Completed**

### Next Steps

- The two UI entry points of #111 still lack a live click acceptance: button timing, loading the save reconnects teammates, the checkbox value read by the child process
- The Workshop Simplified Chinese listing is still not updated (since v0.11.0); waiting to be pasted on the Workshop web page, steam-workshop/description.zh-CN.txt
- The cost of a same-number re-release is written into the CHANGELOG and the status page: 0.12.3 refers to two builds, and bumping the number at the next release restores the one-to-one mapping


## Session 24: 2026-09-14 v0.12.3 third build: live-pass fix plus the ten-goal sweep

**Date**: 2026-09-14
**Task**: 2026-09-14 v0.12.3 third build: live-pass fix plus the ten-goal sweep
**Branch**: `main`

### Summary

Finished the ten goals, merged seven PRs (#116-#122), then republished v0.12.3 a third time to both channels and recorded it.

### Main Changes

- MCP: keep the error envelope on game-data failures, stop hiding a broken event stream behind wait_until_actionable, correct the skill AnyPlayer target rule (#117)
- Release tooling: destructive case for the packaged-links gate, preflight runs the shared metadata checker, artifact check compares the three version sources inside the artifact (#118, #119)
- Diagnostics: a thrown room probe no longer looks like an empty room, and twelve empty catches in GameActionService now log with their chain name (#120, #121)
- Await contract reads the syntax tree across four game-driving files instead of one line shape in one file (#122)
- Republished v0.12.3 a third time: CHANGELOG second-re-cut paragraph, Workshop changeNote, tag moved to c2630a8, asset 555061 bytes, Workshop file_size 1232901 (#123, #124)

### Git Commits

| Hash | Message |
|------|---------|
| `6a4b3a0` | (see git log) |
| `3d2eefb` | (see git log) |
| `74d03c9` | (see git log) |
| `052dac8` | (see git log) |
| `68bbd0a` | (see git log) |
| `c2630a8` | (see git log) |
| `0d42f94` | (see git log) |

### Testing

- [OK] dotnet run --project STS2AIAgent.Tests -c Release: 386 PASS / 0 FAIL; three destructive checks red before restore
- [OK] preflight-release.ps1 exit 0; eight verification gates green including their self-tests
- [OK] Steam Web API GetPublishedFileDetails: file_size 1232901 equals the local content byte sum; GitHub asset digest matches the local SHA256

### Status

[OK] **Completed**

### Next Steps

- Paste steam-workshop/description.zh-CN.txt in the Workshop web editor; ModUploader cannot set a language
- The Steam-networked save path still has no live evidence; the 2026-09-14 pass covered the local-connection path only
- GameStateService.cs keeps seven empty catches of its own kind; read them one by one before extending the no-wordless-catch rule


## Session 25: POSIX path parity: one resolver, 29 offline assertions, and the defects that exposed

**Date**: 2026-09-14
**Task**: POSIX path parity: one resolver, 29 offline assertions, and the defects that exposed
**Branch**: `main`

### Summary

Turned the macOS/Linux path handling from three unexercised copies into one resolver with an offline test that CI enforces, fixed five real defects on the way, and merged both PRs.

### Main Changes

This round turned macOS/Linux path resolution from "three copies, never run" into "one implementation + 29 offline assertions + a CI gate",
and fixed 5 real defects along the way (1 of which I introduced myself).

It started when the user asked "were those two default macOS paths brought in by an early compatibility PR?". The investigation found the picture more scattered than expected:

- `build-mod.sh:91-92` comes from PR #3 (a cherry-pick of the never-merged PR #2, by external contributor Vinluo),
  and that file has been touched by only one commit since it landed
- `lib-sts2.sh:34-35` comes from PR #9 (external contributor WILLOSCAR; the commit author on the branch is the placeholder test@example.com)
- the same path strings also appear a third time, in the embedded Python heredoc of the process matcher
- the path text has not changed by a single character since it was introduced, CI has only windows-latest, and there is no macOS live evidence anywhere in the repo

Key technical finding: the entire macOS detection chain can really run on Windows Git Bash (just point $HOME at a fixture).
This is the foothold for "verifiable": no Mac, no game, and no network needed.

Implementation:

- `scripts/lib-sts2-paths.sh` (new): the only place that knows the install layout. The resolution order matches the Windows side
  (argument -> environment variable -> detection -> conventional default); detection covers the three conventional Steam roots plus each install's own
  `libraryfolders.vdf` (so a game installed in a second library is also found). It does not start the game or open a socket.
- `scripts/test-lib-sts2-paths.sh` (new): 29 assertions. Precedence, vdf reading (escaped backslashes/trailing slash/
  empty file/garbage content), .app bundle layout, data and mods directories, process-match candidate paths.
- `sh-syntax` gate: every .sh goes through bash -n, and the test above is executed. Both CI and preflight run it.
  When picking bash, Git Bash is preferred: a bare `bash` on Windows is often the WSL launcher, which resolves fine on PATH
  but cannot read C:/ paths.
- `mcp_server/tests/test_posix_script_portability.py` (new): the mirror of the Windows-side guard. The install layout
  may appear only in the resolver; every script must source the shared library rather than "mention" it; four destructive cases.

Defects fixed:

- An empty/truncated libraryfolders.vdf went straight into a regex -> threw an exception, breaking all four scripts
- The pgrep fallback of the process matcher had no pattern when "the exe path is known" -> silently returned empty on machines without ps
  (this one I introduced myself during the refactor, and the review probe caught it)
- build-mod.sh inherited a nonexistent install path: everything downstream degraded to empty strings, and the error talked about the data directory
- The candidate list was not deduplicated, so the same library was listed twice via two roots
- sts2_infer_game_root_from_executable returned an empty string when the directory did not exist but still counted as success

Two independent probes reviewed the first version; besides the pgrep regression above, they found several places where "the comment claims A but the code does B"
(all fixed one by one) and two weak assertions (strengthened, with destructive cases added). Mutation verification really broke the source and then restored it, and all five mutations
were caught by the corresponding layer.

A trap during merging: a local experiment proved that squash-merging #127 would make #128 conflict on two doc files (the two PRs changed the same
passage), while rebase/merge had zero conflicts. So #127 was merged with rebase, and #128 was rebased locally onto the new main (git automatically
skipped the already-applied commits), then its base was changed and it was merged, keeping main a linear history.



### Git Commits

| Hash | Message |
|------|---------|
| `5d53d50` | (see git log) |
| `ca0a9cd` | (see git log) |

### Testing

- [OK] bash scripts/test-lib-sts2-paths.sh: all 29 checks passed (needs no game, Steam or network)
- [OK] check_verification_gates.py: nine gates pass, including the new sh-syntax which parses 19 scripts and runs that test
- [OK] MCP suite 198 tests OK; C# suite 386 PASS; preflight exits 0
- [OK] Mutation check against the real sources: five mutations, each turned the responsible guard red
- [OK] Merge rehearsal: squash of #127 conflicts with #128 in two files, rebase/merge is conflict-free

### Status

[OK] **Completed**

### Next Steps

- Real-machine macOS acceptance is still the only way to promote path resolution to proven script usability
- lib-sts2.sh still hardcodes python3 in six places; on macOS and Linux that name is the one that exists, so it was left alone
- CONTRIBUTING.md still describes feature -> dev -> main while dev is 196 commits behind main and the last three PRs went straight to main


## Session 26: Revive dev: fast-forward it back and write down the rule that keeps it from drifting

**Date**: 2026-09-14
**Task**: Revive dev: fast-forward it back and write down the rule that keeps it from drifting
**Branch**: `main`

### Summary

Fast-forwarded dev from 196 commits behind main, added the resync rule to CONTRIBUTING, and merged the change through the revived flow (PR #129 targeting dev).

### Main Changes

The user chose "bring dev back to life" rather than changing the docs, that is, making the process described in CONTRIBUTING true again.

Prior facts: `origin/dev` was 196 commits behind `origin/main` with 0 commits of its own, a strict ancestor, so a fast-forward
is lossless. CI (validate.yml) already covers pushes to `dev`, no PR targets dev, and dev has no branch protection.

What was done:

- First pushed the unpushed local journal commit to main (`4a01e22`)
- Fast-forwarded `dev` to main (`22907b0..4a01e22`), verified that the trees of origin/main and origin/dev are identical,
  and that the old tip `22907b0` is still an ancestor (nothing was lost)
- Added the rule "when to bring dev back": CONTRIBUTING.md now says that after anything lands on main one must
  `git push origin origin/main:dev`, together with the reason (every merge into main adds a commit that dev lacks,
  and a PR going straight into main adds all of its commits, which is the mechanism by which dev became a fork of old main)
- Two docs: CONTRIBUTING.md (version-controlled, goes through a PR) and AGENTS.md (locally gitignored, edited directly)
- This change was committed through the **restored process**: branch -> PR #129 -> base is `dev` -> merged. It is also the first PR into dev

Lesson: write a rule together with its reason, or the reason is the first thing to be dropped. The 196-commit drift happened because
nowhere in the repo did it say when dev should be brought back.



### Git Commits

| Hash | Message |
|------|---------|
| `4a01e22` | (see git log) |
| `a92f5a6` | (see git log) |

### Testing

- [OK] git merge-base --is-ancestor 22907b0 origin/dev: exit 0, so the old dev tip is still reachable and the fast-forward lost nothing
- [OK] origin/main and origin/dev trees are identical after the fast-forward
- [OK] check_verification_gates.py on dev: nine gates pass; offline resolver test 29/29
- [OK] PR #129 CI: contracts and Sourcery both green, merged into dev

### Status

[OK] **Completed**

### Next Steps

- Next release goes dev -> main; after that merge, fast-forward dev back (the new rule)
- dev has no branch protection while main has a PR requirement; worth deciding whether dev should require the contracts check too

### Correction (same day)

The first version of that rule was wrong, and **was refuted by reality within minutes of being written**. The first version said to fast-forward by ref directly:
`git push origin origin/main:dev`. That holds only while `dev` is a strict ancestor of `main`; once a feature PR
is merged into dev (which is the whole point of dev), dev has commits main lacks, and the push is rejected:

```text
! [rejected]        origin/main -> dev (non-fast-forward)
```

The fix (PR #130) states the invariant "dev contains all of main's commits" together with an operation that always works:
`git fetch origin; git checkout dev; git merge origin/main; git push origin dev`. It uses merge rather than
rebase, so published history is only ever appended to; and when dev happens to have no commits of its own, the merge degrades into a fast-forward by itself,
which is exactly the scenario the old wording described, now covered rather than assumed.

Lesson: write a rule together with its reason, and **run it once after writing it**. My two mistakes this round were of the same kind: conclusion first, verification later,
and the first mistake (squash would make stacked PRs conflict) was found by testing before acting, while the second (this rule) was found
when git rejected it on application. Both times it was "running it once" that saved me, not "thinking it over".


## Session 27: Close the drift that forced four re-cuts, ship v0.12.5, and collapse the action surface

**Date**: 2026-09-17
**Task**: Close the drift that forced four re-cuts, ship v0.12.5, and collapse the action surface
**Branch**: `dev`

### Summary

Worked back from a symptom: four same-version re-publishes in three days (v0.12.3 twice, v0.12.4 twice). The second v0.12.4 re-cut fixed a regression the first had introduced -- the combat gate read RunManager.ActionExecutor on the main menu, where there is no executor, so every /state answered 500 and the mod was unusable outside combat. 408 offline tests and nine gates passed that build. The question for this session was therefore not "what else is broken" but "what else is nobody watching".

Three answers, shipped as PR #141. (1) A source contract pins the in-combat guard around the queue read and keeps the gate the only place in the state builder that touches those members. (2) docs/api.md was missing a quarter of the state surface -- an audit found 91 fields across 23 payload records named nowhere a client could read, including seven whole sub-structures (session, multiplayer, multiplayer_lobby, character_select, timeline, modal, game_over) and the 43 key renames the compact agent_view performs, which is what MCP get_game_state returns by default. It also documented an `available` field on the three shop tables that none of those records has ever had. The api-facts gate now pins 16 payload records field-for-field, every GET /health key, the rename table against the builders, and a coarse net requiring every serialized field of every /state payload record to be named somewhere. (3) Nobody was counting the codebase: 82 files, 31,632 lines, two of them holding 49%. Line budgets now ratchet in both C# and Python, and a second check tightens a budget when its file shrinks.

CI caught a real defect in that work: gate messages quote Chinese headings from docs/api.md and the Windows runner's stdout is cp1252, so a gate that PASSED still exited 1 -- and a gate that failed would have had its message replaced by an encoding traceback. Fixed by writing UTF-8 with a replacing handler; self-test case 9i runs the suite under PYTHONIOENCODING=cp1252.

Then the live work. A baseline pass captured 2,346 back-to-back /state + /actions/available samples across 12 screens, which proved the two action surfaces agree at runtime and, separately, turned up two defects offline tests could not find. /state.screen never reported GAME_OVER after a death (the combat branch of ResolveNonModalScreen claimed it because the combat room stays active); eight samples in an unambiguous game-over state all reported COMBAT and the name never appeared once in 2,346. And a faulted game task said "the game task faulted" for twelve different actions, so a request the game refused read identically to a request that broke the mod -- live, a repeated `room Treasure` now names InvalidOperationException: Attempted to start new relic picking session while one was already occurring. Both fixed (#142, #143) and both re-verified on the patched build.

v0.12.5 shipped from f361bdb: GitHub and the Workshop, same source, one upload each, no re-cut. The Unreleased section added this session is where those post-tag fixes waited instead of forcing a fifth re-publish. Packaging emitted its first build-fingerprint.json, so the release record's hashes were copied from the build rather than collected by hand afterwards, and the Workshop's file_size 1238533 matched the local content bytes exactly.

Finally ADR 0001, implemented (#147). BuildAvailableActionNames and BuildAvailableActionsPayload were 301 and 609 lines consulting the same 50 Can* predicates to emit the same 55 names; they now both report one EnumerateAvailableActions walk, as a 12-line projection and a 14-line wrapper. GameStateService.cs 8,559 -> 8,295, budget lowered to match. Verified by replaying the baseline: 45 samples, all 12 screens, zero disagreements, twelve action sets matching exactly including PAUSE_MENU's empty set; the one difference traced to the run holding no potions, confirmed from run.potions. The contracts changed shape rather than being deleted -- ActionSurface.* now asserts neither surface has anywhere to put a predicate or name of its own, which is stronger than "two implementations agree".

The lesson worth keeping: both runtime defects were found by driving a real game, and the refactor was only safe because the baseline existed first. Doing it in the other order is exactly what produced the 0.12.4 regression.

### Git Commits

| Hash | Message |
|------|---------|
| `8e8835b` | (see git log) |
| `3825ead` | (see git log) |
| `abcd0db` | (see git log) |
| `d4af047` | (see git log) |
| `1b39c97` | (see git log) |
| `a4bff32` | (see git log) |
| `e644300` | (see git log) |
| `9bcaee6` | (see git log) |
| `dfd61bf` | (see git log) |
| `f1b2de7` | (see git log) |
| `0324285` | (see git log) |
| `ea732af` | (see git log) |

### Status

[OK] **Completed**


## Session 28: Wrap up the PR #148 verification gates CI

**Date**: 2026-09-17
**Task**: Wrap up the PR #148 verification gates CI
**Branch**: `refactor/split-monoliths`

### Summary

Took over the unfinished CI fix: made arch-facts and doc-links go by Git-tracked content, removed local links that cannot be resolved in a fresh checkout, added regression tests for untracked files and directories; the full local gates/self-test and GitHub contracts are all green.

### Git Commits

| Hash | Message |
|------|---------|
| `bbd1635` | (see git log) |

### Status

[OK] **Completed**


## Session 29: Release v0.13.0 and wrap up

**Date**: 2026-09-19
**Task**: Release v0.13.0 and wrap up
**Branch**: `main`

### Summary

Took over the interrupted v0.13.0 release: verified proxy connectivity, completed the final release DLL isolated live smoke test, created the GitHub Release, uploaded and checked the Workshop content through the Steam Web API, filled in the release records and build fingerprint, merged into main via PRs #162 and #163, and fast-forwarded dev to main.

### Git Commits

| Hash | Message |
|------|---------|
| `dfb5659` | (see git log) |

### Status

[OK] **Completed**


## Session 30: Companion health semantics, demand-driven SSE, predicate split, and the debug churn hook

**Date**: 2026-09-20
**Task**: Companion health semantics, demand-driven SSE, predicate split, and the debug churn hook
**Branch**: `main`

### Summary

Five tasks, all merged through PRs #166-#171. Companion identity now accepts ready and degraded while keeping pid/port/role/service checks (live-verified 14/14 against a real degraded payload produced by forcing one registry lookup to miss). Companion /health returns null for the host-only keys instead of the host state machine's idle text, same key shape. The event poller is demand-driven (0 subscribers built nothing over 12s in the game) with a generation fence so a poll in flight when Stop runs cannot commit, and it will not restart if a poll refuses to end inside the shutdown budget. Full subscriber queues now fail the write and drop that subscriber instead of silently evicting the oldest event; a live run produced 'Disconnected 2 slow event subscriber(s)'. Two bugs were found by the live runs and fixed: every poll re-announced stream_ready (~60 frames in 10s on a stationary screen), and a suppressed repeat still consumed an event_id, which would have left gaps the docs read as loss. GameStateService predicates moved to GameStateService.Predicates.cs: 61 members, 771 lines, base 765 lines smaller with no line added, 4 shared helpers left in the base file, and PredicateRelocationContractTests stores a SHA-256 per moved declaration from the parent commit. A new debug-gated inject_event_churn action makes the overflow path reachable in a live run, which is how the last offline-only item was closed. Verification: 487 C# tests, 226 Python tests, 11/11 gates, MCP tool profile clean, gate self-test, preflight complete.

### Git Commits

| Hash | Message |
|------|---------|
| `c017302` | (see git log) |
| `aac032a` | (see git log) |
| `62972c7` | (see git log) |
| `c3c531e` | (see git log) |
| `2a1bd2a` | (see git log) |

### Status

[OK] **Completed**

## Session 31: v0.14.0: offline plan items, live validation, overlay redesign, and the release

**Date**: 2026-09-21
**Task**: v0.14.0: offline plan items, live validation, overlay redesign, and the release
**Branch**: `main`

### Summary

Finished the plan's offline track, validated the candidate on a real game with a real model, redesigned the in-game overlay, and shipped v0.14.0 to GitHub and the Steam Workshop.

### Main Changes

- 4.1: docs/openapi.json is generated by a stdlib deep module (scripts/api_schema.py) from Router routes and C# wire records; a twelfth gate rejects stale bytes.
- 1.6: an offline decision-quality benchmark (11 snapshot-evidenced cases, deterministic scoring) plus a read-only /state capture seam; no model or network.
- 4.2: sts2_mcp/payloads.py types the two payloads with a fixed field set; the old dict getters are unchanged.
- 5.3: max_tokens vs max_completion_tokens resolved from the server's own error, retried once.
- 6.1 + 6.4: GameStateService.cs 5037 -> 1336 lines, split by screen into 8 partials, proven byte-identical by 218 declaration hashes; ratchet lowered 5150 -> 1400.
- Overlay redesign: spacing and type scales, section cards, primary/secondary buttons, an active-tab pill, and four colour themes selectable in Settings.
- Fixed two live-found launcher defects: the isolated profile enabled only the first STS2AIAgent entry (game then read the mod as disabled), and a non-numeric --clientId silently validated a different profile. Both platforms now agree.
- Released v0.14.0: tag 5f9ea72 via PR #173 -> #174, GitHub asset 637550 bytes, Steam Workshop item 3796486050 updated (file_size 1360359 equals the local byte sum).
- Recorded the Workshop upload root cause: the Windows system proxy stalls SteamPipe content transfer while Steam still logs on; clearing it finished the upload in 8 seconds.

### Git Commits

| Hash | Message |
|------|---------|
| `a3edf1b` | (see git log) |
| `d39ef61` | test(ui): whitelist the three overlay files split out this session |

### Testing

- [OK] preflight-release.ps1 exit 0 - 12 static gates ok
- [OK] 572 core unit and contract tests pass (5 new OverlayLayout.* contracts added)
- [OK] SourceCoverage.UncompiledWhitelist caught the three split files only after they were committed, since it checks tracked sources
- [OK] Live game: mod loaded as 0.14.0, 27/27 reflected members, state-invariants failure_count 0, screenshots confirmed each visual fix
| `4cab34f` | (see git log) |
| `58a08ea` | (see git log) |
| `b32d56d` | (see git log) |
| `4151a37` | (see git log) |
| `e7520f8` | (see git log) |
| `c3118f1` | (see git log) |
| `9d3bc3b` | (see git log) |
| `9b92a8d` | (see git log) |
| `bd45774` | (see git log) |
| `2b7610a` | (see git log) |
| `51828ad` | (see git log) |
| `8bc7d2d` | (see git log) |
| `46d0da9` | (see git log) |

### Testing

- [OK] C# core suite exit 0; Python MCP 302 tests OK; all 12 verification gates pass; preflight-release.ps1 exit 0 with 0 build warnings.
- [OK] Live on game v0.111.0 in an isolated profile: 27/27 reflected members, mod-load --deep-check, state-invariants 0/0, patch-check replay with no mismatches.
- [OK] Real model (CommandCode / deepseek-v4.1-flash): connection test passed for both roles, unattended autoplay reached floor 3 with 45 reasoned decisions, and a local dual-instance co-op run played the teammate with the model.

### Status

[OK] **Completed**

### Next Steps

- Open a PR from feat/overlay-visual-redesign into dev; origin/dev has diverged so the documented fast-forward catch-up needs a human decision.
- Enable the mod in the game settings was needed to test at all: the Steam profile had both STS2AIAgent entries disabled, and start-game-session.ps1's --clientId seeding is ignored when the game launches through Steam.
- Steam two-instance co-op, a complete natural run, and the Vision path still need the game in front of a person.
- Provider sampling for SiliconFlow / OpenRouter / vLLM needs a key and a budget cap; only the CommandCode endpoint was sampled.
- Paste steam-workshop/description.zh-CN.txt into the item's Simplified Chinese listing; Steam exposes no API for non-default descriptions.


## Session 32: Overlay visual redesign and live-test defect fixes

**Date**: 2026-09-21
**Task**: Overlay visual redesign and live-test defect fixes
**Branch**: `feat/overlay-visual-redesign`

### Summary

Redesigned the in-game overlay (themes, layout, controls), split AgentOverlayHost into pages and settings partials, and fixed six defects that only a running game could show.

### Main Changes

- Seven colour themes instead of four, including the first light one (Ivory), with a swatch-grid picker that previews each palette.
- Layout pass: play tab became a dashboard, settings gained a jump bar and a fixed footer, co-op tab inverted so actions precede status, chat footer compacted.
- Split AgentOverlayHost.cs into Pages/Settings partials and extracted OverlaySwatch.cs, lowering the base file's size budget from 1,420 to 1,150.
- Fixed: swatch preview never drew (custom _Draw never called); theme switch skipped PanelContainers; long labels widened the panel to 691px and clipped every card; theme selection was overwritten by the rebuild a theme change triggers; jump buttons landed sections at the viewport bottom; ivory's drop shadow read as a smudge.

### Git Commits

| Hash | Message |
|------|---------|
| `a3edf1b` | (see git log) |
| `736f173` | test(ui): whitelist the three overlay files split out this session |

### Testing

- [OK] preflight-release.ps1 exit 0 - 12 static gates ok
- [OK] 572 core unit and contract tests pass (5 new OverlayLayout.* contracts added)
- [OK] SourceCoverage.UncompiledWhitelist caught the three split files only after they were committed, since it checks tracked sources
- [OK] Live game: mod loaded as 0.14.0, 27/27 reflected members, state-invariants failure_count 0, screenshots confirmed each visual fix

### Status

[OK] **Completed**

### Next Steps

- Open a PR from feat/overlay-visual-redesign into dev. The dev-resync rule from session 26 only applies while main is a strict ancestor of dev, and it is not right now: origin/dev carries the session-31 journal that main does not, so a fast-forward would drop it. Merge instead of reset, and the rule is lossless again once these commits reach main.
- The mod was disabled in the Steam profile, so testing needed it enabled by hand; start-game-session.ps1's --clientId seeding is ignored when the game launches through Steam, which is why the isolated-profile path did not reach it.


## Session 32: Dev audit: 16 fixes and offline verification

**Date**: 2026-09-21
**Task**: Dev audit: 16 fixes and offline verification
**Branch**: `dev`

### Summary

Repaired 16 confirmed dev defects; full offline preflight passed. No live-game validation or remote publication.

### Main Changes

- Fixed settings, teammate HTTP state, MCP diff, event lifecycles and cancellation; recorded findings and prevention rules.

### Git Commits

| Hash | Message |
|------|---------|
| `a4463fe` | (see git log) |

### Testing

- [OK] 589 C# tests; 315 Python tests; 12 verification gates; all 16 preflight steps; 165 cross-language cases matched.

### Status

[OK] **Completed**

### Next Steps

- Live-game validation remains deferred by the user.


## Session 33: Fix all five agent turn integrity findings

**Date**: 2026-09-21
**Task**: Fix all five agent turn integrity findings
**Branch**: `dev`

### Summary

Preserve accepted actions and partial receipts; enforce pending token and proactive budgets; validate action argument objects.

### Main Changes

- Added 26 regression and runtime-wiring tests; kept the shared turn gate through receipt accounting.

### Git Commits

| Hash | Message |
|------|---------|
| `763eabddb10fdb52ff941df6715014bc468a4f8e` | (see git log) |

### Testing

- [OK] C# 615 PASS/0 FAIL; Python 315 OK; full offline preflight exit 0; build 0 warnings/errors.

### Status

[OK] **Completed**

### Next Steps

- Live-game validation remains deferred; no push or release performed.


## Session 34: Optimize the AI-play harness: cheaper prompts, smaller state, recoverable tools

**Date**: 2026-09-21
**Task**: Optimize the AI-play harness: cheaper prompts, smaller state, recoverable tools
**Branch**: `dev`

### Summary

Measure and cut what an external AI pays to play: the per-step prompt, the per-action state payload, and the bytes of the API response - then make tool failures recoverable instead of terminal.

### Main Changes

- In-game prompt injects only the current screen's playbook and puts state plus the act-once instruction last: static block 24,105 to 14,577 chars per COMBAT step (-2,722 tokens, -39.5%), and repeated same-screen steps now share a cacheable prefix.
- act returns the compact agent_view by default instead of echoing the 4,000-9,500-token raw state; raw_state=true is the opt-in and HTTP POST /action is unchanged.
- Dropped JSON indentation: WriteIndented=false removes 35.6% of response bytes (44% on nested combat payloads).
- compact agent_view v11: removed duplicated actions/profiles and pile *_cards arrays; added relic_stacks/relic_descriptions, map nodes and boss coordinates, real card costs, rest option_id, event text_key, shop on_sale.
- Lethal-risk rows now include poison_next_turn and constrict_turn_end, so end_turn_will_kill_player can no longer call a poisoned death safe.
- Added the decide tool (state + available_actions + scene guidance in one call) and structured index errors carrying field/submitted/valid_indices; native MCP failures keep code/details/retryable.
- New offline knowledge: relics.md (289 rows), powers.md (257 rows), 121/121 monster HP, 358 monster moves of which 324 carry resolved amounts.

### Git Commits

| Hash | Message |
|------|---------|
| `46b2cdd` | (see git log) |
| `5fcb7df` | (see git log) |
| `78b1546` | (see git log) |
| `d5f780b` | (see git log) |
| `cbb550b` | (see git log) |
| `13352db` | (see git log) |
| `41e34e1` | (see git log) |
| `712d77c` | (see git log) |

### Testing

- [OK] C# 635 PASS/0 FAIL; Python 346 OK; all 12 verification gates; preflight-release.ps1 exit 0; a clean worktree of the final commit builds and passes 635 tests; GitHub Validate CI green on dev (run 35614200146).

### Status

[OK] **Completed**

### Next Steps

- Live-game validation (test-mod-load -DeepCheck, state-invariants) still requires launching the game.
- Version bump, Workshop upload and the zh-CN listing paste remain manual release steps.


## Session 35: Deepen harness reliability and decision seams

**Date**: 2026-09-22
**Task**: Deepen harness reliability and decision seams
**Branch**: `dev`

### Summary

Explored STS2-Agent beyond token usage, then shipped one-frame decision snapshots, non-blocking action serialization, compact wait/reconciliation results, canonical event knowledge joins, screen-relevant game-data lookup, crystal-sphere reveal gating, stronger patch-check semantics, and broader CI coverage. Full offline suites, release preflight, verification gates, and GitHub Validate passed; pushed dev.

### Git Commits

| Hash | Message |
|------|---------|
| `971f2f3` | (see git log) |

### Status

[OK] **Completed**


## Session 36: Close live validation gaps

**Date**: 2026-09-22
**Task**: Close live validation gaps
**Branch**: `dev`

### Summary

Isolated autoplay cleared Act 1 and ended naturally in Act 2. Thinking-model failures, StepFun stream tool loss, context compaction, overlay clipping, MCP control, and live screenshot capture were fixed and recorded. Full three-act wins, external full runs, and Steam-client dual launch were closed out as out of scope.

### Git Commits

| Hash | Message |
|------|---------|
| `d6b5d80` | (see git log) |
| `69e5f3b` | (see git log) |
| `7520faf` | (see git log) |
| `c6bba38` | (see git log) |
| `7c5917c` | (see git log) |
| `0a55bee` | (see git log) |
| `68df33b` | (see git log) |
| `993c6c3` | (see git log) |
| `5a48ba0` | (see git log) |
| `a1a006d` | (see git log) |
| `09a65cc` | (see git log) |
| `5da06d1` | (see git log) |
| `bce6e06` | (see git log) |
| `2b6fb17` | (see git log) |
| `5389971` | (see git log) |

### Status

[OK] **Completed**


## Session 37: Integrate review fixes and wrap up

**Date**: 2026-09-23
**Task**: Integrate review fixes and wrap up
**Branch**: `dev`

### Summary

Integrated the four review-fix tracks, added mode mutual exclusion, bounded Jev retries and companion settings, per-run isolated sessions, and the dual MCP briefing; fixed the boundary issues pointed out by the second two-axis re-review. Verified the Mod Release build, C# offline tests, 420 Python tests, 12 static gates, and 7 OpenAPI checks; the real two-instance run and the Jev service still await live acceptance.

### Main Changes

- Committed this round's fixes and the code-spec, archived 09-23-audit-remediation; the original WIP baseline backup was not committed.

### Git Commits

| Hash | Message |
|------|---------|
| `628ba75` | (see git log) |
| `455641f` | (see git log) |

### Testing

- [OK] dotnet build Release / C# offline tests / Python 420 tests / verification gates 12 items / API schema 7 items passed.

### Status

[OK] **Completed**

### Next Steps

- Verify the two-instance run, real Jev 429, run switching, and in-run commands with an isolated save and a controlled quota; separately handle the uncommitted 09-22 UI task docs.


## Session 38: Archive outdated docs and close out the status page

**Date**: 2026-09-23
**Task**: Archive outdated docs and close out the status page
**Branch**: `dev`

### Summary

Moved 7 dated old review/verification records verbatim into history/ and registered them in the index, left historical snapshot redirect pages at the original docs/ paths; updated PRODUCT_PLAN_CURRENT.md to the v0.15.0 local review-fix status and made explicit that live acceptance is not done; refreshed the source stats in architecture.md to 164 files / 45,050 lines. All 12 offline gates and git diff --check passed.

### Main Changes

- docs archive: 5 snapshots from 2026-03/08 + 2 review records from 2026-09-21 moved into history/, with redirect pages left at the original paths.
- The current status page now distinguishes this round's v0.15.0 fixes from the old version records; fixed the link from the 09-22 tasks that pointed to an archived task.

### Git Commits

| Hash | Message |
|------|---------|
| `a58f243` | (see git log) |

### Testing

- [OK] python scripts/check_verification_gates.py all 12 items passed; git diff --check passed.

### Status

[OK] **Completed**

### Next Steps

- The four 09-22 tasks are still in planning/progress, and their draft doc changes are kept uncommitted; live acceptance of the real two-instance run and the Jev service is pending.


## Session 39: v0.16.0: live reasoning, dual-layer goal, overlay fit, clean screenshots; released to GitHub and Workshop

**Date**: 2026-09-24
**Task**: v0.16.0: live reasoning, dual-layer goal, overlay fit, clean screenshots; released to GitHub and Workshop
**Branch**: `dev`

### Summary

Fixed user-reported issues after an in-overlay model test run: streamed reasoning now shows live in the overlay (gated by the show-reasoning switch, never persisted); the overlay's right-edge clipping was traced live to the chat options row (457px in a 440px panel) and fixed, plus wrapped warnings and a full Token reading; /vision/screenshot waits for a drawn frame (FramePostDraw) so it no longer contains the overlay. Audited the run's decision logs: Jev really executed (22 jev rows); repeated end_turn were distinct turns; found planner hints keyed by action kind never matched Jev's concrete option ids -> AlignHints, a bounded macro goal, plan provenance read from the truncated planner summary, and one atomic PlayStrategyUpdate merge shared by POST /strategy and both MCP surfaces (which also fixed POST /strategy resetting omitted fields). Three read-only reviews; confirmed findings fixed test-first. Updated the player skill (planner role, play_running conflict, 17 guided tools) and the user-level sts2-agent-dev skill. Live-checked in an isolated keyless game; released v0.16.0 via PR #199, GitHub Release and Workshop (proxy-off upload, Steam relogin lesson recorded). Not live-validated: streaming reasoning, dual-layer play quality, AI teammate.

### Git Commits

| Hash | Message |
|------|---------|
| `746bfc3` | (see git log) |
| `6a10414` | (see git log) |
| `eaa0edc` | (see git log) |
| `0bd5a76` | (see git log) |

### Status

[OK] **Completed**
