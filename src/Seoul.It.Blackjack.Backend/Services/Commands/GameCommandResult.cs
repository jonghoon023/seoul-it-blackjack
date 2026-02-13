using Seoul.It.Blackjack.Core.Contracts;

namespace Seoul.It.Blackjack.Backend.Services.Commands;

internal sealed class GameCommandResult
{
    public GameState State { get; set; } = new();

    public bool ShouldBroadcastState { get; set; } = true;

    public string? BroadcastErrorCode { get; set; }

    public string? BroadcastErrorMessage { get; set; }
}
