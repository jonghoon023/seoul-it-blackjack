using Oxygen.Middleware.Api.Client.Services;

namespace Oxygen.Middleware.Api.Client.Options;

/// <summary>
/// <see cref="IMiddlewareApiClient" /> 에서 사용할 정보가 있는 Options Class 입니다.
/// </summary>
public sealed class MiddlewareApiClientOptions
{
    /// <summary>
	/// 기본 Section 이름입니다.
	/// </summary>
	public const string DefaultSectionName = MiddlewareApiClient.Name;

    /// <summary>
    /// Middleware API Server 와 통신하는 주소를 가져오거나 지정합니다.
    /// </summary>
    public Uri? BaseUrl { get; set; }
}