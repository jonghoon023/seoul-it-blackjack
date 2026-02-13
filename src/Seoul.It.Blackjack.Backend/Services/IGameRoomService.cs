using Seoul.It.Blackjack.Backend.Services.Commands;
using System.Threading.Tasks;

namespace Seoul.It.Blackjack.Backend.Services;

/// <summary>
/// 게임 방의 핵심 명령을 처리하는 서비스 계약입니다.
/// </summary>
internal interface IGameRoomService
{
    /// <summary>
    /// 연결 사용자를 게임에 입장시킵니다.
    /// </summary>
    /// <param name="connectionId">요청 연결 ID입니다.</param>
    /// <param name="name">입장 이름입니다.</param>
    /// <param name="dealerKey">딜러 키입니다.</param>
    /// <returns>처리 결과입니다.</returns>
    Task<GameOperationResult> JoinAsync(string connectionId, string name, string? dealerKey);

    /// <summary>
    /// 연결 사용자를 게임에서 나가게 합니다.
    /// </summary>
    /// <param name="connectionId">요청 연결 ID입니다.</param>
    /// <returns>처리 결과입니다.</returns>
    Task<GameOperationResult> LeaveAsync(string connectionId);

    /// <summary>
    /// 연결 끊김을 퇴장과 같은 규칙으로 처리합니다.
    /// </summary>
    /// <param name="connectionId">끊어진 연결 ID입니다.</param>
    /// <returns>처리 결과입니다.</returns>
    Task<GameOperationResult> DisconnectAsync(string connectionId);

    /// <summary>
    /// 라운드를 시작합니다.
    /// </summary>
    /// <param name="connectionId">요청 연결 ID입니다.</param>
    /// <returns>처리 결과입니다.</returns>
    Task<GameOperationResult> StartRoundAsync(string connectionId);

    /// <summary>
    /// 현재 턴 플레이어의 히트 요청을 처리합니다.
    /// </summary>
    /// <param name="connectionId">요청 연결 ID입니다.</param>
    /// <returns>처리 결과입니다.</returns>
    Task<GameOperationResult> HitAsync(string connectionId);

    /// <summary>
    /// 현재 턴 플레이어의 스탠드 요청을 처리합니다.
    /// </summary>
    /// <param name="connectionId">요청 연결 ID입니다.</param>
    /// <returns>처리 결과입니다.</returns>
    Task<GameOperationResult> StandAsync(string connectionId);
}
