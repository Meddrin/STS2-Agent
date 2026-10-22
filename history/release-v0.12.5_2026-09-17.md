# v0.12.5 Release Record (2026-09-17)

> Historical snapshot: this file records the GitHub Release and Steam Workshop upload of v0.12.5; for the current state see
> [PRODUCT_PLAN_CURRENT.md](../PRODUCT_PLAN_CURRENT.md).

Release commit: `f361bdb` (the `dev -> main` merge of PR #145). The tag `v0.12.5` points at it.
**One release, no re-cut**: the cause of the previous four same-number re-releases (v0.12.3 x2, v0.12.4 x2) was that fixes arriving after the tag
had nowhere to be recorded; the two fixes this round were also found after the tag, but waited in the `## Unreleased` section of `CHANGELOG.md`
until this release finalized them.

## Contents of This Version

Two runtime fixes that players / agents can perceive, **both found by driving the real game, not by reading code**,
and both re-verified live on the patched build.

1. **`/state.screen` reports `GAME_OVER` again after death** (PR #142). After death the combat room is still active, and
   the combat branch of `ResolveNonModalScreen` hit before `NGameOverScreen`, so the whole settlement phase reported `COMBAT`.
   In the live baseline, **all** 8 samples where "the only available action is `continue_game_over`" reported `COMBAT`,
   and `GAME_OVER` never appeared once in 2346 samples.
2. **Failed game actions state the reason the game gave** (PR #143). 12 paths shared one uninformative
   "the game task faulted", so "the game legitimately refused" could not be told apart from "the mod is broken".

The rest is contracts and tooling (PR #141): `docs/api.md` describes `/state` completely for the first time, the `api-facts` gate was expanded,
packaging produces a build fingerprint, and a size ratchet was added.

## GitHub Release

| Field | Value |
| --- | --- |
| tag | `v0.12.5` -> `f361bdb` (dev->main merge of PR #145) |
| Asset | `sts2-ai-agent-v0.12.5-windows.zip` |
| Bytes | 562212 |
| SHA256 | `0E4A15184A679D95C5064247B2DB0FEFA25E3B68D743D9110A4F59DE0318CFED` |
| Release notes | `build/release/notes-v0.12.5.md` (gitignored) |

## Steam Workshop (Item 3796486050)

The upload passed on the first try without getting stuck: this is the first of three times (the previous two got stuck at `Upload starting` and
`PreparingContent`, and both were resolved by restarting the Steam client). Upload command:
`ModUploader.exe upload -w build/steam-workshop/sts2-ai-agent-v0.12.5 -i 3796486050`,
with `HTTP_PROXY` / `HTTPS_PROXY=http://127.0.0.1:10808` in the process environment (a `curl -x` self-check before uploading returned 200).

Steam Web API re-check (`GetPublishedFileDetails`):

| Field | Value | Comparison with local |
| --- | --- | --- |
| `result` | 1 | Item exists |
| `visibility` | 0 (public) | Not reverted to private by the update |
| `file_size` | 1238533 | **Exactly equal** to the byte sum of `content/` |
| `time_updated` | 2026-09-17 09:36:37 | Refreshed immediately after the upload |
| `hcontent_file` | `2962740913650521121` | This upload's content id |
| `consumer_app_id` | 2868840 | Correct |
| `tags` | Tools & APIs / Utility / QoL | All three in place |

Item metrics that day (same API call): 90 subscribers, 194 total subscriptions, 657 views, 14 favorites.

## Build Fingerprint (First Produced Automatically by the Packaging Script)

In the past these numbers were captured by hand with `Get-FileHash` after the upload; since PR #141,
`scripts/lib-build-fingerprint.ps1` writes them into `build-fingerprint.json` next to the artifact at packaging time,
and the `file_size` reconciliation compares against it directly. `source_tree_dirty: false` also proves that the artifact really came from that commit
and was not built from a dirty working tree.

Source commit `f361bdb6b2031a434b4f26d04ed764ec6332b27b`, working tree clean.

Workshop content (5 files, 1238533 bytes in total):

| File | Bytes | SHA256 |
| --- | ---: | --- |
| `STS2AIAgent.dll` | 1199104 | `1624BBF5CBAB50D415FAFCC59C354A6101616899F2FC00E9C8A76CAFF84CD4A7` |
| `STS2AIAgent.pck` | 608 | `CD6403306ACA1A3AF5FDC918C0FDD9F9EF78339FCAEFE5BE4032CECC8440BC76` |
| `STS2AIAgent.json` | 382 | `CF8F434BA424EFC5A8B18C31F9EE2C097F5B1A98667FF1A45FA91584181E343C` |
| `README.md` | 3802 | `6A18A53359E678A43F9A436D6F7C6F6B0AFE0C529C3AB40A27958D8E5DC3F0D6` |
| `LICENSE` | 34637 | `55515BACC3BD3796AF8B3C2864D2BBE1F7B922FB41B827C5C67138F18BB5F008` |

The fingerprint of the GitHub artifact directory is saved separately at `build/release/sts2-ai-agent-v0.12.5-windows-fingerprint.json`
(20 files, 1695285 bytes, gitignored).

## Acceptance Evidence

Live (isolated offline host, `--windowed --force-steam off --clientId 2026091701`, API 18080, single instance):

- **Action-surface baseline**: 2346 back-to-back sample pairs of `/state` and `/actions/available`, 12 screens, 22 action-set variants.
  Zero disagreements in 61 samples on non-combat screens; all 23 disagreements on combat screens are timing artifacts (involving only the gated
  `end_turn` / `play_card` / `use_potion`, with the direction following "which endpoint was read later" rather than "which surface").
- **`GAME_OVER` fix re-verification**: two independent deaths, and all 10 non-empty `game_over` samples report `GAME_OVER`,
  with zero `COMBAT`; the full `continue_game_over` -> `return_to_main_menu` chain still works,
  and `save_verified` reached `true`. The 12 baseline screens were resampled with 25 per-screen comparisons, and **no screen changed its name**.
- **Diagnostics fix re-verification**: a repeated `room Treasure` now returns
  `Console command failed: the game task faulted: InvalidOperationException: Attempted to start
  new relic picking session while one was already occurring.`: the exception is harmless and self-explanatory.
  The success path is unaffected (a successful console command still returns `completed`).

The player's real Steam save was hashed once before and once after the two live rounds: 184 files, identical digest, empty per-file diff.

Offline (on the release commit): C# **415 PASS / 0 FAIL**, `mcp_server` **224 OK**,
`check_verification_gates.py` **all nine gates green**, gate self-test **29 cases**,
`check_release_metadata.py` all five version numbers agree, `preflight-release.ps1` exit 0, mod build 0 warnings.

## Not Done

- **The Workshop Simplified Chinese listing is still the old version** (missing several list items since v0.11.0). `ModUploader` has no language parameter,
  so `steam-workshop/description.zh-CN.txt` can only be pasted by hand on the Workshop web page (the copy in the repository is up to date).
- No subscription-side load smoke check (the 2026-09-09 conclusion stands: after an upload, restarting Steam shows the new content).
