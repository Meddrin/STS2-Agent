namespace STS2AIAgent.Game;

/// <summary>
/// When the mod may cancel a running game action by itself. In a co-op run every client executes
/// the same synchronized action queue and checksums the result after each action. Cancelling an
/// action on one client only (whoever owns it) drops it from that client's history, so the host's
/// next checksum pairs with a different action and the client is kicked with StateDivergence. A
/// card that waits for its owner's choice is not stuck either: the next select action answers it.
/// </summary>
internal static class LocalCancelPolicy
{
    public static bool MayCancelRunningAction(bool isMultiplayer, bool awaitingPlayerChoice)
    {
        return !isMultiplayer && !awaitingPlayerChoice;
    }

    public static bool ShouldRecoverStuckPlay(bool playSettled, bool awaitingPlayerInput)
    {
        return !playSettled && !awaitingPlayerInput;
    }
}
