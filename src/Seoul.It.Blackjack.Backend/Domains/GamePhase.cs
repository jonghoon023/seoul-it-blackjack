namespace Seoul.It.Blackjack.Backend.Domains;

/// <summary>
/// 게임의 현재 단계를 나타내는 열거형입니다.
/// </summary>
public enum GamePhase
{
    /// <summary>
    /// 아무도 게임을 시작하지 않은 상태입니다.
    /// </summary>
    Waiting,

    /// <summary>
    /// 플레이어들의 차례가 진행 중입니다.
    /// </summary>
    PlayersTurn,

    /// <summary>
    /// 딜러 차례가 진행 중입니다.
    /// </summary>
    DealerTurn,

    /// <summary>
    /// 모든 차례가 끝나고 승패 판정이 난 상태입니다.
    /// </summary>
    Finished
}
