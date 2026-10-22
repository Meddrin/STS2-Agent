# Build And Environment Workflow

This document unifies the build, deployment, and runtime environment workflow of STS2 Agent, covering Windows and macOS/Linux.

## 1. Prerequisites

### Common

- Steam is installed and can run Slay the Spire 2
- Python 3.11+
- `uv`

### Windows

- PowerShell 5+ / PowerShell 7+
- .NET SDK (preferably matching the project's current target framework)

### macOS / Linux

- Bash
- .NET SDK
- Godot 4.x (prefer reusing the runtime shipped with the game to package the PCK)

## 2. Key Environment Variables

- `STS2_GAME_ROOT`: game root directory (optional)
- `STS2_DATA_DIR`: game data directory `data_sts2_*` (optional)
- `STS2_MODS_DIR`: the game `mods` directory (optional)
- `STS2_EXE_PATH`: the game executable `SlayTheSpire2.exe` (optional)
- `STS2_APP_MANIFEST`: Steam's `appmanifest_2868840.acf` (optional)
- `STS2_STEAM_EXE`: the Steam client `steam.exe` (optional, needed only for a `-ViaSteam` launch)
- `GODOT_BIN`: path to the Godot executable (optional)
- `STS2_API_BASE_URL`: Mod API address, default `http://127.0.0.1:8080`

Notes:

- `STS2AIAgent.csproj` supports reading the data directory from `STS2_DATA_DIR`.
- The scripts under `scripts/` that need the game (`build-mod.ps1`, `start-game-session.ps1`, `test-mod-load.ps1`, `test-debug-console-gating.ps1`) resolve paths in the same order: **command-line argument -> the environment variables above -> probing -> the conventional default Steam path**; on the Windows side the shared implementation is in `scripts/lib-sts2-paths.ps1`.
- The Windows-side probe reads the Steam install location from the registry, then follows `libraryfolders.vdf` to enumerate all Steam libraries, so a changed drive letter or a game installed in a second library is found automatically; only when the probe finds nothing at all does it fall back to the conventional default install path.
 - The POSIX side (`scripts/lib-sts2-paths.sh`, pulled in by `scripts/lib-sts2.sh`) resolves in the same order and shares the three variable names `STS2_GAME_ROOT`, `STS2_EXE_PATH`, and `STS2_APP_MANIFEST` (`STS2_STEAM_EXE` is read only on the Windows side). The probe covers the conventional Steam root directories of macOS and Linux and reads each install's `libraryfolders.vdf`, so a game installed in a second library is likewise found; only when nothing is found does it fall back to the conventional path.
 - This resolution has an offline test: `bash scripts/test-lib-sts2-paths.sh` (or `python scripts/check_verification_gates.py --only sh-syntax`, which both CI and preflight run). It needs no game, Steam, or network; pointing `$HOME` at a fixture is enough to verify the resolution order, vdf parsing, the `.app` bundle layout, and the second library.
- The parts that need a real machine to confirm (whether the game can start, whether the PCK can be packaged, whether a running game process can be recognized) are outside the declared scope of that test and are still done per the platform acceptance checklist.
 - On Windows use the `.ps1` scripts. Running the `.sh` from Git Bash does get through syntax and resolution (the offline test is run in Git Bash), but `cd`/`pwd` return MSYS-style paths that .NET cannot read, so the `dotnet build` step fails.

## 3. Build And Deploy Mod

### Windows

```powershell
powershell -ExecutionPolicy Bypass -File ".\scripts\build-mod.ps1" -Configuration Release
```

### macOS / Linux

```bash
./scripts/build-mod.sh --configuration Release
```

Common custom parameters:

```bash
./scripts/build-mod.sh \
  --configuration Release \
  --game-root "/path/to/Slay the Spire 2" \
  --data-dir "/path/to/data_sts2_osx_arm64" \
  --mods-dir "/path/to/mods" \
  --godot-exe "/Applications/Godot.app/Contents/MacOS/Godot"
```

After a successful build the following files are copied to the target `mods` directory:

- `STS2AIAgent.dll`
- `STS2AIAgent.pck`

## 4. Start MCP Server

### stdio (recommended)

Windows:

```powershell
powershell -ExecutionPolicy Bypass -File ".\scripts\start-mcp-stdio.ps1"
```

macOS/Linux:

```bash
./scripts/start-mcp-stdio.sh
```

### network (optional)

Windows:

```powershell
powershell -ExecutionPolicy Bypass -File ".\scripts\start-mcp-network.ps1"
```

macOS/Linux:

```bash
./scripts/start-mcp-network.sh
```

Default addresses:

- MCP HTTP: `http://127.0.0.1:8765/mcp`
- Health: `http://127.0.0.1:8765/healthz`

## 5. Verification Checklist

1. The game process has started.
2. `http://127.0.0.1:8080/health` returns `status: ready`.
3. MCP can be imported:

```bash
cd mcp_server
uv run python -c "from sts2_mcp.server import create_server; create_server(); print('MCP_IMPORT_OK')"
```

4. State can be read:

```bash
cd mcp_server
uv run python -c "from sts2_mcp.client import Sts2Client; import json; print(json.dumps(Sts2Client().get_state(), ensure_ascii=False))"
```

5. macOS/Linux core regression entry point:

```bash
./scripts/test-full-regression.sh
```

Non-default install paths can be passed through explicitly:

```bash
./scripts/test-full-regression.sh \
  --game-root "/path/to/Slay the Spire 2" \
  --exe-path "/path/to/SlayTheSpire2.app/Contents/MacOS/Slay the Spire 2" \
  --app-manifest "/path/to/appmanifest_2868840.acf" \
  --app-id 2868840
```

Notes:

- This `bash` regression chain covers the build, Mod loading, debug gating, the MCP tool profile, the main menu lifecycle, the new-run lifecycle, the full state invariants, and the dual-process multiplayer lobby flow.
- `./scripts/test-state-invariants.sh` and `./scripts/test-multiplayer-lobby-flow.sh` can also be run alone for targeted validation.
- If the launch chain needs to write `steam_appid.txt` temporarily, `start-game-session.sh` restores it automatically on exit; `--skip-steam-app-id-file` can also be passed to disable this step.

## 6. Troubleshooting

- Other launch and environment problems (for example `No appID found` on launch):
  - See [docs/troubleshooting.md](./docs/troubleshooting.md).
- `connection refused`:
  - The game is not running, or the Mod did not load successfully.
  - On first load the mods warning must be confirmed in the game.
- `MCP server is up but no state`:
  - Check that `STS2_API_BASE_URL` is correct.
- PCK packaging errors:
  - Prefer using the game's own runtime as `GODOT_BIN`.
