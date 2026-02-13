using Seoul.It.Blackjack.Core.Domain;

namespace Seoul.It.Blackjack.Frontend.Extensions;

/// <summary>
/// 카드 도메인 객체를 화면 자산 경로로 바꾸는 확장 메서드를 제공합니다.
/// </summary>
public static class CardExtensions
{
    /// <summary>
    /// 카드 정보를 정적 SVG 파일 경로로 변환합니다.
    /// </summary>
    /// <param name="card">경로로 바꿀 카드입니다.</param>
    /// <returns>카드 이미지 상대 경로입니다.</returns>
    public static string ToAssetPath(this Card card)
    {
        string suit = card.Suit.ToString().ToLowerInvariant();
        string rank = card.Rank.ToString().ToLowerInvariant();
        return $"cards/{suit}_{rank}.svg";
    }
}
