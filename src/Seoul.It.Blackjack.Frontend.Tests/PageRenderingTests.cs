using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Threading.Tasks;

namespace Seoul.It.Blackjack.Frontend.Tests;

[TestClass]
public sealed class PageRenderingTests
{
    [TestMethod]
    public async Task RootPage_ShouldRespondSuccessfully()
    {
        using TestHostFactory factory = new();
        using HttpClient client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = true,
        });

        HttpResponseMessage response = await client.GetAsync("/");
        string body = await response.Content.ReadAsStringAsync();

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        StringAssert.Contains(body, "<html");
    }

    [TestMethod]
    public async Task TablePage_ShouldRespondSuccessfully()
    {
        using TestHostFactory factory = new();
        using HttpClient client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = true,
        });

        HttpResponseMessage response = await client.GetAsync("/table");
        string body = await response.Content.ReadAsStringAsync();

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        StringAssert.Contains(body, "<html");
    }
}
