namespace Seoul.It.Blackjack.Frontend.Options;

public sealed class FrontendBlackjackOptions
{
    public const string DefaultSectionName = "BlackjackClient";

    public const string DefaultHubUrl = "http://localhost:5000/blackjack";

    public string HubUrl { get; set; } = DefaultHubUrl;
}
