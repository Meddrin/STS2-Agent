using STS2AIAgent.Game;

namespace STS2AIAgent.Tests;

/// <summary>
/// The map can open over an unfinished room (seen live: a co-op partner voted while this player
/// was still in a shop). close_map_screen goes back to that room; the state names the room.
/// Upgrade screens show what each card becomes, and shop and chest relics carry their description.
/// </summary>
internal static class MapOverlayAndPreviewTests
{
    public static void UnderlyingRoomNamesTheRoomUnderTheMap()
    {
        Assert.Equal("Shop", MapOverlayPolicy.UnderlyingRoom("MerchantRoom"));
        Assert.Equal("RestSite", MapOverlayPolicy.UnderlyingRoom("RestSiteRoom"));
        Assert.Equal("Treasure", MapOverlayPolicy.UnderlyingRoom("TreasureRoom"));
        Assert.Equal("Event", MapOverlayPolicy.UnderlyingRoom("EventRoom"));
        Assert.Equal("Combat", MapOverlayPolicy.UnderlyingRoom("CombatRoom"));
        Assert.Equal("Ancient", MapOverlayPolicy.UnderlyingRoom("AncientRoom"));
        Assert.True(MapOverlayPolicy.UnderlyingRoom("MapRoom") == null, "the map room is not an overlay");
        Assert.True(MapOverlayPolicy.UnderlyingRoom(null) == null, "no room means no overlay");
    }

    public static void MapClosesOnlyOverARoomWithItsBackButtonEnabled()
    {
        Assert.True(MapOverlayPolicy.CanClose(mapOpen: true, traveling: false, backButtonEnabled: true, underlyingRoom: "Shop"));
        Assert.False(MapOverlayPolicy.CanClose(mapOpen: true, traveling: false, backButtonEnabled: false, underlyingRoom: "Shop"));
        Assert.False(MapOverlayPolicy.CanClose(mapOpen: true, traveling: true, backButtonEnabled: true, underlyingRoom: "Shop"));
        Assert.False(MapOverlayPolicy.CanClose(mapOpen: true, traveling: false, backButtonEnabled: true, underlyingRoom: null));
        Assert.False(MapOverlayPolicy.CanClose(mapOpen: false, traveling: false, backButtonEnabled: true, underlyingRoom: "Shop"));
    }

    public static void CloseMapScreenIsWiredEndToEnd()
    {
        var actions = AgentSourceFixture.ReadActionService();
        var state = AgentSourceFixture.ReadStateService();
        Assert.Contains("\"close_map_screen\" => ExecuteCloseMapScreenAsync()", actions, StringComparison.Ordinal);
        Assert.Contains("mapScreen.Close();", actions, StringComparison.Ordinal);
        Assert.Contains("name = \"close_map_screen\"", state, StringComparison.Ordinal);
        Assert.Contains("underlying_room = GetMapUnderlyingRoom(runState)", state, StringComparison.Ordinal);
        Assert.Contains("underlying_room = map.underlying_room", state, StringComparison.Ordinal);
    }

    public static void UpgradeScreensAndRelicsCarryWhatTheModelNeeds()
    {
        var state = AgentSourceFixture.ReadStateService();
        Assert.Contains("CardUpgradePreview.For(card, NormalizeCardRulesText)", state, StringComparison.Ordinal);
        Assert.Contains("upgrade_rules_text = upgrade?.RulesText", state, StringComparison.Ordinal);
        Assert.Contains("upgrades = selection.cards", state, StringComparison.Ordinal);
        Assert.Contains("description = relic.description", state, StringComparison.Ordinal);
        var preview = AgentSourceFixture.Read("STS2AIAgent/Game/CardUpgradePreview.cs");
        // The preview must come from a detached copy: never upgrade the card in the deck.
        Assert.Contains("ModelDb.GetById<CardModel>(card.Id).ToMutable()", preview, StringComparison.Ordinal);
        Assert.False(preview.Contains("card.UpgradeInternal", StringComparison.Ordinal), "the deck card must not be upgraded");
    }
}
