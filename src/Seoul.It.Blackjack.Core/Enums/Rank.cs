namespace Seoul.It.Blackjack.Core.Enums;

public enum Rank
{
    Two,
    Three,
    Four,
    Five,
    Six,
    Seven,
    Eight,
    Nine,
    Ten,
    Jack,
    Queen,
    King,
    Ace
}

public static class RaanExtension
{
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
        Rank.Ten or Rank.Jack or Rank.Queen or Rank.Jack or Rank.King => 10,
        _ => throw new NotImplementedException(),
    };
}
