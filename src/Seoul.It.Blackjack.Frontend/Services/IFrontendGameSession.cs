using Seoul.It.Blackjack.Core.Contracts;
using System;
using System.Threading.Tasks;

namespace Seoul.It.Blackjack.Frontend.Services;

/// <summary>
/// 프론트엔드 화면이 게임 세션을 다룰 때 사용하는 서비스 계약입니다.
/// </summary>
public interface IFrontendGameSession : IAsyncDisposable
{
    /// <summary>
    /// 상태가 바뀌었을 때 알리는 이벤트입니다.
    /// </summary>
    event Action<GameState>? StateChanged;

    /// <summary>
    /// 오류를 받았을 때 알리는 이벤트입니다.
    /// </summary>
    event Action<string, string>? ErrorReceived;

    /// <summary>
    /// 서버 연결 여부입니다.
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// 게임 입장 여부입니다.
    /// </summary>
    bool IsJoined { get; }

    /// <summary>
    /// 서버에 연결합니다.
    /// </summary>
    /// <returns>비동기 완료를 나타내는 작업입니다.</returns>
    Task ConnectAsync();

    /// <summary>
    /// 이름과 딜러 키로 게임에 입장합니다.
    /// </summary>
    /// <param name="name">플레이어 이름입니다.</param>
    /// <param name="dealerKey">딜러 키입니다.</param>
    /// <returns>비동기 완료를 나타내는 작업입니다.</returns>
    Task JoinAsync(string name, string? dealerKey);

    /// <summary>
    /// 게임에서 나갑니다.
    /// </summary>
    /// <returns>비동기 완료를 나타내는 작업입니다.</returns>
    Task LeaveAsync();

    /// <summary>
    /// 라운드 시작 요청을 보냅니다.
    /// </summary>
    /// <returns>비동기 완료를 나타내는 작업입니다.</returns>
    Task StartRoundAsync();

    /// <summary>
    /// 히트 요청을 보냅니다.
    /// </summary>
    /// <returns>비동기 완료를 나타내는 작업입니다.</returns>
    Task HitAsync();

    /// <summary>
    /// 스탠드 요청을 보냅니다.
    /// </summary>
    /// <returns>비동기 완료를 나타내는 작업입니다.</returns>
    Task StandAsync();
}
