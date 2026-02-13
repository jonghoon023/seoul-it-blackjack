using Microsoft.AspNetCore.SignalR.Client;
using Seoul.It.Blackjack.Core.Contracts;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Seoul.It.Blackjack.Client;

public sealed class BlackjackClient : IAsyncDisposable, IBlackjackClient
{
    private HubConnection? _connection;

    public event Action<GameState>? StateChanged;

    public event Action<string, string>? Error;

    public async Task ConnectAsync(string url)
    {
        await ConnectAsync(url, null);
    }

    public async Task ConnectAsync(string url, Func<HttpMessageHandler>? createMessageHandler)
    {
        if (_connection is not null)
        {
            return;
        }

        IHubConnectionBuilder builder = new HubConnectionBuilder();
        if (createMessageHandler is null)
        {
            builder = builder.WithUrl(url);
        }
        else
        {
            builder = builder.WithUrl(url, options => options.HttpMessageHandlerFactory = _ => createMessageHandler());
        }

        _connection = builder.Build();

        _connection.On<GameState>(nameof(IBlackjackClient.OnStateChanged), OnStateChanged);
        _connection.On<string, string>(nameof(IBlackjackClient.OnError), OnError);
        await _connection.StartAsync();
    }

    public Task JoinAsync(string name, string? dealerKey = null)
    {
        return EnsureConnection().InvokeAsync(nameof(IBlackjackServer.Join), name, dealerKey);
    }

    public Task LeaveAsync()
    {
        return EnsureConnection().InvokeAsync(nameof(IBlackjackServer.Leave));
    }

    public Task StartRoundAsync()
    {
        return EnsureConnection().InvokeAsync(nameof(IBlackjackServer.StartRound));
    }

    public Task HitAsync()
    {
        return EnsureConnection().InvokeAsync(nameof(IBlackjackServer.Hit));
    }

    public Task StandAsync()
    {
        return EnsureConnection().InvokeAsync(nameof(IBlackjackServer.Stand));
    }

    public Task OnStateChanged(GameState state)
    {
        StateChanged?.Invoke(state);
        return Task.CompletedTask;
    }

    public Task OnError(string code, string message)
    {
        Error?.Invoke(code, message);
        return Task.CompletedTask;
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
