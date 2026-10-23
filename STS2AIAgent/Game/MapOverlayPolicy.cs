namespace STS2AIAgent.Game;

/// <summary>
/// The map can open on top of a room the player has not finished, for example a shop when a co-op
/// partner votes for the next node. The map is then an overlay: closing it returns to that room.
/// The game offers the same through the map's back button, which it enables only in that case.
/// </summary>
internal static class MapOverlayPolicy
{
    /// <summary>The room under an open map, by the room's type name, or null when the map is the room.</summary>
    public static string? UnderlyingRoom(string? roomTypeName)
    {
        return roomTypeName switch
        {
            null or "" or "MapRoom" => null,
            "MerchantRoom" => "Shop",
            "RestSiteRoom" => "RestSite",
            "TreasureRoom" => "Treasure",
            "EventRoom" => "Event",
            "CombatRoom" => "Combat",
            _ => roomTypeName.EndsWith("Room", StringComparison.Ordinal) ? roomTypeName[..^"Room".Length] : roomTypeName
        };
    }

    public static bool CanClose(bool mapOpen, bool traveling, bool backButtonEnabled, string? underlyingRoom)
    {
        return mapOpen && !traveling && backButtonEnabled && underlyingRoom != null;
    }
}
