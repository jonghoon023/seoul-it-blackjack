namespace Seoul.It.Blackjack.Backend.Services.Exceptions;

internal sealed class GameValidationException : GameRoomException
{
    public GameValidationException(string code, string message)
        : base(code, message)
    {
    }
}
