namespace Seoul.It.Blackjack.Backend.Services.Exceptions;

/// <summary>
/// 권한이 없는 요청이 들어왔을 때 사용하는 예외입니다.
/// </summary>
internal sealed class GameAuthorizationException : GameRoomException
{
    /// <summary>
    /// 코드와 메시지를 지정해 권한 예외를 만듭니다.
    /// </summary>
    /// <param name="code">오류 코드입니다.</param>
    /// <param name="message">오류 메시지입니다.</param>
    public GameAuthorizationException(string code, string message)
        : base(code, message)
    {
    }
}
