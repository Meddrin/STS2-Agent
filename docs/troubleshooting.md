# STS2 AI Agent Mod - FAQ

## Steam initialization failed: `No appID found`

### Symptom

Starting `SlayTheSpire2.exe` shows a Steam error:

```text
k_ESteamAPIInitResult_FailedGeneric:
No appID found. Either launch the game from Steam, or put the file
steam_appid.txt containing the correct appID in your game folder.
```

### Cause

- When the game EXE is run directly, the Steam API may be unable to get the current game's AppID.
- The local Steam AppID of `Slay the Spire 2` is confirmed to be `2868840`.

### Fix

Pick one:

1. Start the game from the Steam client.
2. Create `steam_appid.txt` in the game root directory, with this content:

```text
2868840
```

Game root directory:

```text
C:/Program Files (x86)/Steam/steamapps/common/Slay the Spire 2
```

### Script behavior in this project

- When resolving the AppID, `scripts/test-mod-load.ps1` looks for `appmanifest_2868840.acf` in this order: the `-AppManifestPath` parameter, the `STS2_APP_MANIFEST` environment variable, probing the local Steam libraries, the default install path. So it works directly even if the game is installed on another drive.
- If `steam_appid.txt` is missing from the game directory, the script creates it automatically so the short startup test does not fail right away.
- If the log contains `user has not yet seen the mods warning`, this is the first-time mod load confirmation on the Steam save path; the script prints a hint, and you only need to run it once more.

### Notes

- This problem is unrelated to the mod's `/health` route; it is a startup method issue.
- If it still fails, first confirm the Steam client is running, and check that `steam_appid.txt` is in the game root directory and not in the `data_sts2_windows_x86_64` subdirectory.
