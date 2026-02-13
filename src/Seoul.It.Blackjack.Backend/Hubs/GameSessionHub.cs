using Microsoft.AspNetCore.SignalR;
using Seoul.It.Blackjack.Backend.Services;
using Seoul.It.Blackjack.Backend.Services.Commands;
using Seoul.It.Blackjack.Backend.Services.Exceptions;
using Seoul.It.Blackjack.Core.Contracts;
using System;
using System.Threading.Tasks;

namespace Seoul.It.Blackjack.Backend.Hubs;

/// <summary>
/// 클라이언트 요청을 받아 게임 방 서비스로 전달하는 SignalR 허브입니다.
/// </summary>
internal sealed class GameSessionHub(IGameRoomService room) : Hub<IBlackjackClient>, IBlackjackServer
{
    /// <summary>
    /// 이 허브의 고정 엔드포인트 경로입니다.
    /// </summary>
    public const string Endpoint = "/blackjack";

    /// <summary>
    /// 연결이 끊어졌을 때 퇴장 처리와 상태 브로드캐스트를 수행합니다.
    /// </summary>
    /// <param name="exception">연결 종료 시 전달된 예외입니다.</param>
    /// <returns>비동기 완료를 나타내는 작업입니다.</returns>
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

    /// <summary>
    /// 입장 요청을 처리합니다.
    /// </summary>
    /// <param name="name">입장 이름입니다.</param>
    /// <param name="dealerKey">딜러 키입니다.</param>
    /// <returns>비동기 완료를 나타내는 작업입니다.</returns>
    public Task Join(string name, string? dealerKey) =>
        ExecuteAsync(() => room.JoinAsync(Context.ConnectionId, name, dealerKey));

    /// <summary>
    /// 퇴장 요청을 처리합니다.
    /// </summary>
    /// <returns>비동기 완료를 나타내는 작업입니다.</returns>
    public Task Leave() =>
        ExecuteAsync(() => room.LeaveAsync(Context.ConnectionId));

    /// <summary>
    /// 라운드 시작 요청을 처리합니다.
    /// </summary>
    /// <returns>비동기 완료를 나타내는 작업입니다.</returns>
    public Task StartRound() =>
        ExecuteAsync(() => room.StartRoundAsync(Context.ConnectionId));

    /// <summary>
    /// 히트 요청을 처리합니다.
    /// </summary>
    /// <returns>비동기 완료를 나타내는 작업입니다.</returns>
    public Task Hit() =>
        ExecuteAsync(() => room.HitAsync(Context.ConnectionId));

    /// <summary>
    /// 스탠드 요청을 처리합니다.
    /// </summary>
    /// <returns>비동기 완료를 나타내는 작업입니다.</returns>
    public Task Stand() =>
        ExecuteAsync(() => room.StandAsync(Context.ConnectionId));

    /// <summary>
    /// 게임 방 서비스 호출을 실행하고 성공/실패를 규칙에 맞게 전송합니다.
    /// </summary>
    /// <param name="action">실행할 서비스 호출 함수입니다.</param>
    /// <returns>비동기 완료를 나타내는 작업입니다.</returns>
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

    /// <summary>
    /// 처리 결과를 보고 전체 상태/공지 브로드캐스트를 수행합니다.
    /// </summary>
    /// <param name="result">서비스 처리 결과입니다.</param>
    /// <returns>비동기 완료를 나타내는 작업입니다.</returns>
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
