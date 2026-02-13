namespace Seoul.It.Blackjack.Core.Domain;

public sealed class Card
{
    public Card(Suit suit, Rank rank)
    {
        Suit = suit;
        Rank = rank;
    }

    public Suit Suit { get; }

    public Rank Rank { get; }
}
