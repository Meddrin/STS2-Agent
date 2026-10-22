# Make the compact view carry the fields decisions actually need

## Background

The in-game Agent reads only the compact view at each step (the `agent_view` of `get_game_state`, built by
`GameStateService.BuildAgentViewPayload`, `STS2AIAgent/Game/GameStateService.cs:2973`).
It is **not a field subset of raw** but a "textual rewrite": several pieces of information essential for decisions are replaced by human-readable strings or dropped outright,
while raw holds the same data in full. The concrete losses proven so far:

| Lost content | Compact location | What raw has |
|---|---|---|
| Powers of both sides (Strength / Vulnerable / Weak / Thorns / Ritual...) | Neither `:3116-3128` (player) nor `:3154-3165` (enemies) has them | `CombatPlayerPayload.powers` (`:7586`), `CombatEnemyPayload.powers` (`:7754`), filled by `BuildCreaturePowerPayloads` (`:5042`) |
| Enemy intent numbers | Only `intent` (= moveId string) and `move_id` remain | `CombatEnemyPayload.intents[]` (`:7760-7778`: `intent_type/label/damage/hits/total_damage/status_card_count`) |
| Card id (hand / deck / reward / shop / choice / card pack) | `BuildAgentHandCardPayload` (`:3544-3559`) and others have only `i/line/selected/keywords/mods` | `:3831/4987/5387/5667/5747/5770` all have `card_id` |
| Relic id | `:3205-3207` has only names | `RunRelicPayload.relic_id` (`:8001-8014`) |
| Screen beneath an overlay | `BuildAgentModalPayload` (`:3502-3509`) has none | `ModalPayload.underlying_screen` (`:7833`) |
| Unlock payload | compact top level has no `unlock` key | `GameStatePayload.unlock` (`:6925`, `BuildUnlockPayload` `:4722-4736`) |
| Teammate HP lines | compact combat / run have no `players` | `CombatPlayerSummaryPayload` / `RunPlayerSummaryPayload` (`:7607-7657`) |

Consequences (fatal for auto-play):

- It cannot compute the lethal line, does not know how much to block, and cannot read Strength / Vulnerable, so combat decisions degrade into "guessing from memory";
- `skills/sts2-mcp-player/SKILL.md:55-58`, `:71-77` require "look up metadata before deciding", and metadata queries match by id exactly
  (`GameDataFilter.FindItem` goes by the id key), but under compact the model **has no card / relic ids at all**;
- Under an overlay it does not know which room it is in, which conflicts with the "resolve overlays first" rule at `SKILL.md:83`.

## Goals

On the premise of **not turning compact back into raw**, restore the information that is "essential for decisions and already computed in raw":

1. `combat.player.powers` and `combat.enemies[].powers` (reuse the raw power shape, kept compact).
2. `combat.enemies[].intents[]` (numbers: `damage` / `hits` / `total_damage` / `intent_type` / `label`).
3. id regression: hand, deck (each merged group must expose which cards it represents), reward cards, shop cards, choice cards, card packs, relics, chest relic options.
4. `modal.underlying_screen`, the `unlock` section at the compact top level, `combat.players` and `run.players` (teammate HP lines).

## Acceptance criteria

- [x] All the fields above appear inside the matching compact builder method bodies (source contract tests assert each one).
- [x] Each added field name can be traced to its source in a raw payload of the same kind (tests assert the same-named field exists on the raw side, preventing "invented fields").
- [x] Compact size is controlled: powers use short lines, intents carry only numbers and type, and raw's whole structures and full card piles are not dumped in.
- [x] Docs in sync: the agent_view description in `docs/api.md` and the related paragraphs of `skills/sts2-mcp-player/SKILL.md` are updated accordingly;
      the two markers `<!-- BEGIN SHARED PLAY CONTRACT -->` / `<!-- END SHARED PLAY CONTRACT -->` themselves are untouched
      (the file is embedded at mod build time, and the markers decide the boundaries of the embedded slice).
- [x] The action contract block of `docs/api.md` (55 action names) stays verbatim unchanged.
- [x] Full C# and MCP tests, `check_verification_gates.py`, `check_release_package.py`, and preflight pass.

## Out of scope

- Do not change any raw field (existing consumers of `/state` are unaffected).
- Do not change the screen name semantics of `ResolveNonModalScreen`.
- Do not introduce new state endpoints or version number changes.
