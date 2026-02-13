using Seoul.It.Blackjack.Core.Contracts;
using System;
using System.Threading.Tasks;

namespace Seoul.It.Blackjack.Frontend.Services;

public interface IFrontendGameSession : IAsyncDisposable
{
    event Action<GameState>? StateChanged;

    event Action<string, string>? ErrorReceived;

    bool IsConnected { get; }

    bool IsJoined { get; }

    Task ConnectAsync();

    Task JoinAsync(string name, string? dealerKey);

    Task LeaveAsync();

    Task StartRoundAsync();

    Task HitAsync();

    Task StandAsync();
}
