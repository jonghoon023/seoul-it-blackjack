namespace Seoul.It.Blackjack.Backend.Services.Exceptions;

internal sealed class GameAuthorizationException : GameRoomException
{
    public GameAuthorizationException(string code, string message)
        : base(code, message)
    {
    }
}
