# Release Acceptance Checklist

> Historical checklist, archived on 2026-09-09. The thresholds, version, and known limitations below apply only to v0.7.0 and are not current release requirements. For the current release entry point see [release-readiness.md](../docs/release-readiness.md).

- Updated: `2026-04-30`
- Target version: `v0.7.0`
- Goal: confirm that `STS2 AI Agent` meets the release standard of "the multiplayer main flow is releasable, the validation scripts can be rerun, and the release package can be distributed as is".

Highlights of this release:

- Multiplayer map voting can be submitted correctly by the local AI and no longer loses the host's first vote.
- The multiplayer rest site `MEND` now has target metadata and a complete `target_index` execution path.
- The launch scripts, multiplayer validation scripts, and release packaging flow have all been turned into a repeatable sealed-release process.

## 1. Static Gates

The following commands must pass:

```powershell
dotnet build "<repo-root>/STS2AIAgent/STS2AIAgent.csproj" -c Release
python -m py_compile "<repo-root>/mcp_server/src/sts2_mcp/client.py" "<repo-root>/mcp_server/src/sts2_mcp/server.py"
cd "<repo-root>/mcp_server"
uv run python -c "from sts2_mcp.server import create_server; create_server(); print('MCP_IMPORT_OK')"
```

You can also run directly:

```powershell
powershell -ExecutionPolicy Bypass -File "<repo-root>/scripts/preflight-release.ps1"
```

Pass criteria:

- The Mod C# project compiles under the `Release` configuration.
- The MCP Python sources compile.
- The MCP server can be imported and a server instance created.
- `CHANGELOG.md` and the release documents are complete, and the version information is synced to `0.7.0`.

## 2. Installation Gates

### Mod Installation

```powershell
powershell -ExecutionPolicy Bypass -File "<repo-root>/scripts/build-mod.ps1" -Configuration Release
powershell -ExecutionPolicy Bypass -File "<repo-root>/scripts/test-mod-load.ps1" -DeepCheck
```

Pass criteria:

- `STS2AIAgent.dll` has been copied to the game `mods/` directory.
- `STS2AIAgent.pck` has been copied to the game `mods/` directory.
- After the game starts, `/health`, `/state`, and `/actions/available` all return normally.

### MCP Startup

```powershell
cd "<repo-root>/mcp_server"
uv sync
uv run sts2-mcp-server
```

Pass criteria:

- The MCP server starts normally.
- The MCP client can call `health_check` successfully.
- When the Mod is not running, the error returned is understandable and not a crash.

## 3. Debug Gates

The debug console must stay "off by default, available only after being explicitly enabled":

```powershell
powershell -ExecutionPolicy Bypass -File "<repo-root>/scripts/test-debug-console-gating.ps1"
powershell -ExecutionPolicy Bypass -File "<repo-root>/scripts/test-debug-console-gating.ps1" -EnableDebugActions
```

Pass criteria:

- By default, `run_console_command` returns `invalid_action`.
- After explicitly enabling `STS2_ENABLE_DEBUG_ACTIONS=1`, `run_console_command` is available.
- The MCP server registers the debug tool only when the corresponding environment variable is enabled.

## 4. Single-Player Main Flow Gates

At least the following flows must be covered:

1. `MAIN_MENU -> open_character_select`
2. `select_character`
3. `embark`
4. Handle the opening `MODAL` / FTUE
5. `MAP -> choose_map_node`
6. `COMBAT -> play_card -> end_turn`
7. After combat ends, enter rewards or return to the map

Pass criteria:

- `available_actions` is exposed correctly in each phase.
- State transitions are stable after an action executes, with no hung requests.
- The `state-invariants` validation passes.

## 5. Multiplayer Main Flow Gates

Must run:

```powershell
powershell -ExecutionPolicy Bypass -File "<repo-root>/scripts/test-multiplayer-lobby-flow.ps1"
```

Must cover:

1. Host creates a room, Client joins.
2. Both sides select a character and ready up.
3. After entering the run, automatically get past the opening cutscene and reach the map.
4. Both sides' map voting and the vote status echo.
5. Card plays, turn advancement, and reward settlement in the first combat.
6. Rest site `MEND` target metadata is exposed.
7. Omitting `target_index` returns `invalid_target`.
8. After passing `target_index`, multiplayer `MEND` completes within a single request.

Pass criteria:

- The script's overall exit code is `0`.
- Both Host and Client can advance to a stable state, on the map after combat or after the rest site.
- `map.local_vote`, `map.player_votes`, and node vote information are visible.
- `rest.options[*]` exposes `requires_target` and a valid target index for `MEND`.

## 6. Known Limitations

The following limitations are confirmed to exist but do not block `v0.7.0` as a release version:

- After multiplayer reward settlement, the host-side debug command `run_console_command: room RestSite` may still trigger an internal error because of the game's own sync state.
- This issue only affects the debug room-jump validation and does not affect normal multiplayer play under AI takeover.

## 7. Packaging Gates

Generate the release package:

```powershell
powershell -ExecutionPolicy Bypass -File "<repo-root>/scripts/package-release.ps1" -Configuration Release
```

The release directory and the zip should contain at least:

- `mod/STS2AIAgent.dll`
- `mod/STS2AIAgent.pck`
- `mod/mod_id.json`
- `mcp_server/`
- `scripts/start-mcp-stdio.ps1`
- `scripts/start-mcp-network.ps1`
- `README.md`
- `CHANGELOG.md`
- `docs/release-readiness.md`

Pass criteria:

- The release directory is generated successfully.
- The zip is generated successfully.
- The root documents and scripts are complete and can be delivered directly to end users.
