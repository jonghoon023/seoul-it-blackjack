namespace Seoul.It.Blackjack.Backend.Options;

internal sealed class GameRuleOptions
{
    public const string DefaultSectionName = "GameRules";

    public int DeckCount { get; set; } = 4;

    public int DealerStandScore { get; set; } = 17;

    public int MinPlayersToStart { get; set; } = 2;

    public int MinNameLength { get; set; } = 1;

    public int MaxNameLength { get; set; } = 20;
}
