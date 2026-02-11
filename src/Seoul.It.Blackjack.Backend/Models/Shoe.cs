using Seoul.It.Blackjack.Core;
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
        if (_cards.TryPop(out Card? card))
        {
            return card;
        }

        throw new InvalidOperationException("Shoe 에 카드가 더 이상 없습니다.");
    }
}
