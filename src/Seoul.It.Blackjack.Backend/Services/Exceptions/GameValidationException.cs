namespace Seoul.It.Blackjack.Backend.Services.Exceptions;

/// <summary>
/// 입력값이 올바르지 않을 때 사용하는 예외입니다.
/// </summary>
internal sealed class GameValidationException : GameRoomException
{
    /// <summary>
    /// 코드와 메시지를 지정해 검증 예외를 만듭니다.
    /// </summary>
    /// <param name="code">오류 코드입니다.</param>
    /// <param name="message">오류 메시지입니다.</param>
    public GameValidationException(string code, string message)
        : base(code, message)
    {
    }
}
