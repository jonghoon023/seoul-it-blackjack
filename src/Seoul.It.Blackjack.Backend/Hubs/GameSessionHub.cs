using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Seoul.It.Blackjack.Backend.Models;
using Seoul.It.Blackjack.Backend.Options;
using Seoul.It.Blackjack.Core.Contracts;
using System.Collections.Concurrent;

namespace Seoul.It.Blackjack.Backend.Hubs;

internal sealed class GameSessionHub(ILogger<GameSessionHub> logger, IOptions<DealerOptions> options) : Hub<IBlackjackClient>
{
    private readonly ConcurrentDictionary<string, Player> _players = [];
    private readonly Shoe _shoe = new(4);

    public override Task OnConnectedAsync()
    {
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        _players.Remove(Context.ConnectionId, out Player player);
        logger.LogInformation("Remove player {Name}", player.Name);
        return base.OnDisconnectedAsync(exception);
    }

    public Task JoinAsync(string name)
    {
        return Task.CompletedTask;
    }
}
