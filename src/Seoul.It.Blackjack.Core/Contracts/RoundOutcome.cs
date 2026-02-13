namespace Seoul.It.Blackjack.Core.Contracts;

/// <summary>
/// 라운드가 끝났을 때 플레이어의 결과를 나타냅니다.
/// </summary>
public enum RoundOutcome
{
    /// <summary>
    /// 아직 결과가 정해지지 않은 상태입니다.
    /// </summary>
    None,

    /// <summary>
    /// 승리한 상태입니다.
    /// </summary>
    Win,

    /// <summary>
    /// 패배한 상태입니다.
    /// </summary>
    Lose,

    /// <summary>
    /// 무승부(비김) 상태입니다.
    /// </summary>
    Tie
}
