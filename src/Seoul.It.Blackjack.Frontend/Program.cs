using Seoul.It.Blackjack.Frontend.Components;
using Seoul.It.Blackjack.Frontend.Extensions;

namespace Seoul.It.Blackjack.Frontend;

public partial class Program
{
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
