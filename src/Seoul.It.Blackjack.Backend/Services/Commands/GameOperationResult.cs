using Seoul.It.Blackjack.Core.Contracts;

namespace Seoul.It.Blackjack.Backend.Services.Commands;

/// <summary>
/// 게임 명령 처리 후 허브로 전달할 결과 데이터입니다.
/// </summary>
internal sealed class GameOperationResult
{
    /// <summary>
    /// 처리 후의 최신 게임 상태입니다.
    /// </summary>
    public GameState State { get; set; } = new();

    /// <summary>
    /// 상태 브로드캐스트 여부입니다.
    /// </summary>
    public bool ShouldPublishState { get; set; } = true;

    /// <summary>
    /// 전체 공지가 필요할 때 전달할 공지 데이터입니다.
    /// </summary>
    public GameNotice? Notice { get; set; }
}
