using Seoul.It.Blackjack.Backend.Services.Commands;
using System;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Seoul.It.Blackjack.Backend.Services.Infrastructure;

internal sealed class ChannelGameCommandProcessor : IGameCommandProcessor
{
    private readonly Channel<QueueItem> _queue = Channel.CreateUnbounded<QueueItem>(new UnboundedChannelOptions
    {
        SingleReader = true,
        SingleWriter = false,
    });

    public ChannelGameCommandProcessor()
    {
        _ = Task.Run(ProcessLoopAsync);
    }

    public Task<GameOperationResult> EnqueueAsync(GameCommand command, Func<GameOperationResult> handler)
    {
        QueueItem item = new(command, handler);
        _queue.Writer.TryWrite(item);
        return item.Completion.Task;
    }

    private async Task ProcessLoopAsync()
    {
        await foreach (QueueItem item in _queue.Reader.ReadAllAsync())
        {
            try
            {
                GameOperationResult result = item.Handler();
                item.Completion.SetResult(result);
            }
            catch (Exception ex)
            {
                item.Completion.SetException(ex);
            }
        }
    }

    private sealed class QueueItem
    {
        public QueueItem(GameCommand command, Func<GameOperationResult> handler)
        {
            Command = command;
            Handler = handler;
        }

        public GameCommand Command { get; }

        public Func<GameOperationResult> Handler { get; }

        public TaskCompletionSource<GameOperationResult> Completion { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
    }
}
