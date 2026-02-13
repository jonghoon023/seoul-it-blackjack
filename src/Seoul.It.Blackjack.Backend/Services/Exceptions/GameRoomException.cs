using System;

namespace Seoul.It.Blackjack.Backend.Services.Exceptions;

internal class GameRoomException : Exception
{
    public GameRoomException(string code, string message)
        : base(message)
    {
        Code = code;
    }

    public string Code { get; }
}
