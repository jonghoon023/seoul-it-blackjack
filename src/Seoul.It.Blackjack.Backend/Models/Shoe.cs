using Seoul.It.Blackjack.Core.Domain;
using System.Collections.Concurrent;

namespace Seoul.It.Blackjack.Backend.Models;

internal sealed class Shoe
{
    private readonly ConcurrentStack<Card> _cards;

    public Shoe(int deckCount)
    {
        List<Card> cards = [];
        for (int i = 0; i < deckCount; i++)
        {
            cards.AddRange(Deck.Cards);
        }

        Card[] cardArray = cards.ToArray();
        Random.Shared.Shuffle(cardArray);
        _cards = new ConcurrentStack<Card>(cardArray);
    }

    public Card Draw()
    {
        return TryDraw(out Card? card)
            ? card
            : throw new InvalidOperationException("Shoe 에 카드가 더 이상 없습니다.");
    }

    public bool TryDraw(out Card? card)
    {
        return _cards.TryPop(out card);
    }
}
