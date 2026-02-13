using System.Collections.Generic;

namespace Seoul.It.Blackjack.Core.Contracts;

/// <summary>
/// 게임 전체의 현재 상태를 한 번에 전달하는 데이터입니다.
/// </summary>
public sealed class GameState
{
    /// <summary>
    /// 게임 단계입니다.
    /// </summary>
    public GamePhase Phase { get; set; } = GamePhase.Idle;

    /// <summary>
    /// 현재 게임에 참가한 플레이어 목록입니다.
    /// </summary>
    public List<PlayerState> Players { get; set; } = new List<PlayerState>();

    /// <summary>
    /// 딜러 플레이어 ID입니다.
    /// </summary>
    public string DealerPlayerId { get; set; } = string.Empty;

    /// <summary>
    /// 현재 턴인 플레이어 ID입니다.
    /// </summary>
    public string CurrentTurnPlayerId { get; set; } = string.Empty;

    /// <summary>
    /// 화면에 보여줄 상태 메시지입니다.
    /// </summary>
    public string StatusMessage { get; set; } = string.Empty;
}
