using Seoul.It.Blackjack.Backend.Options;

namespace Seoul.It.Blackjack.Backend.Extensions;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDealerOptions(this IServiceCollection services, IConfiguration configuration)
    {
        return services.Configure<DealerOptions>(configuration.GetSection(DealerOptions.DefaultSectionName));
    }

    public static IServiceCollection AddGameRuleOptions(this IServiceCollection services, IConfiguration configuration)
    {
        return services.Configure<GameRuleOptions>(configuration.GetSection(GameRuleOptions.DefaultSectionName));
    }
}
