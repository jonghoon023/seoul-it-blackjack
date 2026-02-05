namespace Seoul.It.Blackjack.Backend.Domains;

/// <summary>
/// 게임에 참여하는 한 명의 사용자를 나타냅니다.
/// 딜러와 일반 플레이어를 모두 표현하기 위해 IsDealer 플래그를 사용합니다.
/// </summary>
public class Player
{
    public Player(string id, string name, bool isDealer)
    {
        Id = id;
        Name = name;
        IsDealer = isDealer;
    }

    /// <summary>
    /// SignalR 연결 식별자입니다. 클라이언트를 구분하는 용도로 사용합니다.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// 사용자가 입력한 이름입니다.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// 딜러 여부를 나타냅니다. true면 딜러, false면 일반 플레이어입니다.
    /// </summary>
    public bool IsDealer { get; }

    /// <summary>
    /// 현재 손에 든 카드 목록입니다.
    /// </summary>
    public List<Card> Cards { get; } = [];

    /// <summary>
    /// 현재 턴인지 여부를 나타냅니다.
    /// </summary>
    public bool IsTurn { get; set; }

    /// <summary>
    /// 버스트(21점 초과) 되었는지 여부입니다.
    /// </summary>
    public bool IsBusted { get; set; }

    /// <summary>
    /// 라운드가 끝났을 때 결과를 저장합니다. "Win", "Lose", "Push" 등.
    /// </summary>
    public string? Result { get; set; }
}
