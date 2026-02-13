using Seoul.It.Blackjack.Backend.Services.Commands;
using System;
using System.Threading.Tasks;

namespace Seoul.It.Blackjack.Backend.Services.Infrastructure;

/// <summary>
/// 게임 명령을 큐에 넣어 순서대로 처리해 주는 계약입니다.
/// </summary>
internal interface IGameCommandProcessor
{
    /// <summary>
    /// 명령과 처리 함수를 큐에 등록하고 처리 결과를 기다립니다.
    /// </summary>
    /// <param name="command">큐에 넣을 명령 데이터입니다.</param>
    /// <param name="handler">실제로 상태를 바꾸는 처리 함수입니다.</param>
    /// <returns>명령 처리 결과입니다.</returns>
    Task<GameOperationResult> EnqueueAsync(GameCommand command, Func<GameOperationResult> handler);
}
