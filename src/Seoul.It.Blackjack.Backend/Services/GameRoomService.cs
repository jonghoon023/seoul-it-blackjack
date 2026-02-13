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

/// <summary>
/// 게임 방의 실제 상태를 보관하고 모든 명령을 순서대로 처리하는 핵심 서비스입니다.
/// </summary>
internal sealed class GameRoomService : IGameRoomService
{
    /// <summary>
    /// 딜러 관련 설정 값입니다.
    /// </summary>
    private readonly DealerOptions _dealerOptions;

    /// <summary>
    /// 게임 규칙 관련 설정 값입니다.
    /// </summary>
    private readonly GameRuleOptions _gameRuleOptions;

    /// <summary>
    /// 연결 ID와 플레이어 ID 매핑 저장소입니다.
    /// </summary>
    private readonly ConnectionRegistry _connections;

    /// <summary>
    /// 규칙 검사기입니다.
    /// </summary>
    private readonly IGameRuleValidator _validator;

    /// <summary>
    /// 라운드 엔진입니다.
    /// </summary>
    private readonly IRoundEngine _roundEngine;

    /// <summary>
    /// 상태 스냅샷 생성기입니다.
    /// </summary>
    private readonly IGameStateSnapshotFactory _snapshotFactory;

    /// <summary>
    /// 명령을 순서대로 처리하기 위한 커맨드 프로세서입니다.
    /// </summary>
    private readonly IGameCommandProcessor _commandProcessor;

    /// <summary>
    /// 현재 방의 플레이어 목록입니다.
    /// </summary>
    private readonly List<PlayerState> _players = new();

    /// <summary>
    /// 현재 라운드에서 사용하는 카드 더미입니다.
    /// </summary>
    private Shoe? _shoe;

    /// <summary>
    /// 현재 게임 단계입니다.
    /// </summary>
    private GamePhase _phase = GamePhase.Idle;

    /// <summary>
    /// 현재 딜러 플레이어 ID입니다.
    /// </summary>
    private string _dealerPlayerId = string.Empty;

    /// <summary>
    /// 현재 턴 플레이어 ID입니다.
    /// </summary>
    private string _currentTurnPlayerId = string.Empty;

    /// <summary>
    /// 현재 상태 메시지입니다.
    /// </summary>
    private string _statusMessage = string.Empty;

    /// <summary>
    /// 게임 방 서비스를 만듭니다.
    /// </summary>
    /// <param name="dealerOptions">딜러 옵션입니다.</param>
    /// <param name="gameRuleOptions">게임 규칙 옵션입니다.</param>
    /// <param name="connections">연결 매핑 저장소입니다.</param>
    /// <param name="validator">규칙 검사기입니다.</param>
    /// <param name="roundEngine">라운드 엔진입니다.</param>
    /// <param name="snapshotFactory">스냅샷 생성기입니다.</param>
    /// <param name="commandProcessor">명령 프로세서입니다.</param>
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

    /// <summary>
    /// 입장 명령을 큐에 넣어 처리합니다.
    /// </summary>
    /// <param name="connectionId">요청 연결 ID입니다.</param>
    /// <param name="name">입장 이름입니다.</param>
    /// <param name="dealerKey">딜러 키입니다.</param>
    /// <returns>처리 결과입니다.</returns>
    public Task<GameOperationResult> JoinAsync(string connectionId, string name, string? dealerKey)
    {
        GameCommand command = new(GameCommandType.Join, connectionId, name, dealerKey);
        return _commandProcessor.EnqueueAsync(command, () => HandleJoin(command));
    }

    /// <summary>
    /// 퇴장 명령을 큐에 넣어 처리합니다.
    /// </summary>
    /// <param name="connectionId">요청 연결 ID입니다.</param>
    /// <returns>처리 결과입니다.</returns>
    public Task<GameOperationResult> LeaveAsync(string connectionId)
    {
        GameCommand command = new(GameCommandType.Leave, connectionId);
        return _commandProcessor.EnqueueAsync(command, () => HandleLeave(command, false));
    }

    /// <summary>
    /// 연결 종료를 큐에 넣어 퇴장 규칙으로 처리합니다.
    /// </summary>
    /// <param name="connectionId">끊어진 연결 ID입니다.</param>
    /// <returns>처리 결과입니다.</returns>
    public Task<GameOperationResult> DisconnectAsync(string connectionId)
    {
        GameCommand command = new(GameCommandType.Disconnect, connectionId);
        return _commandProcessor.EnqueueAsync(command, () => HandleLeave(command, true));
    }

    /// <summary>
    /// 라운드 시작 명령을 큐에 넣어 처리합니다.
    /// </summary>
    /// <param name="connectionId">요청 연결 ID입니다.</param>
    /// <returns>처리 결과입니다.</returns>
    public Task<GameOperationResult> StartRoundAsync(string connectionId)
    {
        GameCommand command = new(GameCommandType.StartRound, connectionId);
        return _commandProcessor.EnqueueAsync(command, () => HandleStartRound(command));
    }

    /// <summary>
    /// 히트 명령을 큐에 넣어 처리합니다.
    /// </summary>
    /// <param name="connectionId">요청 연결 ID입니다.</param>
    /// <returns>처리 결과입니다.</returns>
    public Task<GameOperationResult> HitAsync(string connectionId)
    {
        GameCommand command = new(GameCommandType.Hit, connectionId);
        return _commandProcessor.EnqueueAsync(command, () => HandleHit(command));
    }

    /// <summary>
    /// 스탠드 명령을 큐에 넣어 처리합니다.
    /// </summary>
    /// <param name="connectionId">요청 연결 ID입니다.</param>
    /// <returns>처리 결과입니다.</returns>
    public Task<GameOperationResult> StandAsync(string connectionId)
    {
        GameCommand command = new(GameCommandType.Stand, connectionId);
        return _commandProcessor.EnqueueAsync(command, () => HandleStand(command));
    }

    /// <summary>
    /// 입장 명령의 실제 로직을 처리합니다.
    /// </summary>
    /// <param name="command">처리할 명령입니다.</param>
    /// <returns>처리 결과입니다.</returns>
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

    /// <summary>
    /// 퇴장/연결 종료 명령의 실제 로직을 처리합니다.
    /// </summary>
    /// <param name="command">처리할 명령입니다.</param>
    /// <param name="isDisconnect">연결 종료 여부입니다.</param>
    /// <returns>처리 결과입니다.</returns>
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

    /// <summary>
    /// 라운드 시작 명령의 실제 로직을 처리합니다.
    /// </summary>
    /// <param name="command">처리할 명령입니다.</param>
    /// <returns>처리 결과입니다.</returns>
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

    /// <summary>
    /// 히트 명령의 실제 로직을 처리합니다.
    /// </summary>
    /// <param name="command">처리할 명령입니다.</param>
    /// <returns>처리 결과입니다.</returns>
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

    /// <summary>
    /// 스탠드 명령의 실제 로직을 처리합니다.
    /// </summary>
    /// <param name="command">처리할 명령입니다.</param>
    /// <returns>처리 결과입니다.</returns>
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

    /// <summary>
    /// 현재 일반 플레이어 수를 계산합니다.
    /// </summary>
    /// <returns>일반 플레이어 수입니다.</returns>
    private int CountNonDealerPlayers() => _players.Count(player => !player.IsDealer);

    /// <summary>
    /// 전달된 딜러 키가 설정 키와 일치하는지 확인합니다.
    /// </summary>
    /// <param name="dealerKey">확인할 딜러 키입니다.</param>
    /// <returns>일치하면 true입니다.</returns>
    private bool IsDealerKeyMatched(string? dealerKey) => !string.IsNullOrEmpty(_dealerOptions.Key) && dealerKey == _dealerOptions.Key;

    /// <summary>
    /// 히트/스탠드 요청 전 공통 검증을 수행합니다.
    /// </summary>
    /// <param name="command">검증할 명령입니다.</param>
    /// <returns>검증을 통과한 요청 플레이어입니다.</returns>
    private PlayerState ValidatePlayerAction(GameCommand command)
    {
        return _validator.ValidatePlayerAction(
            _connections,
            _phase,
            _players,
            command.ConnectionId,
            _currentTurnPlayerId);
    }

    /// <summary>
    /// 상태를 Idle로 되돌리고 필요하면 딜러/턴 ID를 비웁니다.
    /// </summary>
    /// <param name="clearDealer">딜러 ID 초기화 여부입니다.</param>
    /// <param name="clearCurrentTurn">현재 턴 ID 초기화 여부입니다.</param>
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

    /// <summary>
    /// 라운드 엔진 결과를 현재 서비스 상태에 반영합니다.
    /// </summary>
    /// <param name="resolution">반영할 라운드 처리 결과입니다.</param>
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

    /// <summary>
    /// 현재 상태로 일반 결과 객체를 만듭니다.
    /// </summary>
    /// <param name="notice">필요할 때 포함할 전체 공지입니다.</param>
    /// <returns>브로드캐스트 가능한 결과 객체입니다.</returns>
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

    /// <summary>
    /// 상태 브로드캐스트 없이 끝낼 때 쓰는 결과 객체를 만듭니다.
    /// </summary>
    /// <returns>조용히 처리할 결과 객체입니다.</returns>
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
