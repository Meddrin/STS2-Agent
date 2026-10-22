# v0.13.0 Release Record (2026-09-19)

> Historical snapshot: this file records the GitHub Release, the Steam Workshop upload, and the final release-binary smoke run of v0.13.0;
> for the current state see [PRODUCT_PLAN_CURRENT.md](../PRODUCT_PLAN_CURRENT.md).

Release commit: `78b085f` (the `dev -> main` merge of PR #161). The tag `v0.13.0` points at it;
the candidate commit `3ee12e7` has exactly the same source tree as the release commit.

## GitHub Release

| Field | Value |
| --- | --- |
| tag | `v0.13.0` → `78b085f0300b4abe189be234c17ed0a3b0518309` |
| Asset | `sts2-ai-agent-v0.13.0-windows.zip` |
| Bytes | 576033 |
| SHA256 | `8CFC0F451EE04D30C4F1708A5BF8553B5860FD1B61F99F249FEAAF87BAB18733` |
| Release | <https://github.com/CharTyr/STS2-Agent/releases/tag/v0.13.0> |

## Steam Workshop (Item 3796486050)

During the upload, SteamPipe timed out once while uploading the manifest and once while downloading the old manifest. After confirming the local proxy
`http://127.0.0.1:10808` was usable, restarting Steam, and retrying, the Steam log recorded on 2026-09-19 at 11:32:26
`Upload finished for workshop item 3796486050 : OK`.

Steam Web API re-check through the same proxy:

| Field | Value | Comparison with local |
| --- | --- | --- |
| `result` | 1 | Item exists |
| `visibility` | 0 (public) | Not reverted to private by the update |
| `file_size` | 1252357 | Exactly equal to the byte sum of `content/` |
| `time_updated` | 2026-09-19 11:32:25 +08:00 | Refreshed immediately after the upload |
| `hcontent_file` | `3840768014407368680` | This upload's content manifest |
| `consumer_app_id` | 2868840 | Correct |
| `tags` | Tools & APIs / Utility / QoL | All three in place |

Item metrics that day: 107 subscribers, 239 total subscriptions, 782 views, 15 favorites.

## Build Fingerprint

Source commit `78b085f0300b4abe189be234c17ed0a3b0518309`, working tree clean.

Workshop content (5 files, 1252357 bytes in total):

| File | Bytes | SHA256 |
| --- | ---: | --- |
| `STS2AIAgent.dll` | 1212928 | `6B9D90C8C13998B2A5EEAA22D7B9D7543A466C5C6688214F73393A489A7FBE42` |
| `STS2AIAgent.pck` | 608 | `8BDDFCE8091E3112CF94F25E4B916E136718D64F79FF064CEB3A093540FDF7C0` |
| `STS2AIAgent.json` | 382 | `097919D60A3EF455897C4FA115A3694A294341A07BFE490F9CA1DB45A3DBE086` |
| `README.md` | 3802 | `6A18A53359E678A43F9A436D6F7C6F6B0AFE0C529C3AB40A27958D8E5DC3F0D6` |
| `LICENSE` | 34637 | `55515BACC3BD3796AF8B3C2864D2BBE1F7B922FB41B827C5C67138F18BB5F008` |

GitHub artifact directory fingerprint: 22 files, 1730338 bytes, with the same source commit and clean state.

## Acceptance Evidence

The release candidate `3ee12e7` (same tree as the main release) completed full acceptance: C# **453 PASS / 0 FAIL**,
`mcp_server` **224 OK**, **all eleven gates green**, `preflight-release.ps1` exit 0.
The isolated live run went through solo embark, combat, rewards, save and continue, and `GAME_OVER` after death; the co-op invite succeeded,
and both players finished card plays and ending their turns.

After repackaging, the DLL is `6B9D…FBE42`, byte-different from the `7A54…` that the candidate acceptance deployed. The source tree is the same, and the difference comes from the
.NET build output rather than a source change; to avoid letting "the tree is the same" stand in for "the release bytes have been run", before release the `6B9D…FBE42` DLL shared by
GitHub and the Workshop was also deployed to an isolated instance for a short smoke run:

- `/health` reports `0.13.0`, `status: ready`, compatibility 27 checks, 0 missing;
- a new run entered combat, played a card successfully (enemy 46 -> 40), ended the turn, and the next turn was still operable;
- `/data/monsters` returned 107 entries, and a spot check of `AEONGLASS` showed 3 moves;
- after `save_and_quit`, `continue_run` restored the same run id `QCYDW4JH5U20` and re-entered an operable combat;
- the request log has no 500, no agent ERROR, no reflection member errors; the per-file hashes of the player's real save are unchanged.

## Known Boundaries

- When the isolated host uses `clientId != 1`, `continue_ai_teammate` returns 409 before loading the save; the save is unchanged on refusal.
  This path is identical to v0.12.5 after whitespace removal and was already recorded on 2026-09-13, so it is not a v0.13.0 regression.
- In this inevitably refused state, `continue_ai_teammate` still appears in the available action list, which is an action-surface issue still to be fixed.
- The success path of properly reconnecting the teammate needs a launch with `clientId 1`, which touches the real save directory, and was not run this round.
- The Workshop Simplified Chinese listing still has to be updated by hand on the web page; `ModUploader` has no language parameter.
