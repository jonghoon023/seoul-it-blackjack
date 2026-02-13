using System.Threading.Tasks;

namespace Seoul.It.Blackjack.Core.Contracts;

/// <summary>
/// 클라이언트가 서버에 요청할 수 있는 명령 계약입니다.
/// </summary>
public interface IBlackjackServer
{
    /// <summary>
    /// 게임에 입장합니다.
    /// </summary>
    /// <param name="name">플레이어 이름입니다.</param>
    /// <param name="dealerKey">딜러로 입장할 때 쓰는 선택 키입니다.</param>
    /// <returns>비동기 완료를 나타내는 작업입니다.</returns>
    Task Join(string name, string? dealerKey);

    /// <summary>
    /// 게임에서 나갑니다.
    /// </summary>
    /// <returns>비동기 완료를 나타내는 작업입니다.</returns>
    Task Leave();

    /// <summary>
    /// 라운드를 시작합니다.
    /// </summary>
    /// <returns>비동기 완료를 나타내는 작업입니다.</returns>
    Task StartRound();

    /// <summary>
    /// 현재 턴 플레이어가 카드 한 장을 더 받습니다.
    /// </summary>
    /// <returns>비동기 완료를 나타내는 작업입니다.</returns>
    Task Hit();

    /// <summary>
    /// 현재 턴 플레이어가 행동을 멈춥니다.
    /// </summary>
    /// <returns>비동기 완료를 나타내는 작업입니다.</returns>
    Task Stand();
}
