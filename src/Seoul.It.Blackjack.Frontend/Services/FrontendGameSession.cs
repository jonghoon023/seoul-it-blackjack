using Seoul.It.Blackjack.Client;
using Seoul.It.Blackjack.Client.Options;
using Seoul.It.Blackjack.Core.Contracts;
using System;
using System.Threading.Tasks;

namespace Seoul.It.Blackjack.Frontend.Services;

/// <summary>
/// 프론트엔드에서 쓰기 쉬운 형태로 블랙잭 클라이언트를 감싼 세션 서비스입니다.
/// </summary>
public sealed class FrontendGameSession : IFrontendGameSession
{
    /// <summary>
    /// 실제 서버 통신을 담당하는 클라이언트입니다.
    /// </summary>
    private readonly BlackjackClient _client;

    /// <summary>
    /// 연결 URL 등 클라이언트 옵션입니다.
    /// </summary>
    private readonly BlackjackClientOptions _options;

    /// <summary>
    /// 세션 서비스를 만들고 내부 이벤트 연결을 설정합니다.
    /// </summary>
    /// <param name="client">블랙잭 클라이언트입니다.</param>
    /// <param name="options">클라이언트 옵션입니다.</param>
    public FrontendGameSession(BlackjackClient client, BlackjackClientOptions options)
    {
        _client = client;
        _options = options;
        _client.StateChanged += HandleStateChanged;
        _client.Error += HandleError;
    }

    /// <summary>
    /// 상태 변경을 화면으로 알리는 이벤트입니다.
    /// </summary>
    public event Action<GameState>? StateChanged;

    /// <summary>
    /// 오류를 화면으로 알리는 이벤트입니다.
    /// </summary>
    public event Action<string, string>? ErrorReceived;

    /// <summary>
    /// 서버 연결 여부입니다.
    /// </summary>
    public bool IsConnected { get; private set; }

    /// <summary>
    /// 게임 입장 여부입니다.
    /// </summary>
    public bool IsJoined { get; private set; }

    /// <summary>
    /// 아직 연결되지 않았다면 서버에 연결합니다.
    /// </summary>
    /// <returns>비동기 완료를 나타내는 작업입니다.</returns>
    public async Task ConnectAsync()
    {
        if (IsConnected)
        {
            return;
        }

        await _client.ConnectAsync(_options.HubUrl);
        IsConnected = true;
    }

    /// <summary>
    /// 서버에 입장 요청을 보내고 입장 상태를 갱신합니다.
    /// </summary>
    /// <param name="name">플레이어 이름입니다.</param>
    /// <param name="dealerKey">딜러 키입니다.</param>
    /// <returns>비동기 완료를 나타내는 작업입니다.</returns>
    public async Task JoinAsync(string name, string? dealerKey)
    {
        await _client.JoinAsync(name, dealerKey);
        IsJoined = true;
    }

    /// <summary>
    /// 서버에 퇴장 요청을 보내고 입장 상태를 해제합니다.
    /// </summary>
    /// <returns>비동기 완료를 나타내는 작업입니다.</returns>
    public async Task LeaveAsync()
    {
        await _client.LeaveAsync();
        IsJoined = false;
    }

    /// <summary>
    /// 라운드 시작 요청을 서버에 보냅니다.
    /// </summary>
    /// <returns>비동기 완료를 나타내는 작업입니다.</returns>
    public Task StartRoundAsync() => _client.StartRoundAsync();

    /// <summary>
    /// 히트 요청을 서버에 보냅니다.
    /// </summary>
    /// <returns>비동기 완료를 나타내는 작업입니다.</returns>
    public Task HitAsync() => _client.HitAsync();

    /// <summary>
    /// 스탠드 요청을 서버에 보냅니다.
    /// </summary>
    /// <returns>비동기 완료를 나타내는 작업입니다.</returns>
    public Task StandAsync() => _client.StandAsync();

    /// <summary>
    /// 내부 이벤트 구독을 해제합니다.
    /// </summary>
    /// <returns>비동기 완료를 나타내는 작업입니다.</returns>
    public ValueTask DisposeAsync()
    {
        _client.StateChanged -= HandleStateChanged;
        _client.Error -= HandleError;
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// 내부 상태 변경 이벤트를 외부 이벤트로 전달합니다.
    /// </summary>
    /// <param name="state">최신 게임 상태입니다.</param>
    private void HandleStateChanged(GameState state)
    {
        StateChanged?.Invoke(state);
    }

    /// <summary>
    /// 내부 오류 이벤트를 외부 이벤트로 전달하고 입장 상태를 보정합니다.
    /// </summary>
    /// <param name="code">오류 코드입니다.</param>
    /// <param name="message">오류 메시지입니다.</param>
    private void HandleError(string code, string message)
    {
        if (code == "GAME_TERMINATED" || code == "NOT_JOINED")
        {
            IsJoined = false;
        }

        ErrorReceived?.Invoke(code, message);
    }
}
