# The reward card choice overlay's screen name is no longer shadowed

## Background

In `GameStateService.ResolveNonModalScreen` (`STS2AIAgent/Game/GameStateService.cs:6802-6866`),
the generic visible grid branch (E4, `:6820-6825`) sits before the switch:

```csharp
if (currentScreen is Node rootNode &&
    currentScreen is not NChooseABundleSelectionScreen &&
    GetVisibleGridCardHolders(rootNode).Count > 0)
{
    return "CARD_SELECTION";
}
```

while `NCardRewardSelectionScreen => "REWARD"` (`:6840`) is in the switch and can never be reached.

The confirmed chain of evidence:

- The reward card choice overlay is pushed with `NOverlayStack.Instance.Push(...)` (`extraction/.../NCardRewardSelectionScreen.cs:149`),
  and `ActiveScreenContext.GetCurrentScreen` `Peek()`s it (`ActiveScreenContext.cs:74-77`), so what E4 looks at is this overlay itself;
- In `_Ready` it does `NGridCardHolder.Create(nCard)` + `AddChildSafely` (`:156, 187-189`), with a non-null `CardModel`
  (`NGridCardHolder.cs:94-106`), and the animation only changes `modulate` (`:196-197`) without affecting `IsVisibleInTree()`;
- So E4 hits, and `/state.screen` reports `CARD_SELECTION`.

Consequences (verified): on this screen `available_actions` has only
`resolve_rewards`/`collect_rewards_and_proceed`/`choose_reward_card`/`skip_reward_cards`,
**without** `select_deck_card`/`confirm_selection`, and the `selection` section is null; yet
the Screen Routing at `skills/sts2-mcp-player/SKILL.md:105` says "`CARD_SELECTION`: use `select_deck_card`",
and `docs/api.md:1359/1377` also write the preconditions of the two reward actions as `screen="REWARD"`. A model that calls by screen name will only get 409.

This is not a new discovery: `.trellis/tasks/archive/2026-09/09-12-residual-audit-gaps/prd.md:41-43` and
`journal-1.md:346,384` both recorded it as a deferred item "to be separately evaluated and confirmed live".

## Goals

Add an early-return branch of the same shape as E1 before E4, so that the reward card choice overlay reports `REWARD`:
`UNLOCK` -> combat hand -> `CARDS_VIEW` -> **`REWARD` (new)** -> generic grid -> `MULTIPLAYER_LOBBY` -> `COMBAT`.

This is exactly the correct precedent already in the repo: `NUnlockCardsScreen` also contains visible `NGridCardHolder`s
(`NUnlockCardsScreen.cs:93`), gets `UNLOCK` by having E1 jump ahead of E4, and is
pinned by `STS2AIAgent.Tests/UnlockScreenContractTests.cs:21-33` with "the unlock branch must precede the visible grid branch".

## Acceptance criteria

- [x] In `ResolveNonModalScreen` the check for `NCardRewardSelectionScreen` precedes the generic grid branch.
- [x] A source contract test pins this order (the same assertion shape as `UnlockScreenContractTests`), and is falsifiable
      (moving the new branch after the generic branch -> red).
- [x] The values of the other shadowed arms are unchanged (`NChooseACardSelectionScreen`, the four `NDeck*Select`,
      `NCardGridSelectionScreen` all still resolve to `CARD_SELECTION`), with tests or assertions proving there is no collateral change.
- [x] Existing tests all green: `ScreenResolutionContractTests` (it parses only the arms after the switch, so it should be unaffected),
      `UnlockScreenContractTests`, and `api-facts` of `check_verification_gates.py` (consistency between the screen set and the docs).
- [x] The doc and script check conclusion is written into the evidence: the existing wording of `docs/api.md` and `SKILL.md` becomes **more accurate** after the change,
      so no doc change is needed; if any needing change is found, change it together and explain.
- [x] Full C# / MCP tests, gates, and preflight pass.

## Out of scope

- `NCardPileScreen` (viewing a pile) and `NCardLibrary` (the compendium) have no matching switch arm, and changing them would degrade the screen name to
  `UNKNOWN` (the skill treats UNKNOWN as transient, which is worse). This round only records evidence and does not touch them.
- Do not change `GetDeckSelectionOptions` (the previous round already deleted its generic fallback).
