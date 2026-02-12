namespace Seoul.It.Blackjack.Core.Contracts;

public interface IBlackjackClient
{
    public Task JoinedAsync(JoinResponse response);

    public Task GameStateUpdatedAsync(GameState state);
}
