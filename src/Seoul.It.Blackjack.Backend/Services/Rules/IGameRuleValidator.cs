using Seoul.It.Blackjack.Core.Contracts;
using System.Collections.Generic;

namespace Seoul.It.Blackjack.Backend.Services.Rules;

internal interface IGameRuleValidator
{
    string NormalizeName(string? name, int minNameLength, int maxNameLength);

    void EnsureJoined(ConnectionRegistry connections, string connectionId);

    void EnsureCanStartRound(
        ConnectionRegistry connections,
        GamePhase phase,
        string dealerPlayerId,
        string connectionId,
        int playerCount,
        int minPlayersToStart);

    PlayerState FindPlayer(IReadOnlyCollection<PlayerState> players, string playerId);

    PlayerState FindDealer(IReadOnlyCollection<PlayerState> players);

    PlayerState ValidatePlayerAction(
        ConnectionRegistry connections,
        GamePhase phase,
        IReadOnlyCollection<PlayerState> players,
        string connectionId,
        string currentTurnPlayerId);
}
