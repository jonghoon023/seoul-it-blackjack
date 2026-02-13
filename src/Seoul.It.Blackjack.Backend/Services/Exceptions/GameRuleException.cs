namespace Seoul.It.Blackjack.Backend.Services.Exceptions;

internal sealed class GameRuleException : GameRoomException
{
    public GameRuleException(string code, string message)
        : base(code, message)
    {
    }
}
