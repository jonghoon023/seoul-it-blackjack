using Seoul.It.Blackjack.Backend.Extensions;
using Seoul.It.Blackjack.Backend.Hubs;
using Seoul.It.Blackjack.Backend.Services;

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
        builder.Services.AddSingleton<ConnectionRegistry>();
        builder.Services.AddSingleton<GameRoomService>();

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
