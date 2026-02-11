using Seoul.It.Blackjack.Core;
using Seoul.It.Blackjack.Core.Enums;

namespace Seoul.It.Blackjack.Backend.Models;

public class Player(string name)
{
    public List<Card> Cards { get; } = [];

    public virtual bool IsDealer => false;

    public string Name => name;

    public int Score => Cards.Sum(card => card.Rank.ToValue());
}
