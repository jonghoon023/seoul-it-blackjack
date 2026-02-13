using Microsoft.AspNetCore.SignalR.Client;
using Seoul.It.Blackjack.Core.Contracts;
using System;
using System.Threading.Tasks;

namespace Seoul.It.Blackjack.Client;

public sealed class BlackjackClient : IAsyncDisposable
{
    private HubConnection? _connection;

    public event Action<GameState>? StateChanged;

    public event Action<string, string>? Error;

    public async Task ConnectAsync(string url)
    {
        if (_connection is not null)
        {
            return;
        }

        _connection = new HubConnectionBuilder()
            .WithUrl(url)
            .Build();

        _connection.On<GameState>("StateChanged", state => StateChanged?.Invoke(state));
        _connection.On<string, string>("Error", (code, message) => Error?.Invoke(code, message));
        await _connection.StartAsync();
    }

    public Task JoinAsync(string name, string? dealerKey = null)
    {
        return EnsureConnection().InvokeAsync("Join", name, dealerKey);
    }

    public Task LeaveAsync()
    {
        return EnsureConnection().InvokeAsync("Leave");
    }

    public Task StartRoundAsync()
    {
        return EnsureConnection().InvokeAsync("StartRound");
    }

    public Task HitAsync()
    {
        return EnsureConnection().InvokeAsync("Hit");
    }

    public Task StandAsync()
    {
        return EnsureConnection().InvokeAsync("Stand");
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
        {
            await _connection.DisposeAsync();
            _connection = null;
        }
    }

    private HubConnection EnsureConnection()
    {
        return _connection ?? throw new InvalidOperationException("먼저 ConnectAsync를 호출해야 합니다.");
    }
}
