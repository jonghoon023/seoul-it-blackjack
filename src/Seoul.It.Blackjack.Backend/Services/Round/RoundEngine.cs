using Seoul.It.Blackjack.Backend.Models;
using Seoul.It.Blackjack.Backend.Services.Commands;
using Seoul.It.Blackjack.Backend.Services.Exceptions;
using Seoul.It.Blackjack.Core.Contracts;
using Seoul.It.Blackjack.Core.Domain;
using System.Collections.Generic;
using System.Linq;

namespace Seoul.It.Blackjack.Backend.Services.Round;

internal sealed class RoundEngine : IRoundEngine
{
    public RoundResolution StartRound(List<PlayerState> players, int deckCount, int dealerStandScore)
    {
        Shoe shoe = new(deckCount);
        foreach (PlayerState player in players)
        {
            player.Cards.Clear();
            player.Score = 0;
            player.TurnState = PlayerTurnState.Playing;
            player.Outcome = RoundOutcome.None;
        }

        foreach (PlayerState player in players)
        {
            DrawCardTo(player, shoe);
            DrawCardTo(player, shoe);
        }

        string currentTurnPlayerId = ResolveNextTurnPlayerId(players);
        RoundResolution inRound = new()
        {
            Phase = GamePhase.InRound,
            CurrentTurnPlayerId = currentTurnPlayerId,
            StatusMessage = "라운드가 시작되었습니다.",
            Shoe = shoe,
        };

        if (HasPlayableNonDealer(players))
        {
            return inRound;
        }

        RoundResolution completed = CompleteRound(players, shoe, dealerStandScore);
        completed.Shoe = shoe;
        return completed;
    }

    public RoundResolution HandleHit(
        List<PlayerState> players,
        Shoe shoe,
        string currentTurnPlayerId,
        PlayerState player,
        int dealerStandScore)
    {
        if (!TryDrawCardTo(player, shoe))
        {
            return EndRoundByShoeEmpty();
        }

        string nextTurnPlayerId = player.TurnState == PlayerTurnState.Playing
            ? currentTurnPlayerId
            : ResolveNextTurnPlayerId(players);
        RoundResolution inRound = new()
        {
            Phase = GamePhase.InRound,
            CurrentTurnPlayerId = nextTurnPlayerId,
            StatusMessage = $"{player.Name} 님이 Hit 했습니다.",
        };

        if (HasPlayableNonDealer(players))
        {
            return inRound;
        }

        return CompleteRound(players, shoe, dealerStandScore);
    }

    public RoundResolution HandleStand(
        List<PlayerState> players,
        Shoe shoe,
        PlayerState player,
        int dealerStandScore)
    {
        player.TurnState = PlayerTurnState.Standing;
        string nextTurnPlayerId = ResolveNextTurnPlayerId(players);
        RoundResolution inRound = new()
        {
            Phase = GamePhase.InRound,
            CurrentTurnPlayerId = nextTurnPlayerId,
            StatusMessage = $"{player.Name} 님이 Stand 했습니다.",
        };

        if (HasPlayableNonDealer(players))
        {
            return inRound;
        }

        return CompleteRound(players, shoe, dealerStandScore);
    }

    public RoundResolution CompleteRound(List<PlayerState> players, Shoe shoe, int dealerStandScore)
    {
        PlayerState dealer = FindDealer(players);
        while (dealer.Score < dealerStandScore && dealer.TurnState == PlayerTurnState.Playing)
        {
            if (!TryDrawCardTo(dealer, shoe))
            {
                return EndRoundByShoeEmpty();
            }
        }

        if (dealer.TurnState == PlayerTurnState.Playing)
        {
            dealer.TurnState = PlayerTurnState.Standing;
        }

        foreach (PlayerState player in players.Where(player => !player.IsDealer))
        {
            if (player.TurnState == PlayerTurnState.Busted)
            {
                player.Outcome = RoundOutcome.Lose;
                continue;
            }

            if (dealer.TurnState == PlayerTurnState.Busted)
            {
                player.Outcome = RoundOutcome.Win;
                continue;
            }

            if (player.Score > dealer.Score)
            {
                player.Outcome = RoundOutcome.Win;
            }
            else if (player.Score < dealer.Score)
            {
                player.Outcome = RoundOutcome.Lose;
            }
            else
            {
                player.Outcome = RoundOutcome.Tie;
            }
        }

        return new RoundResolution
        {
            Phase = GamePhase.Idle,
            CurrentTurnPlayerId = string.Empty,
            StatusMessage = "라운드가 종료되었습니다.",
        };
    }

    public string ResolveNextTurnPlayerId(IReadOnlyCollection<PlayerState> players)
    {
        PlayerState? next = players.FirstOrDefault(player =>
            !player.IsDealer &&
            player.TurnState == PlayerTurnState.Playing);
        return next?.PlayerId ?? string.Empty;
    }

    public bool HasPlayableNonDealer(IReadOnlyCollection<PlayerState> players)
    {
        return players.Any(player =>
            !player.IsDealer &&
            player.TurnState == PlayerTurnState.Playing);
    }

    private static PlayerState FindDealer(IReadOnlyCollection<PlayerState> players)
    {
        PlayerState? dealer = players.SingleOrDefault(player => player.IsDealer);
        if (dealer is null)
        {
            throw new GameAuthorizationException("NOT_DEALER", "딜러가 존재하지 않습니다.");
        }

        return dealer;
    }

    private static void DrawCardTo(PlayerState player, Shoe shoe)
    {
        if (!TryDrawCardTo(player, shoe))
        {
            throw new GameRuleException("SHOE_EMPTY", "카드가 부족해 라운드를 종료합니다.");
        }
    }

    private static bool TryDrawCardTo(PlayerState player, Shoe shoe)
    {
        if (!shoe.TryDraw(out Card card))
        {
            return false;
        }

        player.Cards.Add(card);
        RecalculatePlayerState(player);
        return true;
    }

    private static void RecalculatePlayerState(PlayerState player)
    {
        player.Score = player.Cards.Sum(card => card.Rank.ToValue());
        if (player.Score > 21)
        {
            player.TurnState = PlayerTurnState.Busted;
        }
        else if (player.Score == 21)
        {
            player.TurnState = PlayerTurnState.Standing;
        }
        else if (player.TurnState == PlayerTurnState.Playing)
        {
            player.TurnState = PlayerTurnState.Playing;
        }
    }

    private static RoundResolution EndRoundByShoeEmpty()
    {
        return new RoundResolution
        {
            Phase = GamePhase.Idle,
            CurrentTurnPlayerId = string.Empty,
            StatusMessage = "카드가 부족해 라운드를 종료했습니다.",
            Notice = new GameNotice("SHOE_EMPTY", "카드가 부족해 라운드를 종료했습니다."),
        };
    }
}
