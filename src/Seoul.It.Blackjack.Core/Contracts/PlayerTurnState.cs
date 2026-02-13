namespace Seoul.It.Blackjack.Core.Contracts;

/// <summary>
/// 플레이어가 이번 라운드에서 어떤 턴 상태인지 나타냅니다.
/// </summary>
public enum PlayerTurnState
{
    /// <summary>
    /// 아직 행동할 수 있는 상태입니다.
    /// </summary>
    Playing,

    /// <summary>
    /// 스탠드해서 더 이상 행동하지 않는 상태입니다.
    /// </summary>
    Standing,

    /// <summary>
    /// 점수가 21을 넘어서 탈락한 상태입니다.
    /// </summary>
    Busted
}
