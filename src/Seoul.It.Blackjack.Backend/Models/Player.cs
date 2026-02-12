using Seoul.It.Blackjack.Core.Contracts;
using Seoul.It.Blackjack.Core.Domain;

namespace Seoul.It.Blackjack.Backend.Models;

internal class Player(string id, string name) : IPlayer
{
    private readonly List<Card> _cards = [];

    public virtual bool IsDealer => false;

    public string Id => id;

    public string Name => name;

    public int Score => Cards.Sum(card => card.Rank.ToValue());

    public bool IsBusted => Score > 21;

    public IEnumerable<Card> Cards => [.. _cards];
}
