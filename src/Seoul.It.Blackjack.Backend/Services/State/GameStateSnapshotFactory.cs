using Seoul.It.Blackjack.Core.Contracts;
using Seoul.It.Blackjack.Core.Domain;
using System.Collections.Generic;
using System.Linq;

namespace Seoul.It.Blackjack.Backend.Services.State;

internal sealed class GameStateSnapshotFactory : IGameStateSnapshotFactory
{
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
