using Microsoft.AspNetCore.SignalR;
using Seoul.It.Blackjack.Backend.Services;
using Seoul.It.Blackjack.Backend.Services.Commands;
using Seoul.It.Blackjack.Backend.Services.Exceptions;
using Seoul.It.Blackjack.Core.Contracts;
using System;
using System.Threading.Tasks;

namespace Seoul.It.Blackjack.Backend.Hubs;

internal sealed class GameSessionHub(IGameRoomService room) : Hub<IBlackjackClient>, IBlackjackServer
{
    public const string Endpoint = "/blackjack";

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            GameOperationResult result = await room.DisconnectAsync(Context.ConnectionId);
            await BroadcastResultAsync(result);
        }
        catch (GameRoomException)
        {
            // 연결 종료 중 에러는 무시한다.
        }

        await base.OnDisconnectedAsync(exception);
    }

    public Task Join(string name, string? dealerKey) =>
        ExecuteAsync(() => room.JoinAsync(Context.ConnectionId, name, dealerKey));

    public Task Leave() =>
        ExecuteAsync(() => room.LeaveAsync(Context.ConnectionId));

    public Task StartRound() =>
        ExecuteAsync(() => room.StartRoundAsync(Context.ConnectionId));

    public Task Hit() =>
        ExecuteAsync(() => room.HitAsync(Context.ConnectionId));

    public Task Stand() =>
        ExecuteAsync(() => room.StandAsync(Context.ConnectionId));

    private async Task ExecuteAsync(Func<Task<GameOperationResult>> action)
    {
        try
        {
            GameOperationResult result = await action();
            await BroadcastResultAsync(result);
        }
        catch (GameRoomException ex)
        {
            await Clients.Caller.OnError(ex.Code, ex.Message);
        }
    }

    private async Task BroadcastResultAsync(GameOperationResult result)
    {
        if (result.Notice is not null)
        {
            await Clients.All.OnError(result.Notice.Code, result.Notice.Message);
        }

        if (result.ShouldPublishState)
        {
            await Clients.All.OnStateChanged(result.State);
        }
    }
}
