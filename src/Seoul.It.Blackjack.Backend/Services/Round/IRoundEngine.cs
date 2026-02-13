using Seoul.It.Blackjack.Backend.Models;
using Seoul.It.Blackjack.Core.Contracts;
using System.Collections.Generic;

namespace Seoul.It.Blackjack.Backend.Services.Round;

internal interface IRoundEngine
{
    RoundResolution StartRound(List<PlayerState> players, int deckCount, int dealerStandScore);

    RoundResolution HandleHit(
        List<PlayerState> players,
        Shoe shoe,
        string currentTurnPlayerId,
        PlayerState player,
        int dealerStandScore);

    RoundResolution HandleStand(
        List<PlayerState> players,
        Shoe shoe,
        PlayerState player,
        int dealerStandScore);

    RoundResolution CompleteRound(List<PlayerState> players, Shoe shoe, int dealerStandScore);

    string ResolveNextTurnPlayerId(IReadOnlyCollection<PlayerState> players);

    bool HasPlayableNonDealer(IReadOnlyCollection<PlayerState> players);
}
