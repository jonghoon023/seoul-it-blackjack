using Seoul.It.Blackjack.Core.Domain;

namespace Seoul.It.Blackjack.Backend.Models;

/// <summary>
/// 표준 카드 덱(52장)을 만들어 제공하는 도우미 클래스입니다.
/// </summary>
internal sealed class Deck
{
    /// <summary>
    /// 무늬 4종과 숫자 13종을 조합한 카드 52장을 반환합니다.
    /// </summary>
    public static IEnumerable<Card> Cards => Enum.GetValues<Suit>()
        .SelectMany(suit => Enum.GetValues<Rank>().Select(rank => new Card(suit, rank)));
}
