# v0.16.0 Release Record (2026-09-24)

> Historical snapshot: this page records the process and evidence of this one release at the time and does not represent later states. For the current state see
> [PRODUCT_PLAN_CURRENT.md](../PRODUCT_PLAN_CURRENT.md); for build identification see [build-fingerprints.md](build-fingerprints.md).

| Item | Value |
| --- | --- |
| Version | 0.16.0 (all five version files agree, `check_release_metadata.py` passes) |
| Release PR | #199 (dev -> main), CI `contracts` passed |
| tag / source commit | `v0.16.0` -> `4beaabeb836f50b001759c9b579b426d174360cc` (the merge commit of PR #199), working tree clean at packaging time |
| GitHub Release | [v0.16.0](https://github.com/CharTyr/STS2-Agent/releases/tag/v0.16.0), asset `sts2-ai-agent-v0.16.0-windows.zip` 777852 bytes, SHA256 `43A026B56DD77D9B51969E8720D96CEEC0374CADFE5C25744B70E8EAD172E75F` |
| Steam Workshop | Item 3796486050, public (`visibility` 0), `file_size` 1645031 = local content byte sum, `time_updated` 2026-09-24 02:32:06, content id `1458911249290867048` |
| DLL SHA256 | `7935096C0E95D8B0195EA45BB3720FFE867C4AAB0CDB47B72AF3CFC8EF1F70FB` (the GitHub package and the Workshop package are identical) |

## Contents

See the v0.16.0 section of [CHANGELOG.md](../CHANGELOG.md): live display of thinking content, the macro goal and hint alignment in dual-layer mode,
an external planner that can set `goal` and partial updates that no longer reset the strategy, the root-cause fix for the overlay's clipped right edge, screenshots that no longer include the overlay,
and the 2026-09-23 severe-usability fixes merged into main with this release.

## Validation

- Offline: C# 860, Python 423, all validation gates, and `preflight-release.ps1` passed; the new source contracts were also rerun on a CRLF checkout, with the same 860 passing.
- Three independent read-only reviews (streaming thinking / dual-layer strategy / screenshots and layout); every confirmed issue got a failing test first and was then fixed.
- Live (isolated profile, no model calls throughout): screenshots do not include the overlay; `[STS2AIAgent.Wide]` is 0 on both tabs, including after deleting the first model; the 185 files of the official Steam profile are identical before and after. See the [2026-09-23 validation report](../docs/live-validation-2026-09-23-results.md).
- **Not re-verified live**: streaming thinking display, actual dual-layer play quality (needs real model / Jev calls), the AI teammate.

## Workshop upload process

Following the known practice in [steam-workshop/README.md](../steam-workshop/README.md): before uploading, write the original system proxy value to a file,
temporarily turn off `ProxyEnable`, restart Steam and wait for login, then run `ModUploader.exe upload -w <workspace> -i 3796486050`.

1. 02:30:54 system proxy turned off; 02:31:58 Steam restarted and logged in.
2. 02:31:58 first upload started; at 02:32:06 `workshop_log.txt` recorded `Upload finished for workshop item 3796486050 : OK` (success line count went from 26 to 27). About 8 seconds, consistent with the v0.14.0 record.
3. 02:32:08 system proxy restored to its original value (`ProxyEnable=1`, `127.0.0.1:10808`), then Steam was restarted to return to the user's original proxy environment.
4. Steam Web API (through the proxy) re-check: `file_size` 1645031 equals the local `content/` byte sum, `visibility` 0.

**Steam reconnects slowly after the proxy is restored**: within 4 minutes of the restart the connection log had no `Logged On`, and the CM WebSocket connection failed repeatedly while the proxy was on
(`StartAutoReconnect() ... attempt 6`), although the connectivity test through the proxy was normal. This is the same behavior recorded for v0.14.0,
Steam's own reconnect behavior under this machine's proxy environment, unrelated to the upload. Compared with the connection log: the login before the upload (00:24:33) went over **UDP**
(Steam picks UDP with about 11% probability), while after this restart it kept choosing WebSocket and failing. Handled the same way as v0.14.0: at 02:42 Steam was restarted again with the proxy
off, and after `Logged On` at 02:43:24 the system proxy was **immediately** restored to its original value with no further Steam restart; Steam stayed online afterwards,
and the system proxy `ProxyEnable=1`, `ProxyServer`, and `ProxyOverride` match their pre-upload values field by field.

**Advice for the next upload**: just restore the proxy after uploading and **do not** restart Steam again with the proxy on, since on this machine that step is very likely to leave Steam
stuck reconnecting its WebSocket.

## Incomplete / manual

- The Workshop **Simplified Chinese listing** still needs `steam-workshop/description.zh-CN.txt` pasted in manually on the web page (`ModUploader` has no language parameter).
- Subscription load acceptance (actually loading 0.16.0 from the Workshop directory) was not done: the user has to restart the game with "Play with Mods" to confirm.
