namespace Seoul.It.Blackjack.Backend.Services.Exceptions;

/// <summary>
/// 게임 규칙을 어긴 요청일 때 사용하는 예외입니다.
/// </summary>
internal sealed class GameRuleException : GameRoomException
{
    /// <summary>
    /// 코드와 메시지를 지정해 규칙 예외를 만듭니다.
    /// </summary>
    /// <param name="code">오류 코드입니다.</param>
    /// <param name="message">오류 메시지입니다.</param>
    public GameRuleException(string code, string message)
        : base(code, message)
    {
    }
}
