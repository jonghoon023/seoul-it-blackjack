using Microsoft.Extensions.Options;
using Seoul.It.Blackjack.Backend.Models;
using Seoul.It.Blackjack.Backend.Options;
using Seoul.It.Blackjack.Backend.Services.Commands;
using Seoul.It.Blackjack.Backend.Services.Exceptions;
using Seoul.It.Blackjack.Backend.Services.Infrastructure;
using Seoul.It.Blackjack.Backend.Services.Round;
using Seoul.It.Blackjack.Backend.Services.Rules;
using Seoul.It.Blackjack.Backend.Services.State;
using Seoul.It.Blackjack.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Seoul.It.Blackjack.Backend.Services;

internal sealed class GameRoomService : IGameRoomService
{
    private readonly DealerOptions _dealerOptions;
    private readonly GameRuleOptions _gameRuleOptions;
    private readonly ConnectionRegistry _connections;
    private readonly IGameRuleValidator _validator;
    private readonly IRoundEngine _roundEngine;
    private readonly IGameStateSnapshotFactory _snapshotFactory;
    private readonly IGameCommandProcessor _commandProcessor;
    private readonly List<PlayerState> _players = new();
    private Shoe? _shoe;
    private GamePhase _phase = GamePhase.Idle;
    private string _dealerPlayerId = string.Empty;
    private string _currentTurnPlayerId = string.Empty;
    private string _statusMessage = string.Empty;

    public GameRoomService(
        IOptions<DealerOptions> dealerOptions,
        IOptions<GameRuleOptions> gameRuleOptions,
        ConnectionRegistry connections,
        IGameRuleValidator validator,
        IRoundEngine roundEngine,
        IGameStateSnapshotFactory snapshotFactory,
        IGameCommandProcessor commandProcessor)
    {
        _dealerOptions = dealerOptions.Value;
        _gameRuleOptions = gameRuleOptions.Value;
        _connections = connections;
        _validator = validator;
        _roundEngine = roundEngine;
        _snapshotFactory = snapshotFactory;
        _commandProcessor = commandProcessor;
    }

    public Task<GameOperationResult> JoinAsync(string connectionId, string name, string? dealerKey)
    {
        GameCommand command = new(GameCommandType.Join, connectionId, name, dealerKey);
        return _commandProcessor.EnqueueAsync(command, () => HandleJoin(command));
    }

    public Task<GameOperationResult> LeaveAsync(string connectionId)
    {
        GameCommand command = new(GameCommandType.Leave, connectionId);
        return _commandProcessor.EnqueueAsync(command, () => HandleLeave(command, false));
    }

    public Task<GameOperationResult> DisconnectAsync(string connectionId)
    {
        GameCommand command = new(GameCommandType.Disconnect, connectionId);
        return _commandProcessor.EnqueueAsync(command, () => HandleLeave(command, true));
    }

    public Task<GameOperationResult> StartRoundAsync(string connectionId)
    {
        GameCommand command = new(GameCommandType.StartRound, connectionId);
        return _commandProcessor.EnqueueAsync(command, () => HandleStartRound(command));
    }

    public Task<GameOperationResult> HitAsync(string connectionId)
    {
        GameCommand command = new(GameCommandType.Hit, connectionId);
        return _commandProcessor.EnqueueAsync(command, () => HandleHit(command));
    }

    public Task<GameOperationResult> StandAsync(string connectionId)
    {
        GameCommand command = new(GameCommandType.Stand, connectionId);
        return _commandProcessor.EnqueueAsync(command, () => HandleStand(command));
    }

    private GameOperationResult HandleJoin(GameCommand command)
    {
        if (_phase != GamePhase.Idle)
        {
            throw new GameRuleException("GAME_IN_PROGRESS", "게임이 진행 중이라 참가할 수 없습니다.");
        }

        if (_connections.ContainsConnection(command.ConnectionId))
        {
            throw new GameValidationException("ALREADY_JOINED", "이미 참가한 연결입니다.");
        }

        string normalizedName = _validator.NormalizeName(
            command.Name,
            _gameRuleOptions.MinNameLength,
            _gameRuleOptions.MaxNameLength);
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

    private GameOperationResult HandleLeave(GameCommand command, bool isDisconnect)
    {
        if (!_connections.TryRemove(command.ConnectionId, out string _))
        {
            return CreateSilentResult();
        }

        PlayerState? leavingPlayer = _players.SingleOrDefault(player => player.PlayerId == command.ConnectionId);
        if (leavingPlayer is null)
        {
            return CreateSilentResult();
        }

        _players.Remove(leavingPlayer);
        if (leavingPlayer.IsDealer)
        {
            _players.Clear();
            _connections.Clear();
            ResetToIdle(clearDealer: true, clearCurrentTurn: true);
            _statusMessage = "딜러 퇴장으로 게임이 종료되었습니다.";
            return CreateResult(new GameNotice("GAME_TERMINATED", "딜러가 퇴장하여 게임이 종료되었습니다."));
        }

        if (_phase == GamePhase.InRound && _currentTurnPlayerId == leavingPlayer.PlayerId)
        {
            _currentTurnPlayerId = _roundEngine.ResolveNextTurnPlayerId(_players);
        }

        if (_phase == GamePhase.InRound && CountNonDealerPlayers() == 0)
        {
            ResetToIdle(clearDealer: false, clearCurrentTurn: true);
            _statusMessage = "플레이어가 없어 라운드를 종료했습니다.";
            return CreateResult();
        }

        if (_phase == GamePhase.InRound && !_roundEngine.HasPlayableNonDealer(_players))
        {
            if (_shoe is null)
            {
                throw new InvalidOperationException("Shoe is not initialized.");
            }

            RoundResolution resolution = _roundEngine.CompleteRound(
                _players,
                _shoe,
                _gameRuleOptions.DealerStandScore);
            ApplyRoundResolution(resolution);
            return CreateResult(resolution.Notice);
        }

        _statusMessage = isDisconnect ? "플레이어 연결이 끊어졌습니다." : "플레이어가 퇴장했습니다.";

        return CreateResult();
    }

    private GameOperationResult HandleStartRound(GameCommand command)
    {
        _validator.EnsureCanStartRound(
            _connections,
            _phase,
            _dealerPlayerId,
            command.ConnectionId,
            _players.Count,
            _gameRuleOptions.MinPlayersToStart);

        RoundResolution startResolution = _roundEngine.StartRound(
            _players,
            _gameRuleOptions.DeckCount,
            _gameRuleOptions.DealerStandScore);
        ApplyRoundResolution(startResolution);
        return CreateResult(startResolution.Notice);
    }

    private GameOperationResult HandleHit(GameCommand command)
    {
        PlayerState player = ValidatePlayerAction(command);
        if (_shoe is null)
        {
            throw new InvalidOperationException("Shoe is not initialized.");
        }

        RoundResolution hitResolution = _roundEngine.HandleHit(
            _players,
            _shoe,
            _currentTurnPlayerId,
            player,
            _gameRuleOptions.DealerStandScore);
        ApplyRoundResolution(hitResolution);
        return CreateResult(hitResolution.Notice);
    }

    private GameOperationResult HandleStand(GameCommand command)
    {
        PlayerState player = ValidatePlayerAction(command);
        if (_shoe is null)
        {
            throw new InvalidOperationException("Shoe is not initialized.");
        }

        RoundResolution standResolution = _roundEngine.HandleStand(
            _players,
            _shoe,
            player,
            _gameRuleOptions.DealerStandScore);
        ApplyRoundResolution(standResolution);
        return CreateResult(standResolution.Notice);
    }

    private int CountNonDealerPlayers() => _players.Count(player => !player.IsDealer);

    private bool IsDealerKeyMatched(string? dealerKey) => !string.IsNullOrEmpty(_dealerOptions.Key) && dealerKey == _dealerOptions.Key;

    private PlayerState ValidatePlayerAction(GameCommand command)
    {
        return _validator.ValidatePlayerAction(
            _connections,
            _phase,
            _players,
            command.ConnectionId,
            _currentTurnPlayerId);
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

    private void ApplyRoundResolution(RoundResolution resolution)
    {
        _phase = resolution.Phase;
        _currentTurnPlayerId = resolution.CurrentTurnPlayerId;
        _statusMessage = resolution.StatusMessage;
        if (resolution.Shoe is not null)
        {
            _shoe = resolution.Shoe;
        }
    }

    private GameOperationResult CreateResult(GameNotice? notice = null) => new()
    {
        State = _snapshotFactory.Create(
            _phase,
            _dealerPlayerId,
            _currentTurnPlayerId,
            _statusMessage,
            _players),
        Notice = notice,
    };

    private GameOperationResult CreateSilentResult() => new()
    {
        State = _snapshotFactory.Create(
            _phase,
            _dealerPlayerId,
            _currentTurnPlayerId,
            _statusMessage,
            _players),
        ShouldPublishState = false,
    };
}
