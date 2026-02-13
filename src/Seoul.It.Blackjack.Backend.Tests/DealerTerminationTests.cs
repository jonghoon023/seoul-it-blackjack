using Microsoft.VisualStudio.TestTools.UnitTesting;
using Seoul.It.Blackjack.Core.Contracts;
using System.Linq;
using System.Threading.Tasks;

namespace Seoul.It.Blackjack.Backend.Tests;

[TestClass]
public class DealerTerminationTests
{
    private const string DealerKey = "DEALER_SECRET_KEY";

    [TestMethod]
    public async Task DealerLeave_BroadcastsGameTerminated_AndClearsState()
    {
        using TestHostFactory factory = new();
        await using SignalRTestClient dealer = new(factory);
        await using SignalRTestClient player = new(factory);

        await dealer.ConnectAsync();
        await player.ConnectAsync();
        await dealer.JoinAsync("Dealer", DealerKey);
        await player.JoinAsync("Player");
        await TestWaiter.WaitUntilAsync(() => player.States.Count >= 2);

        await dealer.LeaveAsync();
        await TestWaiter.WaitUntilAsync(() => player.Errors.Any(error => error.Code == "GAME_TERMINATED"));
        await TestWaiter.WaitUntilAsync(() => player.States.Count >= 3);

        GameState state = player.States.Last();
        Assert.AreEqual(GamePhase.Idle, state.Phase);
        Assert.AreEqual(0, state.Players.Count);
    }

    [TestMethod]
    public async Task DealerDisconnect_BroadcastsGameTerminated_AndClearsState()
    {
        using TestHostFactory factory = new();
        await using SignalRTestClient dealer = new(factory);
        await using SignalRTestClient player = new(factory);

        await dealer.ConnectAsync();
        await player.ConnectAsync();
        await dealer.JoinAsync("Dealer", DealerKey);
        await player.JoinAsync("Player");
        await TestWaiter.WaitUntilAsync(() => player.States.Count >= 2);

        await dealer.DisposeAsync();
        await TestWaiter.WaitUntilAsync(() => player.Errors.Any(error => error.Code == "GAME_TERMINATED"));
        await TestWaiter.WaitUntilAsync(() => player.States.Count >= 3);

        GameState state = player.States.Last();
        Assert.AreEqual(GamePhase.Idle, state.Phase);
        Assert.AreEqual(0, state.Players.Count);
    }

    [TestMethod]
    public async Task PlayerLeave_RemovesOnlyThatPlayer()
    {
        using TestHostFactory factory = new();
        await using SignalRTestClient dealer = new(factory);
        await using SignalRTestClient player = new(factory);

        await dealer.ConnectAsync();
        await player.ConnectAsync();
        await dealer.JoinAsync("Dealer", DealerKey);
        await player.JoinAsync("Player");
        await TestWaiter.WaitUntilAsync(() => dealer.States.Count >= 2);

        await player.LeaveAsync();
        await TestWaiter.WaitUntilAsync(() => dealer.States.Count >= 3);

        GameState state = dealer.States.Last();
        Assert.AreEqual(1, state.Players.Count);
        Assert.IsTrue(state.Players.Single().IsDealer);
    }

    [TestMethod]
    public async Task PlayerDisconnect_RemovesOnlyThatPlayer()
    {
        using TestHostFactory factory = new();
        await using SignalRTestClient dealer = new(factory);
        await using SignalRTestClient player = new(factory);

        await dealer.ConnectAsync();
        await player.ConnectAsync();
        await dealer.JoinAsync("Dealer", DealerKey);
        await player.JoinAsync("Player");
        await TestWaiter.WaitUntilAsync(() => dealer.States.Count >= 2);

        await player.DisposeAsync();
        await TestWaiter.WaitUntilAsync(() => dealer.States.Count >= 3);

        GameState state = dealer.States.Last();
        Assert.AreEqual(1, state.Players.Count);
        Assert.IsTrue(state.Players.Single().IsDealer);
    }
}
