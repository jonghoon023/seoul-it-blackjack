namespace Seoul.It.Blackjack.Frontend.Options;

/// <summary>
/// 프론트엔드에서 블랙잭 클라이언트 연결에 쓰는 옵션입니다.
/// </summary>
public sealed class FrontendBlackjackOptions
{
    /// <summary>
    /// 설정 파일에서 옵션 섹션을 찾을 때 쓰는 이름입니다.
    /// </summary>
    public const string DefaultSectionName = "BlackjackClient";

    /// <summary>
    /// 기본 허브 URL입니다.
    /// </summary>
    public const string DefaultHubUrl = "http://localhost:5000/blackjack";

    /// <summary>
    /// 실제 연결에 사용할 허브 URL입니다.
    /// </summary>
    public string HubUrl { get; set; } = DefaultHubUrl;
}
