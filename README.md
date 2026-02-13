# seoul-it-blackjack

## Seoul.It.Blackjack.Client DI 사용 예시

```csharp
using Microsoft.Extensions.DependencyInjection;
using Seoul.It.Blackjack.Client;
using Seoul.It.Blackjack.Client.Extensions;
using Seoul.It.Blackjack.Client.Options;

ServiceCollection services = new();
services.AddBlackjackClient(options =>
{
    options.HubUrl = "http://localhost:5000/blackjack";
});

ServiceProvider provider = services.BuildServiceProvider();

BlackjackClient client = provider.GetRequiredService<BlackjackClient>();
BlackjackClientOptions options = provider.GetRequiredService<BlackjackClientOptions>();

client.StateChanged += state => Console.WriteLine($"Phase: {state.Phase}");
client.Error += (code, message) => Console.WriteLine($"{code}: {message}");

await client.ConnectAsync(options.HubUrl); // 자동 연결이 아니라 호출자가 명시적으로 호출
await client.JoinAsync("player-1");
```

핵심:
- `AddBlackjackClient(...)`는 `BlackjackClient`를 `Singleton`으로 등록합니다.
- DI 등록만으로는 연결되지 않으며, `ConnectAsync(...)`를 직접 호출해야 합니다.
