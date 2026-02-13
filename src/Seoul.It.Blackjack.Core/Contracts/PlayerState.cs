using Seoul.It.Blackjack.Core.Domain;
using System.Collections.Generic;

namespace Seoul.It.Blackjack.Core.Contracts;

/// <summary>
/// 플레이어 한 명의 현재 상태를 담는 데이터입니다.
/// </summary>
public sealed class PlayerState
{
    /// <summary>
    /// 플레이어를 구분하는 고유 ID입니다.
    /// </summary>
    public string PlayerId { get; set; } = string.Empty;

    /// <summary>
    /// 화면에 보여줄 플레이어 이름입니다.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 딜러 여부입니다.
    /// true면 딜러, false면 일반 플레이어입니다.
    /// </summary>
    public bool IsDealer { get; set; }

    /// <summary>
    /// 현재 손에 들고 있는 카드 목록입니다.
    /// </summary>
    public List<Card> Cards { get; set; } = new List<Card>();

    /// <summary>
    /// 현재 카드 점수 합계입니다.
    /// </summary>
    public int Score { get; set; }

    /// <summary>
    /// 이번 라운드의 턴 상태입니다.
    /// </summary>
    public PlayerTurnState TurnState { get; set; } = PlayerTurnState.Playing;

    /// <summary>
    /// 라운드 결과입니다.
    /// </summary>
    public RoundOutcome Outcome { get; set; } = RoundOutcome.None;
}
