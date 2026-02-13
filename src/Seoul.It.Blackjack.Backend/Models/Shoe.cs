using Seoul.It.Blackjack.Core.Domain;
using System.Collections.Concurrent;

namespace Seoul.It.Blackjack.Backend.Models;

/// <summary>
/// 여러 덱을 섞어 카드 뽑기를 제공하는 카드 더미(Shoe)입니다.
/// </summary>
internal sealed class Shoe
{
    /// <summary>
    /// 카드를 실제로 보관하는 스택입니다.
    /// </summary>
    private readonly ConcurrentStack<Card> _cards;

    /// <summary>
    /// 지정한 덱 개수로 신발(Shoe)을 만들고 카드를 섞습니다.
    /// </summary>
    /// <param name="deckCount">포함할 덱 개수입니다.</param>
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

    /// <summary>
    /// 카드 한 장을 뽑습니다.
    /// 카드가 없으면 예외를 발생시킵니다.
    /// </summary>
    /// <returns>뽑힌 카드 한 장입니다.</returns>
    public Card Draw()
    {
        return TryDraw(out Card card)
            ? card
            : throw new InvalidOperationException("Shoe에 카드가 더 이상 없습니다.");
    }

    /// <summary>
    /// 카드 한 장 뽑기를 시도합니다.
    /// </summary>
    /// <param name="card">성공 시 뽑힌 카드가 담깁니다.</param>
    /// <returns>카드를 뽑으면 true, 없으면 false입니다.</returns>
    public bool TryDraw(out Card card)
    {
        if (_cards.TryPop(out Card? popped) && popped is not null)
        {
            card = popped;
            return true;
        }

        card = null!;
        return false;
    }
}
