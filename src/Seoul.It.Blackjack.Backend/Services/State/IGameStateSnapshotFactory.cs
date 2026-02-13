using Seoul.It.Blackjack.Core.Contracts;
using System.Collections.Generic;

namespace Seoul.It.Blackjack.Backend.Services.State;

internal interface IGameStateSnapshotFactory
{
    GameState Create(
        GamePhase phase,
        string dealerPlayerId,
        string currentTurnPlayerId,
        string statusMessage,
        IReadOnlyCollection<PlayerState> players);
}
