using Seoul.It.Blackjack.Core.Contracts;

namespace Seoul.It.Blackjack.Backend.Services.Commands;

internal sealed class GameOperationResult
{
    public GameState State { get; set; } = new();

    public bool ShouldPublishState { get; set; } = true;

    public GameNotice? Notice { get; set; }
}
