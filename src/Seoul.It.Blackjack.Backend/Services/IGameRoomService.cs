using Seoul.It.Blackjack.Backend.Services.Commands;
using System.Threading.Tasks;

namespace Seoul.It.Blackjack.Backend.Services;

internal interface IGameRoomService
{
    Task<GameOperationResult> JoinAsync(string connectionId, string name, string? dealerKey);

    Task<GameOperationResult> LeaveAsync(string connectionId);

    Task<GameOperationResult> DisconnectAsync(string connectionId);

    Task<GameOperationResult> StartRoundAsync(string connectionId);

    Task<GameOperationResult> HitAsync(string connectionId);

    Task<GameOperationResult> StandAsync(string connectionId);
}
