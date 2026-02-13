namespace Seoul.It.Blackjack.Backend.Services.Commands;

/// <summary>
/// 모든 사용자에게 함께 알릴 공지 메시지 데이터입니다.
/// </summary>
internal sealed class GameNotice
{
    /// <summary>
    /// 공지 데이터를 만듭니다.
    /// </summary>
    /// <param name="code">공지 코드입니다.</param>
    /// <param name="message">공지 내용입니다.</param>
    public GameNotice(string code, string message)
    {
        Code = code;
        Message = message;
    }

    /// <summary>
    /// 공지 코드입니다.
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// 공지 내용입니다.
    /// </summary>
    public string Message { get; }
}
