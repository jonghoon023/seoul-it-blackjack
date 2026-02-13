using System;
using Microsoft.Extensions.DependencyInjection;
using Seoul.It.Blackjack.Client.Options;

namespace Seoul.It.Blackjack.Client.Extensions;

/// <summary>
/// 블랙잭 클라이언트를 DI에 등록하기 위한 확장 메서드 모음입니다.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 옵션을 적용해 <see cref="BlackjackClient" />를 DI 컨테이너에 등록합니다.
    /// </summary>
    /// <param name="services">서비스 컬렉션입니다.</param>
    /// <param name="configure">클라이언트 옵션 설정 함수입니다.</param>
    /// <returns>같은 서비스 컬렉션을 반환합니다.</returns>
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
        services.AddScoped<BlackjackClient>();
        return services;
    }
}
