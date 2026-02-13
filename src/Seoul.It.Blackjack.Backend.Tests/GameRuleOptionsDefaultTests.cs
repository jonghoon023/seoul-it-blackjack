using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;

namespace Seoul.It.Blackjack.Backend.Tests;

[TestClass]
public class GameRuleOptionsDefaultTests
{
    private const string DealerKey = "DEALER_SECRET_KEY";

    [TestMethod]
    public async Task StartRound_WithOnlyDealer_UsesDefaultMinPlayerRule()
    {
        using TestHostFactory factory = new();
        await using SignalRTestClient dealer = new(factory);

        await dealer.ConnectAsync();
        await dealer.JoinAsync("Dealer", DealerKey);
        await TestWaiter.WaitUntilAsync(() => dealer.States.Count >= 1);

        await dealer.StartRoundAsync();
        await TestWaiter.WaitUntilAsync(() => dealer.Errors.Count >= 1);

        Assert.AreEqual("INSUFFICIENT_PLAYERS", dealer.Errors.Last().Code);
    }
}
