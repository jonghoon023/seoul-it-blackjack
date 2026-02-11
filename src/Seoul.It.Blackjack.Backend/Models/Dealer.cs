namespace Seoul.It.Blackjack.Backend.Models;

public class Dealer(string id) : Player("Dealer")
{
    public override bool IsDealer => true;
}
