namespace Seoul.It.Blackjack.Backend.Models;

internal class Dealer(string id) : Player(id, "Dealer")
{
    public override bool IsDealer => true;
}
