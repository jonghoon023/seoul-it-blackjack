using Seoul.It.Blackjack.Core.Contracts;
using Seoul.It.Blackjack.Core.Domain;
using System.Collections.Generic;
using System.Linq;

namespace Seoul.It.Blackjack.Backend.Services.State;

/// <summary>
/// 내부 상태를 외부 전송용 GameState로 복사해 만드는 구현체입니다.
/// </summary>
internal sealed class GameStateSnapshotFactory : IGameStateSnapshotFactory
{
    /// <summary>
    /// 현재 상태 값을 복사해 독립적인 GameState 스냅샷을 만듭니다.
    /// </summary>
    /// <param name="phase">게임 단계입니다.</param>
    /// <param name="dealerPlayerId">딜러 플레이어 ID입니다.</param>
    /// <param name="currentTurnPlayerId">현재 턴 플레이어 ID입니다.</param>
    /// <param name="statusMessage">상태 메시지입니다.</param>
    /// <param name="players">플레이어 목록입니다.</param>
    /// <returns>복사된 전송용 상태입니다.</returns>
    public GameState Create(
        GamePhase phase,
        string dealerPlayerId,
        string currentTurnPlayerId,
        string statusMessage,
        IReadOnlyCollection<PlayerState> players)
    {
        return new GameState
        {
            Phase = phase,
            DealerPlayerId = dealerPlayerId,
            CurrentTurnPlayerId = currentTurnPlayerId,
            StatusMessage = statusMessage,
            Players = players.Select(ClonePlayer).ToList(),
        };
    }

    /// <summary>
    /// 플레이어 한 명의 상태를 깊은 복사로 복제합니다.
    /// </summary>
    /// <param name="source">원본 플레이어 상태입니다.</param>
    /// <returns>복사된 플레이어 상태입니다.</returns>
    private static PlayerState ClonePlayer(PlayerState source)
    {
        return new PlayerState
        {
            PlayerId = source.PlayerId,
            Name = source.Name,
            IsDealer = source.IsDealer,
            Cards = source.Cards.Select(card => new Card(card.Suit, card.Rank)).ToList(),
            Score = source.Score,
            TurnState = source.TurnState,
            Outcome = source.Outcome,
        };
    }
}
