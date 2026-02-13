using System.Threading.Tasks;

namespace Seoul.It.Blackjack.Core.Contracts;

/// <summary>
/// 서버가 클라이언트에게 보내는 콜백 계약입니다.
/// </summary>
public interface IBlackjackClient
{
    /// <summary>
    /// 게임 상태가 바뀌었을 때 최신 상태를 전달합니다.
    /// </summary>
    /// <param name="state">최신 게임 상태입니다.</param>
    /// <returns>비동기 완료를 나타내는 작업입니다.</returns>
    Task OnStateChanged(GameState state);

    /// <summary>
    /// 처리 중 오류가 생겼을 때 오류 코드와 메시지를 전달합니다.
    /// </summary>
    /// <param name="code">고정된 오류 코드입니다.</param>
    /// <param name="message">사람이 읽을 수 있는 오류 설명입니다.</param>
    /// <returns>비동기 완료를 나타내는 작업입니다.</returns>
    Task OnError(string code, string message);
}
