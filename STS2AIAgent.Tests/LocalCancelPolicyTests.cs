using STS2AIAgent.Game;

namespace STS2AIAgent.Tests;

/// <summary>
/// A co-op client that cancels a synchronized action on its own (for example a partner's card that
/// happens to run while this client waits on its own card's choice) diverges from the host. Seen
/// live on 2026-10-22: four StateDivergence kicks, each right after the AI played a choice card.
/// </summary>
internal static class LocalCancelPolicyTests
{
    public static void CoopNeverCancelsLocally()
    {
        Assert.False(LocalCancelPolicy.MayCancelRunningAction(isMultiplayer: true, awaitingPlayerChoice: false));
        Assert.False(LocalCancelPolicy.MayCancelRunningAction(isMultiplayer: true, awaitingPlayerChoice: true));
    }

    public static void SoloCancelsOnlyAStuckAction()
    {
        Assert.True(LocalCancelPolicy.MayCancelRunningAction(isMultiplayer: false, awaitingPlayerChoice: false));
        Assert.False(LocalCancelPolicy.MayCancelRunningAction(isMultiplayer: false, awaitingPlayerChoice: true));
    }

    public static void ACardWaitingForAChoiceIsNotRecovered()
    {
        Assert.False(LocalCancelPolicy.ShouldRecoverStuckPlay(playSettled: false, awaitingPlayerInput: true));
        Assert.False(LocalCancelPolicy.ShouldRecoverStuckPlay(playSettled: true, awaitingPlayerInput: false));
        Assert.True(LocalCancelPolicy.ShouldRecoverStuckPlay(playSettled: false, awaitingPlayerInput: false));
    }

    public static void BothCancelPathsAskThePolicy()
    {
        var actionSource = AgentSourceFixture.ReadActionService();
        var cancelBody = Squash(MethodBody(actionSource, "internal static bool TryCancelRunningPlayerAction()"));
        var playCardBody = Squash(MethodBody(actionSource, "private static async Task<ActionResponsePayload> ExecutePlayCardAsync("));

        Assert.Contains("LocalCancelPolicy.MayCancelRunningAction(isMultiplayer,awaitingChoice)", cancelBody, StringComparison.Ordinal);
        Assert.Contains("netService.Type.IsMultiplayer()", cancelBody, StringComparison.Ordinal);
        Assert.Contains("LocalCancelPolicy.ShouldRecoverStuckPlay(stable,IsPlayCardAwaitingPlayerInput())", playCardBody, StringComparison.Ordinal);
        // The policy check comes before any cancel call.
        Assert.True(
            cancelBody.IndexOf("LocalCancelPolicy.MayCancelRunningAction", StringComparison.Ordinal)
                < cancelBody.IndexOf("running.Cancel()", StringComparison.Ordinal),
            "TryCancelRunningPlayerAction must ask LocalCancelPolicy before cancelling anything.");
    }

    private static string MethodBody(string source, string signature)
    {
        var start = source.IndexOf(signature, StringComparison.Ordinal);
        Assert.True(start >= 0, "missing method: " + signature);
        var open = source.IndexOf('{', start);
        var depth = 0;
        for (var i = open; i < source.Length; i++)
        {
            if (source[i] == '{') depth++;
            else if (source[i] == '}' && --depth == 0) return source.Substring(open, i - open + 1);
        }

        throw new InvalidOperationException("unterminated method: " + signature);
    }

    private static string Squash(string text)
    {
        return new string(text.Where(c => !char.IsWhiteSpace(c)).ToArray());
    }
}
