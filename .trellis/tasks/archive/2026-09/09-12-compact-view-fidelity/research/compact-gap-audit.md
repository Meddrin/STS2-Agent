# Evidence: field gaps in the compact view

## Conclusion

The compact view (`BuildAgentViewPayload`, `GameStateService.cs:2973-3040`) is not a subset trim of raw
but a textual rewrite. The following decision-critical information does not exist in compact, while raw has already computed it:

1. **Powers of both sides missing**: compact's `player` (`:3116-3128`) and `enemies` (`:3154-3165`) both lack powers;
   raw has `CombatPlayerPayload.powers` (`:7586`) and `CombatEnemyPayload.powers` (`:7754`),
   filled by `BuildCreaturePowerPayloads` (`:5042`).
2. **Enemy intent numbers missing**: compact keeps only `intent` / `move_id`; raw's `CombatEnemyIntentPayload`
   (`:7760-7778`) carries `intent_type/label/damage/hits/total_damage/status_card_count`,
   computed by `BuildEnemyIntentPayload` (`:5159-5187`). The only aggregate in compact is
   `combat.lethal_risks[].incoming_damage` (`:2868-2890`) and `end_turn_will_kill_player`.
3. **Card ids all missing**: all `card_id` assignment points are in the raw builders (`:3831/4987/5387/5667/5747/5770`);
   the compact builders `BuildAgentHandCardPayload` (`:3544-3559`), `BuildAgentChoiceCardPayload` (`:3577-3584`),
   and `BuildAgentPricedCardPayload` (`:3635-3642`) all lack it; the deck is also merged by `BuildAgentCardStacks` (`:3663-3682`)
   by `GroupKey` (`:8058`).
4. **Relics have only names**: `:3205-3207`; raw's `RunRelicPayload` (`:8001-8014`) carries
   `relic_id/description/stack/is_melted`. Same for chests: raw `ChestRelicOptionPayload.relic_id` (`:7302`) ->
   compact has only `"name [rarity]"` (`:3487-3491`).
5. **Overlay lacks `underlying_screen`**: raw `ModalPayload.underlying_screen` (`:7833`, filled at `:4866`) ->
   compact `BuildAgentModalPayload` (`:3502-3509`) has none.
6. **The whole `unlock` block is dropped**: raw `GameStatePayload.unlock` (`:6925`, `BuildUnlockPayload` `:4722-4736`);
   the compact top level (`:3003-3038`) lacks the key.
7. **Teammate summaries missing**: raw `CombatPayload.players` (`:7607-7634`) and `RunPayload.players` (`:7636-7657`);
   compact `BuildAgentCombatPayload` / `BuildAgentRunPayload` both lack them.

## Why it matters

- `skills/sts2-mcp-player/SKILL.md:55-58`, `:71-77` require "look up metadata before deciding", and metadata queries match by id
  (`GameDataFilter.FindItem`); compact has no ids => that workflow fails for cards / relics.
- `SKILL.md:83` "Resolve overlays before room flow" needs to know the room beneath the overlay.
- Missing combat numbers => cannot compute the lethal line, cannot allocate block by intent; `lethal_risks` only gives the sum over all intents.

## Reverse (in compact but not in raw)

`glossary`, `profiles`, `actions`, the `draw/discard/exhaust` text stacks, `combat.hand[].can_play_result`
(`:3549`): compact has its own processing and is not a pure trim; this change is additive only and leaves these untouched.
