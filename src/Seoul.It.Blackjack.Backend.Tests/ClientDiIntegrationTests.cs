using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Seoul.It.Blackjack.Client;
using Seoul.It.Blackjack.Client.Extensions;
using Seoul.It.Blackjack.Client.Options;
using Seoul.It.Blackjack.Core.Contracts;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Seoul.It.Blackjack.Backend.Tests;

[TestClass]
public class ClientDiIntegrationTests
{
    [TestMethod]
    public async Task AddBlackjackClient_RegistersSingletonAndAppliesOptions()
    {
        const string hubUrl = "http://localhost/blackjack";

        ServiceCollection services = new();
        services.AddBlackjackClient(options => options.HubUrl = hubUrl);
        await using ServiceProvider provider = services.BuildServiceProvider();

        BlackjackClient first = provider.GetRequiredService<BlackjackClient>();
        BlackjackClient second = provider.GetRequiredService<BlackjackClient>();
        BlackjackClientOptions options = provider.GetRequiredService<BlackjackClientOptions>();

        Assert.AreSame(first, second);
        Assert.AreEqual(hubUrl, options.HubUrl);
    }

    [TestMethod]
    public async Task AddBlackjackClient_ConnectJoin_ReceivesStateChanged()
    {
        using TestHostFactory factory = new();
        Uri hubUrl = new(factory.Server.BaseAddress, "/blackjack");

        ServiceCollection services = new();
        services.AddBlackjackClient(options => options.HubUrl = hubUrl.ToString());
        await using ServiceProvider provider = services.BuildServiceProvider();

        BlackjackClient client = provider.GetRequiredService<BlackjackClient>();
        BlackjackClientOptions options = provider.GetRequiredService<BlackjackClientOptions>();
        TaskCompletionSource<GameState> stateReceived = new(TaskCreationOptions.RunContinuationsAsynchronously);
        client.StateChanged += state => stateReceived.TrySetResult(state);

        await client.ConnectAsync(options.HubUrl, () => factory.Server.CreateHandler());
        await client.JoinAsync("ClientDiUser");

        Task completed = await Task.WhenAny(stateReceived.Task, Task.Delay(3000));
        Assert.AreEqual(stateReceived.Task, completed);

        GameState state = await stateReceived.Task;
        Assert.IsTrue(state.Players.Any(player => player.Name == "ClientDiUser"));
    }
}
