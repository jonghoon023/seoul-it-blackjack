namespace Seoul.It.Blackjack.Client.Options;

/// <summary>
/// 블랙잭 클라이언트 연결 설정을 담는 옵션 클래스입니다.
/// </summary>
public sealed class BlackjackClientOptions
{
    /// <summary>
    /// 연결할 SignalR 허브 URL입니다.
    /// </summary>
    public string HubUrl { get; set; } = string.Empty;
}
