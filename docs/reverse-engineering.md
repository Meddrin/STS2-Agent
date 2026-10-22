# STS2 AI Agent Mod - Reverse engineering notes

## Purpose

Records the confirmed game runtime facts, the next reverse-engineering targets, and the questions still to verify, so the implementation phase is built on real types and real entry points.

Last updated: 2026-03-10

---

## Confirmed facts

### Game and assemblies

- Game directory: `C:/Program Files (x86)/Steam/steamapps/common/Slay the Spire 2`
- Assembly directory: `C:/Program Files (x86)/Steam/steamapps/common/Slay the Spire 2/data_sts2_windows_x86_64`
- Main assembly: `sts2.dll`
- Version information source:
  - `release_info.json`
  - `sts2.runtimeconfig.json`

### Confirmed runtime information

| Item | Value |
|------|----|
| Game version | `v0.98.2` |
| Release commit | `f4eeecc6` |
| Release date | `2026-03-06T15:52:37-08:00` |
| Target framework | `net9.0` |
| Bundled runtime | `Microsoft.NETCore.App 9.0.7` |
| Harmony | `0Harmony.dll 2.4.2.0` |
| Godot C# bindings | `GodotSharp.dll 4.5.1.0` |

### Confirmed implementation-level inferences

- The game is not a Unity + BepInEx setup; it runs on Godot 4.5.1 + the C# runtime
- Harmony patching is a viable route, because `0Harmony.dll` is already in the game directory
- A local HTTP API is also viable, because `System.Net.HttpListener.dll` is already in the game directory
- The exact mod loading mechanism still has to be confirmed through a real example or the decompiled entry point

---

## Key facts confirmed by formal decompilation

The following comes from the formal decompilation of `sts2.dll` with `ilspycmd`, and is more trustworthy than the earlier string scan.

### Mod loading mechanism

| Fact | Description |
|------|------|
| Scan directory | `Path.Combine(directoryName, "mods")` |
| Scan method | Recursively walks the directory looking for `.pck` files |
| DLL location | Same directory and same name as the `.pck`, such as `STS2AIAgent.pck` with `STS2AIAgent.dll` |
| Package requirement | Must contain `res://mod_manifest.json` |
| Manifest fields | `pck_name`, `name`, `author`, `description`, `version` |
| Name constraint | `pck_name` in `mod_manifest.json` must match the `.pck` file name |
| Load method | `.pck` is loaded with `ProjectSettings.LoadResourcePack(...)` |
| Assembly loading | The same-named `.dll` is loaded first through `AssemblyLoadContext.LoadFromAssemblyPath(...)`, then the `.pck` |
| User gate | `SaveManager.Instance.SettingsSave.ModSettings?.PlayerAgreedToModLoading` must be `true`, otherwise loading is skipped |

### Mod settings persistence

| Fact | Description |
|------|------|
| Settings file | `%APPDATA%/SlayTheSpire2/default/1/settings.save` |
| File format | Plain-text JSON |
| Matching field | `mod_settings.mods_enabled` |
| Matching property | `ModSettings.PlayerAgreedToModLoading` |

### Mod initialization mechanism

| Fact | Description |
|------|------|
| Attribute type | `MegaCrit.Sts2.Core.Modding.ModInitializerAttribute` |
| Target | Applied to a class, not to a method |
| Argument | The name string of the initialization method |
| Method requirement | `static`, either `public` or `non-public` |
| Fallback path | If the assembly has no `ModInitializerAttribute`, `Harmony.PatchAll(assembly)` is run on the whole assembly |

### Combat action mechanism

| Fact | Description |
|------|------|
| Minimal card play entry | `CardModel.TryManualPlay(Creature? target)` |
| Actual enqueue | `CardModel.EnqueueManualPlay(Creature? target)` |
| Enqueue implementation | `RunManager.Instance.ActionQueueSynchronizer.RequestEnqueue(new PlayCardAction(this, target))` |
| Candidate stable-state point | `CombatManager.WaitUntilQueueIsEmptyOrWaitingOnNonPlayerDrivenAction()` |
| Wait mechanism | Listens to `ActionExecutor.AfterActionExecuted` until no player-driven action is ready |

### Map and reward progression

| Fact | Description |
|------|------|
| Map action class | `MoveToMapCoordAction(Player player, MapCoord destination)` |
| Map progression | Outside test mode, calls `NMapScreen.Instance.TravelToMapCoord(_destination)` |
| Progression after rewards | `RunManager.ProceedFromTerminalRewardsScreen()` |
| Behavior after rewards | Opens the map, or resumes the previous room in a combat event |

### Where the current screen comes from

`NetScreenType` looks more like a multiplayer sync screen type and does not cover the full single-player logic.  
The better entry for identifying `/state.screen` is:

- `MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext.ActiveScreenContext`
- `ActiveScreenContext.GetCurrentScreen()`

This method checks, in order:

1. `FeedbackScreen`
2. `OpenModal`
3. Card/relic inspection screens
4. Main menu and submenus
5. `NMapScreen`
6. `NOverlayStack`
7. `EventRoom`
8. `CombatRoom`
9. `TreasureRoom`
10. `RestSiteRoom`
11. `MapRoom`
12. `MerchantRoom`

This shows that `/state.screen` is best built on `ActiveScreenContext + NRun` room objects, not on a single simplified enum.

### Completed runtime validation

- The game log confirmed that `mods/STS2AIAgent.pck` is recognized
- Before the setting is enabled, it logs:
  - `Skipping loading mod STS2AIAgent.pck, user has not yet seen the mods warning`
- After setting `mod_settings.mods_enabled` to `true` in `settings.save`, the log confirmed:
  - Mod assembly loaded
  - Initialization method called
  - `STS2AIAgent` HTTP server started
- A `/health` response was received in the real game process

---

## Real symbols found so far

The following comes from a string scan of `sts2.dll`. It is not equivalent to formal decompilation, but it is enough to prove that a batch of key names and entry points really exist.

### Modding related

| Symbol | Description |
|------|------|
| `MegaCrit.Sts2.Core.Modding.ModManifest` | Confirmed: a mod manifest type exists |
| `MegaCrit.Sts2.Core.Modding.ModManager` | Confirmed: a mod manager exists |
| `ModInitializerAttribute` | Confirmed: a mod initialization attribute exists |
| `CallModInitializer` | Confirmed: initializer-calling logic exists |
| `MegaCrit.Sts2.Core.Nodes.Screens.ModdingScreen.NModdingScreen` | Confirmed: a Modding UI exists |

### Combat and action related

| Symbol | Description |
|------|------|
| `MegaCrit.Sts2.Core.Combat.CombatManager` | Confirmed: the real namespace is `Core.Combat` |
| `MegaCrit.Sts2.Core.Runs.RunManager` | Confirmed: the real namespace is `Core.Runs` |
| `MegaCrit.Sts2.Core.Entities.Creatures.Creature` | Confirmed: the creature base class exists |
| `EnqueueManualPlay` | Confirmed: the manual card play entry name exists |
| `TryPlayCard` | Confirmed: card play logic exists |
| `UsePotion` | Confirmed: potion use logic exists |
| `MegaCrit.Sts2.Core.GameActions.PlayCardAction` | Confirmed: a card play action object exists |
| `MegaCrit.Sts2.Core.GameActions.UsePotionAction` | Confirmed: a potion action object exists |
| `ActionQueue` | Confirmed: an action queue exists |
| `ActionQueueSynchronizer` | Confirmed: an action queue synchronizer exists |
| `WaitUntilQueueIsEmptyOrWaitingOnNonPlayerDrivenAction` | Very likely a candidate wait point for "state is stable" |

### Scene and node related

| Symbol | Description |
|------|------|
| `MegaCrit.Sts2.Core.Nodes.Screens.Map.NMapScreen` | Map screen node |
| `MegaCrit.Sts2.Core.Nodes.Rooms.NMapRoom` | Map room node |
| `MegaCrit.Sts2.Core.Nodes.Rooms.NEventRoom` | Event room node |
| `MegaCrit.Sts2.Core.Nodes.Rooms.NMerchantRoom` | Shop room node |
| `MegaCrit.Sts2.Core.Nodes.Rooms.NRestSiteRoom` | Rest site room node |
| `MegaCrit.Sts2.Core.Nodes.Rooms.NTreasureRoom` | Chest room node |
| `MegaCrit.Sts2.Core.Nodes.Screens.NRewardsScreen` | Rewards screen node |
| `MegaCrit.Sts2.Core.Nodes.Rewards.NRewardButton` | Reward button node |
| `MegaCrit.Sts2.Core.Nodes.Combat.NIntent` | Combat intent UI node |

### Flow candidate points

| Symbol | Description |
|------|------|
| `RunManager.<EnterMapPointInternal>d__180` | Candidate entry for map progression |
| `RunManager.<ProceedFromTerminalRewardsScreen>d__198` | Candidate entry for continuing the flow after rewards end |
| `RunManager.<EnterRoomInternal>d__190` | Candidate entry for entering a room |
| `CombatManager.<StartTurn>d__92` | Candidate point for the start of the player turn |
| `CombatManager.<DoTurnEnd>d__116` | Candidate point for turn end |
| `CombatManager.<EndCombatInternal>d__108` | Candidate point for combat end |

### Notes

- The real namespace of `CombatManager` does not fully match the `MegaCrit.Sts2.Core` in the original plan; the current scan results better support `MegaCrit.Sts2.Core.Combat.CombatManager`
- This means that, before formal decompilation, type names in the old requirements doc cannot keep being treated as facts
- The four symbols most worth verifying first are:
  - `ModInitializerAttribute`
  - `CallModInitializer`
  - `EnqueueManualPlay`
  - `WaitUntilQueueIsEmptyOrWaitingOnNonPlayerDrivenAction`

---

## Current blockers

Although the first round of formal decompilation is done, the following still needs to be confirmed:

- Whether the full directory structure of a minimal runnable mod still holds in more complex resource scenarios
- Action execution entry points in non-combat scenes
- The concrete execution methods for rest site, shop, and event
- The shortest access chains for extracting player, enemy, and pile state

---

## Reverse-engineering target list

### Tier 1 targets

These objects must be confirmed for the minimal vertical slice:

| Target | Purpose |
|------|------|
| `RunManager` | Run state, action queue, global context |
| `CombatManager` | Combat start, combat state entry |
| `Player` | Player state, resources, deck |
| `Creature` | Properties shared by enemies and the player |
| `CardModel` | Card play entry and card metadata |
| `ModelDb` | Model database and ID resolution |
| screen enum or scene object | The real source of `/state.screen` |

### Tier 2 targets

These objects are needed for main-line progression and Phase 4:

| Target | Purpose |
|------|------|
| Map node types | Map navigation |
| Reward screen objects | Reward state and choices |
| Event objects | Event options |
| Shop objects | Shop items and purchase behavior |
| Rest site objects | Rest, upgrade, and similar operations |
| Monster Intent types | Combat intent and damage preview |

---

## Planned output format

After formal decompilation, this file should at least gain the following:

### Type index table

| Logical name | Real type name | Namespace | Access method | Notes |
|----------|------------|----------|----------|------|
| Run management | TBD | TBD | TBD | TBD |

### Key entry table

| Logical action | Type | Method/field | Description |
|----------|------|-----------|------|
| Get the current run | TBD | TBD | TBD |

### Patch candidate points

| Location | Purpose | Type |
|------|------|------|
| `CombatManager.SetUpCombat` | Cache the combat reference | Postfix candidate |

---

## Suggested decompilation steps

1. Run the following command:

```powershell
ilspycmd -p -o "<repo-root>/extraction/decompiled" "C:/Program Files (x86)/Steam/steamapps/common/Slay the Spire 2/data_sts2_windows_x86_64/sts2.dll"
```

2. Search mainly for the following keywords:
   - `ModInitializer`
   - `RunManager`
   - `CombatManager`
   - `SetUpCombat`
   - `ActionQueue`
   - `CardModel`
   - `Intent`
   - `Reward`
   - `Shop`
   - `Map`

3. Add the confirmed results to this file according to the "Type index table" and "Key entry table"

### Temporary workarounds

When symbols need to be added quickly, the repository script can be used for a string scan:

```powershell
powershell -ExecutionPolicy Bypass -File "<repo-root>/scripts/scan-assembly-strings.ps1" -AssemblyPath "C:/Program Files (x86)/Steam/steamapps/common/Slay the Spire 2/data_sts2_windows_x86_64/sts2.dll" -Patterns ModManager,ModInitializerAttribute,CombatManager,RunManager,EnqueueManualPlay,UsePotion
```

This script can only be used to discover symbols quickly and cannot replace formal decompilation.

---

## First-round key validation questions

1. What exactly is the mod entry attribute or interface
2. How `screen` is mapped from `ActiveScreenContext` to a stable API value
3. Whether `end_turn` should directly use the internal `CombatManager` flow instead of simulating button input
4. Whether map node selection should preferably reuse `MoveToMapCoordAction`
5. Whether reward, shop, and rest site should be entered through room objects or screen objects
6. Which objects are the most stable to read from first for the minimal `/state` implementation

---

## Connection to the implementation phase

Before `Phase 1B` starts, at least the following must be confirmed:

- Mod entry
- Minimal usable patch point
- State reading entry
- Minimal action entry

If any one of these four is unconfirmed, the mod skeleton can be built first, but business implementation should not keep expanding.
