namespace Seoul.It.Blackjack.Backend.Services.Commands;

/// <summary>
/// 큐에 넣어 직렬 처리할 게임 명령 데이터입니다.
/// </summary>
internal sealed class GameCommand
{
    /// <summary>
    /// 게임 명령 데이터를 만듭니다.
    /// </summary>
    /// <param name="type">명령 종류입니다.</param>
    /// <param name="connectionId">요청한 연결 ID입니다.</param>
    /// <param name="name">입장 시 사용할 이름입니다.</param>
    /// <param name="dealerKey">입장 시 사용할 딜러 키입니다.</param>
    public GameCommand(
        GameCommandType type,
        string connectionId,
        string? name = null,
        string? dealerKey = null)
    {
        Type = type;
        ConnectionId = connectionId;
        Name = name;
        DealerKey = dealerKey;
    }

    /// <summary>
    /// 명령 종류입니다.
    /// </summary>
    public GameCommandType Type { get; }

    /// <summary>
    /// 명령을 보낸 연결 ID입니다.
    /// </summary>
    public string ConnectionId { get; }

    /// <summary>
    /// 입장 명령에서 사용하는 이름입니다.
    /// </summary>
    public string? Name { get; }

    /// <summary>
    /// 입장 명령에서 사용하는 딜러 키입니다.
    /// </summary>
    public string? DealerKey { get; }
}
