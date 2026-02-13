using Microsoft.VisualStudio.TestTools.UnitTesting;
using Seoul.It.Blackjack.Core.Contracts;
using System.Linq;
using System.Threading.Tasks;

namespace Seoul.It.Blackjack.Backend.Tests;

[TestClass]
public class RoundCompletionTests
{
    private const string DealerKey = "DEALER_SECRET_KEY";

    [TestMethod]
    public async Task Stand_CompletesRound_AndMovesToIdle()
    {
        using TestHostFactory factory = new();
        await using SignalRTestClient dealer = new(factory);
        await using SignalRTestClient player = new(factory);

        await dealer.ConnectAsync();
        await player.ConnectAsync();
        await dealer.JoinAsync("Dealer", DealerKey);
        await player.JoinAsync("Player");
        await dealer.StartRoundAsync();
        await TestWaiter.WaitUntilAsync(() => player.States.Count >= 3);

        await player.StandAsync();
        await TestWaiter.WaitUntilAsync(() => player.States.Count >= 4);

        GameState state = player.States.Last();
        Assert.AreEqual(GamePhase.Idle, state.Phase);
        Assert.IsTrue(string.IsNullOrEmpty(state.CurrentTurnPlayerId));
        Assert.AreEqual(2, state.Players.Count);
        Assert.AreNotEqual(
            RoundOutcome.None,
            state.Players.Single(value => !value.IsDealer).Outcome);
    }

    [TestMethod]
    public async Task NextRound_ResetsRoundFields_ButKeepsIdleSnapshotBeforeStart()
    {
        using TestHostFactory factory = new();
        await using SignalRTestClient dealer = new(factory);
        await using SignalRTestClient player = new(factory);

        await dealer.ConnectAsync();
        await player.ConnectAsync();
        await dealer.JoinAsync("Dealer", DealerKey);
        await player.JoinAsync("Player");
        await dealer.StartRoundAsync();
        await TestWaiter.WaitUntilAsync(() => player.States.Count >= 3);

        await player.StandAsync();
        await TestWaiter.WaitUntilAsync(() => player.States.Count >= 4);

        GameState idleState = player.States.Last();
        PlayerState idlePlayer = idleState.Players.Single(value => !value.IsDealer);
        Assert.IsTrue(idlePlayer.Cards.Count >= 2);
        Assert.AreNotEqual(RoundOutcome.None, idlePlayer.Outcome);

        await dealer.StartRoundAsync();
        await TestWaiter.WaitUntilAsync(() => player.States.Count >= 5);

        GameState nextRoundState = player.States.Last();
        PlayerState nextRoundPlayer = nextRoundState.Players.Single(value => !value.IsDealer);
        Assert.AreEqual(GamePhase.InRound, nextRoundState.Phase);
        Assert.AreEqual(2, nextRoundPlayer.Cards.Count);
        Assert.AreEqual(RoundOutcome.None, nextRoundPlayer.Outcome);
    }
}
