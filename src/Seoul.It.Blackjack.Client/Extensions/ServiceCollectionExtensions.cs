using Microsoft.Extensions.DependencyInjection;
using Seoul.It.Blackjack.Client.Options;
using System;

namespace Seoul.It.Blackjack.Client.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBlackjackClient(
        this IServiceCollection services,
        Action<BlackjackClientOptions> configure)
    {
        if (services is null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (configure is null)
        {
            throw new ArgumentNullException(nameof(configure));
        }

        BlackjackClientOptions options = new();
        configure(options);

        services.AddSingleton(options);
        services.AddSingleton<BlackjackClient>();
        return services;
    }
}
