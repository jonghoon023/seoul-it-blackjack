using Microsoft.Extensions.Options;
using Seoul.It.Blackjack.Backend.Options;
using Seoul.It.Blackjack.Core.Contracts;
using System.Threading.Channels;

namespace Seoul.It.Blackjack.Backend.Models;

internal sealed class GameRoom
{
    private readonly IOptions<DealerOptions> _options;
    private readonly List<IPlayer> _players = [];
    private readonly Shoe _shoe = new(4);

    private readonly Channel<Func<Task>> _queue =
        Channel.CreateUnbounded<Func<Task>>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });

    public GameRoom(IOptions<DealerOptions> options)
    {
        _options = options;
        _ = Task.Run(ProcessLoop);
    }

    public GamePhase Phase { get; private set; } = GamePhase.Lobby;

    public Task<GameState> JoinAsync(string id, string name, string? dealerKey)
    {
        return Enqueue(() =>
        {
            if (!_players.Any(player => player.Id == id))
            {
                IPlayer player = dealerKey == _options.Value.Key ?
                    new Dealer(id) : new Player(id, name);
                _players.Add(player);
            }

            return new GameState(string.Empty, [.. _players]);
        });
    }

    public Task<GameState> LeaveAsync(string id)
    {
        return Enqueue(() =>
        {
            if (_players.SingleOrDefault(player => player.Id == id) is IPlayer player)
            {
                _players.Remove(player);
            }

            return new GameState(string.Empty, [.. _players]);
        });
    }

    public Task<GameState> StartAsync(string callerId)
    {
        return Enqueue(() =>
        {
            if (!IsDealer(callerId))
            {
                throw new InvalidOperationException("게임의 시작은 딜러만 할 수 있습니다.");
            }

            Phase = GamePhase.InGame;
            return new GameState(string.Empty, [.. _players]);
        });
    }

    public Task<GameState> EndAsync(string callerId)
    {
        return Enqueue(() =>
        {
            if (!IsDealer(callerId))
            {
                throw new InvalidOperationException("게임의 종료는 딜러만 할 수 있습니다.");
            }

            Phase = GamePhase.Lobby;
            return new GameState(string.Empty, [.. _players]);
        });
    }

    private bool IsDealer(string playerId) => _players.SingleOrDefault(player => player.IsDealer)?.Id == playerId;

    private async Task ProcessLoop()
    {
        await foreach (Func<Task> work in _queue.Reader.ReadAllAsync())
        {
            await work();
        }
    }

    private Task<T> Enqueue<T>(Func<T> func)
    {
        TaskCompletionSource<T> tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
        _queue.Writer.TryWrite(() =>
        {
            try
            {
                T result = func();
                tcs.SetResult(result);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }

            return Task.CompletedTask;
        });

        return tcs.Task;
    }
}
