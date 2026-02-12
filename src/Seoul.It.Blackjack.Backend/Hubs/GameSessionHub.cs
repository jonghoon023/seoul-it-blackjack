using Microsoft.AspNetCore.SignalR;
using Seoul.It.Blackjack.Backend.Models;
using Seoul.It.Blackjack.Core.Contracts;

namespace Seoul.It.Blackjack.Backend.Hubs;

internal sealed class GameSessionHub(GameRoom room) 
    : Hub<IBlackjackClient>, IBlackjackServer
{
    public override Task OnConnectedAsync()
    {
        return base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // 모든 사용자에게 알림
        GameState state = await room.LeaveAsync(Context.ConnectionId);
        await Clients.All.GameStateUpdatedAsync(state);
    }

    public async Task JoinAsync(string name, string? dealerKey)
    {
        if (room.Phase != GamePhase.Lobby)
        {
            throw new HubException("게임이 진행 중이라 입장할 수 없습니다.");
        }

        string playerId = Context.ConnectionId;
        GameState state = await room.JoinAsync(Context.ConnectionId, name, dealerKey);

        // 참가한 사용자에게만 알림
        JoinResponse response = new(playerId, name);
        await Clients.Caller.JoinedAsync(response);

        // 모든 사용자에게 알림
        await Clients.All.GameStateUpdatedAsync(state);
    }

    public async Task StartGameAsync()
    {
        GameState state = await room.StartAsync(Context.ConnectionId);
        await Clients.All.GameStateUpdatedAsync(state);
    }

    public async Task EndGameAsync()
    {
        GameState state = await room.EndAsync(Context.ConnectionId);
        await Clients.All.GameStateUpdatedAsync(state);
    }

    public Task HitAsync()
    {
        throw new NotImplementedException();
    }

    public Task StandAsync()
    {
        throw new NotImplementedException();
    }
}
