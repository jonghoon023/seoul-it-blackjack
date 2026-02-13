using Seoul.It.Blackjack.Backend.Services.Exceptions;
using Seoul.It.Blackjack.Core.Contracts;
using System.Collections.Generic;
using System.Linq;

namespace Seoul.It.Blackjack.Backend.Services.Rules;

/// <summary>
/// 게임 규칙과 권한 조건을 검사하는 구현체입니다.
/// </summary>
internal sealed class GameRuleValidator : IGameRuleValidator
{
    /// <summary>
    /// 이름을 정리하고 길이 규칙을 확인합니다.
    /// </summary>
    /// <param name="name">입력 이름입니다.</param>
    /// <param name="minNameLength">최소 길이입니다.</param>
    /// <param name="maxNameLength">최대 길이입니다.</param>
    /// <returns>Trim 처리된 이름입니다.</returns>
    public string NormalizeName(string? name, int minNameLength, int maxNameLength)
    {
        string normalized = (name ?? string.Empty).Trim();
        if (normalized.Length < minNameLength || normalized.Length > maxNameLength)
        {
            throw new GameValidationException("INVALID_NAME", "?대쫫? 1~20?먯뿬???⑸땲??");
        }

        return normalized;
    }

    /// <summary>
    /// 연결 사용자가 이미 입장했는지 확인합니다.
    /// </summary>
    /// <param name="connections">연결 매핑 저장소입니다.</param>
    /// <param name="connectionId">확인할 연결 ID입니다.</param>
    public void EnsureJoined(ConnectionRegistry connections, string connectionId)
    {
        if (!connections.ContainsConnection(connectionId))
        {
            throw new GameValidationException("NOT_JOINED", "癒쇱? 李멸??댁빞 ?⑸땲??");
        }
    }

    /// <summary>
    /// 라운드 시작 조건(입장 여부, 단계, 딜러 권한, 최소 인원)을 검사합니다.
    /// </summary>
    /// <param name="connections">연결 매핑 저장소입니다.</param>
    /// <param name="phase">현재 게임 단계입니다.</param>
    /// <param name="dealerPlayerId">현재 딜러 ID입니다.</param>
    /// <param name="connectionId">요청 연결 ID입니다.</param>
    /// <param name="playerCount">현재 참가 인원 수입니다.</param>
    /// <param name="minPlayersToStart">최소 시작 인원 수입니다.</param>
    public void EnsureCanStartRound(
        ConnectionRegistry connections,
        GamePhase phase,
        string dealerPlayerId,
        string connectionId,
        int playerCount,
        int minPlayersToStart)
    {
        EnsureJoined(connections, connectionId);
        if (phase != GamePhase.Idle)
        {
            throw new GameRuleException("GAME_IN_PROGRESS", "?대? 寃뚯엫??吏꾪뻾 以묒엯?덈떎.");
        }

        if (string.IsNullOrEmpty(dealerPlayerId) || dealerPlayerId != connectionId)
        {
            throw new GameAuthorizationException("NOT_DEALER", "?쒕윭留??쇱슫?쒕? ?쒖옉?????덉뒿?덈떎.");
        }

        if (playerCount < minPlayersToStart)
        {
            throw new GameRuleException("INSUFFICIENT_PLAYERS", "?쇱슫?쒕? ?쒖옉?섎젮硫?理쒖냼 2紐낆씠 ?꾩슂?⑸땲??");
        }
    }

    /// <summary>
    /// 플레이어 목록에서 ID가 같은 플레이어를 찾아 반환합니다.
    /// </summary>
    /// <param name="players">플레이어 목록입니다.</param>
    /// <param name="playerId">찾을 플레이어 ID입니다.</param>
    /// <returns>찾은 플레이어입니다.</returns>
    public PlayerState FindPlayer(IReadOnlyCollection<PlayerState> players, string playerId)
    {
        PlayerState? player = players.SingleOrDefault(value => value.PlayerId == playerId);
        if (player is null)
        {
            throw new GameValidationException("NOT_JOINED", "癒쇱? 李멸??댁빞 ?⑸땲??");
        }

        return player;
    }

    /// <summary>
    /// 플레이어 목록에서 딜러를 찾아 반환합니다.
    /// </summary>
    /// <param name="players">플레이어 목록입니다.</param>
    /// <returns>딜러 플레이어입니다.</returns>
    public PlayerState FindDealer(IReadOnlyCollection<PlayerState> players)
    {
        PlayerState? dealer = players.SingleOrDefault(player => player.IsDealer);
        if (dealer is null)
        {
            throw new GameAuthorizationException("NOT_DEALER", "?쒕윭媛 議댁옱?섏? ?딆뒿?덈떎.");
        }

        return dealer;
    }

    /// <summary>
    /// 히트/스탠드 요청 전 공통 규칙을 검사하고 요청 플레이어를 반환합니다.
    /// </summary>
    /// <param name="connections">연결 매핑 저장소입니다.</param>
    /// <param name="phase">현재 게임 단계입니다.</param>
    /// <param name="players">플레이어 목록입니다.</param>
    /// <param name="connectionId">요청 연결 ID입니다.</param>
    /// <param name="currentTurnPlayerId">현재 턴 플레이어 ID입니다.</param>
    /// <returns>검사를 통과한 요청 플레이어입니다.</returns>
    public PlayerState ValidatePlayerAction(
        ConnectionRegistry connections,
        GamePhase phase,
        IReadOnlyCollection<PlayerState> players,
        string connectionId,
        string currentTurnPlayerId)
    {
        EnsureJoined(connections, connectionId);
        if (phase != GamePhase.InRound)
        {
            throw new GameRuleException("GAME_NOT_INROUND", "寃뚯엫??吏꾪뻾 以묒씠 ?꾨떃?덈떎.");
        }

        PlayerState player = FindPlayer(players, connectionId);
        if (player.IsDealer)
        {
            throw new GameRuleException("DEALER_IS_AUTO", "?쒕윭???먮룞?쇰줈 吏꾪뻾?⑸땲??");
        }

        if (currentTurnPlayerId != player.PlayerId)
        {
            throw new GameRuleException("NOT_YOUR_TURN", "?꾩옱 ?댁쓽 ?뚮젅?댁뼱媛 ?꾨떃?덈떎.");
        }

        if (player.TurnState != PlayerTurnState.Playing)
        {
            throw new GameRuleException("ALREADY_DONE", "?대? ?됰룞???앸궃 ?뚮젅?댁뼱?낅땲??");
        }

        return player;
    }
}
