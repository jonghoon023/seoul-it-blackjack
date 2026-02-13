using Seoul.It.Blackjack.Core.Domain;
using System.Collections.Generic;

namespace Seoul.It.Blackjack.Core.Contracts;

public sealed class PlayerState
{
    public string PlayerId { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public bool IsDealer { get; set; }

    public List<Card> Cards { get; set; } = new List<Card>();

    public int Score { get; set; }

    public PlayerTurnState TurnState { get; set; } = PlayerTurnState.Playing;

    public RoundOutcome Outcome { get; set; } = RoundOutcome.None;
}
