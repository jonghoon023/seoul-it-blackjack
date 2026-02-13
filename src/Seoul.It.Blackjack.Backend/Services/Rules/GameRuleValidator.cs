using Seoul.It.Blackjack.Backend.Services.Exceptions;
using Seoul.It.Blackjack.Core.Contracts;
using System.Collections.Generic;
using System.Linq;

namespace Seoul.It.Blackjack.Backend.Services.Rules;

/// <summary>
/// 게임 규칙과 권한 조건을 검사하는 구현체입니다.
/// </summary>
internal sealed class GameRuleValidator : IGameRuleValidator
{
    /// <summary>
    /// 이름을 정리하고 길이 규칙을 확인합니다.
    /// </summary>
    /// <param name="name">입력 이름입니다.</param>
    /// <param name="minNameLength">최소 길이입니다.</param>
    /// <param name="maxNameLength">최대 길이입니다.</param>
    /// <returns>Trim 처리된 이름입니다.</returns>
    public string NormalizeName(string? name, int minNameLength, int maxNameLength)
    {
        string normalized = (name ?? string.Empty).Trim();
        if (normalized.Length < minNameLength || normalized.Length > maxNameLength)
        {
            throw new GameValidationException("INVALID_NAME", "이름은 1~20자여야 합니다.");
        }

        return normalized;
    }

    /// <summary>
    /// 연결 사용자가 이미 입장했는지 확인합니다.
    /// </summary>
    /// <param name="connections">연결 매핑 저장소입니다.</param>
    /// <param name="connectionId">확인할 연결 ID입니다.</param>
    public void EnsureJoined(ConnectionRegistry connections, string connectionId)
    {
        if (!connections.ContainsConnection(connectionId))
        {
            throw new GameValidationException("NOT_JOINED", "먼저 참가해야 합니다.");
        }
    }

    /// <summary>
    /// 라운드 시작 조건(입장 여부, 단계, 딜러 권한, 최소 인원)을 검사합니다.
    /// </summary>
    /// <param name="connections">연결 매핑 저장소입니다.</param>
    /// <param name="phase">현재 게임 단계입니다.</param>
    /// <param name="dealerPlayerId">현재 딜러 ID입니다.</param>
    /// <param name="connectionId">요청 연결 ID입니다.</param>
    /// <param name="playerCount">현재 참가 인원 수입니다.</param>
    /// <param name="minPlayersToStart">최소 시작 인원 수입니다.</param>
    public void EnsureCanStartRound(
        ConnectionRegistry connections,
        GamePhase phase,
        string dealerPlayerId,
        string connectionId,
        int playerCount,
        int minPlayersToStart)
    {
        EnsureJoined(connections, connectionId);
        if (phase != GamePhase.Idle)
        {
            throw new GameRuleException("GAME_IN_PROGRESS", "이미 게임이 진행 중입니다.");
        }

        if (string.IsNullOrEmpty(dealerPlayerId) || dealerPlayerId != connectionId)
        {
            throw new GameAuthorizationException("NOT_DEALER", "딜러만 라운드를 시작할 수 있습니다.");
        }

        if (playerCount < minPlayersToStart)
        {
            throw new GameRuleException("INSUFFICIENT_PLAYERS", "라운드를 시작하려면 최소 2명이 필요합니다.");
        }
    }

    /// <summary>
    /// 플레이어 목록에서 ID가 같은 플레이어를 찾아 반환합니다.
    /// </summary>
    /// <param name="players">플레이어 목록입니다.</param>
    /// <param name="playerId">찾을 플레이어 ID입니다.</param>
    /// <returns>찾은 플레이어입니다.</returns>
    public PlayerState FindPlayer(IReadOnlyCollection<PlayerState> players, string playerId)
    {
        PlayerState? player = players.SingleOrDefault(value => value.PlayerId == playerId);
        if (player is null)
        {
            throw new GameValidationException("NOT_JOINED", "먼저 참가해야 합니다.");
        }

        return player;
    }

    /// <summary>
    /// 플레이어 목록에서 딜러를 찾아 반환합니다.
    /// </summary>
    /// <param name="players">플레이어 목록입니다.</param>
    /// <returns>딜러 플레이어입니다.</returns>
    public PlayerState FindDealer(IReadOnlyCollection<PlayerState> players)
    {
        PlayerState? dealer = players.SingleOrDefault(player => player.IsDealer);
        if (dealer is null)
        {
            throw new GameAuthorizationException("NOT_DEALER", "딜러가 존재하지 않습니다.");
        }

        return dealer;
    }

    /// <summary>
    /// 히트/스탠드 요청 전 공통 규칙을 검사하고 요청 플레이어를 반환합니다.
    /// </summary>
    /// <param name="connections">연결 매핑 저장소입니다.</param>
    /// <param name="phase">현재 게임 단계입니다.</param>
    /// <param name="players">플레이어 목록입니다.</param>
    /// <param name="connectionId">요청 연결 ID입니다.</param>
    /// <param name="currentTurnPlayerId">현재 턴 플레이어 ID입니다.</param>
    /// <returns>검사를 통과한 요청 플레이어입니다.</returns>
    public PlayerState ValidatePlayerAction(
        ConnectionRegistry connections,
        GamePhase phase,
        IReadOnlyCollection<PlayerState> players,
        string connectionId,
        string currentTurnPlayerId)
    {
        EnsureJoined(connections, connectionId);
        if (phase != GamePhase.InRound)
        {
            throw new GameRuleException("GAME_NOT_INROUND", "게임이 진행 중이 아닙니다.");
        }

        PlayerState player = FindPlayer(players, connectionId);
        if (player.IsDealer)
        {
            throw new GameRuleException("DEALER_IS_AUTO", "딜러는 자동으로 진행됩니다.");
        }

        if (currentTurnPlayerId != player.PlayerId)
        {
            throw new GameRuleException("NOT_YOUR_TURN", "현재 당신의 턴이 아닙니다.");
        }

        if (player.TurnState != PlayerTurnState.Playing)
        {
            throw new GameRuleException("ALREADY_DONE", "이미 행동이 끝난 플레이어입니다.");
        }

        return player;
    }
}
