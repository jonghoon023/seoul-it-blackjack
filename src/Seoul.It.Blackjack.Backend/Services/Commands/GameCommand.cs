namespace Seoul.It.Blackjack.Backend.Services.Commands;

internal sealed class GameCommand
{
    public GameCommand(
        GameCommandType type,
        string connectionId,
        string? name = null,
        string? dealerKey = null)
    {
        Type = type;
        ConnectionId = connectionId;
        Name = name;
        DealerKey = dealerKey;
    }

    public GameCommandType Type { get; }

    public string ConnectionId { get; }

    public string? Name { get; }

    public string? DealerKey { get; }
}
