using System.Text.Json;
using System.Net.Http.Json;
using Oxygen.Jira.Sdk.Models.Enums;
using Mangoslab.Drawing.Layers.Interfaces;
using Oxygen.Middleware.Api.Client.Models;
using Oxygen.Middleware.Api.Client.Requests;
using Oxygen.Middleware.Api.Client.Responses;
using Oxygen.Middleware.Api.Client.Extensions;
using Oxygen.Middleware.Api.Client.Models.Jira;
using Oxygen.Middleware.Api.Client.Models.Slack;
using Oxygen.Middleware.Api.Client.Models.Teams;
using Oxygen.Middleware.Api.Client.Models.Notion;
using Oxygen.Middleware.Api.Client.Models.Trello;
using Oxygen.Middleware.Api.Client.Models.Status;
using Oxygen.Middleware.Api.Client.Models.Evernote;
using Oxygen.Middleware.Api.Client.Models.MainServer;

namespace Oxygen.Middleware.Api.Client.Services;

/// <summary>
/// <see cref="IMiddlewareApiClient" /> 의 구현체 Class 입니다.
/// </summary>
internal class MiddlewareApiClient : IMiddlewareApiClient
{
    internal const string Name = "MiddlewareApi";
    private const string ExceptionMessageFromDeserializedContentIsNull = "응답은 올바르게 받아왔으나 반환할 객체로 역직렬화하지 못했습니다.";

    /// <summary>
    /// <para> API 요청을 보낼 때 사용하는 <see cref="HttpClient" /> 객체입니다. </para>
    /// <para> * <see cref="HttpClient.Dispose(bool)" /> 함수를 호출하지 않습니다. 자세한 내용은 seealso 항목을 참고하세요. * </para>
    /// </summary>
    /// <seealso href="https://docs.microsoft.com/ko-kr/aspnet/core/fundamentals/http-requests?view=aspnetcore-2.1#httpclient-and-lifetime-management-3" />
    private readonly HttpClient _httpClient;

    /// <summary>
    /// <see cref="MiddlewareApiClient" /> 를 초기화합니다.
    /// </summary>
    /// <param name="httpClientFactory"> <see cref="HttpClient" /> 를 생성하는 <see cref="IHttpClientFactory" /> 구현체입니다. </param>
    public MiddlewareApiClient(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient(Name);
    }

    /// <inheritdoc cref="IDisposable.Dispose" />
    public void Dispose()
    {
        _httpClient.Dispose();
        GC.SuppressFinalize(this);
    }

    #region Common
    /// <inheritdoc cref="IMiddlewareApiClient.GetLoginUrlAsync(PlatformType, string)" />
    public async Task<string> GetLoginUrlAsync(PlatformType platform, string callbackUrl)
    {
        var requestUrl = GetRequestUri($"api/{platform}/login-url", new Dictionary<string, object>
        {
            { nameof(callbackUrl), callbackUrl }
        });

        using var response = await _httpClient.GetAsync(requestUrl).ConfigureAwait(false);
        if (response.IsSuccessStatusCode)
        {
            var loginUrlResponse = await response.Content.ReadFromJsonAsync<LoginUrlResponse>().ConfigureAwait(false);
            return loginUrlResponse?.Url ?? string.Empty;
        }

        return string.Empty;
    }

    /// <inheritdoc cref="IMiddlewareApiClient.ValidateAsync(PlatformType)" />
    public async Task<bool> ValidateAsync(PlatformType platform)
    {
        using var response = await _httpClient.GetAsync($"api/{platform}/validate").ConfigureAwait(false);
        return response.IsSuccessStatusCode;
    }

    /// <inheritdoc cref="IMiddlewareApiClient.RegisterAsync(PlatformType, Uri)" />
    public async Task<bool> RegisterAsync(PlatformType platform, Uri callbackUrl)
    {
        var requestUrl = GetRequestUri($"api/{platform}/register", new Dictionary<string, object>
        {
            { nameof(callbackUrl), callbackUrl.GetLeftPart(UriPartial.Path) }
        });

        using var payload = JsonContent.Create(new RegisterRequest(callbackUrl));
        using var response = await _httpClient.PutAsync(requestUrl, payload).ConfigureAwait(false);

        return response.IsSuccessStatusCode;
    }

    /// <inheritdoc cref="IMiddlewareApiClient.UnregisterAsync(PlatformType)" />
    public async Task<bool> UnregisterAsync(PlatformType platform)
    {
        using var response = await _httpClient.DeleteAsync($"api/{platform}/unregister").ConfigureAwait(false);
        return response.IsSuccessStatusCode;
    }
    #endregion

    #region Status
    /// <inheritdoc cref="IMiddlewareApiClient.GetMainServerStatusAsync" />
    public async Task<ServerStatus> GetMainServerStatusAsync()
    {
        return await _httpClient.GetFromJsonAsync<ServerStatus>("api/status").ConfigureAwait(false)
            ?? throw new JsonException(ExceptionMessageFromDeserializedContentIsNull);
    }

    /// <inheritdoc cref="IMiddlewareApiClient.GetApiServerStatusAsync(PlatformType)" />
    public async Task<ServerStatus> GetApiServerStatusAsync(PlatformType platform)
    {
        return await _httpClient.GetFromJsonAsync<ServerStatus>($"api/status/{platform}").ConfigureAwait(false)
            ?? throw new JsonException(ExceptionMessageFromDeserializedContentIsNull);
    }
    #endregion

    #region Main Server
    /// <inheritdoc cref="IMiddlewareApiClient.AuthenticateAsync" />
    public async Task<bool> AuthenticateAsync()
    {
        using var response = await _httpClient.PutAsync("api/authenticate", null).ConfigureAwait(false);
        return response.IsSuccessStatusCode;
    }

    /// <inheritdoc cref="IMiddlewareApiClient.GetWeatherForecastsAsync" />
    public async IAsyncEnumerable<WeatherForecast> GetWeatherForecastsAsync()
    {
        using var response = await _httpClient.GetAsync("api/weather-forecast").ConfigureAwait(false);
        if (response.IsSuccessStatusCode)
        {
            var weatherForecasts = await response.Content.ReadFromJsonAsync<WeatherForecast[]>().ConfigureAwait(false);
            foreach (var weatherForecast in weatherForecasts ?? Array.Empty<WeatherForecast>())
            {
                yield return weatherForecast;
            }
        }

        yield break;
    }

    /// <inheritdoc cref="IMiddlewareApiClient.GetPrinters" />
    public async IAsyncEnumerable<Printer> GetPrinters()
    {
        using var response = await _httpClient.GetAsync("api/printers").ConfigureAwait(false);
        if (response.IsSuccessStatusCode)
        {
            var printers = await response.Content.ReadFromJsonAsync<Printer[]>().ConfigureAwait(false);
            foreach (var printer in printers ?? Array.Empty<Printer>())
            {
                yield return printer;
            }
        }

        yield break;
    }

    /// <inheritdoc cref="IMiddlewareApiClient.PrintAsync(Guid, ILayer[])" />
    public async Task<bool> PrintAsync(Guid printerId, ILayer[] layers)
    {
        var layersRequest = new LayersRequest(layers);
        using var payload = layersRequest.ToHttpContent();
        using var response = await _httpClient.PostAsync($"api/printers/{printerId}/print", payload).ConfigureAwait(false);

        return response.IsSuccessStatusCode;
    }
    #endregion

    #region Evernote
    /// <inheritdoc cref="IMiddlewareApiClient.GetNotebooksAsync" />
    public async IAsyncEnumerable<Notebook> GetNotebooksAsync()
    {
        using var response = await _httpClient.GetAsync("api/evernote/notebooks").ConfigureAwait(false);

        if (response.IsSuccessStatusCode)
        {
            var notebooks = await response.Content.ReadFromJsonAsync<Notebook[]>().ConfigureAwait(false);
            foreach (var notebook in notebooks ?? Array.Empty<Notebook>())
            {
                yield return notebook;
            }
        }

        yield break;
    }

    /// <inheritdoc cref="IMiddlewareApiClient.GetNotesAsync(Guid)" />
    public async IAsyncEnumerable<Note> GetNotesAsync(Guid notebookGuid)
    {
        using var response = await _httpClient.GetAsync($"api/evernote/notebooks/{notebookGuid}").ConfigureAwait(false);

        if (response.IsSuccessStatusCode)
        {
            var notes = await response.Content.ReadFromJsonAsync<Note[]>().ConfigureAwait(false);
            foreach (var note in notes ?? Array.Empty<Note>())
            {
                yield return note;
            }
        }

        yield break;
    }

    /// <inheritdoc cref="IMiddlewareApiClient.CreateNoteAsync(Guid, ILayer[])" />
    public async Task<bool> CreateNoteAsync(Guid notebookGuid, ILayer[] layers)
    {
        var layersRequest = new LayersRequest(layers);
        using var payload = layersRequest.ToHttpContent();
        using var response = await _httpClient.PostAsync($"api/evernote/notes/{notebookGuid}", payload).ConfigureAwait(false);

        return response.IsSuccessStatusCode;
    }

    /// <inheritdoc cref="IMiddlewareApiClient.ReadNoteAsync(Guid)" />
    public async Task<Content> ReadNoteAsync(Guid noteGuid)
    {
        using var response = await _httpClient.GetAsync($"api/evernote/notes/{noteGuid}").ConfigureAwait(false);
        await response.ThrowIfFailedApiRequestAsync();

        return await response.Content.ReadFromJsonAsync<Content>().ConfigureAwait(false)
            ?? throw new JsonException(ExceptionMessageFromDeserializedContentIsNull);
    }

    /// <inheritdoc cref="IMiddlewareApiClient.UpdateNoteAsync(Guid, ILayer[])" />
    public async Task<bool> UpdateNoteAsync(Guid noteGuid, ILayer[] layers)
    {
        var layersRequest = new LayersRequest(layers);
        using var payload = layersRequest.ToHttpContent();
        using var response = await _httpClient.PutAsync($"api/evernote/notes/{noteGuid}", payload).ConfigureAwait(false);

        return response.IsSuccessStatusCode;
    }

    /// <inheritdoc cref="IMiddlewareApiClient.DeleteNoteAsync(Guid)" />
    public async Task<bool> DeleteNoteAsync(Guid noteGuid)
    {
        using var response = await _httpClient.DeleteAsync($"api/evernote/notes/{noteGuid}").ConfigureAwait(false);
        return response.IsSuccessStatusCode;
    }
    #endregion

    #region Jira
    /// <inheritdoc cref="IMiddlewareApiClient.GetOrganizationsFromJiraAsync" />
    public async IAsyncEnumerable<JiraOrganization> GetOrganizationsFromJiraAsync()
    {
        using var response = await _httpClient.GetAsync("api/jira/organizations").ConfigureAwait(false);

        if (response.IsSuccessStatusCode)
        {
            var organizations = await response.Content.ReadFromJsonAsync<JiraOrganization[]>().ConfigureAwait(false);
            foreach (var organization in organizations ?? Array.Empty<JiraOrganization>())
            {
                yield return organization;
            }
        }

        yield break;
    }

    /// <inheritdoc cref="IMiddlewareApiClient.GetIssuesAsync(Guid)" />
    public async IAsyncEnumerable<Issue> GetIssuesAsync(Guid cloudId)
    {
        using var response = await _httpClient.GetAsync($"api/jira/{cloudId}/issues").ConfigureAwait(false);

        if (response.IsSuccessStatusCode)
        {
            var issues = await response.Content.ReadFromJsonAsync<Issue[]>().ConfigureAwait(false);
            foreach (var issue in issues ?? Array.Empty<Issue>())
            {
                yield return issue;
            }
        }

        yield break;
    }

    /// <inheritdoc cref="IMiddlewareApiClient.CreateIssueAsync(Guid, string, IssueType, ILayer[])" />
    public async Task<bool> CreateIssueAsync(Guid cloudId, string project, IssueType type, ILayer[] layers)
    {
        var layersRequest = new LayersRequest(layers);
        using var payload = layersRequest.ToHttpContent();
        using var response = await _httpClient.PostAsync($"api/jira/{cloudId}?projectKey={project}&type={type}", payload).ConfigureAwait(false);

        return response.IsSuccessStatusCode;
    }

    /// <inheritdoc cref="IMiddlewareApiClient.ReadIssueAsync(Guid, int)" />
    public async Task<Content> ReadIssueAsync(Guid cloudId, int issueId)
    {
        using var response = await _httpClient.GetAsync($"api/jira/{cloudId}/{issueId}").ConfigureAwait(false);
        await response.ThrowIfFailedApiRequestAsync();

        return await response.Content.ReadFromJsonAsync<Content>().ConfigureAwait(false)
            ?? throw new JsonException(ExceptionMessageFromDeserializedContentIsNull);
    }

    /// <inheritdoc cref="IMiddlewareApiClient.ReadIssueAsync(Guid, string)" />
    public async Task<Content> ReadIssueAsync(Guid cloudId, string issueKey)
    {
        using var response = await _httpClient.GetAsync($"api/jira/{cloudId}/{issueKey}").ConfigureAwait(false);
        await response.ThrowIfFailedApiRequestAsync();

        return await response.Content.ReadFromJsonAsync<Content>().ConfigureAwait(false)
            ?? throw new JsonException(ExceptionMessageFromDeserializedContentIsNull);
    }

    /// <inheritdoc cref="IMiddlewareApiClient.UpdateIssueAsync(Guid, int, ILayer[])" />
    public async Task<bool> UpdateIssueAsync(Guid cloudId, int issueId, ILayer[] layers)
    {
        var layersRequest = new LayersRequest(layers);
        using var payload = layersRequest.ToHttpContent();
        using var response = await _httpClient.PutAsync($"api/jira/{cloudId}/{issueId}", payload).ConfigureAwait(false);

        return response.IsSuccessStatusCode;
    }

    /// <inheritdoc cref="IMiddlewareApiClient.DeleteIssueAsync(Guid, int)" />
    public async Task<bool> DeleteIssueAsync(Guid cloudId, int issueId)
    {
        using var response = await _httpClient.DeleteAsync($"api/jira/{cloudId}/{issueId}").ConfigureAwait(false);
        return response.IsSuccessStatusCode;
    }
    #endregion

    #region Notion
    /// <inheritdoc cref="IMiddlewareApiClient.CreatePageAsync(ILayer[])" />
    public async Task<bool> CreatePageAsync(ILayer[] layers)
    {
        var layersRequest = new LayersRequest(layers);
        using var payload = layersRequest.ToHttpContent();
        using var response = await _httpClient.PostAsync($"api/notion/page", payload).ConfigureAwait(false);

        return response.IsSuccessStatusCode;
    }

    /// <inheritdoc cref="IMiddlewareApiClient.CreatePageAsync(Guid, ILayer[])" />
    public async Task<bool> CreatePageAsync(Guid pageId, ILayer[] layers)
    {
        var layersRequest = new LayersRequest(layers);
        using var payload = layersRequest.ToHttpContent();
        using var response = await _httpClient.PostAsync($"api/notion/page/{pageId}", payload).ConfigureAwait(false);

        return response.IsSuccessStatusCode;
    }

    /// <inheritdoc cref="IMiddlewareApiClient.ReadPageAsync()" />
    public async Task<PageContent> ReadPageAsync()
    {
        using var response = await _httpClient.GetAsync($"api/notion/page").ConfigureAwait(false);
        await response.ThrowIfFailedApiRequestAsync();

        return await response.Content.ReadFromJsonAsync<PageContent>().ConfigureAwait(false)
            ?? throw new JsonException(ExceptionMessageFromDeserializedContentIsNull);
    }

    /// <inheritdoc cref="IMiddlewareApiClient.ReadPageAsync(Guid)" />
    public async Task<PageContent> ReadPageAsync(Guid pageId)
    {
        using var response = await _httpClient.GetAsync($"api/notion/page/{pageId}").ConfigureAwait(false);
        await response.ThrowIfFailedApiRequestAsync();

        return await response.Content.ReadFromJsonAsync<PageContent>().ConfigureAwait(false)
            ?? throw new JsonException(ExceptionMessageFromDeserializedContentIsNull);
    }

    /// <inheritdoc cref="IMiddlewareApiClient.UpdatePageAsync(Guid, ILayer[])" />
    public async Task<bool> UpdatePageAsync(Guid pageId, ILayer[] layers)
    {
        var layersRequest = new LayersRequest(layers);
        using var payload = layersRequest.ToHttpContent();
        using var response = await _httpClient.PutAsync($"api/notion/page/{pageId}", payload).ConfigureAwait(false);

        return response.IsSuccessStatusCode;
    }

    /// <inheritdoc cref="IMiddlewareApiClient.DeletePageAsync(Guid)" />
    public async Task<bool> DeletePageAsync(Guid pageId)
    {
        using var response = await _httpClient.DeleteAsync($"api/notion/page/{pageId}").ConfigureAwait(false);
        return response.IsSuccessStatusCode;
    }
    #endregion

    #region Slack
    /// <inheritdoc cref="IMiddlewareApiClient.GetWorkspacesAsync" />
    public async IAsyncEnumerable<Workspace> GetWorkspacesAsync()
    {
        using var response = await _httpClient.GetAsync("api/slack/workspaces").ConfigureAwait(false);

        if (response.IsSuccessStatusCode)
        {
            var workspaces = await response.Content.ReadFromJsonAsync<Workspace[]>().ConfigureAwait(false);
            foreach (var workspace in workspaces ?? Array.Empty<Workspace>())
            {
                yield return workspace;
            }
        }

        yield break;
    }

    /// <inheritdoc cref="IMiddlewareApiClient.GetChannelsAsync(string)" />
    public async IAsyncEnumerable<Channel> GetChannelsAsync(string workspaceId)
    {
        using var response = await _httpClient.GetAsync($"api/slack/workspaces/{workspaceId}").ConfigureAwait(false);

        if (response.IsSuccessStatusCode)
        {
            var channels = await response.Content.ReadFromJsonAsync<Channel[]>().ConfigureAwait(false);
            foreach (var channel in channels ?? Array.Empty<Channel>())
            {
                yield return channel;
            }
        }

        yield break;
    }

    /// <inheritdoc cref="IMiddlewareApiClient.GetChatsAsync(string, string)" />
    public async IAsyncEnumerable<SlackChat> GetChatsAsync(string workspaceId, string channelId)
    {
        using var response = await _httpClient.GetAsync($"api/slack/workspaces/{workspaceId}/{channelId}").ConfigureAwait(false);

        if (response.IsSuccessStatusCode)
        {
            var chats = await response.Content.ReadFromJsonAsync<SlackChat[]>().ConfigureAwait(false);
            foreach (var chat in chats ?? Array.Empty<SlackChat>())
            {
                yield return chat;
            }
        }

        yield break;
    }

    /// <inheritdoc cref="IMiddlewareApiClient.GetMessageFromSlackAsync(string, string, string)" />
    public async Task<Content> GetMessageFromSlackAsync(string workspaceId, string channelId, string chatId)
    {
        using var response = await _httpClient.GetAsync($"api/slack/workspaces/{workspaceId}/{channelId}/{chatId}").ConfigureAwait(false);
        await response.ThrowIfFailedApiRequestAsync();

        return await response.Content.ReadFromJsonAsync<Content>().ConfigureAwait(false)
            ?? throw new JsonException(ExceptionMessageFromDeserializedContentIsNull);
    }
    #endregion

    #region Teams
    /// <inheritdoc cref="IMiddlewareApiClient.GetChatsAsync()" />
    public async IAsyncEnumerable<TeamsChat> GetChatsAsync()
    {
        using var response = await _httpClient.GetAsync("api/teams/chats").ConfigureAwait(false);

        if (response.IsSuccessStatusCode)
        {
            var chats = await response.Content.ReadFromJsonAsync<TeamsChat[]>().ConfigureAwait(false);
            foreach (var chat in chats ?? Array.Empty<TeamsChat>())
            {
                yield return chat;
            }
        }

        yield break;
    }

    /// <inheritdoc cref="IMiddlewareApiClient.GetMessagesAsync(string)" />
    public async IAsyncEnumerable<Message> GetMessagesAsync(string chatId)
    {
        using var response = await _httpClient.GetAsync($"api/teams/chats/{chatId}").ConfigureAwait(false);

        if (response.IsSuccessStatusCode)
        {
            var messages = await response.Content.ReadFromJsonAsync<Message[]>().ConfigureAwait(false);
            foreach (var message in messages ?? Array.Empty<Message>())
            {
                yield return message;
            }
        }

        yield break;
    }

    /// <inheritdoc cref="IMiddlewareApiClient.GetMessageFromTeamsAsync(string, long)" />
    public async Task<Content> GetMessageFromTeamsAsync(string chatId, long messageId)
    {
        using var response = await _httpClient.GetAsync($"api/teams/chats/{chatId}/{messageId}").ConfigureAwait(false);
        await response.ThrowIfFailedApiRequestAsync();

        return await response.Content.ReadFromJsonAsync<Content>().ConfigureAwait(false)
            ?? throw new JsonException(ExceptionMessageFromDeserializedContentIsNull);
    }

    /// <inheritdoc cref="IMiddlewareApiClient.SendMessageForTeamsAsync(string, ILayer[])" />
    public async Task<bool> SendMessageForTeamsAsync(string chatId, ILayer[] layers)
    {
        var layersRequest = new LayersRequest(layers);
        using var payload = layersRequest.ToHttpContent();
        using var response = await _httpClient.PostAsync($"api/teams/chats/{chatId}", payload).ConfigureAwait(false);

        return response.IsSuccessStatusCode;
    }
    #endregion

    #region Trello
    /// <inheritdoc cref="IMiddlewareApiClient.GetOrganizationsFromTrelloAsync()" />
    public async IAsyncEnumerable<TrelloOrganization> GetOrganizationsFromTrelloAsync()
    {
        using var response = await _httpClient.GetAsync($"api/trello/organizations").ConfigureAwait(false);

        if (response.IsSuccessStatusCode)
        {
            var organizations = await response.Content.ReadFromJsonAsync<TrelloOrganization[]>().ConfigureAwait(false);
            foreach (var organization in organizations ?? Array.Empty<TrelloOrganization>())
            {
                yield return organization;
            }
        }

        yield break;
    }

    /// <inheritdoc cref="IMiddlewareApiClient.GetBoardsAsync()" />
    public async IAsyncEnumerable<Board> GetBoardsAsync()
    {
        using var response = await _httpClient.GetAsync("api/trello/boards").ConfigureAwait(false);

        if (response.IsSuccessStatusCode)
        {
            var boards = await response.Content.ReadFromJsonAsync<Board[]>().ConfigureAwait(false);
            foreach (var board in boards ?? Array.Empty<Board>())
            {
                yield return board;
            }
        }

        yield break;
    }

    /// <inheritdoc cref="IMiddlewareApiClient.GetBoardsAsync(string)" />
    public async IAsyncEnumerable<Board> GetBoardsAsync(string organizationId)
    {
        using var response = await _httpClient.GetAsync($"api/trello/organizations/{organizationId}").ConfigureAwait(false);

        if (response.IsSuccessStatusCode)
        {
            var boards = await response.Content.ReadFromJsonAsync<Board[]>().ConfigureAwait(false);
            foreach (var board in boards ?? Array.Empty<Board>())
            {
                yield return board;
            }
        }

        yield break;
    }

    /// <inheritdoc cref="IMiddlewareApiClient.GetListsAsync(string)" />
    public async IAsyncEnumerable<List> GetListsAsync(string boardId)
    {
        using var response = await _httpClient.GetAsync($"api/trello/boards/{boardId}").ConfigureAwait(false);

        if (response.IsSuccessStatusCode)
        {
            var lists = await response.Content.ReadFromJsonAsync<List[]>().ConfigureAwait(false);
            foreach (var list in lists ?? Array.Empty<List>())
            {
                yield return list;
            }
        }

        yield break;
    }

    /// <inheritdoc cref="IMiddlewareApiClient.GetCardsAsync(string)" />
    public async IAsyncEnumerable<Card> GetCardsAsync(string listId)
    {
        using var response = await _httpClient.GetAsync($"api/trello/lists/{listId}").ConfigureAwait(false);

        if (response.IsSuccessStatusCode)
        {
            var cards = await response.Content.ReadFromJsonAsync<Card[]>().ConfigureAwait(false);
            foreach (var card in cards ?? Array.Empty<Card>())
            {
                yield return card;
            }
        }

        yield break;
    }

    /// <inheritdoc cref="IMiddlewareApiClient.CreateCardAsync(string, ILayer[])" />
    public async Task<bool> CreateCardAsync(string listId, ILayer[] layers)
    {
        var layersRequest = new LayersRequest(layers);
        using var payload = layersRequest.ToHttpContent();
        using var response = await _httpClient.PostAsync($"api/trello/lists/{listId}", payload).ConfigureAwait(false);

        return response.IsSuccessStatusCode;
    }

    /// <inheritdoc cref="IMiddlewareApiClient.ReadCardAsync(string)" />
    public async Task<Content> ReadCardAsync(string cardId)
    {
        using var response = await _httpClient.GetAsync($"api/trello/cards/{cardId}").ConfigureAwait(false);
        await response.ThrowIfFailedApiRequestAsync();

        return await response.Content.ReadFromJsonAsync<Content>().ConfigureAwait(false)
            ?? throw new JsonException(ExceptionMessageFromDeserializedContentIsNull);
    }

    /// <inheritdoc cref="IMiddlewareApiClient.UpdateCardAsync(string, string, ILayer[])" />
    public async Task<bool> UpdateCardAsync(string listId, string cardId, ILayer[] layers)
    {
        var layersRequest = new LayersRequest(layers);
        using var payload = layersRequest.ToHttpContent();
        using var response = await _httpClient.PutAsync($"api/trello/lists/{listId}/{cardId}", payload).ConfigureAwait(false);

        return response.IsSuccessStatusCode;
    }

    /// <inheritdoc cref="IMiddlewareApiClient.DeleteCardAsync(string)" />
    public async Task<bool> DeleteCardAsync(string cardId)
    {
        using var response = await _httpClient.DeleteAsync($"api/trello/cards/{cardId}").ConfigureAwait(false);
        return response.IsSuccessStatusCode;
    }
    #endregion

    /// <summary>
    /// API 요청을 전송할 주소를 가져옵니다.
    /// </summary>
    /// <param name="endpoint"> API 요청을 전송할 Endpoint 입니다. </param>
    /// <param name="query"> API 에 요청을 보낼 때 필요한 Query 입니다. </param>
    /// <returns> Api 요청 주소를 <see cref="Uri" /> 객체로 가져옵니다. </returns>
    private static Uri GetRequestUri(string endpoint, IReadOnlyDictionary<string, object> query)
    {
        return new Uri(endpoint, UriKind.Relative).AddParameters(query);
    }
}