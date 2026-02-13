using Seoul.It.Blackjack.Backend.Models;
using Seoul.It.Blackjack.Backend.Services.Commands;
using Seoul.It.Blackjack.Core.Contracts;

namespace Seoul.It.Blackjack.Backend.Services.Round;

/// <summary>
/// 라운드 엔진이 계산한 결과 값을 묶어 전달하는 데이터입니다.
/// </summary>
internal sealed class RoundResolution
{
    /// <summary>
    /// 처리 후 게임 단계입니다.
    /// </summary>
    public GamePhase Phase { get; set; } = GamePhase.InRound;

    /// <summary>
    /// 처리 후 현재 턴 플레이어 ID입니다.
    /// </summary>
    public string CurrentTurnPlayerId { get; set; } = string.Empty;

    /// <summary>
    /// 처리 후 상태 메시지입니다.
    /// </summary>
    public string StatusMessage { get; set; } = string.Empty;

    /// <summary>
    /// 처리 과정에서 생성/사용한 카드 더미입니다.
    /// </summary>
    public Shoe? Shoe { get; set; }

    /// <summary>
    /// 전체 사용자에게 보낼 공지입니다.
    /// </summary>
    public GameNotice? Notice { get; set; }
}
