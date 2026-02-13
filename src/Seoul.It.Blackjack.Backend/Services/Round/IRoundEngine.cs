using Seoul.It.Blackjack.Backend.Models;
using Seoul.It.Blackjack.Core.Contracts;
using System.Collections.Generic;

namespace Seoul.It.Blackjack.Backend.Services.Round;

/// <summary>
/// 라운드 진행 규칙(배분, 턴 처리, 정산)을 담당하는 엔진 계약입니다.
/// </summary>
internal interface IRoundEngine
{
    /// <summary>
    /// 새 라운드를 시작하고 초기 카드 배분을 처리합니다.
    /// </summary>
    /// <param name="players">플레이어 목록입니다.</param>
    /// <param name="deckCount">사용할 덱 개수입니다.</param>
    /// <param name="dealerStandScore">딜러 스탠드 기준 점수입니다.</param>
    /// <returns>라운드 시작 처리 결과입니다.</returns>
    RoundResolution StartRound(List<PlayerState> players, int deckCount, int dealerStandScore);

    /// <summary>
    /// 히트 요청을 처리합니다.
    /// </summary>
    /// <param name="players">플레이어 목록입니다.</param>
    /// <param name="shoe">카드 더미입니다.</param>
    /// <param name="currentTurnPlayerId">현재 턴 플레이어 ID입니다.</param>
    /// <param name="player">요청 플레이어입니다.</param>
    /// <param name="dealerStandScore">딜러 스탠드 기준 점수입니다.</param>
    /// <returns>히트 처리 결과입니다.</returns>
    RoundResolution HandleHit(
        List<PlayerState> players,
        Shoe shoe,
        string currentTurnPlayerId,
        PlayerState player,
        int dealerStandScore);

    /// <summary>
    /// 스탠드 요청을 처리합니다.
    /// </summary>
    /// <param name="players">플레이어 목록입니다.</param>
    /// <param name="shoe">카드 더미입니다.</param>
    /// <param name="player">요청 플레이어입니다.</param>
    /// <param name="dealerStandScore">딜러 스탠드 기준 점수입니다.</param>
    /// <returns>스탠드 처리 결과입니다.</returns>
    RoundResolution HandleStand(
        List<PlayerState> players,
        Shoe shoe,
        PlayerState player,
        int dealerStandScore);

    /// <summary>
    /// 일반 플레이어 턴이 끝났을 때 딜러 자동 진행과 결과 정산을 처리합니다.
    /// </summary>
    /// <param name="players">플레이어 목록입니다.</param>
    /// <param name="shoe">카드 더미입니다.</param>
    /// <param name="dealerStandScore">딜러 스탠드 기준 점수입니다.</param>
    /// <returns>정산 처리 결과입니다.</returns>
    RoundResolution CompleteRound(List<PlayerState> players, Shoe shoe, int dealerStandScore);

    /// <summary>
    /// 다음으로 행동할 일반 플레이어 ID를 계산합니다.
    /// </summary>
    /// <param name="players">플레이어 목록입니다.</param>
    /// <returns>다음 턴 플레이어 ID입니다. 없으면 빈 문자열입니다.</returns>
    string ResolveNextTurnPlayerId(IReadOnlyCollection<PlayerState> players);

    /// <summary>
    /// 아직 행동 가능한 일반 플레이어가 남아 있는지 확인합니다.
    /// </summary>
    /// <param name="players">플레이어 목록입니다.</param>
    /// <returns>남아 있으면 true입니다.</returns>
    bool HasPlayableNonDealer(IReadOnlyCollection<PlayerState> players);
}
