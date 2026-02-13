using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Seoul.It.Blackjack.Frontend.Pages;
using Seoul.It.Blackjack.Frontend.Services;
using System;
using BunitContext = Bunit.BunitContext;

namespace Seoul.It.Blackjack.Frontend.Tests;

[TestClass]
public sealed class EntryPageBunitTests
{
    [TestMethod]
    public void NextButtonIsDisabledWhenNameIsBlank()
    {
        using BunitContext context = new();
        context.Services.AddScoped(_ => new FrontendEntryState());

        IRenderedComponent<Entry> component = context.Render<Entry>();

        Assert.IsTrue(component.Find("#next").HasAttribute("disabled"));
    }

    [TestMethod]
    public void NextButtonStoresEntryStateAndNavigatesToTable()
    {
        using BunitContext context = new();
        context.Services.AddScoped(_ => new FrontendEntryState());

        IRenderedComponent<Entry> component = context.Render<Entry>();
        component.Find("#playerName").Change("Alice");
        component.Find("#dealerKey").Change("DEALER_SECRET_KEY");
        component.Find("#next").Click();

        FrontendEntryState state = context.Services.GetRequiredService<FrontendEntryState>();
        NavigationManager navigation = context.Services.GetRequiredService<NavigationManager>();

        Assert.AreEqual("Alice", state.PlayerName);
        Assert.AreEqual("DEALER_SECRET_KEY", state.DealerKey);
        Assert.IsTrue(navigation.Uri.EndsWith("/table", StringComparison.OrdinalIgnoreCase));
    }
}
