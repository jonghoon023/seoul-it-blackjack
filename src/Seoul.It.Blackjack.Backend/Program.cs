using Seoul.It.Blackjack.Backend.Extensions;
using Seoul.It.Blackjack.Backend.Hubs;
using Seoul.It.Blackjack.Backend.Services;
using Seoul.It.Blackjack.Backend.Services.Infrastructure;
using Seoul.It.Blackjack.Backend.Services.Round;
using Seoul.It.Blackjack.Backend.Services.Rules;
using Seoul.It.Blackjack.Backend.Services.State;

namespace Seoul.It.Blackjack.Backend;

/// <summary>
/// 백엔드 서버 시작 지점을 담는 클래스입니다.
/// </summary>
public partial class Program
{
    /// <summary>
    /// 백엔드 웹 애플리케이션을 구성하고 실행합니다.
    /// </summary>
    /// <param name="args">프로그램 시작 인자입니다.</param>
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
        builder.Services.AddSingleton<IGameCommandProcessor, ChannelGameCommandProcessor>();
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
