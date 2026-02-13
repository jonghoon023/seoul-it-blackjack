namespace Seoul.It.Blackjack.Backend.Options;

/// <summary>
/// 딜러 입장 규칙에 필요한 설정 값을 담습니다.
/// </summary>
public sealed class DealerOptions
{
    /// <summary>
    /// 설정 파일에서 딜러 섹션을 찾을 때 쓰는 이름입니다.
    /// </summary>
    public const string DefaultSectionName = "Dealer";

    /// <summary>
    /// 딜러로 입장할 때 비교하는 비밀 키입니다.
    /// </summary>
    public string? Key { get; set; }
}
