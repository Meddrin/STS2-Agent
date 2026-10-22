# Evidence: satisfiability of the embedded contract

## Chain (verified)

`STS2AIAgent.csproj:32-33` (embeds only `SKILL.md` + `references/screen-playbooks.md`)
-> `PlayPrompt.cs:43-47` (`ReadEmbedded` + `ExtractSharedContract`)
-> `PlayPrompt.BuildPlaySystem()` (`:57-71`; `:62` lists tools, `:65/67` splice the contract and playbook)
-> `AgentLoop.cs:115-119` (rebuilds the system prompt every step). For the `instructions` of native MCP see `NativeMcpServer.cs:374`.

## The only hard unsatisfiable item

| Contract requirement | Line | In-game | Evidence |
|---|---|---|---|
| `Call health_check once at session start` | SKILL.md:52, 53 | ❌ | `AgentTools.Play` (`AgentTools.cs:70-78`) lacks that tool; `health_check` exists only in `AgentTools.Mcp` (`:80-83`, `:82`). After the call `AgentLoop.cs:547` returns `Unknown tool` |

Addendum: `PlayPrompt.cs:62` lists the in-game tool list (without health_check) in the same passage, contradicting the requirement.

## Similar item in the playbooks

`screen-playbooks.md:93`: debug `fight THE_ARCHITECT_EVENT_ENCOUNTER`. In-game there is no console tool,
`act` has no `command` slot (`AgentTools.cs:7-26`), and `run_console_command` is not in `available_actions`
(`GameStateService.cs:2528-2810`) => `AgentLoop.cs:593-603` judges it illegal.
It is feasible on the external MCP side (`ActionRequest.command` `GameActionService.cs:6623`;
registered when `STS2_ENABLE_DEBUG_ACTIONS=1`).

## The other contract requirements: each satisfiable (sampled and checked)

The guided loop, `wait_until_actionable`, the three data tools, the 9 compact id fields
(hand.card_id `GameStateService.cs:3601`; enemies[].enemy_id `3183`; run.deck[].card_ids `3734`;
relic_ids `3245`; the fields of selection/reward/shop/bundles/chest at `3298/3324/3696/3343/3542`),
the four values of `session.mode/phase` (`165-204`), `error.code/retryable/status_code` (`AgentErrorEnvelope.cs:21,39-46`),
`game_over.phase=summary_animating` (`4966-4970`), `unlock_type/items` (`3036-3043`),
`character_select.embark` (`4470`), `shop.open` (`3384`), enemy `intents[].damage/hits/total_damage`
and `powers` (`3190-3199`, `3137/3189`), `map.options[].i/local_vote/votes` (`3462-3481`),
`capstone.options[].i` (`1168-1185`): all exist.

## Routing action name check

The action names mentioned in contract lines 99-118 **all** exist in `BuildAvailableActionNames` (`2528-2810`).
Two non-hard gaps: the `MAIN_MENU` label comes from `NSubmenu`/`NLogoAnimation`/`NMainMenu` at the same time
(`6861-6863`) while `continue_run`/`switch_profile` recognize only `NMainMenu`; the routing section lacks the
`TIMELINE` (`6855`) and `CARDS_VIEW` (`6815-6818`) lines (covered by playbook 5-17).

## Doc-layer inconsistency (corroboration)

`README.md:183` and `README.zh-CN.md:183` both say "Built-in tools match in-game autoplay: health_check, ...",
which does not match `AgentTools.cs:70-78`.
