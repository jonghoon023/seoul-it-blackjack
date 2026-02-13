using System.Text.Json;
using Oxygen.Jira.Sdk.Models.Enums;
using Mangoslab.Drawing.Layers.Interfaces;
using Oxygen.Middleware.Api.Client.Models;
using Oxygen.Middleware.Api.Client.Exceptions;
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
/// Oxygen.Middleware.Api Server 와의 통신을 돕는 Client Service 입니다.
/// </summary>
public interface IMiddlewareApiClient : IDisposable
{
    #region Common
    /// <summary>
    /// 사용자가 Platform 에 Login 할 수 있는 주소를 가져옵니다.
    /// </summary>
    /// <param name="platform"> Login 할 Platform 의 형식입니다. </param>
    /// <param name="callbackUrl"> Platform 에 Login 후 다시 되돌아올 Callback Url 입니다. </param>
    /// <returns> Login 할 수 있는 주소를 문자열로 가져옵니다. </returns>
    Task<string> GetLoginUrlAsync(PlatformType platform, string callbackUrl);

    /// <summary>
    /// 사용자가 Platform 에 접근할 권한이 있는지 확인합니다.
    /// </summary>
    /// <param name="platform"> Login 할 Platform 의 형식입니다. </param>
    /// <returns> 사용자가 Platform 에 접근할 수 있는 권한이 있으면 <see langword="true" /> 를 반환하고, 접근할 수 있는 권한이 없으면 <see langword="false" /> 를 반환합니다. </returns>
    Task<bool> ValidateAsync(PlatformType platform);

    /// <summary>
    /// Platform 에 사용자를 등록합니다.
    /// </summary>
    /// <param name="platform"> Login 할 Platform 의 형식입니다. </param>
    /// <param name="callbackUrl"> 검증용으로 사용하는 Callback Url 주소입니다. </param>
    /// <returns> 사용자를 Platform 에 등록했으면 <see langword="true" /> 를 반환하고, 등록하지 못했으면 <see langword="false" /> 를 반환합니다. </returns>
    Task<bool> RegisterAsync(PlatformType platform, Uri callbackUrl);

    /// <summary>
    /// Platform 에 등록된 사용자를 삭제합니다.
    /// </summary>
    /// <param name="platform"> Login 할 Platform 의 형식입니다. </param>
    /// <returns> 사용자를 Platform 에 등록된 사용자를 삭제했으면 <see langword="true" /> 를 반환하고, 삭제하지 못했으면 <see langword="false" /> 를 반환합니다. </returns>
    Task<bool> UnregisterAsync(PlatformType platform);
    #endregion

    #region Status
    /// <summary>
    /// Main Server 의 현재 상태를 가져옵니다.
    /// </summary>
    /// <returns> Main Server 의 현재 상태를 <see cref="ServerStatus" /> 객체로 가져옵니다. </returns>
    /// <exception cref="JsonException"> API 요청 후 응답을 받아왔지만 <see cref="PageContent" /> 객체로 변환할 수 없을 때 발생합니다. </exception>
    Task<ServerStatus> GetMainServerStatusAsync();

    /// <summary>
    /// API Server 의 현재 상태를 가져옵니다.
    /// </summary>
    /// <param name="platform"> <see cref="PlatformType" /> 형식입니다. </param>
    /// <returns> API Server 의 현재 상태를 <see cref="ServerStatus" /> 객체로 가져옵니다. </returns>
    /// <exception cref="JsonException"> API 요청 후 응답을 받아왔지만 <see cref="PageContent" /> 객체로 변환할 수 없을 때 발생합니다. </exception>
    Task<ServerStatus> GetApiServerStatusAsync(PlatformType platform);
    #endregion

    #region Main Server
    /// <summary>
    /// 사용자를 Main Server 에 등록 후 결과를 가져옵니다.
    /// </summary>
    /// <returns> 사용자를 등록하는데 성공했으면 <see langword="true" /> 를 반환하고, 실패했으면 <see langword="false" /> 를 반환합니다. </returns>
    Task<bool> AuthenticateAsync();

    /// <summary>
    /// Main Server 의 <see cref="WeatherForecast" /> 목록을 비동기적으로 가져옵니다.
    /// </summary>
    /// <returns> <see cref="IAsyncEnumerable{T}" /> 형식으로 <see cref="WeatherForecast" /> 목록을 가져옵니다. </returns>
    IAsyncEnumerable<WeatherForecast> GetWeatherForecastsAsync();

    /// <summary>
    /// Main Server 에 등록되어 있는 사용자가 등록한 <see cref="Printer" /> 목록을 비동기적으로 가져옵니다.
    /// </summary>
    /// <returns> <see cref="IAsyncEnumerable{T}" /> 형식으로 <see cref="Printer" /> 목록을 가져옵니다. </returns>
    IAsyncEnumerable<Printer> GetPrinters();

    /// <summary>
    /// Printer 에 인쇄를 요청합니다.
    /// </summary>
    /// <param name="printerId"> 인쇄를 요청할 Printer 를 식별할 수 있는 Id 입니다. </param>
    /// <param name="layers"> 인쇄할 내용이 담긴 <see cref="ILayer" /> 배열입니다. </param>
    /// <returns> 인쇄에 성공했으면 <see langword="true" /> 를 반환하고, 실패했으면  <see langword="false" /> 를 반환합니다. </returns>
    Task<bool> PrintAsync(Guid printerId, ILayer[] layers);
    #endregion

    #region Evernote
    /// <summary>
    /// Evernote 의 <see cref="Notebook" /> 목록을 비동기적으로 가져옵니다.
    /// </summary>
    /// <returns> <see cref="IAsyncEnumerable{T}" /> 형식으로 <see cref="Notebook" /> 목록을 가져옵니다. </returns>
    IAsyncEnumerable<Notebook> GetNotebooksAsync();

    /// <summary>
    /// Evernote 의 <see cref="Notebook" /> 목록을 비동기적으로 가져옵니다.
    /// </summary>
    /// <param name="notebookGuid"> <see cref="Notebook" /> 을 식별할 수 있는 <see cref="Notebook.Guid" /> 입니다. </param>
    /// <returns> <see cref="IAsyncEnumerable{T}" /> 형식으로 <see cref="Notebook" /> 목록을 가져옵니다. </returns>
    IAsyncEnumerable<Note> GetNotesAsync(Guid notebookGuid);

    /// <summary>
    /// Evernote 에 <see cref="Note" /> 를 생성합니다.
    /// </summary>
    /// <param name="notebookGuid"> <see cref="Notebook" /> 을 식별할 수 있는 <see cref="Notebook.Guid" /> 입니다. </param>
    /// <param name="layers"> <see cref="Note" /> 의 내용입니다. </param>
    /// <returns> <see cref="Note" /> 를 생성했으면 <see langword="true" /> 를 반환하고, 실패했으면 <see langword="false" /> 를 반환합니다. </returns>
    Task<bool> CreateNoteAsync(Guid notebookGuid, ILayer[] layers);

    /// <summary>
    /// Evernote 의 <see cref="Note" /> 내용을 가져옵니다.
    /// </summary>
    /// <param name="noteGuid"> <see cref="Note" /> 를 식별할 수 있는 <see cref="Note.Guid" /> 입니다. </param>
    /// <returns> <see cref="Note" /> 의 내용을 <see cref="Content" /> 객체로 가져옵니다. </returns>
    /// <exception cref="JsonException"> API 요청 후 응답을 받아왔지만 <see cref="Content" /> 객체로 변환할 수 없을 때 발생합니다. </exception>
    /// <exception cref="ApiRequestException"> API 요청에 실패했을 때 발생합니다. </exception>
    Task<Content> ReadNoteAsync(Guid noteGuid);

    /// <summary>
    /// Evernote 의 <see cref="Note" /> 내용을 갱신합니다.
    /// </summary>
    /// <param name="noteGuid"> <see cref="Note" /> 를 식별할 수 있는 <see cref="Note.Guid" /> 입니다. </param>
    /// <param name="layers"> <see cref="Note" /> 의 내용입니다. </param>
    /// <returns> <see cref="Note" /> 를 갱신했으면 <see langword="true" /> 를 반환하고, 실패했으면 <see langword="false" /> 를 반환합니다. </returns>
    Task<bool> UpdateNoteAsync(Guid noteGuid, ILayer[] layers);

    /// <summary>
    /// Evernote 의 <see cref="Note" /> 를 삭제합니다.
    /// </summary>
    /// <param name="noteGuid"> <see cref="Note" /> 를 식별할 수 있는 <see cref="Note.Guid" /> 입니다. </param>
    /// <returns> <see cref="Note" /> 를 삭제했으면 <see langword="true" /> 를 반환하고, 실패했으면 <see langword="false" /> 를 반환합니다. </returns>
    Task<bool> DeleteNoteAsync(Guid noteGuid);
    #endregion

    #region Jira
    /// <summary>
    /// Jira 의 <see cref="JiraOrganization" /> 목록을 비동기적으로 가져옵니다.
    /// </summary>
    /// <returns> <see cref="IAsyncEnumerable{T}" /> 형식으로 <see cref="JiraOrganization" /> 목록을 가져옵니다. </returns>
    IAsyncEnumerable<JiraOrganization> GetOrganizationsFromJiraAsync();

    /// <summary>
    /// Jira 의 <see cref="Issue" /> 목록을 비동기적으로 가져옵니다.
    /// </summary>
    /// <param name="cloudId"> Cloud 를 식별할 수 있는 Id 입니다. </param>
    /// <returns> <see cref="IAsyncEnumerable{T}" /> 형식으로 <see cref="Issue" /> 목록을 가져옵니다. </returns>
    IAsyncEnumerable<Issue> GetIssuesAsync(Guid cloudId);

    /// <summary>
    /// Jira 에 <see cref="Issue" /> 를 생성합니다.
    /// </summary>
    /// <param name="cloudId"> Cloud 를 식별할 수 있는 Id 입니다. </param>
    /// <param name="project"> Project 의 Key 입니다. (Lithium project의 경우: <c> LT </c> 입니다.) </param>
    /// <param name="type"> Issue 의 형식입니다. </param>
    /// <param name="layers"> <see cref="Issue" /> 의 내용입니다. </param>
    /// <returns> <see cref="Issue" /> 를 생성했으면 <see langword="true" /> 를 반환하고, 실패했으면 <see langword="false" /> 를 반환합니다. </returns>
    Task<bool> CreateIssueAsync(Guid cloudId, string project, IssueType type, ILayer[] layers);

    /// <summary>
    /// Jira 의 <see cref="Issue" /> 목록을 비동기적으로 가져옵니다.
    /// </summary>
    /// <param name="cloudId"> Cloud 를 식별할 수 있는 <see cref="JiraOrganization.Id" /> 입니다. </param>
    /// <param name="issueId"> <see cref="Issue" /> 를 식별할 수 있는 <see cref="Issue.Id" /> 입니다. </param>
    /// <returns> <see cref="Issue" /> 의 내용을 <see cref="Content" /> 객체로 가져옵니다. </returns>
    /// <exception cref="JsonException"> API 요청 후 응답을 받아왔지만 <see cref="Content" /> 객체로 변환할 수 없을 때 발생합니다. </exception>
    /// <exception cref="ApiRequestException"> API 요청에 실패했을 때 발생합니다. </exception>
    Task<Content> ReadIssueAsync(Guid cloudId, int issueId);

    /// <summary>
    /// Jira 의 <see cref="Issue" /> 목록을 비동기적으로 가져옵니다.
    /// </summary>
    /// <param name="cloudId"> Cloud 를 식별할 수 있는 <see cref="JiraOrganization.Id" /> 입니다. </param>
    /// <param name="issueKey"> <see cref="Issue" /> 를 식별할 수 있는 <see cref="Issue.Key" /> 입니다. </param>
    /// <returns> <see cref="Issue" /> 의 내용을 <see cref="Content" /> 객체로 가져옵니다. </returns>
    /// <exception cref="JsonException"> API 요청 후 응답을 받아왔지만 <see cref="Content" /> 객체로 변환할 수 없을 때 발생합니다. </exception>
    /// <exception cref="ApiRequestException"> API 요청에 실패했을 때 발생합니다. </exception>
    Task<Content> ReadIssueAsync(Guid cloudId, string issueKey);

    /// <summary>
    /// Jira 의 <see cref="Issue" /> 를 갱신합니다.
    /// </summary>
    /// <param name="cloudId"> Cloud 를 식별할 수 있는 Id 입니다. </param>
    /// <param name="issueId"> <see cref="Issue" /> 를 식별할 수 있는 <see cref="Issue.Id" /> 입니다. </param>
    /// <param name="layers"> <see cref="Issue" /> 의 내용입니다. </param>
    /// <returns> <see cref="Issue" /> 를 갱신했으면 <see langword="true" /> 를 반환하고, 실패했으면 <see langword="false" /> 를 반환합니다. </returns>
    Task<bool> UpdateIssueAsync(Guid cloudId, int issueId, ILayer[] layers);

    /// <summary>
    /// Jira 의 <see cref="Issue" /> 를 삭제합니다.
    /// </summary>
    /// <param name="cloudId"> Cloud 를 식별할 수 있는 Id 입니다. </param>
    /// <param name="issueId"> <see cref="Issue" /> 를 식별할 수 있는 <see cref="Issue.Id" /> 입니다. </param>
    /// <returns> <see cref="Issue" /> 를 삭제했으면 <see langword="true" /> 를 반환하고, 실패했으면 <see langword="false" /> 를 반환합니다. </returns>
    Task<bool> DeleteIssueAsync(Guid cloudId, int issueId);
    #endregion

    #region Notion
    /// <summary>
    /// Notion 의 최상위 <see cref="Page" /> 를 생성합니다.
    /// </summary>
    /// <param name="layers"> 최상위 <see cref="Page" /> 의 내용입니다. </param>
    /// <returns> 최상위 <see cref="Page" /> 를 생성했으면 <see langword="true" /> 를 반환하고, 실패했으면 <see langword="false" /> 를 반환합니다. </returns>
    Task<bool> CreatePageAsync(ILayer[] layers);

    /// <summary>
    /// Notion 에 <see cref="Page" /> 를 생성합니다.
    /// </summary>
    /// <param name="pageId"> 부모 <see cref="Page" /> 를 식별할 수 있는 <see cref="Page.Id" /> 입니다. </param>
    /// <param name="layers"> 하위 <see cref="Page" /> 의 내용입니다. </param>
    /// <returns> 하위 <see cref="Page" /> 를 생성했으면 <see langword="true" /> 를 반환하고, 실패했으면 <see langword="false" /> 를 반환합니다. </returns>
    Task<bool> CreatePageAsync(Guid pageId, ILayer[] layers);

    /// <summary>
    /// Notion 의 최상위 <see cref="Page" /> 를 가져옵니다.
    /// </summary>
    /// <returns> <see cref="Page" /> 의 내용을 <see cref="PageContent" /> 객체로 가져옵니다. </returns>
    /// <exception cref="JsonException"> API 요청 후 응답을 받아왔지만 <see cref="PageContent" /> 객체로 변환할 수 없을 때 발생합니다. </exception>
    /// <exception cref="ApiRequestException"> API 요청에 실패했을 때 발생합니다. </exception>
    Task<PageContent> ReadPageAsync();

    /// <summary>
    /// Notion 의 <see cref="Page" /> 를 가져옵니다.
    /// </summary>
    /// <param name="pageId"> 가져올 <see cref="Page" /> 를 식별할 수 있는 <see cref="Page.Id" /> 입니다. </param>
    /// <returns> 하위 <see cref="Page" /> 의 내용을 <see cref="PageContent" /> 객체로 가져옵니다. </returns>
    /// <exception cref="JsonException"> API 요청 후 응답을 받아왔지만 <see cref="PageContent" /> 객체로 변환할 수 없을 때 발생합니다. </exception>
    /// <exception cref="ApiRequestException"> API 요청에 실패했을 때 발생합니다. </exception>
    Task<PageContent> ReadPageAsync(Guid pageId);

    /// <summary>
    /// Notion 의 <see cref="Page" /> 를 갱신합니다.
    /// </summary>
    /// <param name="pageId"> 갱신할 <see cref="Page" /> 를 식별할 수 있는 <see cref="Page.Id" /> 입니다. </param>
    /// <param name="layers"> 갱신할 <see cref="Page" /> 의 내용입니다. </param>
    /// <returns> 하위 <see cref="Page" /> 를 갱신했으면 <see langword="true" /> 를 반환하고, 실패했으면 <see langword="false" /> 를 반환합니다. </returns>
    Task<bool> UpdatePageAsync(Guid pageId, ILayer[] layers);

    /// <summary>
    /// Notion 의 <see cref="Page" /> 를 삭제합니다.
    /// </summary>
    /// <param name="pageId"> 삭제할 <see cref="Page" /> 를 식별할 수 있는 <see cref="Page.Id" /> 입니다. </param>
    /// <returns> 하위 <see cref="Page" /> 를 삭제했으면 <see langword="true" /> 를 반환하고, 실패했으면 <see langword="false" /> 를 반환합니다. </returns>
    Task<bool> DeletePageAsync(Guid pageId);
    #endregion

    #region Slack
    /// <summary>
    /// Slack 의 <see cref="Workspace" /> 목록을 비동기적으로 가져옵니다.
    /// </summary>
    /// <returns> <see cref="IAsyncEnumerable{T}" /> 형식으로 <see cref="Workspace" /> 목록을 가져옵니다. </returns>
    IAsyncEnumerable<Workspace> GetWorkspacesAsync();

    /// <summary>
    /// Slack 의 <see cref="Channel" /> 목록을 비동기적으로 가져옵니다.
    /// </summary>
    /// <param name="workspaceId"> <see cref="Workspace" /> 를 식별할 수 있는 <see cref="Workspace.Id" /> 입니다. </param>
    /// <returns> <see cref="IAsyncEnumerable{T}" /> 형식으로 <see cref="Channel" /> 목록을 가져옵니다. </returns>
    IAsyncEnumerable<Channel> GetChannelsAsync(string workspaceId);

    /// <summary>
    /// Slack 의 <see cref="SlackChat" /> 목록을 비동기적으로 가져옵니다.
    /// </summary>
    /// <param name="workspaceId"> <see cref="Workspace" /> 를 식별할 수 있는 <see cref="Workspace.Id" /> 입니다. </param>
    /// <param name="channelId"> <see cref="Channel" /> 을 식별할 수 있는 <see cref="Channel.Id" /> 입니다. </param>
    /// <returns> <see cref="IAsyncEnumerable{T}" /> 형식으로 <see cref="SlackChat" /> 목록을 가져옵니다. </returns>
    IAsyncEnumerable<SlackChat> GetChatsAsync(string workspaceId, string channelId);

    /// <summary>
    /// Slack 의 <see cref="SlackChat" /> 목록을 비동기적으로 가져옵니다.
    /// </summary>
    /// <param name="workspaceId"> <see cref="Workspace" /> 를 식별할 수 있는 <see cref="Workspace.Id" /> 입니다. </param>
    /// <param name="channelId"> <see cref="Channel" /> 을 식별할 수 있는 <see cref="Channel.Id" /> 입니다. </param>
    /// <param name="chatId"> <see cref="SlackChat" /> 을 식별할 수 있는 <see cref="SlackChat.Id" /> 입니다. </param>
    /// <returns> <see cref="SlackChat" /> 의 내용을 <see cref="Content" /> 객체로 가져옵니다. </returns>
    /// <exception cref="JsonException"> API 요청 후 응답을 받아왔지만 <see cref="PageContent" /> 객체로 변환할 수 없을 때 발생합니다. </exception>
    /// <exception cref="ApiRequestException"> API 요청에 실패했을 때 발생합니다. </exception>
    Task<Content> GetMessageFromSlackAsync(string workspaceId, string channelId, string chatId);
    #endregion

    #region Teams
    /// <summary>
    /// Teams 의 기업용 (개인채팅) 에서 <see cref="SlackChat" /> 목록을 비동기적으로 가져옵니다.
    /// </summary>
    /// <returns> <see cref="IAsyncEnumerable{T}" /> 형식으로 <see cref="TeamsChat" /> 목록을 가져옵니다. </returns>
    IAsyncEnumerable<TeamsChat> GetChatsAsync();

    /// <summary>
    /// Teams 의 기업용 (개인채팅) 에서 <see cref="Message" /> 목록을 비동기적으로 가져옵니다.
    /// </summary>
    /// <param name="chatId"> <see cref="TeamsChat" /> 을 식별할 수 있는 <see cref="TeamsChat.Id" /> 입니다. </param>
    /// <returns> <see cref="IAsyncEnumerable{T}" /> 형식으로 <see cref="TeamsChat" /> 목록을 가져옵니다. </returns>
    IAsyncEnumerable<Message> GetMessagesAsync(string chatId);

    /// <summary>
    /// Teams 의 기업용 (개인채팅) 에서 <see cref="TeamsChat" /> 의 내용을 가져옵니다.
    /// </summary>
    /// <param name="chatId"> <see cref="TeamsChat" /> 을 식별할 수 있는 <see cref="TeamsChat.Id" /> 입니다. </param>
    /// <param name="messageId"> <see cref="Message" /> 를 식별할 수 있는 <see cref="Message.Id" /> 입니다. </param>
    /// <returns> <see cref="Message" /> 의 내용을 <see cref="Content" /> 객체로 가져옵니다. </returns>
    /// <exception cref="JsonException"> API 요청 후 응답을 받아왔지만 <see cref="Content" /> 객체로 변환할 수 없을 때 발생합니다. </exception>
    /// <exception cref="ApiRequestException"> API 요청에 실패했을 때 발생합니다. </exception>
    Task<Content> GetMessageFromTeamsAsync(string chatId, long messageId);

    /// <summary>
    /// Teams 의 기업용 (개인채팅) 에 <see cref="Message" /> 를 전송합니다.
    /// </summary>
    /// <param name="chatId"> <see cref="TeamsChat" /> 을 식별할 수 있는 <see cref="TeamsChat.Id" /> 입니다. </param>
    /// <param name="layers"> <see cref="Message" /> 의 내용입니다. </param>
    /// <returns> <see cref="Message" /> 를 전송했으면 <see langword="true" /> 를 반환하고, 실패했으면 <see langword="false" /> 를 반환합니다. </returns>
    Task<bool> SendMessageForTeamsAsync(string chatId, ILayer[] layers);
    #endregion

    #region Trello
    /// <summary>
    /// Trello 의 <see cref="TrelloOrganization" /> 목록을 비동기적으로 가져옵니다.
    /// </summary>
    /// <returns> <see cref="IAsyncEnumerable{T}" /> 형식으로 <see cref="TrelloOrganization" /> 목록을 가져옵니다. </returns>
    IAsyncEnumerable<TrelloOrganization> GetOrganizationsFromTrelloAsync();

    /// <summary>
    /// Trello 의 <see cref="Board" /> 목록을 비동기적으로 가져옵니다.
    /// </summary>
    /// <returns> <see cref="IAsyncEnumerable{T}" /> 형식으로 <see cref="Board" /> 목록을 가져옵니다. </returns>
    IAsyncEnumerable<Board> GetBoardsAsync();

    /// <summary>
    /// Trello 의 <see cref="Board" /> 목록을 비동기적으로 가져옵니다.
    /// </summary>
    /// <param name="organizationId"> <see cref="TrelloOrganization" /> 을 식별할 수 있는 <see cref="TrelloOrganization.Id" /> 입니다. </param>
    /// <returns> <see cref="IAsyncEnumerable{T}" /> 형식으로 <see cref="Board" /> 목록을 가져옵니다. </returns>
    IAsyncEnumerable<Board> GetBoardsAsync(string organizationId);

    /// <summary>
    /// Trello 의 <see cref="List" /> 목록을 비동기적으로 가져옵니다.
    /// </summary>
    /// <param name="boardId"> <see cref="Board" /> 을 식별할 수 있는 <see cref="Board.Id" /> 입니다. </param>
    /// <returns> <see cref="IAsyncEnumerable{T}" /> 형식으로 <see cref="Board" /> 목록을 가져옵니다. </returns>
    IAsyncEnumerable<List> GetListsAsync(string boardId);

    /// <summary>
    /// Trello 의 <see cref="Card" /> 목록을 비동기적으로 가져옵니다.
    /// </summary>
    /// <param name="listId"> <see cref="List" /> 를 식별할 수 있는 <see cref="List.Id" /> 입니다. </param>
    /// <returns> <see cref="IAsyncEnumerable{T}" /> 형식으로 <see cref="Card" /> 목록을 가져옵니다. </returns>
    IAsyncEnumerable<Card> GetCardsAsync(string listId);

    /// <summary>
    /// Trello 에 <see cref="Card" /> 를 생성합니다.
    /// </summary>
    /// <param name="listId"> <see cref="List" /> 를 식별할 수 있는 <see cref="List.Id" /> 입니다. </param>
    /// <param name="layers"> <see cref="Card" /> 의 내용입니다. </param>
    /// <returns> <see cref="Card" /> 를 생성했으면 <see langword="true" /> 를 반환하고, 실패했으면 <see langword="false" /> 를 반환합니다. </returns>
    Task<bool> CreateCardAsync(string listId, ILayer[] layers);

    /// <summary>
    /// Trello 의 <see cref="Card" /> 의 내용을 가져옵니다.
    /// </summary>
    /// <param name="listId"> <see cref="List" /> 를 식별할 수 있는 <see cref="List.Id" /> 입니다. </param>
    /// <returns> <see cref="Card" /> 의 내용을 <see cref="Content" /> 객체로 가져옵니다. </returns>
    /// <exception cref="JsonException"> API 요청 후 응답을 받아왔지만 <see cref="PageContent" /> 객체로 변환할 수 없을 때 발생합니다. </exception>
    /// <exception cref="ApiRequestException"> API 요청에 실패했을 때 발생합니다. </exception>
    Task<Content> ReadCardAsync(string listId);

    /// <summary>
    /// Trello 의 <see cref="Card" /> 를 갱신합니다.
    /// </summary>
    /// <param name="listId"> <see cref="Card" /> 가 속해있는 <see cref="List" /> 를 식별할 수 있는 <see cref="List.Id" /> 입니다. </param>
    /// <param name="cardId"> <see cref="Card" /> 를 식별할 수 있는 <see cref="Card.Id" /> 입니다. </param>
    /// <param name="layers"> <see cref="Card" /> 의 내용입니다. </param>
    /// <returns> <see cref="Card" /> 를 갱신했으면 <see langword="true" /> 를 반환하고, 실패했으면 <see langword="false" /> 를 반환합니다. </returns>
    Task<bool> UpdateCardAsync(string listId, string cardId, ILayer[] layers);

    /// <summary>
    /// Trello 의 <see cref="Card" /> 를 삭제합니다.
    /// </summary>
    /// <param name="cardId"> <see cref="Card" /> 를 식별할 수 있는 <see cref="Card.Id" /> 입니다. </param>
    /// <returns> <see cref="Card" /> 를 삭제했으면 <see langword="true" /> 를 반환하고, 실패했으면 <see langword="false" /> 를 반환합니다. </returns>
    Task<bool> DeleteCardAsync(string cardId);
    #endregion
}