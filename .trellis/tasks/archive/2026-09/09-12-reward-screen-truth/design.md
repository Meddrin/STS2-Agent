# Design

## Change (single point, 4 lines)

In `ResolveNonModalScreen` of `GameStateService.cs`, insert after the `CARDS_VIEW` early return and before the generic grid early return:

```csharp
// The reward-card overlay carries visible grid card holders, so the generic grid branch below
// would name it CARD_SELECTION and send the model after select_deck_card, an action this screen
// deliberately does not offer. Its own switch arm (REWARD) can only be reached from here, the
// same way NUnlockScreen claims UNLOCK ahead of the same generic branch.
if (currentScreen is NCardRewardSelectionScreen)
{
    return "REWARD";
}
```

## Tests

Create `STS2AIAgent.Tests/RewardScreenContractTests.cs` (registered by the main agent), asserting:

1. Within the method body of `ResolveNonModalScreen`, the index of `NCardRewardSelectionScreen` is **less than**
   the index of the generic grid criterion (`GetVisibleGridCardHolders`);
2. Between the two there is `return "REWARD";` (using Contains after `WithoutWhitespace`, to avoid whitespace sensitivity);
3. After the generic grid branch, the arms of `NChooseACardSelectionScreen` and `NCardGridSelectionScreen` still resolve to
   `CARD_SELECTION` (preventing someone from lifting them to the front along the way and changing semantics).

Items 1 and 2 share the source and shape of `UnlockScreenContractTests`; follow its style rather than inventing a new pattern.

## Risks and live items

- This is a flip of the `screen` semantics of **one overlay state**: `CARD_SELECTION` -> `REWARD`.
  Offline can only prove "the code order is correct and existing tests do not go red"; the real impact surface (skill routing, the `docs/api.md` contract,
  the reward-clearing loop at `scripts/run_sts2_validation.py:1332-1345`) needs live confirmation.
- The `api-facts` gate re-extracts the screen names of early-return branches; `REWARD` is already in the screen enum of `docs/api.md`, so it will not go red
  (confirm with one `--only api-facts` run at implementation time).
