using Seoul.It.Blackjack.Backend.Services.Commands;
using System;
using System.Threading.Tasks;

namespace Seoul.It.Blackjack.Backend.Services.Infrastructure;

internal interface IGameCommandProcessor
{
    Task<GameOperationResult> EnqueueAsync(GameCommand command, Func<GameOperationResult> handler);
}
