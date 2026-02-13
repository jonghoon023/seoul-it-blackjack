using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Seoul.It.Blackjack.Core.Contracts;
using Seoul.It.Blackjack.Core.Domain;
using Seoul.It.Blackjack.Frontend.Pages;
using Seoul.It.Blackjack.Frontend.Services;
using System;
using System.Collections.Generic;
using BunitContext = Bunit.BunitContext;

namespace Seoul.It.Blackjack.Frontend.Tests;

[TestClass]
public sealed class TablePageBunitTests
{
    [TestMethod]
    public void TableRedirectsToRootWhenEntryStateIsMissing()
    {
        using BunitContext context = new();
        FakeFrontendGameSession session = new();
        context.Services.AddScoped<IFrontendGameSession>(_ => session);
        context.Services.AddScoped(_ => new FrontendEntryState());

        IRenderedComponent<Table> component = context.Render<Table>();
        NavigationManager navigation = context.Services.GetRequiredService<NavigationManager>();

        component.WaitForAssertion(() =>
        {
            Assert.IsTrue(navigation.Uri.EndsWith("/", StringComparison.Ordinal));
        });
        Assert.AreEqual(0, session.ConnectCallCount);
        Assert.AreEqual(0, session.JoinCallCount);
    }

    [TestMethod]
    public void TableAutoConnectsAndJoinsOnFirstRender()
    {
        using BunitContext context = new();
        FakeFrontendGameSession session = new();
        context.Services.AddScoped<IFrontendGameSession>(_ => session);
        context.Services.AddScoped(_ => new FrontendEntryState
        {
            PlayerName = "Alice",
            DealerKey = "DEALER_SECRET_KEY",
        });

        IRenderedComponent<Table> component = context.Render<Table>();

        component.WaitForAssertion(() =>
        {
            Assert.AreEqual(1, session.ConnectCallCount);
            Assert.AreEqual(1, session.JoinCallCount);
        });

        Assert.AreEqual("Alice", session.LastJoinName);
        Assert.AreEqual("DEALER_SECRET_KEY", session.LastJoinDealerKey);
    }

    [TestMethod]
    public void TableRendersStateAndHandlesButtonsAndErrors()
    {
        using BunitContext context = new();
        FakeFrontendGameSession session = new();
        context.Services.AddScoped<IFrontendGameSession>(_ => session);
        context.Services.AddScoped(_ => new FrontendEntryState
        {
            PlayerName = "Alice",
            DealerKey = string.Empty,
        });

        IRenderedComponent<Table> component = context.Render<Table>();
        component.WaitForAssertion(() => Assert.AreEqual(1, session.JoinCallCount));

        session.RaiseState(new GameState
        {
            Phase = GamePhase.InRound,
            DealerPlayerId = "dealer-1",
            CurrentTurnPlayerId = "player-1",
            StatusMessage = "라운드가 시작되었습니다.",
            Players = new List<PlayerState>
            {
                new()
                {
                    PlayerId = "player-1",
                    Name = "Alice",
                    IsDealer = false,
                    Score = 11,
                    TurnState = PlayerTurnState.Playing,
                    Outcome = RoundOutcome.None,
                    Cards = new List<Card>
                    {
                        new(Suit.Spades, Rank.Ace),
                        new(Suit.Hearts, Rank.Ten),
                    },
                },
            },
        });

        component.WaitForAssertion(() =>
        {
            Assert.AreEqual("Phase: InRound", component.Find("[data-testid='phase']").TextContent.Trim());
            StringAssert.Contains(component.Markup, "cards/spades_ace.svg");
            StringAssert.Contains(component.Markup, "cards/hearts_ten.svg");
        });

        component.Find("#startRound").Click();
        component.Find("#hit").Click();
        component.Find("#stand").Click();
        component.Find("#leave").Click();

        Assert.AreEqual(1, session.StartRoundCallCount);
        Assert.AreEqual(1, session.HitCallCount);
        Assert.AreEqual(1, session.StandCallCount);
        Assert.AreEqual(1, session.LeaveCallCount);

        session.RaiseError("NOT_DEALER", "딜러만 라운드를 시작할 수 있습니다.");

        component.WaitForAssertion(() =>
        {
            StringAssert.Contains(component.Markup, "NOT_DEALER");
        });
    }
}
