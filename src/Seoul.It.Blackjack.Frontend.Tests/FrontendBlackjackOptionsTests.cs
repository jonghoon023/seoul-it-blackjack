using Microsoft.VisualStudio.TestTools.UnitTesting;
using Seoul.It.Blackjack.Frontend.Options;

namespace Seoul.It.Blackjack.Frontend.Tests;

[TestClass]
public sealed class FrontendBlackjackOptionsTests
{
    [TestMethod]
    public void Defaults_ShouldMatchExpectedValues()
    {
        FrontendBlackjackOptions options = new();

        Assert.AreEqual("BlackjackClient", FrontendBlackjackOptions.DefaultSectionName);
        Assert.AreEqual("http://localhost:5000/blackjack", FrontendBlackjackOptions.DefaultHubUrl);
        Assert.AreEqual("http://localhost:5000/blackjack", options.HubUrl);
    }
}
