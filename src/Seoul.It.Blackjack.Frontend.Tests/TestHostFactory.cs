using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Seoul.It.Blackjack.Frontend;

namespace Seoul.It.Blackjack.Frontend.Tests;

internal sealed class TestHostFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
    }
}
