using Seoul.It.Blackjack.Backend.Services.Commands;
using System;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Seoul.It.Blackjack.Backend.Services.Infrastructure;

/// <summary>
/// 채널 큐를 이용해 게임 명령을 순서대로 처리하는 구현체입니다.
/// </summary>
internal sealed class ChannelGameCommandProcessor : IGameCommandProcessor
{
    /// <summary>
    /// 처리 대기 중인 명령을 쌓아두는 큐입니다.
    /// </summary>
    private readonly Channel<QueueItem> _queue = Channel.CreateUnbounded<QueueItem>(new UnboundedChannelOptions
    {
        SingleReader = true,
        SingleWriter = false,
    });

    /// <summary>
    /// 명령 처리 루프를 시작합니다.
    /// </summary>
    public ChannelGameCommandProcessor()
    {
        _ = Task.Run(ProcessLoopAsync);
    }

    /// <summary>
    /// 명령을 큐에 넣고 순서가 올 때까지 기다린 뒤 결과를 반환합니다.
    /// </summary>
    /// <param name="command">큐에 넣을 명령 데이터입니다.</param>
    /// <param name="handler">실제 명령 처리 함수입니다.</param>
    /// <returns>처리 결과입니다.</returns>
    public Task<GameOperationResult> EnqueueAsync(GameCommand command, Func<GameOperationResult> handler)
    {
        QueueItem item = new(command, handler);
        _queue.Writer.TryWrite(item);
        return item.Completion.Task;
    }

    /// <summary>
    /// 큐에서 명령을 하나씩 꺼내 순서대로 실행합니다.
    /// </summary>
    /// <returns>루프 작업입니다.</returns>
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

    /// <summary>
    /// 큐에 저장할 명령 1건의 데이터입니다.
    /// </summary>
    private sealed class QueueItem
    {
        /// <summary>
        /// 큐 아이템을 만듭니다.
        /// </summary>
        /// <param name="command">명령 데이터입니다.</param>
        /// <param name="handler">명령 처리 함수입니다.</param>
        public QueueItem(GameCommand command, Func<GameOperationResult> handler)
        {
            Command = command;
            Handler = handler;
        }

        /// <summary>
        /// 처리할 게임 명령입니다.
        /// </summary>
        public GameCommand Command { get; }

        /// <summary>
        /// 명령 처리 함수입니다.
        /// </summary>
        public Func<GameOperationResult> Handler { get; }

        /// <summary>
        /// 처리 완료를 알리기 위한 TaskCompletionSource입니다.
        /// </summary>
        public TaskCompletionSource<GameOperationResult> Completion { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
    }
}
