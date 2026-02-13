namespace Seoul.It.Blackjack.Core.Contracts;

/// <summary>
/// 게임이 지금 어느 단계에 있는지 나타냅니다.
/// </summary>
public enum GamePhase
{
    /// <summary>
    /// 라운드 시작 전 대기 상태입니다.
    /// </summary>
    Idle,

    /// <summary>
    /// 라운드가 진행 중인 상태입니다.
    /// </summary>
    InRound
}
