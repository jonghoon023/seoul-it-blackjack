using Seoul.It.Blackjack.Core.Contracts;

namespace Seoul.It.Blackjack.Backend.Models;

public class Dealer(string id) : Player(id, "Dealer")
{
    public override bool IsDealer => true;
}
