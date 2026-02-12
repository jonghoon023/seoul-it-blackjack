namespace Seoul.It.Blackjack.Core.Contracts;

public record GameState(string CurrentTurnPlayerId, IEnumerable<Player> Players);
