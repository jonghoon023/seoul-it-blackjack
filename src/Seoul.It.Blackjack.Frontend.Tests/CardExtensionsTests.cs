using Microsoft.VisualStudio.TestTools.UnitTesting;
using Seoul.It.Blackjack.Core.Domain;
using Seoul.It.Blackjack.Frontend.Extensions;

namespace Seoul.It.Blackjack.Frontend.Tests;

[TestClass]
public sealed class CardExtensionsTests
{
    [TestMethod]
    public void ToAssetPath_ShouldReturnExpectedRelativePath()
    {
        Card card = new(Suit.Spades, Rank.Ace);

        string path = card.ToAssetPath();

        Assert.AreEqual("cards/spades_ace.svg", path);
    }
}
