# ADR 0001: Keep one judgment for the action surface

Status: **Implemented** (2026-09-17, verified against a live-game replay)
Date: 2026-09-16, implemented 2026-09-17

## Background

`available_actions` in `/state` is produced by `GameStateService.BuildAvailableActionNames`;
the descriptors in `/actions/available` are produced by `GameStateService.BuildAvailableActionsPayload`.

The two are **two hand-written implementations of the same judgment**. A mechanical comparison of the source on 2026-09-16:

| | Lines | `Can*` predicates queried | Action names emitted |
| --- | --- | --- | --- |
| `BuildAvailableActionNames` | 301 | 50 | 55 |
| `BuildAvailableActionsPayload` | 609 | 50 | 55 |
| Intersection | n/a | **50 (identical)** | **55 (identical)** |

The sets are identical; only the emission order starts to differ after item 28. In other words, **910 lines of code answer the same question**,
and that they agree today is purely the result of careful manual upkeep, with no mechanism guaranteeing it.

The cost is concrete:

- Adding an action means editing two places, and the order, predicates, and parameters must all match. `AGENTS.md` and
  `.trellis/spec/mod/game-actions.md` both spell out "add it in both places" as a step: **the docs teach people to maintain the duplication**.
- A local contract was already written once for this: the only reason `CombatGate.ActionsAskTheSharedGate` exists is to keep these two
  methods from diverging on three combat predicates. That patches the duplication instead of removing it.
- Once they diverge, a client is told on one endpoint that an action is available and on the other that it is not. This is exactly the kind of defect that
  forced the same-number re-release of v0.12.4 (there, one response contradicted itself).

## Decision

Collapse the judgment into one place: a method that produces, per screen, a sequence of "action + parameter requirements".
`available_actions` takes its name projection, and `/actions/available` takes its full descriptors.

## Result of the implementation (2026-09-17)

`EnumerateAvailableActions` became the only place that decides. `BuildAvailableActionNames` went from 301 lines to a 12-line
pure projection, and `BuildAvailableActionsPayload` became a 14-line endpoint wrapper; `GameStateService.cs` went from 8559 to 8295 lines.

**Live replay comparison** (ADR step 3, isolated host `--clientId 2026091701`): 45 back-to-back sample pairs that walked all 12
screen types covered by the baseline.

- **0 divergences between the two surfaces**, and 0 mismatches between `screen` and `descriptor_screen`.
- **All 12 action sets match the baseline exactly**, including the **empty set** edge case of `PAUSE_MENU` (both sides are empty arrays).
- The only difference is that `CARD_SELECTION` lacks one `discard_potion`: in this run all 3 potion slots were empty, while the baseline run held potions.
  Checked against the live `run.potions`: a difference in game state, not in code.
- The baseline has no sets for `GAME_OVER`, because it predates the #142 fix; its action sets
  (`[continue_game_over]` / `[return_to_main_menu]`) are identical to what the baseline recorded at the time under `COMBAT`.

As planned, the emission order is now the descriptor side's order (the position of `crystal_*` changed). The sets are unchanged, and no client depends on order:
the skill and `state-invariants` only check membership.

The contract was rewritten accordingly: `ActionSurface.*` went from "two implementations stay consistent" to "both come from the same traversal, and neither has anywhere to put
its own predicates or action names". This is a strictly stronger promise: the old version could only discover a divergence after someone wrote one, and it covered only the predicates and names
it knew to compare.

## Why it was not done on 2026-09-16

These 910 lines decide what the agent is allowed to do. A mistake does not break one feature; it means **the whole agent loses the correct action surface**,
and this project's live validation can only be done by hand on a machine with the game installed: there is no automated live regression.

Rewriting it with offline evidence alone would repeat the v0.12.4 mistake: that was also an "obviously correct" refactor,
with 408 offline tests and all nine gates green, and the result was a 500 on every `/state` on the main menu.

So the bar for this work was not "think it through" but **"be able to run it against the live game"**.

## What was done in the meantime

Two source contracts, `ActionSurface.SameActionsOnBothSurfaces` and `ActionSurface.SamePredicatesOnBothSurfaces`,
pin the action-name set and the predicate set of the two surfaces to be equal. It **does not remove the duplication, it only makes it loud**:
add an action to one surface only and the test immediately turns red and names the one that was missed.

The emission order is **deliberately not compared**. The two differ today, and no client depends on it (the skill and `state-invariants`
only check membership). Pinning the order would only record a coincidence as a contract.

## How it was done

1. First capture a baseline on the live game: for each screen (combat / map / shop / rest / event / chest / character select / lobby / results /
   pause menu / timeline), record the full contents of `available_actions` and `/actions/available`.
2. Extract the unified enumeration method and make both surfaces read it.
3. Replay step 1 on the same machine and compare screen by screen. The sets must match; for any order change, explicitly confirm that no client depends on it.
4. Rewrite the two `ActionSurface.*` contracts as "both surfaces come from one source" instead of "two implementations stay consistent".
5. Change the "add an action" step in `AGENTS.md` and `.trellis/spec/mod/game-actions.md` from two places to one.

## Related

- Size ratchet: `STS2AIAgent.Tests/SourceShapeContractTests.cs` (the two files made up 49% of this mod's C# code)
- Architecture debt record: the "Code shape and its known debts" section of `.trellis/spec/mod/architecture.md`
