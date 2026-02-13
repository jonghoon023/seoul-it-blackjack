using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.TestHost;
using Seoul.It.Blackjack.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Seoul.It.Blackjack.Backend.Tests;

internal sealed class SignalRTestClient : IAsyncDisposable
{
    private readonly HubConnection _connection;

    public SignalRTestClient(TestHostFactory factory)
    {
        Uri hubUrl = new(factory.Server.BaseAddress, "/blackjack");
        _connection = new HubConnectionBuilder()
            .WithUrl(
                hubUrl,
                options => options.HttpMessageHandlerFactory = _ => factory.Server.CreateHandler())
            .Build();

        _connection.On<GameState>(nameof(IBlackjackClient.OnStateChanged), state =>
        {
            Events.Add((nameof(IBlackjackClient.OnStateChanged), null));
            States.Add(state);
        });
        _connection.On<string, string>(nameof(IBlackjackClient.OnError), (code, message) =>
        {
            Events.Add((nameof(IBlackjackClient.OnError), code));
            Errors.Add((code, message));
        });
    }

    public List<GameState> States { get; } = new();

    public List<(string Code, string Message)> Errors { get; } = new();

    public List<(string Type, string? Code)> Events { get; } = new();

    public Task ConnectAsync() => _connection.StartAsync();

    public Task JoinAsync(string name, string? dealerKey = null) => _connection.InvokeAsync(nameof(IBlackjackServer.Join), name, dealerKey);

    public Task LeaveAsync() => _connection.InvokeAsync(nameof(IBlackjackServer.Leave));

    public Task StartRoundAsync() => _connection.InvokeAsync(nameof(IBlackjackServer.StartRound));

    public Task HitAsync() => _connection.InvokeAsync(nameof(IBlackjackServer.Hit));

    public Task StandAsync() => _connection.InvokeAsync(nameof(IBlackjackServer.Stand));

    public ValueTask DisposeAsync() => _connection.DisposeAsync();
}
