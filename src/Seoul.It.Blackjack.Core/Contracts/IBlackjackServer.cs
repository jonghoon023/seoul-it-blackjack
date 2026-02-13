using System.Threading.Tasks;

namespace Seoul.It.Blackjack.Core.Contracts;

public interface IBlackjackServer
{
    Task Join(string name, string? dealerKey);

    Task Leave();

    Task StartRound();

    Task Hit();

    Task Stand();
}
