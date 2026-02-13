using Seoul.It.Blackjack.Client;
using Seoul.It.Blackjack.Client.Options;
using Seoul.It.Blackjack.Core.Contracts;
using System;
using System.Threading.Tasks;

namespace Seoul.It.Blackjack.Frontend.Services;

public sealed class FrontendGameSession : IFrontendGameSession
{
    private readonly BlackjackClient _client;
    private readonly BlackjackClientOptions _options;

    public FrontendGameSession(BlackjackClient client, BlackjackClientOptions options)
    {
        _client = client;
        _options = options;
        _client.StateChanged += HandleStateChanged;
        _client.Error += HandleError;
    }

    public event Action<GameState>? StateChanged;

    public event Action<string, string>? ErrorReceived;

    public bool IsConnected { get; private set; }

    public bool IsJoined { get; private set; }

    public async Task ConnectAsync()
    {
        if (IsConnected)
        {
            return;
        }

        await _client.ConnectAsync(_options.HubUrl);
        IsConnected = true;
    }

    public async Task JoinAsync(string name, string? dealerKey)
    {
        await _client.JoinAsync(name, dealerKey);
        IsJoined = true;
    }

    public async Task LeaveAsync()
    {
        await _client.LeaveAsync();
        IsJoined = false;
    }

    public Task StartRoundAsync() => _client.StartRoundAsync();

    public Task HitAsync() => _client.HitAsync();

    public Task StandAsync() => _client.StandAsync();

    public ValueTask DisposeAsync()
    {
        _client.StateChanged -= HandleStateChanged;
        _client.Error -= HandleError;
        return ValueTask.CompletedTask;
    }

    private void HandleStateChanged(GameState state)
    {
        StateChanged?.Invoke(state);
    }

    private void HandleError(string code, string message)
    {
        if (code == "GAME_TERMINATED" || code == "NOT_JOINED")
        {
            IsJoined = false;
        }

        ErrorReceived?.Invoke(code, message);
    }
}
