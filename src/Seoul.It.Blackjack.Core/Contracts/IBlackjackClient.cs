using System.Threading.Tasks;

namespace Seoul.It.Blackjack.Core.Contracts;

public interface IBlackjackClient
{
    Task StateChanged(GameState state);

    Task Error(string code, string message);
}
