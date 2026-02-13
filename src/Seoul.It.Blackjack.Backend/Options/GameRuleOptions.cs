namespace Seoul.It.Blackjack.Backend.Options;

/// <summary>
/// 게임 규칙에 필요한 숫자 설정을 모아둔 옵션입니다.
/// </summary>
internal sealed class GameRuleOptions
{
    /// <summary>
    /// 설정 파일에서 게임 규칙 섹션을 찾을 때 쓰는 이름입니다.
    /// </summary>
    public const string DefaultSectionName = "GameRules";

    /// <summary>
    /// 신발(Shoe)을 만들 때 사용할 덱 개수입니다.
    /// </summary>
    public int DeckCount { get; set; } = 4;

    /// <summary>
    /// 딜러가 이 점수 이상이면 스탠드하는 기준 점수입니다.
    /// </summary>
    public int DealerStandScore { get; set; } = 17;

    /// <summary>
    /// 라운드를 시작하기 위한 최소 참가 인원입니다.
    /// </summary>
    public int MinPlayersToStart { get; set; } = 2;

    /// <summary>
    /// 플레이어 이름의 최소 길이입니다.
    /// </summary>
    public int MinNameLength { get; set; } = 1;

    /// <summary>
    /// 플레이어 이름의 최대 길이입니다.
    /// </summary>
    public int MaxNameLength { get; set; } = 20;
}
