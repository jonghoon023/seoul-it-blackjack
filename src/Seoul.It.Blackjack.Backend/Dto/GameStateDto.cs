namespace Seoul.It.Blackjack.Backend.Dto;

/// <summary>
/// 클라이언트에 전송되는 게임 상태 정보입니다. 프론트엔드(블레이저 등)가 없는 현재 단계에서도
/// 콘솔 테스트 클라이언트가 상태를 이해하기 위해 사용됩니다.
/// </summary>
public class GameStateDto
{
    /// <summary>
    /// 현재 게임 단계입니다. Waiting/PlayersTurn/DealerTurn/Finished 중 하나의 문자열로 전달됩니다.
    /// </summary>
    public string Phase { get; set; } = "Waiting";

    /// <summary>
    /// 딜러의 상태 정보입니다.
    /// </summary>
    public DealerDto Dealer { get; set; } = new DealerDto();

    /// <summary>
    /// 플레이어 목록입니다.
    /// </summary>
    public List<PlayerDto> Players { get; set; } = new List<PlayerDto>();

    /// <summary>
    /// 안내 메시지 또는 게임 설명을 넣을 수 있는 문자열입니다.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 마지막에 어떤 액션이 있었는지 기록합니다. 예: "Alice Hit".
    /// </summary>
    public string LastAction { get; set; } = string.Empty;
}

/// <summary>
/// 딜러의 정보입니다. 홀카드(뒷면 카드)를 공개했는지 여부에 따라 카드 목록 표현이 달라집니다.
/// </summary>
public class DealerDto
{
    public List<string> Cards { get; set; } = new List<string>();
    public int Score { get; set; }
    public bool IsHoleRevealed { get; set; }
}

/// <summary>
/// 플레이어의 상태 정보입니다.
/// </summary>
public class PlayerDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<string> Cards { get; set; } = new List<string>();
    public int Score { get; set; }
    public bool IsTurn { get; set; }
    public bool IsBusted { get; set; }
    public string? Result { get; set; }
}
