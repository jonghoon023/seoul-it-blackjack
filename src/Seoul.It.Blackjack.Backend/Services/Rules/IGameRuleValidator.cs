using Seoul.It.Blackjack.Core.Contracts;
using System.Collections.Generic;

namespace Seoul.It.Blackjack.Backend.Services.Rules;

/// <summary>
/// 게임 규칙과 권한을 검사하는 검증기 계약입니다.
/// </summary>
internal interface IGameRuleValidator
{
    /// <summary>
    /// 이름을 정리(Trim)하고 길이 규칙을 확인합니다.
    /// </summary>
    /// <param name="name">검사할 이름입니다.</param>
    /// <param name="minNameLength">최소 길이입니다.</param>
    /// <param name="maxNameLength">최대 길이입니다.</param>
    /// <returns>정리된 이름입니다.</returns>
    string NormalizeName(string? name, int minNameLength, int maxNameLength);

    /// <summary>
    /// 연결 사용자가 이미 입장했는지 확인합니다.
    /// </summary>
    /// <param name="connections">연결 매핑 저장소입니다.</param>
    /// <param name="connectionId">확인할 연결 ID입니다.</param>
    void EnsureJoined(ConnectionRegistry connections, string connectionId);

    /// <summary>
    /// 라운드 시작이 가능한지 한 번에 확인합니다.
    /// </summary>
    /// <param name="connections">연결 매핑 저장소입니다.</param>
    /// <param name="phase">현재 게임 단계입니다.</param>
    /// <param name="dealerPlayerId">현재 딜러 ID입니다.</param>
    /// <param name="connectionId">요청 연결 ID입니다.</param>
    /// <param name="playerCount">현재 참가 인원 수입니다.</param>
    /// <param name="minPlayersToStart">시작 최소 인원 수입니다.</param>
    void EnsureCanStartRound(
        ConnectionRegistry connections,
        GamePhase phase,
        string dealerPlayerId,
        string connectionId,
        int playerCount,
        int minPlayersToStart);

    /// <summary>
    /// ID로 플레이어를 찾아 반환합니다.
    /// </summary>
    /// <param name="players">플레이어 목록입니다.</param>
    /// <param name="playerId">찾을 플레이어 ID입니다.</param>
    /// <returns>찾은 플레이어입니다.</returns>
    PlayerState FindPlayer(IReadOnlyCollection<PlayerState> players, string playerId);

    /// <summary>
    /// 현재 딜러를 찾아 반환합니다.
    /// </summary>
    /// <param name="players">플레이어 목록입니다.</param>
    /// <returns>딜러 플레이어입니다.</returns>
    PlayerState FindDealer(IReadOnlyCollection<PlayerState> players);

    /// <summary>
    /// 히트/스탠드 요청 전 공통 규칙을 검사하고 요청 플레이어를 반환합니다.
    /// </summary>
    /// <param name="connections">연결 매핑 저장소입니다.</param>
    /// <param name="phase">현재 게임 단계입니다.</param>
    /// <param name="players">플레이어 목록입니다.</param>
    /// <param name="connectionId">요청 연결 ID입니다.</param>
    /// <param name="currentTurnPlayerId">현재 턴 플레이어 ID입니다.</param>
    /// <returns>검사를 통과한 요청 플레이어입니다.</returns>
    PlayerState ValidatePlayerAction(
        ConnectionRegistry connections,
        GamePhase phase,
        IReadOnlyCollection<PlayerState> players,
        string connectionId,
        string currentTurnPlayerId);
}
