using Seoul.It.Blackjack.Backend.Options;

namespace Seoul.It.Blackjack.Backend.Extensions;

/// <summary>
/// 백엔드에서 쓰는 옵션 객체를 DI에 등록하는 도우미 메서드 모음입니다.
/// </summary>
internal static class ServiceCollectionExtensions
{
    /// <summary>
    /// 딜러 관련 설정을 DI 옵션으로 등록합니다.
    /// </summary>
    /// <param name="services">서비스 컬렉션입니다.</param>
    /// <param name="configuration">앱 설정 값입니다.</param>
    /// <returns>같은 서비스 컬렉션을 반환합니다.</returns>
    public static IServiceCollection AddDealerOptions(this IServiceCollection services, IConfiguration configuration)
    {
        return services.Configure<DealerOptions>(configuration.GetSection(DealerOptions.DefaultSectionName));
    }

    /// <summary>
    /// 게임 규칙 관련 설정을 DI 옵션으로 등록합니다.
    /// </summary>
    /// <param name="services">서비스 컬렉션입니다.</param>
    /// <param name="configuration">앱 설정 값입니다.</param>
    /// <returns>같은 서비스 컬렉션을 반환합니다.</returns>
    public static IServiceCollection AddGameRuleOptions(this IServiceCollection services, IConfiguration configuration)
    {
        return services.Configure<GameRuleOptions>(configuration.GetSection(GameRuleOptions.DefaultSectionName));
    }
}
