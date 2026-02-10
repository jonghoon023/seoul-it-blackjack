using Microsoft.AspNetCore.SignalR;

namespace Seoul.It.Blackjack.Backend.Hubs;

public class GameSessionHub : Hub
{
    public GameSessionHub(IConfiguration configuration)
    {
        _dealerKey = configuration["DealerKey"] ?? string.Empty;
    }
}
