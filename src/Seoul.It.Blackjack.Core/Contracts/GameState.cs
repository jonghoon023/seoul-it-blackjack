using System.Collections.Generic;

namespace Seoul.It.Blackjack.Core.Contracts;

public sealed class GameState
{
    public GamePhase Phase { get; set; } = GamePhase.Idle;

    public List<PlayerState> Players { get; set; } = new List<PlayerState>();

    public string DealerPlayerId { get; set; } = string.Empty;

    public string CurrentTurnPlayerId { get; set; } = string.Empty;

    public string StatusMessage { get; set; } = string.Empty;
}
