using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Threading.Tasks;

namespace Seoul.It.Blackjack.Frontend.Tests;

[TestClass]
public sealed class StaticCardAssetsTests
{
    [TestMethod]
    public async Task CardSvg_ShouldBeServedFromWwwroot()
    {
        using TestHostFactory factory = new();
        using HttpClient client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = true,
        });

        HttpResponseMessage response = await client.GetAsync("/cards/clubs_ace.svg");
        string body = await response.Content.ReadAsStringAsync();

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.IsTrue(response.Content.Headers.ContentType?.MediaType?.Contains("svg", System.StringComparison.OrdinalIgnoreCase) ?? false);
        StringAssert.Contains(body, "<svg");
    }
}
