using Microsoft.AspNetCore.SignalR.Client;

namespace Seoul.It.Blackjack.TestClient;

/// <summary>
/// 콘솔 테스트 클라이언트는 프론트엔드가 없는 환경에서도 서버 동작을 확인할 수 있게 해줍니다.
/// 이 프로젝트는 선택 사항이며, 학생이 백엔드만 구현할 때는 사용하지 않아도 됩니다.
/// </summary>
internal class Program
{
    static async Task Main()
    {
        Console.WriteLine("SignalR 테스트 클라이언트를 시작합니다.");
        Console.Write("서버 주소 입력 (예: http://localhost:5000): ");
        var serverUrl = Console.ReadLine();
        // 허브 엔드포인트 설정
        var connection = new HubConnectionBuilder()
            .WithUrl($"{serverUrl}/hub/blackjack")
            .WithAutomaticReconnect()
            .Build();

        // 서버 → 클라이언트 이벤트 등록
        connection.On<string>("RoleAssigned", role =>
        {
            Console.WriteLine($"서버로부터 역할이 할당되었습니다: {role}");
        });
        connection.On<GameStateDto>("StateUpdated", state =>
        {
            Console.WriteLine("--- 게임 상태 업데이트 ---");
            Console.WriteLine($"단계: {state.Phase}");
            Console.WriteLine($"딜러 카드: {string.Join(", ", state.Dealer.Cards)} (점수: {state.Dealer.Score})");
            foreach (var player in state.Players)
            {
                Console.WriteLine($"플레이어 {player.Name} - 카드: {string.Join(", ", player.Cards)} (점수: {player.Score}), " +
                                  $"턴: {player.IsTurn}, 버스트: {player.IsBusted}, 결과: {player.Result}");
            }
            Console.WriteLine($"메시지: {state.Message}");
            Console.WriteLine($"마지막 액션: {state.LastAction}");
            Console.WriteLine("---------------------------\n");
        });
        connection.On<string>("SystemMessage", msg =>
        {
            Console.WriteLine($"[System] {msg}");
        });

        await connection.StartAsync();
        Console.WriteLine("서버에 연결되었습니다.");

        // 사용자 정보 등록
        Console.Write("이름을 입력하세요: ");
        var name = Console.ReadLine() ?? string.Empty;
        Console.Write("딜러 키(없으면 Enter): ");
        var key = Console.ReadLine();
        await connection.InvokeAsync("Register", name, string.IsNullOrWhiteSpace(key) ? null : key);

        // 명령 루프
        while (true)
        {
            Console.WriteLine("명령을 입력하세요 (start, hit, stand, reset, state, exit): ");
            var command = Console.ReadLine()?.Trim().ToLower();
            switch (command)
            {
                case "start":
                    await connection.InvokeAsync("StartGame");
                    break;
                case "hit":
                    await connection.InvokeAsync("Hit");
                    break;
                case "stand":
                    await connection.InvokeAsync("Stand");
                    break;
                case "reset":
                    await connection.InvokeAsync("ResetRound");
                    break;
                case "state":
                    await connection.InvokeAsync("RequestState");
                    break;
                case "exit":
                    await connection.StopAsync();
                    return;
                default:
                    Console.WriteLine("알 수 없는 명령입니다.");
                    break;
            }
        }
    }
}

#region DTO 복사본
// 서버 DTO를 그대로 복사해 놓았습니다. 서버 프로젝트에 대한 참조 없이도 빌드할 수 있게 하기 위함입니다.
public class GameStateDto
{
    public string Phase { get; set; } = string.Empty;
    public DealerDto Dealer { get; set; } = new DealerDto();
    public List<PlayerDto> Players { get; set; } = new List<PlayerDto>();
    public string Message { get; set; } = string.Empty;
    public string LastAction { get; set; } = string.Empty;
}
public class DealerDto
{
    public List<string> Cards { get; set; } = new List<string>();
    public int Score { get; set; }
    public bool IsHoleRevealed { get; set; }
}
public class PlayerDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<string> Cards { get; set; } = new List<string>();
    public int Score { get; set; }
    public bool IsTurn { get; set; }
    public bool IsBusted { get; set; }
    public string? Result { get; set; }
}
#endregion
