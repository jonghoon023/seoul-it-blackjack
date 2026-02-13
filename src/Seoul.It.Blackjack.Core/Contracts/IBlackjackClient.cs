using System.Threading.Tasks;

namespace Seoul.It.Blackjack.Core.Contracts;

public interface IBlackjackClient
{
    Task OnStateChanged(GameState state);

    Task OnError(string code, string message);
}
