using System;
using System.Threading.Tasks;

namespace Seoul.It.Blackjack.Backend.Tests;

internal static class TestWaiter
{
    public static async Task WaitUntilAsync(Func<bool> condition, int timeoutMs = 3000)
    {
        int elapsed = 0;
        while (!condition())
        {
            if (elapsed >= timeoutMs)
            {
                throw new TimeoutException("Condition was not met within timeout.");
            }

            await Task.Delay(50);
            elapsed += 50;
        }
    }
}
