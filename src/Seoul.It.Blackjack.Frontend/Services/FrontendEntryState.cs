namespace Seoul.It.Blackjack.Frontend.Services;

/// <summary>
/// Entry 페이지에서 입력한 값을 Table 페이지로 전달하기 위한 상태 객체입니다.
/// </summary>
public sealed class FrontendEntryState
{
    /// <summary>
    /// 사용자가 입력한 플레이어 이름입니다.
    /// </summary>
    public string PlayerName { get; set; } = string.Empty;

    /// <summary>
    /// 사용자가 입력한 딜러 키입니다.
    /// </summary>
    public string DealerKey { get; set; } = string.Empty;
}
