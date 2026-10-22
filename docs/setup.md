# STS2 AI Agent Mod - Environment and runtime setup

## Purpose

Records the environment facts confirmed on the current development machine, the missing dependencies, and the suggested install steps, so later implementation phases do not proceed on wrong assumptions.

Last updated: 2026-03-10

---

## Confirmed facts on this machine

### Workspace

- Project root: `<repo-root>`
- Git is initialized in the repository, but there is no code skeleton yet

### Game installation

- Game directory: `C:/Program Files (x86)/Steam/steamapps/common/Slay the Spire 2`
- Assembly directory: `C:/Program Files (x86)/Steam/steamapps/common/Slay the Spire 2/data_sts2_windows_x86_64`
- Main assembly: `sts2.dll`
- `sts2.dll` size: `8,870,912` bytes
- Game version: `v0.98.2`
- Release commit: `f4eeecc6`
- Release date: `2026-03-06T15:52:37-08:00`

### Runtime and dependencies

- `sts2.runtimeconfig.json` points to `net9.0`
- The game ships `Microsoft.NETCore.App 9.0.7`
- The following are confirmed to exist in the game directory:
  - `0Harmony.dll` `2.4.2.0`
  - `GodotSharp.dll` `4.5.1.0`
  - `System.Net.HttpListener.dll`

### Current development machine tool status

| Tool | Status | Notes |
|------|------|------|
| `.NET Runtime` | Installed | Runtimes `6.0.16` and `8.0.14` exist on the host |
| `.NET SDK` | Installed | `9.0.311` |
| `python` | Installed | Interpreter at `<python-install>/python.exe` |
| `py` | Installed | `py -3.11 --version` returns `Python 3.11.9` |
| `uv` | Installed | `0.10.9` |
| `ilspycmd` | Installed | `9.1.0.7988` |
| `godot` CLI | Installed | `Godot 4.5.1 Mono`, currently invoked through an explicit path |

### Mod directory status

- No existing `mods/` directory was found in the game root
- Formal decompilation confirmed that ModManager scans `Path.Combine(directoryName, "mods")`
- That is, the final directory is: `C:/Program Files (x86)/Steam/steamapps/common/Slay the Spire 2/mods`
- If the directory does not exist, create it manually when installing the minimal mod later

---

## Current blockers

The following items still block the full integration phase:

1. Add the minimal implementation of `/state` and `/action` to the mod
2. Validate more routes in a real running game, not just `/health`
3. Establish a stable Godot packaging flow for later `.pck` resource extensions

---

## Suggested install order

1. Install the `.NET 9 SDK`
2. Install `Python 3.11+`
3. Install `uv`
4. Install `ilspycmd`
5. Install the `Godot 4.5.1 Mono` editor

After that, run the following checks:

```powershell
dotnet --list-sdks
py -3.11 --version
uv --version
ilspycmd --version
godot --version
```

---

## Suggested directory conventions

### Project directories

- `STS2AIAgent/`: C# mod project
- `mcp_server/`: Python MCP server
- `docs/`: protocol, reverse engineering, and runtime docs
- `extraction/`: decompilation output and temporary analysis results

### Reverse-engineering output directory

Decompilation results are recommended to go to:

`<repo-root>/extraction/decompiled`

---

## First validation checklist

### Environment validation

- `dotnet --list-sdks` shows `9.0.311`
- `py -3.11 --version` returns `Python 3.11.9`
- `uv --version` returns `0.10.9`
- `ilspycmd --version` returns `9.1.0.7988`
- `"<godot-console-exe>" --version` returns `4.5.1`

### Game fact validation

- `release_info.json` is still `v0.98.2`
- `sts2.runtimeconfig.json` still declares `net9.0`
- `0Harmony.dll` and `GodotSharp.dll` still exist

### Mod install validation

- Confirmed that the final `mods/` directory is `mods/` under the game root
- The `.dll` and `.pck` must sit side by side in the same directory with the same name
- A minimal mod can be placed in the directory and is recognized by the game
- If the log shows `user has not yet seen the mods warning`, enable it in `settings.save`:
  - `"mod_settings": { "mods_enabled": true }`

### Currently available build commands

```powershell
dotnet build "<repo-root>/STS2AIAgent/STS2AIAgent.csproj"
```

### Currently available automation scripts

```powershell
powershell -ExecutionPolicy Bypass -File "<repo-root>/scripts/build-mod.ps1"
powershell -ExecutionPolicy Bypass -File "<repo-root>/scripts/test-mod-load.ps1"
```

- Before running `build-mod.ps1`, pass the Godot console executable via `-GodotExe`, or set the `GODOT_BIN` environment variable
- `build-mod.ps1` builds the DLL, packs a minimal `.pck`, and installs it into the game `mods/` directory
- `test-mod-load.ps1` starts the game briefly and polls `http://127.0.0.1:8080/health`

### Completed validation

- Generated and installed:
  - `C:/Program Files (x86)/Steam/steamapps/common/Slay the Spire 2/mods/STS2AIAgent.dll`
  - `C:/Program Files (x86)/Steam/steamapps/common/Slay the Spire 2/mods/STS2AIAgent.pck`
- The short-start validation confirmed `/health` returns 200
- Confirmed that the game log contains the mod scan, initialization, and HTTP server start records

---

## Next steps

1. Implement the minimal fields of `GET /state`
2. Implement a minimal `POST /action`
3. Add finer-grained logging and error returns to the mod
4. Start preparing minimal client integration on the MCP side
