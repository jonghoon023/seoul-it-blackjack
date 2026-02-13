using System;

namespace Seoul.It.Blackjack.Core.Domain;

/// <summary>
/// 카드 숫자(랭크) 종류를 나타냅니다.
/// </summary>
public enum Rank
{
    /// <summary>
    /// 숫자 2 카드입니다.
    /// </summary>
    Two,

    /// <summary>
    /// 숫자 3 카드입니다.
    /// </summary>
    Three,

    /// <summary>
    /// 숫자 4 카드입니다.
    /// </summary>
    Four,

    /// <summary>
    /// 숫자 5 카드입니다.
    /// </summary>
    Five,

    /// <summary>
    /// 숫자 6 카드입니다.
    /// </summary>
    Six,

    /// <summary>
    /// 숫자 7 카드입니다.
    /// </summary>
    Seven,

    /// <summary>
    /// 숫자 8 카드입니다.
    /// </summary>
    Eight,

    /// <summary>
    /// 숫자 9 카드입니다.
    /// </summary>
    Nine,

    /// <summary>
    /// 숫자 10 카드입니다.
    /// </summary>
    Ten,

    /// <summary>
    /// 잭 카드입니다.
    /// </summary>
    Jack,

    /// <summary>
    /// 퀸 카드입니다.
    /// </summary>
    Queen,

    /// <summary>
    /// 킹 카드입니다.
    /// </summary>
    King,

    /// <summary>
    /// 에이스 카드입니다.
    /// </summary>
    Ace
}

/// <summary>
/// 카드 숫자(Rank)를 점수로 바꿀 때 쓰는 도우미 메서드를 제공합니다.
/// </summary>
public static class RankExtension
{
    /// <summary>
    /// 카드 숫자를 이 프로젝트의 블랙잭 점수 규칙에 맞게 정수 점수로 바꿉니다.
    /// </summary>
    /// <param name="rank">점수로 바꿀 카드 숫자입니다.</param>
    /// <returns>규칙에 맞는 카드 점수입니다.</returns>
    public static int ToValue(this Rank rank) => rank switch
    {
        Rank.Ace => 1,
        Rank.Two => 2,
        Rank.Three => 3,
        Rank.Four => 4,
        Rank.Five => 5,
        Rank.Six => 6,
        Rank.Seven => 7,
        Rank.Eight => 8,
        Rank.Nine => 9,
        Rank.Ten or Rank.Jack or Rank.Queen or Rank.King => 10,
        _ => throw new ArgumentOutOfRangeException(nameof(rank), rank, "Unknown card rank."),
    };
}
