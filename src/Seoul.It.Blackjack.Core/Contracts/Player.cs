using Seoul.It.Blackjack.Core.Domain;

namespace Seoul.It.Blackjack.Core.Contracts;

public class Player(string id, string name)
{
    public string Id => id;

    public virtual bool IsDealer => false;

    public string Name => name;

    public int Score => Cards.Sum(card => card.Rank.ToValue());

    public bool IsBusted => Score > 21;

    public List<Card> Cards { get; } = [];
}
