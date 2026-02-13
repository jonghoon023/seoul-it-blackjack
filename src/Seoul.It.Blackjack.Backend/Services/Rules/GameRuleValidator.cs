using Seoul.It.Blackjack.Backend.Services.Exceptions;
using Seoul.It.Blackjack.Core.Contracts;
using System.Collections.Generic;
using System.Linq;

namespace Seoul.It.Blackjack.Backend.Services.Rules;

internal sealed class GameRuleValidator : IGameRuleValidator
{
    public string NormalizeName(string? name, int minNameLength, int maxNameLength)
    {
        string normalized = (name ?? string.Empty).Trim();
        if (normalized.Length < minNameLength || normalized.Length > maxNameLength)
        {
            throw new GameValidationException("INVALID_NAME", "이름은 1~20자여야 합니다.");
        }

        return normalized;
    }

    public void EnsureJoined(ConnectionRegistry connections, string connectionId)
    {
        if (!connections.ContainsConnection(connectionId))
        {
            throw new GameValidationException("NOT_JOINED", "먼저 참가해야 합니다.");
        }
    }

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

    public PlayerState FindPlayer(IReadOnlyCollection<PlayerState> players, string playerId)
    {
        PlayerState? player = players.SingleOrDefault(value => value.PlayerId == playerId);
        if (player is null)
        {
            throw new GameValidationException("NOT_JOINED", "먼저 참가해야 합니다.");
        }

        return player;
    }

    public PlayerState FindDealer(IReadOnlyCollection<PlayerState> players)
    {
        PlayerState? dealer = players.SingleOrDefault(player => player.IsDealer);
        if (dealer is null)
        {
            throw new GameAuthorizationException("NOT_DEALER", "딜러가 존재하지 않습니다.");
        }

        return dealer;
    }

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
            throw new GameRuleException("NOT_YOUR_TURN", "현재 턴의 플레이어가 아닙니다.");
        }

        if (player.TurnState != PlayerTurnState.Playing)
        {
            throw new GameRuleException("ALREADY_DONE", "이미 행동이 끝난 플레이어입니다.");
        }

        return player;
    }
}
