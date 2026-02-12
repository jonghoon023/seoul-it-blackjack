namespace Seoul.It.Blackjack.Core.Contracts;

public interface IBlackjackClient
{
    public Task JoinedAsync(IPlayer player);

    public Task LeavedAsync();

    public Task GameStateUpdatedAsync(GameState state);
}
