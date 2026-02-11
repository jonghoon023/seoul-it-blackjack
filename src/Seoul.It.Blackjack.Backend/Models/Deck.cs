using Seoul.It.Blackjack.Core;
using Seoul.It.Blackjack.Core.Enums;

namespace Seoul.It.Blackjack.Backend.Models;

internal sealed class Deck
{
    public static IEnumerable<Card> Cards => Enum.GetValues<Suit>()
        .SelectMany(suit => Enum.GetValues<Rank>().Select(rank => new Card(suit, rank)));
}
