using System;

namespace Seoul.It.Blackjack.Backend.Services.Exceptions;

/// <summary>
/// 게임 방 처리 중 발생하는 사용자 정의 예외의 기본 클래스입니다.
/// </summary>
internal class GameRoomException : Exception
{
    /// <summary>
    /// 코드와 메시지를 지정해 예외를 만듭니다.
    /// </summary>
    /// <param name="code">오류 코드입니다.</param>
    /// <param name="message">오류 메시지입니다.</param>
    public GameRoomException(string code, string message)
        : base(message)
    {
        Code = code;
    }

    /// <summary>
    /// 클라이언트에 전달할 오류 코드입니다.
    /// </summary>
    public string Code { get; }
}
