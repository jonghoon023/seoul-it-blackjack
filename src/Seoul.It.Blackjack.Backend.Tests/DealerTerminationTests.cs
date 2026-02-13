using Microsoft.VisualStudio.TestTools.UnitTesting;
using Seoul.It.Blackjack.Core.Contracts;
using System.Collections.Generic;
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
    public async Task DealerLeave_BroadcastsTerminationAndResetExactlyOnce_InOrder()
    {
        using TestHostFactory factory = new();
        await using SignalRTestClient dealer = new(factory);
        await using SignalRTestClient player = new(factory);

        await dealer.ConnectAsync();
        await player.ConnectAsync();
        await dealer.JoinAsync("Dealer", DealerKey);
        await player.JoinAsync("Player");
        await TestWaiter.WaitUntilAsync(() => player.States.Count >= 2);

        int stateBefore = player.States.Count;
        int errorBefore = player.Errors.Count(value => value.Code == "GAME_TERMINATED");
        int eventBefore = player.Events.Count;

        await dealer.LeaveAsync();
        await TestWaiter.WaitUntilAsync(() =>
            player.Errors.Count(value => value.Code == "GAME_TERMINATED") == errorBefore + 1);
        await TestWaiter.WaitUntilAsync(() => player.States.Count == stateBefore + 1);

        Assert.AreEqual(errorBefore + 1, player.Errors.Count(value => value.Code == "GAME_TERMINATED"));

        List<(string Type, string? Code)> events = player.Events.Skip(eventBefore).ToList();
        Assert.AreEqual(2, events.Count);
        Assert.AreEqual("Error", events[0].Type);
        Assert.AreEqual("GAME_TERMINATED", events[0].Code);
        Assert.AreEqual("StateChanged", events[1].Type);
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
