using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Seoul.It.Blackjack.Client.Extensions;
using Seoul.It.Blackjack.Frontend.Options;
using Seoul.It.Blackjack.Frontend.Services;

namespace Seoul.It.Blackjack.Frontend.Extensions;

/// <summary>
/// 프론트엔드에서 쓰는 옵션/서비스를 DI에 등록하는 확장 메서드 모음입니다.
/// </summary>
internal static class ServiceCollectionExtensions
{
    /// <summary>
    /// 프론트엔드 블랙잭 옵션을 설정 파일에서 읽어 DI에 등록합니다.
    /// </summary>
    /// <param name="services">서비스 컬렉션입니다.</param>
    /// <param name="configuration">앱 설정 값입니다.</param>
    /// <returns>같은 서비스 컬렉션을 반환합니다.</returns>
    public static IServiceCollection AddFrontendBlackjackOptions(this IServiceCollection services, IConfiguration configuration)
    {
        return services.Configure<FrontendBlackjackOptions>(
            configuration.GetSection(FrontendBlackjackOptions.DefaultSectionName));
    }

    /// <summary>
    /// 프론트엔드가 사용할 블랙잭 클라이언트를 등록합니다.
    /// </summary>
    /// <param name="services">서비스 컬렉션입니다.</param>
    /// <param name="configuration">앱 설정 값입니다.</param>
    /// <returns>같은 서비스 컬렉션을 반환합니다.</returns>
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

    /// <summary>
    /// 프론트엔드 화면에서 사용할 상태/세션 서비스를 등록합니다.
    /// </summary>
    /// <param name="services">서비스 컬렉션입니다.</param>
    /// <returns>같은 서비스 컬렉션을 반환합니다.</returns>
    public static IServiceCollection AddFrontendServices(this IServiceCollection services)
    {
        services.AddScoped<FrontendEntryState>();
        services.AddScoped<IFrontendGameSession, FrontendGameSession>();
        return services;
    }
}
