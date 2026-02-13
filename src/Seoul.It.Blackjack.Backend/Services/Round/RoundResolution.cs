using Seoul.It.Blackjack.Backend.Models;
using Seoul.It.Blackjack.Backend.Services.Commands;
using Seoul.It.Blackjack.Core.Contracts;

namespace Seoul.It.Blackjack.Backend.Services.Round;

internal sealed class RoundResolution
{
    public GamePhase Phase { get; set; } = GamePhase.InRound;

    public string CurrentTurnPlayerId { get; set; } = string.Empty;

    public string StatusMessage { get; set; } = string.Empty;

    public Shoe? Shoe { get; set; }

    public GameNotice? Notice { get; set; }
}
