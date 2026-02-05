namespace Seoul.It.Blackjack.Backend.Domains;

/// <summary>
/// 카드 한 장을 표현하는 간단한 클래스입니다.
/// Rank(카드 숫자/문자)와 Suit(무늬)를 속성으로 갖습니다.
/// </summary>
public class Card
{
    /// <summary>
    /// 카드의 무늬입니다. 예: "♠", "♥", "♦", "♣".
    /// </summary>
    public string Suit { get; }

    /// <summary>
    /// 카드의 숫자 또는 문자입니다. 예: "A", "2", ..., "10", "J", "Q", "K".
    /// </summary>
    public string Rank { get; }

    public Card(string rank, string suit)
    {
        Rank = rank;
        Suit = suit;
    }

    /// <summary>
    /// 사용자에게 카드 정보를 보여줄 때 사용할 문자열을 반환합니다.
    /// 예: "A♠", "10♥" 같은 형태입니다.
    /// </summary>
    public override string ToString() => $"{Suit}{Rank}";
}
