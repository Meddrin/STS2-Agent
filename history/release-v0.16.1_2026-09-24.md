# v0.16.1 Release Record (2026-09-24)

> Historical snapshot: this page records the process and evidence of this one release at the time and does not represent later states. For the current state see
> [PRODUCT_PLAN_CURRENT.md](../PRODUCT_PLAN_CURRENT.md); for build identification see [build-fingerprints.md](build-fingerprints.md).

| Item | Value |
| --- | --- |
| Version | 0.16.1 (all five version files agree, `check_release_metadata.py` passes) |
| Release PR | #200 (dev -> main), CI `contracts` passed |
| tag / source commit | `v0.16.1` -> `a64e052710fa08a74b54418a78af6bd17d54b2fc` (the Release commit on `dev`), working tree clean at packaging time |
| GitHub Release | [v0.16.1](https://github.com/CharTyr/STS2-Agent/releases/tag/v0.16.1), asset `sts2-ai-agent-v0.16.1-windows.zip` 779153 bytes, SHA256 `835E22E871DD4A217766F5E02043312D2B144E9D81CEB00B75B32B49D37C7551` |
| Steam Workshop | Item 3796486050, public (`visibility` 0), `file_size` 1646567 = local content byte sum, `time_updated` 2026-09-24 11:54:48, content id `1146374597091505946` |
| DLL SHA256 | `CD9707AA463005977D885D1D5D6E206BA28B1CF82A69F67132F34019472E1553` (Workshop package) |

## Contents

One focused fix: in solo mode or while auto-play is running, `invite_ai_teammate` / `continue_ai_teammate` no longer sit silently at
`pending` forever. These player-fixable precondition refusals are now recorded as `Rejected` with a readable reason (switch to multiplayer mode / pause auto-play first),
and the call returns 409 pointing at the cause; attempts that are genuinely in flight concurrently stay `pending` as before. See the v0.16.1 section of [CHANGELOG.md](../CHANGELOG.md).

## Validation

- Reproduced the original failure live (two invites, 8 minutes with no feedback: `dual_launch_outcome` stayed Idle, `dual_status` did not change, no dual-instance process);
  after the fix, the same isolated profile showed: the invite returns **409 `invite_failed`**, with an error message pointing at the switch to two-player mode; `/health` shows
  `dual_launch_outcome=Rejected` and the reason appears in `dual_status`.
- Offline: C# 866, Python 423, all validation gates, `preflight-release.ps1`, CI passed.
- The 185 files of the official Steam profile hashed identically before and after.
- **Not tested live**: the full teammate launch and exit in a Steam-enabled environment (friends / lobby multiplayer). A Steam-off environment forbids multiplayer lobbies, so this counts as an environment-limited scenario;
  this version was only validated up to "a precondition refusal has a clear reason".

## Workshop upload process

Following [steam-workshop/README.md](../steam-workshop/README.md) and the corrections from last time:

1. 11:53:44 the original proxy value was saved (`build/live-2026-09-24/proxy-saved.json`), `ProxyEnable` was temporarily turned off, Steam was restarted, and it logged in at 11:54:38.
2. Baseline success line count 27; first upload at 11:54:38; at 11:54:49 `workshop_log.txt` recorded `Upload finished for workshop item 3796486050 : OK` (-> 28).
3. At 11:54:58 the system proxy was restored (`ProxyEnable=1`, `127.0.0.1:10808`) and Steam was **not restarted again**: following the lesson from v0.16.0, with the proxy on Steam
   tends to get stuck reconnecting its WebSocket. The connection Steam established while the proxy was off stayed online.
4. Steam Web API (through the proxy) re-check: `file_size` 1646567 equals the local `content/` byte sum, `visibility` 0.

## Incomplete / manual

- The Workshop **Simplified Chinese listing** still needs `steam-workshop/description.zh-CN.txt` pasted in manually on the web page (`ModUploader` has no language parameter).
- Subscription load acceptance (actually loading 0.16.1 from the Workshop directory) was not done.
