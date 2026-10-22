# STS2 (Slay the Spire 2) Coverage Gap List

> Historical snapshot: this file is a gap list compiled on 2026-03-10 from decompilation results and does not represent the current state. The
> "not yet covered" items listed here (CHEST / EVENT / REST / SHOP / POTION / CHARACTER_SELECT / GAME_OVER and so on) have all since
> landed in code (for the actions and payloads see the action table in the [API docs](../docs/api.md) and the [current status page](../PRODUCT_PLAN_CURRENT.md)).
> This file is kept only as a record of the development boundary at that time.

Updated: `2026-03-10`

> Below, decompiled source is referenced as `extraction/decompiled/**` paths and **not as links**: that directory is gitignored
> and does not exist on GitHub or in a fresh checkout. Their purpose is to locate class names and namespaces, and they cannot be opened by design;
> writing them as links would be a promise that cannot be kept, and the `doc-links` gate would flag them red as is.

This document is based only on the decompilation of the `sts2.dll` of **Slay the Spire 2** and is **not about the first game**.

Purpose:

- Let Claude compare against the current project implementation and fill in the main-flow coverage of Slay the Spire 2
- Make clear which scenarios are not in the current main line but would actually block the automatic flow
- Provide a development boundary for the later Phase 4B / 4C / 5 / 6

---

## Main Flows Covered So Far

The current project already fully or partially covers:

- Main menu recognition
- The current map node and the next reachable nodes
- Combat state reading
- Combat actions:
  - `end_turn`
  - `play_card`
- Reward settlement:
  - `claim_reward`
  - `choose_reward_card`
  - `skip_reward_cards`
  - `collect_rewards_and_proceed`
- Deck card selection:
  - `select_deck_card`
- Run state information:
  - `run.deck`
  - `run.relics`
  - `run.potions`
  - `run.gold`
- Full map structure export:
  - `map.nodes`

But these are still not enough to cover the **complete flow of one STS2 run**.

---

## Key Systems Still Not Fully Covered

### 1. Treasure Room / Relic Selection

Current state:

- `screen = "CHEST"` is already recognized
- but the relic selection sub-flow after opening a chest is not yet truly covered

Decompilation entry points:

- `extraction/decompiled/MegaCrit.Sts2.Core.Nodes.Rooms/NTreasureRoom.cs`
- `extraction/decompiled/MegaCrit.Sts2.Core.Nodes.Screens.TreasureRoomRelic/NTreasureRoomRelicCollection.cs`
- `extraction/decompiled/MegaCrit.Sts2.Core.Multiplayer.Game/TreasureRoomRelicSynchronizer.cs`

Key facts:

- After opening the chest it enters `NTreasureRoomRelicCollection`
- It must wait for `RelicPickingFinished()`
- Relic selection is driven through `RunManager.Instance.TreasureRoomRelicSynchronizer.PickRelicLocally(holder.Index)`
- There is an empty-chest branch: `CompleteWithNoRelics()`

Suggested additions:

- `chest` payload
- `chest.relic_options`
- `choose_treasure_relic`
- Empty chest handling

Risks:

- Doing only `proceed` is not enough; it gets stuck at the relic selection screen
- If one assumes a relic is always present, it hangs on an empty chest or special scenarios

---

### 2. Event System

Current state:

- `screen = "EVENT"` is already recognized
- `event` is still `null`

Decompilation entry points:

- `extraction/decompiled/MegaCrit.Sts2.Core.Nodes.Rooms/NEventRoom.cs`
- `extraction/decompiled/MegaCrit.Sts2.Core.Events/EventOption.cs`
- `extraction/decompiled/MegaCrit.Sts2.Core.Nodes.Events/NEventOptionButton.cs`
- `extraction/decompiled/MegaCrit.Sts2.Core.Multiplayer.Game/EventSynchronizer.cs`

Key facts:

- Event options carry not only text but also these semantics:
  - `IsLocked`
  - `IsProceed`
  - `WillKillPlayer`
  - `Relic`
  - `HoverTips`
- After an event finishes it automatically turns into a Proceed button
- There is an `EmbeddedCombatRoom`

Suggested additions:

- `event.event_id`
- `event.title`
- `event.description`
- `event.options[]`
- `choose_event_option`

`event.options[]` should at least contain:

- `index`
- `title`
- `description`
- `locked`
- `is_proceed`
- `will_kill_player`
- `has_relic_preview`

Risks:

- An event is not a simple button list
- Some options enter a nested combat, and after coming back the event or settlement must continue

---

### 3. Event Nested Combat

Current state:

- The current project covers only the ordinary combat room logic
- The nested combat switch inside an event has not been validated separately

Decompilation entry points:

- `extraction/decompiled/MegaCrit.Sts2.Core.Nodes.Rooms/NEventRoom.cs`
- `extraction/decompiled/MegaCrit.Sts2.Core.Nodes/NRun.cs`
- `extraction/decompiled/MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext/ActiveScreenContext.cs`

Key facts:

- `NEventRoom` provides `EmbeddedCombatRoom`
- The current active screen may directly become the combat room inside the event
- This means `screen = "COMBAT"` does not necessarily come from an ordinary combat room

Suggested additions:

- Live validation:
  - `EVENT -> COMBAT -> REWARD/MAP`
  - `EVENT -> COMBAT -> EVENT`
- In the state machine, avoid mistaking a combat inside an event for the end of an ordinary room flow

Risks:

- If it is handled only as an ordinary combat, the event chain may break midway

---

### 4. Rest Site System

Current state:

- `screen = "REST"` is already recognized
- `rest` is still `null`

Decompilation entry points:

- `extraction/decompiled/MegaCrit.Sts2.Core.Nodes.Rooms/NRestSiteRoom.cs`
- `extraction/decompiled/MegaCrit.Sts2.Core.Entities.RestSite/RestSiteOption.cs`
- `extraction/decompiled/MegaCrit.Sts2.Core.Nodes.RestSite/NRestSiteButton.cs`
- `extraction/decompiled/MegaCrit.Sts2.Core.Multiplayer.Game/RestSiteSynchronizer.cs`

Key facts:

- A rest site does not have a fixed two options
- By default there are at least:
  - `Heal`
  - `Smith`
  - In multiplayer there may also be `Mend`
- The directory also contains:
  - `Cook`
  - `Dig`
  - `Hatch`
  - `Lift`
  - `Clone`

Suggested additions:

- `rest.options[]`
- Generic action:
  - `choose_rest_option`
  - or `rest_site_action`

`rest.options[]` should at least contain:

- `index`
- `option_id`
- `title`
- `description`
- `enabled`

Risks:

- Do not hard-code only "Rest / Smith"
- Slay the Spire 2 has many special rest options, so a generic option model is required

---

### 5. Rest Site Smith Sub-flow

Current state:

- `select_deck_card` already exists
- but it has not been confirmed that it is truly wired through to the rest site card upgrade sub-flow

Decompilation entry points:

- `extraction/decompiled/MegaCrit.Sts2.Core.Entities.RestSite/SmithRestSiteOption.cs`

Key facts:

- `Smith` does not complete immediately inside the room
- It goes through `CardSelectCmd.FromDeckForUpgrade(...)`
- It needs manual confirmation and can be canceled

Suggested additions:

- Live validation:
  - `REST -> choose smith -> CARD_SELECTION -> select_deck_card -> confirm -> back to REST / proceed`
- If the current `select_deck_card` applies only to card removal, its semantics need to be tightened or extended

Risks:

- If only the rest button click is done and the upgrade card selection is not handled, the flow gets stuck

---

### 6. Shop System

Current state:

- `screen = "SHOP"` is already recognized
- `shop` is still `null`

Decompilation entry points:

- `extraction/decompiled/MegaCrit.Sts2.Core.Nodes.Rooms/NMerchantRoom.cs`
- `extraction/decompiled/MegaCrit.Sts2.Core.Nodes.Screens.Shops/NMerchantInventory.cs`
- `extraction/decompiled/MegaCrit.Sts2.Core.Entities.Merchant/MerchantInventory.cs`
- `extraction/decompiled/MegaCrit.Sts2.Core.Nodes.Screens.Shops/NMerchantSlot.cs`
- `extraction/decompiled/MegaCrit.Sts2.Core.Nodes.Screens.Shops/NMerchantCardRemoval.cs`

Key facts:

- The shop is a two-layer flow:
  - The outer room `NMerchantRoom`
  - The inner inventory `NMerchantInventory`
- The inventory is divided into:
  - Character cards
  - Colorless cards
  - relic
  - potion
  - Card removal service
- `NMerchantInventory.IsOpen` is the key state

Suggested additions:

- `shop` payload
- `shop.is_open`
- `shop.cards`
- `shop.relics`
- `shop.potions`
- `shop.card_removal`
- Actions:
  - `open_shop_inventory`
  - `close_shop_inventory`
  - `buy_card`
  - `buy_relic`
  - `buy_potion`
  - `remove_card_at_shop`

Risks:

- If only the goods list is exported and the inventory open/closed state is not, the AI can hardly know whether it can buy right now
- The card removal service is not an ordinary goods slot and needs separate handling

---

### 7. Potion System

Current state:

- Potion slots are only read in `run.potions`
- There is no action support

Decompilation entry points:

- `extraction/decompiled/MegaCrit.Sts2.Core.Nodes.Potions/NPotionHolder.cs`
- `extraction/decompiled/MegaCrit.Sts2.Core.Nodes.Potions/NPotionPopup.cs`

Key facts:

- Potions support at least:
  - Use
  - Discard
- They also distinguish:
  - `CombatOnly`
  - `AnyTime`
  - `Automatic`
- They are affected by these states:
  - Whether in combat
  - Whether it is the player's turn
  - `PlayerActionsDisabled`
  - Whether on the card selection screen
  - Whether a target is needed

Suggested additions:

- `use_potion`
- `discard_potion`
- Where necessary, add to the state:
  - `usable`
  - `discardable`
  - `requires_target`

Risks:

- Without potion support, strictly speaking combat capability is still incomplete
- Targeted potions and any-time potions bring extra branches

---

### 8. Character Select / Run Start

Current state:

- `screen = "CHARACTER_SELECT"` is already recognized
- There is no run start action

Decompilation entry points:

- `extraction/decompiled/MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect/NCharacterSelectScreen.cs`

Key facts:

- There are character buttons
- There is `OnEmbarkPressed`
- There is an Ascension panel
- There is a random character
- There is ready / unready logic

If the goal is only "taking over midway through a dungeon run", this part can come later.  
If the goal is "playing a whole run fully automatically from the main menu", this part must be added.

Suggested additions:

- `character_select` payload
- At least support:
  - Select a character
  - Start the run (Embark)

Risks:

- Without run start support, it cannot be called "finishing a whole run from the main menu"

---

### 9. Game Over / Settlement Wrap-up

Current state:

- `screen = "GAME_OVER"` is already recognized
- `game_over` is still `null`

Decompilation entry points:

- `extraction/decompiled/MegaCrit.Sts2.Core.Nodes.Screens.GameOverScreen/NGameOverScreen.cs`

Key facts:

- The settlement screen has at least:
  - continue
  - view run
  - main menu
  - leaderboard
  - timeline

Suggested additions:

- `game_over` payload
- At least include:
  - `win`
  - `score`
  - `floor`
  - `character`
- Minimal actions:
  - `return_to_main_menu`
  - or `continue_from_game_over`

Risks:

- Without game over wrap-up, the full run closed loop still does not hold

---

## Hidden Threads That Are Easy to Miss but Block the Automatic Flow

### A. Overlay Layer

Decompilation entry points:

- `extraction/decompiled/MegaCrit.Sts2.Core.Nodes.Screens.Overlays/NOverlayStack.cs`
- `extraction/decompiled/MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext/ActiveScreenContext.cs`
- `extraction/decompiled/MegaCrit.Sts2.Core.AutoSlay/AutoSlayer.cs`

Key facts:

- The current screen may come first from an overlay rather than the room itself
- Rewards, card selection, some event follow-ups, and chest relic selection may all run on an overlay

Suggestions:

- Judge every new feature by the "current active screen" rather than the "current room type"
- Verify the stable state after the screen switch for every action

---

### B. Modal / FTUE

Decompilation entry points:

- `extraction/decompiled/Properties/AssemblyInfo.cs`
- `extraction/decompiled/MegaCrit.Sts2.Core.AutoSlay/AutoSlayer.cs`
- `extraction/decompiled/MegaCrit.Sts2.Core.Saves/SaveManager.cs`

Key facts:

- FTUE and various confirmation popups really block input
- These scenarios may appear in combat, the map, rest sites, the shop, shuffling, potions, and elsewhere

Pick one of two:

1. Turn off the FTUE uniformly before running  
2. Add a minimal `modal` / `confirm` handling capability

Risks:

- If the FTUE is not turned off in the official test environment, it is easy to get "the interfaces are all correct, but the AI is stuck by a popup"

---

### C. Synchronizer Entry Points

Decompilation entry points:

- `extraction/decompiled/MegaCrit.Sts2.Core.Multiplayer.Game/EventSynchronizer.cs`
- `extraction/decompiled/MegaCrit.Sts2.Core.Multiplayer.Game/RestSiteSynchronizer.cs`
- `extraction/decompiled/MegaCrit.Sts2.Core.Multiplayer.Game/TreasureRoomRelicSynchronizer.cs`
- `extraction/decompiled/MegaCrit.Sts2.Core.Multiplayer.Game/MapSelectionSynchronizer.cs`

Key facts:

- In Slay the Spire 2 many non-combat actions are ultimately driven through a synchronizer
- This is often a more stable action entry point than "simulating a click on some button"

Suggestions:

- When developing, Claude should look for the synchronizer / action queue entry first
- UI clicks should be only the last fallback

---

## Suggested Development Order for Claude

Recommended order:

1. Chest relic selection
2. Events
3. Rest sites
4. Shop
5. Potions
6. Game Over
7. Character select / Embark

Also note these three cross-validation items:

- `EVENT -> embedded combat -> reward/map`
- `REST -> smith -> card selection -> confirm`
- `CHEST -> empty chest / relic pick -> proceed`

---

## Phase 6 Full-Flow Acceptance Criteria

If "Phase 6 complete" is to be declared in the future, at least the following must be met:

- Can start a run from the main menu
- Can cross the map, combat, rewards, events, rest sites, shop, and chests
- Can use or handle potions
- Can handle settlement and game over
- Does not depend on manual clicks to improvise operations
- Has end-to-end regression validation

Otherwise it can only count as "most of the main flow is wired through", not truly full coverage.
