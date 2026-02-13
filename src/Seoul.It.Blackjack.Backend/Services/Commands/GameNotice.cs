namespace Seoul.It.Blackjack.Backend.Services.Commands;

internal sealed class GameNotice
{
    public GameNotice(string code, string message)
    {
        Code = code;
        Message = message;
    }

    public string Code { get; }

    public string Message { get; }
}
