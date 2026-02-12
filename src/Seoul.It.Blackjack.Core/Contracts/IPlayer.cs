using Seoul.It.Blackjack.Core.Domain;

namespace Seoul.It.Blackjack.Core.Contracts;

public interface IPlayer
{
    public bool IsDealer { get; }

    public string Id { get; }

    public string Name { get; }

    public int Score { get; }

    public bool IsBusted { get; }

    public IEnumerable<Card> Cards { get; }
}
