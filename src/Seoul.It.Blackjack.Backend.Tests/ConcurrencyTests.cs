using Microsoft.VisualStudio.TestTools.UnitTesting;
using Seoul.It.Blackjack.Core.Contracts;
using System.Linq;
using System.Threading.Tasks;

namespace Seoul.It.Blackjack.Backend.Tests;

[TestClass]
public class ConcurrencyTests
{
    private const string DealerKey = "DEALER_SECRET_KEY";

    [TestMethod]
    public async Task ConcurrentHitRequests_AreSerializedAndStayConsistent()
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

        int firstStatesBefore = first.States.Count;
        int secondStatesBefore = second.States.Count;

        Task hit1 = first.HitAsync();
        Task hit2 = second.HitAsync();
        await Task.WhenAll(hit1, hit2);

        await TestWaiter.WaitUntilAsync(() =>
            first.States.Count > firstStatesBefore &&
            second.States.Count > secondStatesBefore);
        await TestWaiter.WaitUntilAsync(() => second.Errors.Any(error => error.Code == "NOT_YOUR_TURN"));

        GameState state = first.States.Last();
        Assert.IsTrue(state.Players.Count >= 3);
        Assert.IsTrue(state.Players.Any(player => player.IsDealer));
    }
}
