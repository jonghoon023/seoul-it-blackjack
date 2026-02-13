using Seoul.It.Blackjack.Frontend.Components;
using Seoul.It.Blackjack.Frontend.Extensions;

namespace Seoul.It.Blackjack.Frontend;

/// <summary>
/// 프론트엔드 웹 앱 시작 지점을 담는 클래스입니다.
/// </summary>
public partial class Program
{
    /// <summary>
    /// 프론트엔드 웹 애플리케이션을 구성하고 실행합니다.
    /// </summary>
    /// <param name="args">프로그램 시작 인자입니다.</param>
    private static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        builder.Services.AddFrontendBlackjackOptions(builder.Configuration);
        builder.Services.AddFrontendBlackjackClient(builder.Configuration);
        builder.Services.AddFrontendServices();

        WebApplication app = builder.Build();

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseAntiforgery();

        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        app.Run();
    }
}
