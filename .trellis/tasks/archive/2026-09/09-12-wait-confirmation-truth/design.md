# Design: observable success

## Rules (written as pure policy, no longer scattered across wait loops)

1. **Success only when the target state is observed**: a failing wait has only two honest endings: "the target was observed" or "timed out unconfirmed".
2. **A vanished source node only means losing the ability to observe**: at that point it must go back to the "current active screen" and judge again, instead of declaring success outright.
3. **Success only when the requested value matches the observed value**: a boolean target like `ready` must report unconfirmed when the source node vanishes.

## Landing points

### 1. `STS2AIAgent/Game/MenuTransitionPolicy.cs` (pure policy, already in the test project compile list)

Add two testable judgments:

```csharp
// Whether the target submenu was observed: the observed screen type must be an instance of the target type (including subclasses)
public static bool IsSubmenuObserved(Type? observedScreenType, Type targetSubmenuType);

// Whether the boolean target was observed: the source node must still be valid, and the observed value must match the requested value
public static bool IsFlagObserved(bool sourceNodeValid, bool observedValue, bool requestedValue);
```

Semantic requirements: when `sourceNodeValid == false` `IsFlagObserved` must return `false`;
when `observedScreenType == null` `IsSubmenuObserved` must return `false`.

### 2. `STS2AIAgent/Game/GameActionService.cs`

- `WaitForMainMenuSubmenuOpenAsync<TSubmenu>` (:5609-5630):
  change the loop body to: run the existing `props` check each frame first; when `!IsInstanceValid(screen)` **break** (no longer `return true`),
  and after the loop ends uniformly `return MenuTransitionPolicy.IsSubmenuObserved(ActiveScreenContext.Instance.GetCurrentScreen()?.GetType(), typeof(TSubmenu));`
  Requirement: when the node vanishes but the target has really become the active screen it **still returns true** (no false pending introduced).
- `WaitForCharacterSelectionTransitionAsync` (:5826+): likewise replace `return true;` with break +
  a uniform final judgment (the final judgment is "current character id == target id", which cannot be judged when the node vanishes -> false).
- `WaitForLobbyReadyTransitionAsync` (:5879-5894): when the node vanishes no longer `return ready;`,
  and after the loop ends uniformly `return MenuTransitionPolicy.IsFlagObserved(IsInstanceValid(screen), ..., ready)`.
  Note that before reading `screen.Lobby.LocalPlayer.isReady` one must still check `IsInstanceValid` first (Godot throws on access after an object is destroyed).
- `select_deck_card` (:2028+): after computing `TryGetCombatHandSelectionMetadata` /
  `TryGetCardGridSelectionMetadata`, if neither holds (that is, there is no clickable target),
  throw `ApiException(409, "invalid_action", ...)`, with details carrying `action/screen/option_index/option_count`.
  First confirm with the current state of `CanSelectDeckCard` and `GetDeckSelectionOptions` that this path is really reachable (see research).
- `confirm_timeline_overlay` (:702+): the three branches (tutorial / unlock / other overlays) re-fetch values **before each click**
  using the matching CanXxx/Getter of `GameStateService`; if they cannot be fetched, throw 409/503; after the click keep using the existing
  `WaitFor...` confirmation, and return `pending` if unconfirmed.
- `crystal_set_tool` (:1607-1645): first read the implementation of `GameStateService.TrySetCrystalSphereTool`;
  if its return value already implies "current tool == requested tool", keep `completed` and write the evidence in a comment;
  otherwise read back the current tool after the click, and if it has not become the target value, `status:"pending"` (do not falsely report completed).
- `run_console_command` (:4924-5018): `status = stable && !consoleTimedOut ? "completed" : "pending"`;
  sync the `stable` field (it must not be `stable:true` after a timeout).

### 3. Tests `STS2AIAgent.Tests/`

- The truth table of the new `MenuTransitionPolicy` judgments (including `sourceNodeValid=false`, type mismatch, subclass match, null).
- Source contract assertions (`AgentSourceFixture`): inside the loop bodies of the three wait helpers `IsInstanceValid(screen))s*{?s*return true` and similar
  **no longer appears**; the status expression of `run_console_command` contains
  `consoleTimedOut`. After writing the assertions, self-check falsifiability (changing the code back to the old form should turn red immediately).
