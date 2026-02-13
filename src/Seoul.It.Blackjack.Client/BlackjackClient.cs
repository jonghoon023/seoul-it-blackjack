using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Seoul.It.Blackjack.Core.Contracts;

namespace Seoul.It.Blackjack.Client;

/// <summary>
/// SignalR 연결을 감싸서 블랙잭 서버 호출을 쉽게 만들어 주는 클라이언트 SDK입니다.
/// </summary>
public sealed class BlackjackClient : IAsyncDisposable, IBlackjackClient
{
    /// <summary>
    /// 실제 SignalR 허브 연결 객체입니다.
    /// </summary>
    private HubConnection? _connection;

    /// <summary>
    /// 서버에서 상태 변경을 받았을 때 외부로 전달하는 이벤트입니다.
    /// </summary>
    public event Action<GameState>? StateChanged;

    /// <summary>
    /// 서버에서 오류를 받았을 때 외부로 전달하는 이벤트입니다.
    /// </summary>
    public event Action<string, string>? Error;

    /// <summary>
    /// 기본 연결 방식으로 허브 URL에 연결합니다.
    /// </summary>
    /// <param name="url">연결할 허브 URL입니다.</param>
    /// <returns>비동기 완료를 나타내는 작업입니다.</returns>
    public async Task ConnectAsync(string url)
    {
        await ConnectAsync(url, null);
    }

    /// <summary>
    /// 사용자 지정 HttpMessageHandler를 사용해 허브 URL에 연결합니다.
    /// </summary>
    /// <param name="url">연결할 허브 URL입니다.</param>
    /// <param name="createMessageHandler">테스트 등에서 쓸 핸들러 생성 함수입니다.</param>
    /// <returns>비동기 완료를 나타내는 작업입니다.</returns>
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

    /// <summary>
    /// 서버에 입장 요청을 보냅니다.
    /// </summary>
    /// <param name="name">플레이어 이름입니다.</param>
    /// <param name="dealerKey">딜러 키입니다.</param>
    /// <returns>비동기 완료를 나타내는 작업입니다.</returns>
    public Task JoinAsync(string name, string? dealerKey = null)
    {
        return EnsureConnection().InvokeAsync(nameof(IBlackjackServer.Join), name, dealerKey);
    }

    /// <summary>
    /// 서버에 퇴장 요청을 보냅니다.
    /// </summary>
    /// <returns>비동기 완료를 나타내는 작업입니다.</returns>
    public Task LeaveAsync()
    {
        return EnsureConnection().InvokeAsync(nameof(IBlackjackServer.Leave));
    }

    /// <summary>
    /// 서버에 라운드 시작 요청을 보냅니다.
    /// </summary>
    /// <returns>비동기 완료를 나타내는 작업입니다.</returns>
    public Task StartRoundAsync()
    {
        return EnsureConnection().InvokeAsync(nameof(IBlackjackServer.StartRound));
    }

    /// <summary>
    /// 서버에 히트 요청을 보냅니다.
    /// </summary>
    /// <returns>비동기 완료를 나타내는 작업입니다.</returns>
    public Task HitAsync()
    {
        return EnsureConnection().InvokeAsync(nameof(IBlackjackServer.Hit));
    }

    /// <summary>
    /// 서버에 스탠드 요청을 보냅니다.
    /// </summary>
    /// <returns>비동기 완료를 나타내는 작업입니다.</returns>
    public Task StandAsync()
    {
        return EnsureConnection().InvokeAsync(nameof(IBlackjackServer.Stand));
    }

    /// <summary>
    /// 서버에서 상태 변경 이벤트를 받았을 때 호출됩니다.
    /// </summary>
    /// <param name="state">최신 게임 상태입니다.</param>
    /// <returns>비동기 완료를 나타내는 작업입니다.</returns>
    public Task OnStateChanged(GameState state)
    {
        StateChanged?.Invoke(state);
        return Task.CompletedTask;
    }

    /// <summary>
    /// 서버에서 오류 이벤트를 받았을 때 호출됩니다.
    /// </summary>
    /// <param name="code">오류 코드입니다.</param>
    /// <param name="message">오류 메시지입니다.</param>
    /// <returns>비동기 완료를 나타내는 작업입니다.</returns>
    public Task OnError(string code, string message)
    {
        Error?.Invoke(code, message);
        return Task.CompletedTask;
    }

    /// <summary>
    /// 내부 연결 객체를 정리합니다.
    /// </summary>
    /// <returns>비동기 완료를 나타내는 작업입니다.</returns>
    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
        {
            await _connection.DisposeAsync();
            _connection = null;
        }
    }

    /// <summary>
    /// 연결이 준비되었는지 확인하고 연결 객체를 반환합니다.
    /// </summary>
    /// <returns>사용 가능한 허브 연결 객체입니다.</returns>
    private HubConnection EnsureConnection()
    {
        return _connection ?? throw new InvalidOperationException("먼저 ConnectAsync를 호출해야 합니다.");
    }
}
