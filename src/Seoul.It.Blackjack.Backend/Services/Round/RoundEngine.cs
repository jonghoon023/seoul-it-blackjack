using Seoul.It.Blackjack.Backend.Models;
using Seoul.It.Blackjack.Backend.Services.Commands;
using Seoul.It.Blackjack.Backend.Services.Exceptions;
using Seoul.It.Blackjack.Core.Contracts;
using Seoul.It.Blackjack.Core.Domain;
using System.Collections.Generic;
using System.Linq;

namespace Seoul.It.Blackjack.Backend.Services.Round;

/// <summary>
/// 라운드 규칙(카드 배분, 턴 처리, 딜러 자동 진행, 결과 계산)을 처리하는 엔진입니다.
/// </summary>
internal sealed class RoundEngine : IRoundEngine
{
    /// <summary>
    /// 새 라운드를 시작하고 모든 플레이어에게 초기 카드 2장을 배분합니다.
    /// </summary>
    /// <param name="players">플레이어 목록입니다.</param>
    /// <param name="deckCount">사용할 덱 개수입니다.</param>
    /// <param name="dealerStandScore">딜러 스탠드 기준 점수입니다.</param>
    /// <returns>라운드 시작 처리 결과입니다.</returns>
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

    /// <summary>
    /// 히트 요청을 처리해 카드 1장을 추가하고 턴/종료 상태를 계산합니다.
    /// </summary>
    /// <param name="players">플레이어 목록입니다.</param>
    /// <param name="shoe">카드 더미입니다.</param>
    /// <param name="currentTurnPlayerId">현재 턴 플레이어 ID입니다.</param>
    /// <param name="player">히트를 요청한 플레이어입니다.</param>
    /// <param name="dealerStandScore">딜러 스탠드 기준 점수입니다.</param>
    /// <returns>히트 처리 결과입니다.</returns>
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

    /// <summary>
    /// 스탠드 요청을 처리해 다음 턴 또는 정산 단계로 진행합니다.
    /// </summary>
    /// <param name="players">플레이어 목록입니다.</param>
    /// <param name="shoe">카드 더미입니다.</param>
    /// <param name="player">스탠드를 요청한 플레이어입니다.</param>
    /// <param name="dealerStandScore">딜러 스탠드 기준 점수입니다.</param>
    /// <returns>스탠드 처리 결과입니다.</returns>
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

    /// <summary>
    /// 일반 플레이어 턴이 모두 끝났을 때 딜러 자동 진행과 결과 계산을 수행합니다.
    /// </summary>
    /// <param name="players">플레이어 목록입니다.</param>
    /// <param name="shoe">카드 더미입니다.</param>
    /// <param name="dealerStandScore">딜러 스탠드 기준 점수입니다.</param>
    /// <returns>정산 처리 결과입니다.</returns>
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

    /// <summary>
    /// 다음으로 행동 가능한 일반 플레이어의 ID를 계산합니다.
    /// </summary>
    /// <param name="players">플레이어 목록입니다.</param>
    /// <returns>다음 플레이어 ID입니다. 없으면 빈 문자열입니다.</returns>
    public string ResolveNextTurnPlayerId(IReadOnlyCollection<PlayerState> players)
    {
        PlayerState? next = players.FirstOrDefault(player =>
            !player.IsDealer &&
            player.TurnState == PlayerTurnState.Playing);
        return next?.PlayerId ?? string.Empty;
    }

    /// <summary>
    /// 아직 행동 가능한 일반 플레이어가 남아 있는지 확인합니다.
    /// </summary>
    /// <param name="players">플레이어 목록입니다.</param>
    /// <returns>남아 있으면 true입니다.</returns>
    public bool HasPlayableNonDealer(IReadOnlyCollection<PlayerState> players)
    {
        return players.Any(player =>
            !player.IsDealer &&
            player.TurnState == PlayerTurnState.Playing);
    }

    /// <summary>
    /// 플레이어 목록에서 딜러를 찾아 반환합니다.
    /// </summary>
    /// <param name="players">플레이어 목록입니다.</param>
    /// <returns>딜러 플레이어입니다.</returns>
    private static PlayerState FindDealer(IReadOnlyCollection<PlayerState> players)
    {
        PlayerState? dealer = players.SingleOrDefault(player => player.IsDealer);
        if (dealer is null)
        {
            throw new GameAuthorizationException("NOT_DEALER", "딜러가 존재하지 않습니다.");
        }

        return dealer;
    }

    /// <summary>
    /// 지정한 플레이어에게 카드 한 장을 강제로 배분합니다.
    /// </summary>
    /// <param name="player">카드를 받을 플레이어입니다.</param>
    /// <param name="shoe">카드 더미입니다.</param>
    private static void DrawCardTo(PlayerState player, Shoe shoe)
    {
        if (!TryDrawCardTo(player, shoe))
        {
            throw new GameRuleException("SHOE_EMPTY", "카드가 부족해 라운드를 종료합니다.");
        }
    }

    /// <summary>
    /// 지정한 플레이어에게 카드 한 장 배분을 시도합니다.
    /// </summary>
    /// <param name="player">카드를 받을 플레이어입니다.</param>
    /// <param name="shoe">카드 더미입니다.</param>
    /// <returns>성공하면 true, 카드가 없으면 false입니다.</returns>
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

    /// <summary>
    /// 플레이어 카드 합계를 다시 계산하고 턴 상태를 업데이트합니다.
    /// </summary>
    /// <param name="player">다시 계산할 플레이어입니다.</param>
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

    /// <summary>
    /// 카드 부족으로 라운드를 종료해야 할 때 사용하는 결과를 만듭니다.
    /// </summary>
    /// <returns>SHOE_EMPTY 공지가 포함된 종료 결과입니다.</returns>
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
