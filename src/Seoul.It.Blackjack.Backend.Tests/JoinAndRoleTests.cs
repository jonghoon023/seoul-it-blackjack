using Microsoft.VisualStudio.TestTools.UnitTesting;
using Seoul.It.Blackjack.Core.Contracts;
using System.Linq;
using System.Threading.Tasks;

namespace Seoul.It.Blackjack.Backend.Tests;

[TestClass]
public class JoinAndRoleTests
{
    private const string DealerKey = "DEALER_SECRET_KEY";

    [TestMethod]
    public async Task Join_WithDealerKey_AssignsDealerRole()
    {
        using TestHostFactory factory = new();
        await using SignalRTestClient dealer = new(factory);

        await dealer.ConnectAsync();
        await dealer.JoinAsync("Dealer", DealerKey);
        await TestWaiter.WaitUntilAsync(() => dealer.States.Count >= 1);

        GameState state = dealer.States.Last();
        Assert.AreEqual(1, state.Players.Count);
        Assert.IsTrue(state.Players.Single().IsDealer);
    }

    [TestMethod]
    public async Task Join_WithInvalidDealerKey_JoinsAsPlayer()
    {
        using TestHostFactory factory = new();
        await using SignalRTestClient player = new(factory);

        await player.ConnectAsync();
        await player.JoinAsync("Player", "WRONG_KEY");
        await TestWaiter.WaitUntilAsync(() => player.States.Count >= 1);

        GameState state = player.States.Last();
        Assert.IsFalse(state.Players.Single().IsDealer);
    }

    [TestMethod]
    public async Task Join_WithEmptyName_ReturnsInvalidNameError()
    {
        using TestHostFactory factory = new();
        await using SignalRTestClient client = new(factory);

        await client.ConnectAsync();
        await client.JoinAsync("   ");
        await TestWaiter.WaitUntilAsync(() => client.Errors.Count >= 1);

        Assert.AreEqual("INVALID_NAME", client.Errors.Last().Code);
    }

    [TestMethod]
    public async Task Join_WhenDealerAlreadyExists_ReturnsDealerAlreadyExistsError()
    {
        using TestHostFactory factory = new();
        await using SignalRTestClient dealer = new(factory);
        await using SignalRTestClient another = new(factory);

        await dealer.ConnectAsync();
        await another.ConnectAsync();
        await dealer.JoinAsync("Dealer", DealerKey);
        await TestWaiter.WaitUntilAsync(() => dealer.States.Count >= 1);

        await another.JoinAsync("AnotherDealer", DealerKey);
        await TestWaiter.WaitUntilAsync(() => another.Errors.Count >= 1);

        Assert.AreEqual("DEALER_ALREADY_EXISTS", another.Errors.Last().Code);
    }

    [TestMethod]
    public async Task Join_WhenSameConnectionJoinsTwice_ReturnsAlreadyJoinedError()
    {
        using TestHostFactory factory = new();
        await using SignalRTestClient client = new(factory);

        await client.ConnectAsync();
        await client.JoinAsync("Player");
        await TestWaiter.WaitUntilAsync(() => client.States.Count >= 1);

        await client.JoinAsync("PlayerAgain");
        await TestWaiter.WaitUntilAsync(() => client.Errors.Count >= 1);

        Assert.AreEqual("ALREADY_JOINED", client.Errors.Last().Code);
    }
}
