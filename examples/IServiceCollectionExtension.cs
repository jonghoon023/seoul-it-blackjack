using Microsoft.Extensions.Options;
using Oxygen.Middleware.Api.Client.Options;
using Oxygen.Middleware.Api.Client.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Oxygen.Middleware.Api.Client.Extensions;

/// <summary>
/// <see cref="IServiceCollection" /> 에 대한 확장 함수가 있는 정적 Class 입니다.
/// </summary>
public static class IServiceCollectionExtension
{
    /// <summary>
	/// API 요청 후 최대 기다리는 시간입니다.
	/// </summary>
	/// <seealso href="https://docs.microsoft.com/ko-kr/dotnet/api/system.net.http.httpclient.timeout" />
	private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(100);

    /// <summary>
    /// Middleware API Server 와 통신하는 <see cref="IMiddlewareApiClient" /> 를 등록합니다.
    /// </summary>
    /// <typeparam name="TMessageHandler"> API 통신을 보내기 전에 요청을 가공할 <see cref="DelegatingHandler" /> 를 상속받은 객체의 형식입니다. </typeparam>
    /// <param name="services"> <see cref="IServiceCollection" /> 구현체입니다. </param>
    /// <returns> <see cref="IMiddlewareApiClient" /> 를 등록 후 <see cref="IServiceCollection" /> 구현체를 반환합니다. </returns>
    public static IServiceCollection AddMiddlewareApiClient<TMessageHandler>(this IServiceCollection services)
        where TMessageHandler : DelegatingHandler
    {
        return services.AddMiddlewareApiClient<TMessageHandler>(Timeout);
    }

    /// <summary>
    /// Middleware API Server 와 통신하는 <see cref="IMiddlewareApiClient" /> 를 등록합니다.
    /// </summary>
    /// <typeparam name="TMessageHandler"> API 통신을 보내기 전에 요청을 가공할 <see cref="DelegatingHandler" /> 를 상속받은 객체의 형식입니다. </typeparam>
    /// <param name="services"> <see cref="IServiceCollection" /> 구현체입니다. </param>
    /// <param name="timeout"> API 요청 후 최대 기다리는 시간입니다. </param>
    /// <returns> <see cref="IMiddlewareApiClient" /> 를 등록 후 <see cref="IServiceCollection" /> 구현체를 반환합니다. </returns>
    public static IServiceCollection AddMiddlewareApiClient<TMessageHandler>(this IServiceCollection services, TimeSpan timeout)
        where TMessageHandler : DelegatingHandler
    {
        services
            .AddHttpClient(timeout)
            .AddHttpMessageHandler<TMessageHandler>();

        return services.AddScoped<IMiddlewareApiClient, MiddlewareApiClient>();
    }

    /// <summary>
    /// Middleware API Server 와 통신하는 <see cref="IMiddlewareApiClient" /> 를 등록합니다.
    /// </summary>
    /// <typeparam name="TMessageHandler"> API 통신을 보내기 전에 요청을 가공할 <see cref="DelegatingHandler" /> 를 상속받은 객체의 형식입니다. </typeparam>
    /// <param name="services"> <see cref="IServiceCollection" /> 구현체입니다. </param>
    /// <param name="configureHandler"> <typeparamref name="TMessageHandler" /> 를 반환하는 <see cref="Func{T, TResult}" /> 입니다. </param>
    /// <returns> <see cref="IMiddlewareApiClient" /> 를 등록 후 <see cref="IServiceCollection" /> 구현체를 반환합니다. </returns>
    public static IServiceCollection AddMiddlewareApiClient<TMessageHandler>(this IServiceCollection services, Func<IServiceProvider, TMessageHandler> configureHandler)
        where TMessageHandler : DelegatingHandler
    {
        return services.AddMiddlewareApiClient(Timeout, configureHandler);
    }

    /// <summary>
    /// Middleware API Server 와 통신하는 <see cref="IMiddlewareApiClient" /> 를 등록합니다.
    /// </summary>
    /// <typeparam name="TMessageHandler"> API 통신을 보내기 전에 요청을 가공할 <see cref="DelegatingHandler" /> 를 상속받은 객체의 형식입니다. </typeparam>
    /// <param name="services"> <see cref="IServiceCollection" /> 구현체입니다. </param>
    /// <param name="timeout"> API 요청 후 최대 기다리는 시간입니다. </param>
    /// <param name="configureHandler"> <typeparamref name="TMessageHandler" /> 를 반환하는 <see cref="Func{T, TResult}" /> 입니다. </param>
    /// <returns> <see cref="IMiddlewareApiClient" /> 를 등록 후 <see cref="IServiceCollection" /> 구현체를 반환합니다. </returns>
    public static IServiceCollection AddMiddlewareApiClient<TMessageHandler>(this IServiceCollection services, TimeSpan timeout, Func<IServiceProvider, TMessageHandler> configureHandler)
        where TMessageHandler : DelegatingHandler
    {
        services
            .AddHttpClient(timeout)
            .AddHttpMessageHandler(configureHandler);

        return services.AddScoped<IMiddlewareApiClient, MiddlewareApiClient>();
    }

    /// <summary>
    /// <see cref="HttpClient" /> 를 생성하는 <see cref="IHttpClientBuilder" /> 를 등록합니다.
    /// </summary>
    /// <param name="services"> <see cref="IServiceCollection" /> 구현체입니다. </param>
    /// <param name="timeout"> API 요청 후 최대 기다리는 시간입니다. </param>
    /// <returns> <see cref="IHttpClientBuilder" /> 를 등록 후 <see cref="IHttpClientBuilder" /> 구현체를 반환합니다. </returns>
    private static IHttpClientBuilder AddHttpClient(this IServiceCollection services, TimeSpan timeout)
    {
        return services
            .AddHttpClient(MiddlewareApiClient.Name)
            .ConfigureHttpClient((provider, httpClient) =>
            {
                var options = provider.GetRequiredService<IOptions<MiddlewareApiClientOptions>>().Value;
                httpClient.BaseAddress = options.BaseUrl;
                httpClient.Timeout = timeout;
            });
    }
}