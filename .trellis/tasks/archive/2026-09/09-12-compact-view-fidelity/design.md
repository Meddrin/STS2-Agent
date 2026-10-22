# Design: additions to the compact view

## Landing point 1: combat (`BuildAgentCombatPayload`, `GameStateService.cs:3110-3168`)

- `player` gains `powers`: squash raw `combat.player.powers` into short lines (reuse the existing power line formatting;
  if there is no ready-made function, take the two fields `power_id` + `amount`).
- Each `enemies[]` gains:
  - `powers`: same as above;
  - `intents`: `intent_type` / `label` / `damage` / `hits` / `total_damage` / `status_card_count`.
- The data source is the raw `combat` variable (already held by `BuildAgentCombatPayload`); **no new reflection or game API is needed**.
- Go by the real field names at implementation time; if the raw shape differs from the examples in this file, give the original field names in the evidence.

## Landing point 2: id regression

| Location | Addition |
|---|---|
| `BuildAgentHandCardPayload` (`:3544-3559`) | `card_id` |
| `BuildAgentChoiceCardPayload` (`:3577-3584`) | `card_id` (choice screen / reward card) |
| `BuildAgentPricedCardPayload` (`:3635-3642`) | `card_id` (shop) |
| `BuildAgentCardStacks` (`:3663-3682`) | Each merged group exposes `card_id` and `count`; if a group may merge different ids, expose a `card_ids` array |
| Relics (`:3205-3207`) | `relic_id` |
| Chest relic options (`:3487-3491`) | `relic_id` |
| Card pack (bundle) options | Aligned with the raw `card_id` / `relic_id` |

## Landing point 3: overlays and teammates

- `BuildAgentModalPayload` (`:3502-3509`) gains `underlying_screen` (raw `modal.underlying_screen`).
- The compact top level (`:3003-3038`) gains `unlock` (reusing `BuildUnlockPayload`, `:4722-4736`).
- `BuildAgentCombatPayload` gains `players` (raw `:7607-7634`); `BuildAgentRunPayload` gains `players` (`:7636-7657`).

## Test strategy (GameStateService.cs is not in the test project, so only source contracts + reverse cross-checks)

Add `STS2AIAgent.Tests/CompactViewFidelityTests.cs` (registered by the main agent), each shaped like:

1. Use `AgentSourceFixture.MethodBody(Read("STS2AIAgent/Game/GameStateService.cs"), "BuildAgentCombatPayload")`
   to assert that `powers`, `intents`, `total_damage`, `players` appear;
2. For each added field name, assert it **also** appears at the corresponding type definition in raw (for example `total_damage` appears in the
   `CombatEnemyIntentPayload` section), turning "invented fields" red;
3. Assert that compact has not been dumped wholesale from raw (for example raw-only fields like `valid_target_indices` do not appear in
   `BuildAgentCombatPayload`).

After writing the assertions, self-check falsifiability: temporarily delete one added field -> the matching test turns red -> restore.

## Compatibility and rollback

- Purely additive fields; old consumers will not fail because of extra keys.
- Rollback = a single method-level diff, which can be reverted as a whole.
