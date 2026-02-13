using Microsoft.Extensions.Options;
using Seoul.It.Blackjack.Backend.Models;
using Seoul.It.Blackjack.Backend.Options;
using Seoul.It.Blackjack.Backend.Services.Commands;
using Seoul.It.Blackjack.Backend.Services.Exceptions;
using Seoul.It.Blackjack.Core.Contracts;
using Seoul.It.Blackjack.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Seoul.It.Blackjack.Backend.Services;

internal sealed class GameRoomService
{
    private readonly DealerOptions _dealerOptions;
    private readonly ConnectionRegistry _connections;
    private readonly List<PlayerState> _players = new();
    private readonly Channel<QueueItem> _queue = Channel.CreateUnbounded<QueueItem>(new UnboundedChannelOptions
    {
        SingleReader = true,
        SingleWriter = false,
    });
    private Shoe? _shoe;
    private GamePhase _phase = GamePhase.Idle;
    private string _dealerPlayerId = string.Empty;
    private string _currentTurnPlayerId = string.Empty;
    private string _statusMessage = string.Empty;

    public GameRoomService(IOptions<DealerOptions> dealerOptions, ConnectionRegistry connections)
    {
        _dealerOptions = dealerOptions.Value;
        _connections = connections;
        _ = Task.Run(ProcessLoopAsync);
    }

    public Task<GameCommandResult> JoinAsync(string connectionId, string name, string? dealerKey)
    {
        GameCommand command = new(GameCommandType.Join, connectionId, name, dealerKey);
        return Enqueue(command, () => HandleJoin(command));
    }

    public Task<GameCommandResult> LeaveAsync(string connectionId)
    {
        GameCommand command = new(GameCommandType.Leave, connectionId);
        return Enqueue(command, () => HandleLeave(command, false));
    }

    public Task<GameCommandResult> DisconnectAsync(string connectionId)
    {
        GameCommand command = new(GameCommandType.Disconnect, connectionId);
        return Enqueue(command, () => HandleLeave(command, true));
    }

    public Task<GameCommandResult> StartRoundAsync(string connectionId)
    {
        GameCommand command = new(GameCommandType.StartRound, connectionId);
        return Enqueue(command, () => HandleStartRound(command));
    }

    public Task<GameCommandResult> HitAsync(string connectionId)
    {
        GameCommand command = new(GameCommandType.Hit, connectionId);
        return Enqueue(command, () => HandleHit(command));
    }

    public Task<GameCommandResult> StandAsync(string connectionId)
    {
        GameCommand command = new(GameCommandType.Stand, connectionId);
        return Enqueue(command, () => HandleStand(command));
    }

    private GameCommandResult HandleJoin(GameCommand command)
    {
        if (_phase != GamePhase.Idle)
        {
            throw new GameRuleException("GAME_IN_PROGRESS", "게임이 진행 중이라 참가할 수 없습니다.");
        }

        if (_connections.ContainsConnection(command.ConnectionId))
        {
            throw new GameValidationException("ALREADY_JOINED", "이미 참가한 연결입니다.");
        }

        string normalizedName = NormalizeName(command.Name);
        bool requestedDealer = IsDealerKeyMatched(command.DealerKey);
        bool hasDealer = _players.Any(player => player.IsDealer);
        if (requestedDealer && hasDealer)
        {
            throw new GameRuleException("DEALER_ALREADY_EXISTS", "이미 딜러가 존재합니다.");
        }

        PlayerState player = new()
        {
            PlayerId = command.ConnectionId,
            Name = normalizedName,
            IsDealer = requestedDealer,
            Score = 0,
            TurnState = PlayerTurnState.Playing,
            Outcome = RoundOutcome.None,
        };
        _players.Add(player);
        _connections.Add(command.ConnectionId, player.PlayerId);
        if (player.IsDealer)
        {
            _dealerPlayerId = player.PlayerId;
        }

        _statusMessage = player.IsDealer ? "딜러가 참가했습니다." : "플레이어가 참가했습니다.";
        return CreateResult();
    }

    private GameCommandResult HandleLeave(GameCommand command, bool isDisconnect)
    {
        if (!_connections.TryRemove(command.ConnectionId, out string _))
        {
            return new GameCommandResult
            {
                State = CreateStateSnapshot(),
                ShouldBroadcastState = false,
            };
        }

        PlayerState? leavingPlayer = _players.SingleOrDefault(player => player.PlayerId == command.ConnectionId);
        if (leavingPlayer is null)
        {
            return new GameCommandResult
            {
                State = CreateStateSnapshot(),
                ShouldBroadcastState = false,
            };
        }

        _players.Remove(leavingPlayer);
        if (leavingPlayer.IsDealer)
        {
            _players.Clear();
            _connections.Clear();
            ResetToIdle(clearDealer: true, clearCurrentTurn: true);
            _statusMessage = "딜러 퇴장으로 게임이 종료되었습니다.";
            return CreateResult("GAME_TERMINATED", "딜러가 퇴장하여 게임이 종료되었습니다.");
        }

        if (_phase == GamePhase.InRound && _currentTurnPlayerId == leavingPlayer.PlayerId)
        {
            _currentTurnPlayerId = ResolveNextTurnPlayerId();
        }

        if (_phase == GamePhase.InRound && CountNonDealerPlayers() == 0)
        {
            ResetToIdle(clearDealer: false, clearCurrentTurn: true);
            _statusMessage = "플레이어가 없어 라운드를 종료했습니다.";
            return CreateResult();
        }

        if (_phase == GamePhase.InRound && !HasPlayableNonDealer())
        {
            _statusMessage = "플레이어 행동이 모두 종료되어 딜러 진행을 시작합니다.";
            return RunDealerAndCompleteRound();
        }

        _statusMessage = isDisconnect ? "플레이어 연결이 끊어졌습니다." : "플레이어가 퇴장했습니다.";

        return CreateResult();
    }

    private GameCommandResult HandleStartRound(GameCommand command)
    {
        EnsureJoined(command.ConnectionId);
        if (_phase != GamePhase.Idle)
        {
            throw new GameRuleException("GAME_IN_PROGRESS", "이미 게임이 진행 중입니다.");
        }

        if (string.IsNullOrEmpty(_dealerPlayerId) || _dealerPlayerId != command.ConnectionId)
        {
            throw new GameAuthorizationException("NOT_DEALER", "딜러만 라운드를 시작할 수 있습니다.");
        }

        if (_players.Count < 2)
        {
            throw new GameRuleException("INSUFFICIENT_PLAYERS", "라운드를 시작하려면 최소 2명이 필요합니다.");
        }

        _phase = GamePhase.InRound;
        _shoe = new Shoe(4);
        foreach (PlayerState player in _players)
        {
            player.Cards.Clear();
            player.Score = 0;
            player.TurnState = PlayerTurnState.Playing;
            player.Outcome = RoundOutcome.None;
        }

        foreach (PlayerState player in _players)
        {
            DrawCardTo(player);
            DrawCardTo(player);
        }

        _currentTurnPlayerId = ResolveNextTurnPlayerId();
        _statusMessage = "라운드가 시작되었습니다.";
        return CompleteRoundIfNeeded();
    }

    private GameCommandResult HandleHit(GameCommand command)
    {
        PlayerState player = ValidatePlayerAction(command);
        if (!TryDrawCardTo(player))
        {
            return EndRoundByShoeEmpty();
        }

        _statusMessage = $"{player.Name} 님이 Hit 했습니다.";
        if (player.TurnState != PlayerTurnState.Playing)
        {
            _currentTurnPlayerId = ResolveNextTurnPlayerId();
        }

        return CompleteRoundIfNeeded();
    }

    private GameCommandResult HandleStand(GameCommand command)
    {
        PlayerState player = ValidatePlayerAction(command);
        player.TurnState = PlayerTurnState.Standing;
        _currentTurnPlayerId = ResolveNextTurnPlayerId();
        _statusMessage = $"{player.Name} 님이 Stand 했습니다.";
        return CompleteRoundIfNeeded();
    }

    private void DrawCardTo(PlayerState player)
    {
        if (_shoe is null)
        {
            throw new InvalidOperationException("Shoe is not initialized.");
        }

        if (!TryDrawCardTo(player))
        {
            throw new GameRuleException("SHOE_EMPTY", "카드가 부족해 라운드를 종료합니다.");
        }
    }

    private string ResolveNextTurnPlayerId()
    {
        PlayerState? next = _players.FirstOrDefault(player => !player.IsDealer && player.TurnState == PlayerTurnState.Playing);
        return next?.PlayerId ?? string.Empty;
    }

    private int CountNonDealerPlayers() => _players.Count(player => !player.IsDealer);

    private bool HasPlayableNonDealer() => _players.Any(player => !player.IsDealer && player.TurnState == PlayerTurnState.Playing);

    private bool IsDealerKeyMatched(string? dealerKey) => !string.IsNullOrEmpty(_dealerOptions.Key) && dealerKey == _dealerOptions.Key;

    private string NormalizeName(string? name)
    {
        string normalized = (name ?? string.Empty).Trim();
        if (normalized.Length < 1 || normalized.Length > 20)
        {
            throw new GameValidationException("INVALID_NAME", "이름은 1~20자여야 합니다.");
        }

        return normalized;
    }

    private void EnsureJoined(string connectionId)
    {
        if (!_connections.ContainsConnection(connectionId))
        {
            throw new GameValidationException("NOT_JOINED", "먼저 참가해야 합니다.");
        }
    }

    private void EnsureInRound()
    {
        if (_phase != GamePhase.InRound)
        {
            throw new GameRuleException("GAME_NOT_INROUND", "게임이 진행 중이 아닙니다.");
        }
    }

    private PlayerState ValidatePlayerAction(GameCommand command)
    {
        EnsureJoined(command.ConnectionId);
        EnsureInRound();
        PlayerState player = FindPlayer(command.ConnectionId);
        if (player.IsDealer)
        {
            throw new GameRuleException("DEALER_IS_AUTO", "딜러는 자동으로 진행됩니다.");
        }

        if (_currentTurnPlayerId != player.PlayerId)
        {
            throw new GameRuleException("NOT_YOUR_TURN", "현재 턴의 플레이어가 아닙니다.");
        }

        if (player.TurnState != PlayerTurnState.Playing)
        {
            throw new GameRuleException("ALREADY_DONE", "이미 행동이 끝난 플레이어입니다.");
        }

        return player;
    }

    private PlayerState FindPlayer(string playerId)
    {
        PlayerState? player = _players.SingleOrDefault(value => value.PlayerId == playerId);
        if (player is null)
        {
            throw new GameValidationException("NOT_JOINED", "먼저 참가해야 합니다.");
        }

        return player;
    }

    private PlayerState FindDealer()
    {
        PlayerState? dealer = _players.SingleOrDefault(player => player.IsDealer);
        if (dealer is null)
        {
            throw new GameAuthorizationException("NOT_DEALER", "딜러가 존재하지 않습니다.");
        }

        return dealer;
    }

    private GameCommandResult CompleteRoundIfNeeded()
    {
        if (_phase != GamePhase.InRound)
        {
            return CreateResult();
        }

        if (HasPlayableNonDealer())
        {
            return CreateResult();
        }

        return RunDealerAndCompleteRound();
    }

    private GameCommandResult RunDealerAndCompleteRound()
    {
        PlayerState dealer = FindDealer();
        while (dealer.Score < 17 && dealer.TurnState == PlayerTurnState.Playing)
        {
            if (!TryDrawCardTo(dealer))
            {
                return EndRoundByShoeEmpty();
            }
        }

        if (dealer.TurnState == PlayerTurnState.Playing)
        {
            dealer.TurnState = PlayerTurnState.Standing;
        }

        foreach (PlayerState player in _players.Where(player => !player.IsDealer))
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

        _phase = GamePhase.Idle;
        _currentTurnPlayerId = string.Empty;
        _statusMessage = "라운드가 종료되었습니다.";
        return CreateResult();
    }

    private bool TryDrawCardTo(PlayerState player)
    {
        if (_shoe is null)
        {
            throw new InvalidOperationException("Shoe is not initialized.");
        }

        if (!_shoe.TryDraw(out Card? card))
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

    private GameCommandResult EndRoundByShoeEmpty()
    {
        _phase = GamePhase.Idle;
        _currentTurnPlayerId = string.Empty;
        _statusMessage = "카드가 부족해 라운드를 종료했습니다.";
        return CreateResult("SHOE_EMPTY", "카드가 부족해 라운드를 종료했습니다.");
    }

    private void ResetToIdle(bool clearDealer, bool clearCurrentTurn)
    {
        _phase = GamePhase.Idle;
        if (clearDealer)
        {
            _dealerPlayerId = string.Empty;
        }

        if (clearCurrentTurn)
        {
            _currentTurnPlayerId = string.Empty;
        }
    }

    private GameCommandResult CreateResult(string? broadcastErrorCode = null, string? broadcastErrorMessage = null) => new()
    {
        State = CreateStateSnapshot(),
        BroadcastErrorCode = broadcastErrorCode,
        BroadcastErrorMessage = broadcastErrorMessage,
    };

    private GameState CreateStateSnapshot() => new()
    {
        Phase = _phase,
        DealerPlayerId = _dealerPlayerId,
        CurrentTurnPlayerId = _currentTurnPlayerId,
        StatusMessage = _statusMessage,
        Players = _players.Select(ClonePlayer).ToList(),
    };

    private static PlayerState ClonePlayer(PlayerState source) => new()
    {
        PlayerId = source.PlayerId,
        Name = source.Name,
        IsDealer = source.IsDealer,
        Cards = source.Cards.Select(card => new Card(card.Suit, card.Rank)).ToList(),
        Score = source.Score,
        TurnState = source.TurnState,
        Outcome = source.Outcome,
    };

    private async Task ProcessLoopAsync()
    {
        await foreach (QueueItem item in _queue.Reader.ReadAllAsync())
        {
            try
            {
                GameCommandResult result = item.Handler();
                item.Completion.SetResult(result);
            }
            catch (Exception ex)
            {
                item.Completion.SetException(ex);
            }
        }
    }

    private Task<GameCommandResult> Enqueue(GameCommand command, Func<GameCommandResult> handler)
    {
        QueueItem item = new(command, handler);
        _queue.Writer.TryWrite(item);
        return item.Completion.Task;
    }

    private sealed class QueueItem
    {
        public QueueItem(GameCommand command, Func<GameCommandResult> handler)
        {
            Command = command;
            Handler = handler;
        }

        public GameCommand Command { get; }

        public Func<GameCommandResult> Handler { get; }

        public TaskCompletionSource<GameCommandResult> Completion { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
    }
}
