# Evidence

## Deliverables

| File | Change |
|---|---|
| `STS2AIAgent/Game/GameStateService.cs` | Only incremental field additions inside the compact builder methods + the `FormatPowerLine` helper + new fields on `AgentCardDescriptor` |
| `STS2AIAgent.Tests/CompactViewFidelityTests.cs` (new) | 4 source contracts + raw same-name cross-check assertions |
| `docs/api.md` | New section `### Additional fields of the compact agent_view` (the action contract block untouched) |
| `skills/sts2-mcp-player/SKILL.md` | Quick Start item 3, the overlay rule, the `COMBAT` entry, the `UNLOCK` entry (the two markers are still at lines 48 / 141) |

### Added fields (compact key -> same source in raw)

`unlock` (top level), `combat.player.powers`, `combat.players[]`, `combat.enemies[].powers`,
`combat.enemies[].intents[]` (`i/intent_type/label/damage/hits/total_damage/status_card_count`),
`run.relic_ids`, `run.players[]`, `chest.relics[].relic_id`, `modal.underlying_screen`,
`card_id` for hand / choice / reward / card pack / shop, `card_ids` for deck and pile merged groups, the helper `FormatPowerLine`.

Two deliberate backward-compatibility trade-offs: `run.relics` is still the array of names, with a new `relic_ids` of the same order and length;
merged groups expose a deduplicated ascending `card_ids` rather than a single `card_id` (the `*N` count is already in `line`).

## Acceptance

| Criterion | Evidence |
|---|---|
| Fields enter compact | `PASS CompactViewFidelity.Powers / IntentNumbers / CardAndRelicIds / OverlayAndParty` |
| Each field has the same source in raw | The tests assert for each added key that the same name appears on the raw side (preventing invented fields) |
| Size is controlled | Only short lines and numbers were added; whole raw structures were not carried over (raw-only fields like `valid_target_indices` did not enter compact) |
| Docs in sync | New section in `docs/api.md`; SKILL.md updated in four places with the markers intact |
| Falsifiable | Delete 4 representative fields one by one (enemy `powers`, `intent.total_damage`, choice `card_id`, `modal.underlying_screen`) -> the matching tests all red -> restore all green |

## Gates

C# 335 PASS / 0 FAIL; `dotnet build` 0 warnings 0 errors; MCP 167 OK;
`check_verification_gates.py` (including api-facts), `check_release_package.py`, and preflight all green.

## Live-only verification

Whether the powers / intents values in a real fight are correct and non-empty, the degraded shapes of multi-hit attacks and buff-type intents,
whether the index spaces of `players[]` and `target_index` align in a multiplayer run, whether `run.relic_ids` and `run.relics` are strictly in the same order and length,
whether merged-group `card_ids` cover the boundary of same-named cards with different ids, and the value of `modal.underlying_screen` under each overlay.
