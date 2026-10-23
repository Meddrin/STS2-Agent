using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace STS2AIAgent.Game;

/// <summary>
/// What a card becomes after its next upgrade, for upgrade selection screens (rest-site Smith,
/// events, combat hand upgrades). The text is read from a detached canonical copy, the same way the
/// upstream /data/cards export does it: the card in the deck is never touched, so nothing here can
/// change game state on one co-op client only. Enchantments on the deck card are not reflected.
/// Results are cached per card id and level so polling does not create a copy every frame.
/// </summary>
internal static class CardUpgradePreview
{
    internal readonly record struct Preview(string RulesText, int EnergyCost);

    private static readonly Dictionary<(string Id, int Level), Preview?> Cache = new();

    public static Preview? For(CardModel card, Func<string, string> normalizeRulesText)
    {
        try
        {
            if (card.CurrentUpgradeLevel >= card.MaxUpgradeLevel)
            {
                return null;
            }

            var key = (card.Id.Entry, card.CurrentUpgradeLevel);
            if (Cache.TryGetValue(key, out var cached))
            {
                return cached;
            }

            var upgraded = ModelDb.GetById<CardModel>(card.Id).ToMutable();
            for (var level = 0; level <= card.CurrentUpgradeLevel; level++)
            {
                upgraded.UpgradeInternal();
            }

            upgraded.FinalizeUpgradeInternal();
            var text = normalizeRulesText(upgraded.GetDescriptionForPile(PileType.None));
            Preview? preview = new Preview(text, upgraded.EnergyCost.GetWithModifiers(CostModifiers.All));
            Cache[key] = preview;
            return preview;
        }
        catch
        {
            // A failed preview reads as "no preview", never as a wrong one.
            return null;
        }
    }
}
