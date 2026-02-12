using Seoul.It.Blackjack.Backend.Extensions;
using Seoul.It.Blackjack.Backend.Hubs;
using Seoul.It.Blackjack.Backend.Models;

namespace Seoul.It.Blackjack.Backend;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddSignalR();

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddDealerOptions(builder.Configuration);
        builder.Services.AddSingleton<GameRoom>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.MapHub<GameSessionHub>(GameSessionHub.Endpoint);

        app.UseHttpsRedirection();

        app.MapControllers();

        app.Run();
    }
}