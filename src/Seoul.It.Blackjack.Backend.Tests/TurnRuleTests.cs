using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;

namespace Seoul.It.Blackjack.Backend.Tests;

[TestClass]
public class TurnRuleTests
{
    private const string DealerKey = "DEALER_SECRET_KEY";

    [TestMethod]
    public async Task StartRound_ByNonDealer_ReturnsNotDealer()
    {
        using TestHostFactory factory = new();
        await using SignalRTestClient dealer = new(factory);
        await using SignalRTestClient player = new(factory);

        await dealer.ConnectAsync();
        await player.ConnectAsync();
        await dealer.JoinAsync("Dealer", DealerKey);
        await player.JoinAsync("Player");
        await TestWaiter.WaitUntilAsync(() => player.States.Count >= 2);

        await player.StartRoundAsync();
        await TestWaiter.WaitUntilAsync(() => player.Errors.Count >= 1);

        Assert.AreEqual("NOT_DEALER", player.Errors.Last().Code);
    }

    [TestMethod]
    public async Task Join_InRound_ReturnsGameInProgress()
    {
        using TestHostFactory factory = new();
        await using SignalRTestClient dealer = new(factory);
        await using SignalRTestClient player = new(factory);
        await using SignalRTestClient lateJoiner = new(factory);

        await dealer.ConnectAsync();
        await player.ConnectAsync();
        await lateJoiner.ConnectAsync();
        await dealer.JoinAsync("Dealer", DealerKey);
        await player.JoinAsync("Player");
        await dealer.StartRoundAsync();
        await TestWaiter.WaitUntilAsync(() => dealer.States.Count >= 3);

        await lateJoiner.JoinAsync("Late");
        await TestWaiter.WaitUntilAsync(() => lateJoiner.Errors.Count >= 1);

        Assert.AreEqual("GAME_IN_PROGRESS", lateJoiner.Errors.Last().Code);
    }

    [TestMethod]
    public async Task Hit_OutOfTurn_ReturnsNotYourTurn()
    {
        using TestHostFactory factory = new();
        await using SignalRTestClient dealer = new(factory);
        await using SignalRTestClient first = new(factory);
        await using SignalRTestClient second = new(factory);

        await dealer.ConnectAsync();
        await first.ConnectAsync();
        await second.ConnectAsync();
        await dealer.JoinAsync("Dealer", DealerKey);
        await first.JoinAsync("First");
        await second.JoinAsync("Second");
        await dealer.StartRoundAsync();
        await TestWaiter.WaitUntilAsync(() => second.States.Count >= 4);

        await second.HitAsync();
        await TestWaiter.WaitUntilAsync(() => second.Errors.Count >= 1);

        Assert.AreEqual("NOT_YOUR_TURN", second.Errors.Last().Code);
    }

    [TestMethod]
    public async Task Hit_ByDealer_ReturnsDealerIsAuto()
    {
        using TestHostFactory factory = new();
        await using SignalRTestClient dealer = new(factory);
        await using SignalRTestClient player = new(factory);

        await dealer.ConnectAsync();
        await player.ConnectAsync();
        await dealer.JoinAsync("Dealer", DealerKey);
        await player.JoinAsync("Player");
        await dealer.StartRoundAsync();
        await TestWaiter.WaitUntilAsync(() => dealer.States.Count >= 3);

        await dealer.HitAsync();
        await TestWaiter.WaitUntilAsync(() => dealer.Errors.Count >= 1);

        Assert.AreEqual("DEALER_IS_AUTO", dealer.Errors.Last().Code);
    }

    [TestMethod]
    public async Task Hit_BroadcastsStateToAllClients()
    {
        using TestHostFactory factory = new();
        await using SignalRTestClient dealer = new(factory);
        await using SignalRTestClient player = new(factory);
        await using SignalRTestClient observer = new(factory);

        await dealer.ConnectAsync();
        await player.ConnectAsync();
        await observer.ConnectAsync();
        await dealer.JoinAsync("Dealer", DealerKey);
        await player.JoinAsync("Player");
        await observer.JoinAsync("Observer");
        await dealer.StartRoundAsync();
        await TestWaiter.WaitUntilAsync(() => observer.States.Count >= 4);

        int dealerBefore = dealer.States.Count;
        int playerBefore = player.States.Count;
        int observerBefore = observer.States.Count;

        await player.HitAsync();
        await TestWaiter.WaitUntilAsync(() =>
            dealer.States.Count > dealerBefore &&
            player.States.Count > playerBefore &&
            observer.States.Count > observerBefore);
    }
}
