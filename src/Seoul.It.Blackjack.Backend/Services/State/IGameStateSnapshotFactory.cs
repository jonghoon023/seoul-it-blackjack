using Seoul.It.Blackjack.Core.Contracts;
using System.Collections.Generic;

namespace Seoul.It.Blackjack.Backend.Services.State;

/// <summary>
/// 내부 상태를 외부 전송용 GameState 스냅샷으로 만드는 계약입니다.
/// </summary>
internal interface IGameStateSnapshotFactory
{
    /// <summary>
    /// 현재 상태 값을 복사해 전송용 GameState를 만듭니다.
    /// </summary>
    /// <param name="phase">게임 단계입니다.</param>
    /// <param name="dealerPlayerId">딜러 플레이어 ID입니다.</param>
    /// <param name="currentTurnPlayerId">현재 턴 플레이어 ID입니다.</param>
    /// <param name="statusMessage">상태 메시지입니다.</param>
    /// <param name="players">플레이어 목록입니다.</param>
    /// <returns>복사된 스냅샷 상태입니다.</returns>
    GameState Create(
        GamePhase phase,
        string dealerPlayerId,
        string currentTurnPlayerId,
        string statusMessage,
        IReadOnlyCollection<PlayerState> players);
}
