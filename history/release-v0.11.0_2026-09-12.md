# v0.11.0 Release and Workshop Upload Record (2026-09-12)

> Historical snapshot: this file is the release and Workshop upload record for v0.11.0 and does not represent the current state; see [PRODUCT_PLAN_CURRENT.md](../PRODUCT_PLAN_CURRENT.md) for the current status.

## Conclusion

The GitHub Release and the Steam Workshop upload are **both done**.

| Item | Value |
| --- | --- |
| Release commit | `84631b9` (feat(i18n)), pushed to `main` |
| CI | Validate run `34629717120` → success |
| Tag | `v0.11.0` → `84631b9` |
| Release | https://github.com/CharTyr/STS2-Agent/releases/tag/v0.11.0 , asset `sts2-ai-agent-v0.11.0-windows.zip` (677015 bytes) |
| Workshop item | `3796486050`, `time_updated = 2026-09-12 02:33:39` |
| Workshop manifest | `3781676487912021003` |
| Workshop `file_size` | `1135844`: **exactly equal** to the v0.11.0 content byte sum (dll 1096704 + pck 608 + json 381 + README 3514 + LICENSE 34637) |
| Visibility | `0` (public), not reverted to private |
| Description text | Now contains the new line `The whole interface follows your game language...` (2526 characters) |

The version number is synced in five places (`mod_manifest.json` / `mod_id.json` / `Router.cs` / `pyproject.toml` / `uv.lock`), and `check_release_metadata.py` passes.

## Nearly Misjudged: Stuck Is Not Failed

In the client this upload showed as stuck in `k_EItemUpdateStatusPreparingConfig` for a long time (about 14 minutes with no progress and no error); after the 5th attempt I killed the process by hand and judged it a failure. **In fact it then succeeded on its own.**

The cause of the misjudgment: I only looked at the stdout of `ModUploader.exe` and its own `mod-uploader.log` (which is written only at the end), and did not look at the Steam client's log. **The Steam client's log is the authority.**

```
C:\Program Files (x86)\Steam\logs\workshop_log.txt
```

The key lines of this run:

```
[2026-09-12 02:02:30] Upload starting for workshop item 3796486050 by AppID 2868840
[2026-09-12 02:16:55] Upload workshop item 3796486050 failed (Failed to download manifest
                      "steampipe-partner.akamaized.net/depot/2868840/manifest/.../5/..." (timeout))
[2026-09-12 02:33:35] Upload workshop item 3796486050 failed (Failed to download manifest ... (timeout))
[2026-09-12 02:33:40] Uploaded new content ( ManifestID 3781676487912021003 ) for item 3796486050.
[2026-09-12 02:33:41] Upload finished for workshop item 3796486050 : OK
```

## The Real Sticking Point

It was not proxy mode and not `ModUploader` itself: **the Steam client timed out while fetching the item's old manifest**.

- The host is `steampipe-partner.akamaized.net` (Akamai). On this machine's network that domain is unstable: there were 8 related failure records in total, all of them this one.
- Other Steam CDN requests in the same period were fine and went through domestic mirrors: `st.dl.eccdnx.com`(999), `dl.steam.clngaa.com`(937), `xz.pphimalayanrt.com`(720).
- This fetch happens in the `PreparingConfig` stage and is initiated by the Steam client. `ModUploader.exe` had **no TCP connection at all** during this time (measured), so variables such as `HTTP_PROXY` on the uploader process cannot affect it, which also explains why adding proxy variables only got past `PreparingContent` and no further.
- Each attempt spends about 14 minutes on this step before recording a failure, and then **the next retry may succeed**. This is not unique to v0.11.0: the v0.10.7 upload (failed at 01:10:16 -> retried at 01:10:58 -> succeeded at 01:11:03) followed the same pattern.

## What to Do Next Time

1. **Always judge success or failure by `Upload finished ... : OK` in `workshop_log.txt`**, and re-check `time_updated` / `file_size` through the Web API; do not go by the uploader's stdout:
   ```powershell
   Invoke-WebRequest -Uri 'https://api.steampowered.com/ISteamRemoteStorage/GetPublishedFileDetails/v1/' `
     -Method POST -Body 'itemcount=1&publishedfileids%5B0%5D=3796486050' `
     -ContentType 'application/x-www-form-urlencoded' -Proxy 'http://127.0.0.1:10808'
   ```
2. **Do not rush to kill the process**: a single attempt can take more than 15 minutes, and there are internal retries. Allow 20-30 minutes, or rerun once as is after a failure.
3. To reduce the uncertainty of this step, route `steampipe-partner.akamaized.net` through the proxy as well (v2rayN split rules), or switch lines.
4. The upload command and how to observe it are below; a workspace with the `-2` suffix is a repackaged copy and is equivalent in content to the first workspace.

```powershell
Start-Process -FilePath 'C:/Users/chart/AppData/Local/sts2-mod-uploader/win-x64/ModUploader.exe' `
  -ArgumentList @('upload','-w','<workspace>','-i','3796486050') `
  -WindowStyle Hidden -PassThru `
  -RedirectStandardOutput "$env:TEMP/moduploader-stdout.txt" `
  -RedirectStandardError  "$env:TEMP/moduploader-stderr.txt"
```

## About TUN

Post-hoc check: the `xray_tun` adapter (ifIndex 61) was Up but **had no valid routes installed** (only link-local / multicast), and the default route was still on Ethernet and Radmin VPN. So "TUN is on" did not actually take over traffic on this machine. This is unrelated to this conclusion (Steam's HTTP layer works normally through the system proxy `127.0.0.1:10808` anyway, as `124.72.137.38:80 / 127.0.0.1:10808` in `content_log` shows), but if you want to rely on TUN next time, first confirm that `Get-NetRoute | Where ifIndex -eq <tun>` has an actual `0.0.0.0/0` or split route.

## Inconsistency Fixed in Passing This Round

On the first packaging, the line I inserted was LF while the rest of the description file was CRLF, so the `description` in the generated `workshop.json` mixed two kinds of line endings. I normalized `description.en.txt` / `description.zh-CN.txt` in the repository to CRLF and repackaged; `git diff` was empty, showing the repository copies were already in CRLF form, so this fixed my own insertion. The version uploaded to the Workshop is the corrected one.

## Environmental Impact of This Run (Restored)

- To let Steam read the system proxy, Steam was restarted once (no game was running at the time); after the restart it logged in normally (`76561198420578597`).
- During testing the saves for game language and window size were changed; both places (the Steam profile and the isolated `clientId` profile) were restored from backup to `zhs` / 1920×1080.

