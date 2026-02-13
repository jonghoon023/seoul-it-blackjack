using Seoul.It.Blackjack.Backend.Extensions;
using Seoul.It.Blackjack.Backend.Hubs;
using Seoul.It.Blackjack.Backend.Services;
using Seoul.It.Blackjack.Backend.Services.Round;
using Seoul.It.Blackjack.Backend.Services.Rules;
using Seoul.It.Blackjack.Backend.Services.State;

namespace Seoul.It.Blackjack.Backend;

public partial class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddSignalR();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddDealerOptions(builder.Configuration);
        builder.Services.AddGameRuleOptions(builder.Configuration);
        builder.Services.AddSingleton<ConnectionRegistry>();
        builder.Services.AddSingleton<IGameRuleValidator, GameRuleValidator>();
        builder.Services.AddSingleton<IRoundEngine, RoundEngine>();
        builder.Services.AddSingleton<IGameStateSnapshotFactory, GameStateSnapshotFactory>();
        builder.Services.AddSingleton<IGameRoomService, GameRoomService>();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.MapHub<GameSessionHub>(GameSessionHub.Endpoint);
        app.Run();
    }
}
