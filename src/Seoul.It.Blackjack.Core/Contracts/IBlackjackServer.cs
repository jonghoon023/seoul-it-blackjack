namespace Seoul.It.Blackjack.Core.Contracts;

public interface IBlackjackServer
{
    Task JoinAsync(string name, string? dealerKey);

    Task LeaveAsync();

    Task StartGameAsync();

    Task EndGameAsync();

    Task HitAsync();

    Task StandAsync();
}
