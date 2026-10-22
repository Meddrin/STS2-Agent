# Phase 5 full-chain completion

Last updated: `2026-03-11`

This document covers only the capabilities added in Phase 5: character select, potions, blocking modals, and Game Over wrap-up. The complete base protocol is still defined by [api.md](../docs/api.md).

---

## New state

### `character_select`

Present only when `screen = CHARACTER_SELECT`.

| Field | Type | Description |
| --- | --- | --- |
| `selected_character_id` | string \| null | Character currently selected by the local player |
| `can_embark` | boolean | Whether the start button can currently be clicked |
| `local_ready` | boolean | Whether the local player is ready |
| `is_waiting_for_players` | boolean | Whether it is still waiting for other players / a UI transition |
| `ascension` | number | Current ascension level |
| `max_ascension` | number | Highest ascension level currently selectable |
| `characters[]` | object[] | Visible character list |

`character_select.characters[]`

| Field | Type | Description |
| --- | --- | --- |
| `index` | number | The `option_index` used by `select_character` |
| `character_id` | string | Internal character ID |
| `name` | string | Character display name |
| `is_locked` | boolean | Whether the character is locked |
| `is_selected` | boolean | Whether this is the currently selected character |
| `is_random` | boolean | Whether this is the random slot |

### `modal`

Present only when a blocking modal or FTUE exists; in that case `screen = MODAL`, and the executable actions narrow to the modal actions.

| Field | Type | Description |
| --- | --- | --- |
| `type_name` | string | Godot node type name |
| `underlying_screen` | string \| null | The original screen below the modal, such as `MAP` / `CHARACTER_SELECT` |
| `can_confirm` | boolean | Whether `confirm_modal` can be executed |
| `can_dismiss` | boolean | Whether `dismiss_modal` can be executed |
| `confirm_label` | string \| null | Text of the confirm button |
| `dismiss_label` | string \| null | Text of the cancel / close button |

### `game_over`

Present only when `screen = GAME_OVER`.

| Field | Type | Description |
| --- | --- | --- |
| `is_victory` | boolean | Whether this is a victory result |
| `floor` | number \| null | Floor reached |
| `character_id` | string \| null | Character ID of this run |
| `can_continue` | boolean | Whether the UI can still page forward / enter the summary |
| `can_return_to_main_menu` | boolean | Whether `return_to_main_menu` can be executed |
| `showing_summary` | boolean | Whether the summary page is being shown |

### `run.potions[]` incremental fields

| Field | Type | Description |
| --- | --- | --- |
| `usage` | string \| null | How the potion is used |
| `target_type` | string \| null | Target type |
| `is_queued` | boolean | Whether it is already queued for use |
| `requires_target` | boolean | Whether `target_index` must be provided |
| `can_use` | boolean | Whether it can be used now |
| `can_discard` | boolean | Whether it can be discarded now |

---

## New actions

### `select_character`

- Precondition: `screen = CHARACTER_SELECT`
- Parameters: `option_index` is required, corresponding to `character_select.characters[]`
- Failure cases: index out of range, character locked, button unavailable

Request example:

```json
{ "action": "select_character", "option_index": 0 }
```

### `embark`

- Precondition: `screen = CHARACTER_SELECT` and `character_select.can_embark = true`
- Parameters: none
- Stable condition: entering the map, opening a blocking modal, or the lobby state clearly advancing

```json
{ "action": "embark" }
```

### `use_potion`

- Precondition: the potion slot holds a potion, and the matching `run.potions[i].can_use = true`
- Parameters: `option_index` is required; when `requires_target = true`, `target_index` is also required
- Currently supported: untargeted potions and single-enemy-target potions

```json
{ "action": "use_potion", "option_index": 1 }
{ "action": "use_potion", "option_index": 0, "target_index": 0 }
```

### `discard_potion`

- Precondition: the matching `run.potions[i].can_discard = true`
- Parameters: `option_index` is required

```json
{ "action": "discard_potion", "option_index": 2 }
```

### `confirm_modal`

- Precondition: `screen = MODAL` and `modal.can_confirm = true`
- Parameters: none

```json
{ "action": "confirm_modal" }
```

### `dismiss_modal`

- Precondition: `screen = MODAL` and `modal.can_dismiss = true`
- Parameters: none

```json
{ "action": "dismiss_modal" }
```

### `return_to_main_menu`

- Precondition: `screen = GAME_OVER` and `game_over.can_return_to_main_menu = true`
- Parameters: none

```json
{ "action": "return_to_main_menu" }
```

---

## Recommended flows

### Start of a run

```text
CHARACTER_SELECT
  -> select_character
  -> embark
  -> MODAL? -> confirm_modal / dismiss_modal
  -> MAP
```

### Potions in combat

```text
COMBAT
  -> check run.potions[]
  -> use_potion / discard_potion
  -> continue with play_card / end_turn
```

### Wrap-up

```text
GAME_OVER
  -> check game_over
  -> return_to_main_menu
```

---

## Live acceptance focus

1. Does `selected_character_id` update immediately after character select?
2. After `embark`, does it possibly enter `MODAL` first, then `MAP`?
3. When an FTUE / tutorial modal appears, are normal actions correctly blocked, exposing only `confirm_modal` / `dismiss_modal`?
4. Do single-enemy-target potions require and correctly consume `target_index`?
5. After a potion is used, do `occupied` / `is_queued` / `can_use` in `run.potions[]` change in sync?
6. Does `return_to_main_menu` reliably leave `GAME_OVER` without getting stuck on the summary page?
