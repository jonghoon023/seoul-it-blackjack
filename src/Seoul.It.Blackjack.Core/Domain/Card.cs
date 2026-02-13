namespace Seoul.It.Blackjack.Core.Domain;

/// <summary>
/// 카드 한 장을 나타내는 클래스입니다.
/// </summary>
public sealed class Card
{
    /// <summary>
    /// 카드 한 장을 새로 만듭니다.
    /// </summary>
    /// <param name="suit">카드의 무늬입니다.</param>
    /// <param name="rank">카드의 숫자(랭크)입니다.</param>
    public Card(Suit suit, Rank rank)
    {
        Suit = suit;
        Rank = rank;
    }

    /// <summary>
    /// 카드의 무늬입니다.
    /// </summary>
    public Suit Suit { get; }

    /// <summary>
    /// 카드의 숫자(랭크)입니다.
    /// </summary>
    public Rank Rank { get; }
}
