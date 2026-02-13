using Seoul.It.Blackjack.Core.Domain;

namespace Seoul.It.Blackjack.Frontend.Extensions;

public static class CardExtensions
{
    public static string ToAssetPath(this Card card)
    {
        string suit = card.Suit.ToString().ToLowerInvariant();
        string rank = card.Rank.ToString().ToLowerInvariant();
        return $"cards/{suit}_{rank}.svg";
    }
}
