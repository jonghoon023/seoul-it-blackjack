using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Seoul.It.Blackjack.Client.Extensions;
using Seoul.It.Blackjack.Frontend.Options;
using Seoul.It.Blackjack.Frontend.Services;

namespace Seoul.It.Blackjack.Frontend.Extensions;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFrontendBlackjackOptions(this IServiceCollection services, IConfiguration configuration)
    {
        return services.Configure<FrontendBlackjackOptions>(
            configuration.GetSection(FrontendBlackjackOptions.DefaultSectionName));
    }

    public static IServiceCollection AddFrontendBlackjackClient(this IServiceCollection services, IConfiguration configuration)
    {
        FrontendBlackjackOptions options = new();
        configuration
            .GetSection(FrontendBlackjackOptions.DefaultSectionName)
            .Bind(options);
        if (string.IsNullOrWhiteSpace(options.HubUrl))
        {
            options.HubUrl = FrontendBlackjackOptions.DefaultHubUrl;
        }

        return services.AddBlackjackClient(clientOptions =>
        {
            clientOptions.HubUrl = options.HubUrl;
        });
    }

    public static IServiceCollection AddFrontendServices(this IServiceCollection services)
    {
        services.AddScoped<FrontendEntryState>();
        services.AddScoped<IFrontendGameSession, FrontendGameSession>();
        return services;
    }
}
