using Seoul.It.Blackjack.Core.Contracts;
using Seoul.It.Blackjack.Frontend.Services;
using System;
using System.Threading.Tasks;

namespace Seoul.It.Blackjack.Frontend.Tests;

internal sealed class FakeFrontendGameSession : IFrontendGameSession, IDisposable
{
    public event Action<GameState>? StateChanged;

    public event Action<string, string>? ErrorReceived;

    public bool IsConnected { get; private set; }

    public bool IsJoined { get; private set; }

    public int ConnectCallCount { get; private set; }

    public int JoinCallCount { get; private set; }

    public int LeaveCallCount { get; private set; }

    public int StartRoundCallCount { get; private set; }

    public int HitCallCount { get; private set; }

    public int StandCallCount { get; private set; }

    public string LastJoinName { get; private set; } = string.Empty;

    public string? LastJoinDealerKey { get; private set; }

    public Task ConnectAsync()
    {
        ConnectCallCount++;
        IsConnected = true;
        return Task.CompletedTask;
    }

    public Task JoinAsync(string name, string? dealerKey)
    {
        JoinCallCount++;
        LastJoinName = name;
        LastJoinDealerKey = dealerKey;
        IsJoined = true;
        return Task.CompletedTask;
    }

    public Task LeaveAsync()
    {
        LeaveCallCount++;
        IsJoined = false;
        return Task.CompletedTask;
    }

    public Task StartRoundAsync()
    {
        StartRoundCallCount++;
        return Task.CompletedTask;
    }

    public Task HitAsync()
    {
        HitCallCount++;
        return Task.CompletedTask;
    }

    public Task StandAsync()
    {
        StandCallCount++;
        return Task.CompletedTask;
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    public void Dispose()
    {
    }

    public void RaiseState(GameState state)
    {
        StateChanged?.Invoke(state);
    }

    public void RaiseError(string code, string message)
    {
        ErrorReceived?.Invoke(code, message);
    }
}
