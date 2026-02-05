using Microsoft.AspNetCore.SignalR;
using Seoul.It.Blackjack.Backend.Domains;

namespace Seoul.It.Blackjack.Backend.Hubs
{
    /// <summary>
    /// SignalR Hub는 MVC 패턴의 Controller 역할을 수행합니다.
    /// 클라이언트의 요청을 받아 게임 세션에 전달하고, 변경된 상태를 모든 클라이언트에 브로드캐스트합니다.
    /// </summary>
    public class GameHub : Hub
    {
        // 서버에 하나만 존재하는 게임 세션
        private static readonly GameSession _session = new GameSession();
        private readonly string _dealerKey;

        public GameHub(IConfiguration configuration)
        {
            _dealerKey = configuration["DealerKey"] ?? string.Empty;
        }

        /// <summary>
        /// 클라이언트가 연결될 때 호출됩니다. 특별한 처리 없이 base 호출만 합니다.
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        /// <summary>
        /// 클라이언트가 연결을 끊을 때 호출됩니다. 세션에서 플레이어를 제거하고 상태를 업데이트합니다.
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _session.RemovePlayer(Context.ConnectionId);
            Console.WriteLine($"{Context.ConnectionId} disconnected");
            // 모든 클라이언트에게 상태 업데이트
            await Clients.All.SendAsync("StateUpdated", _session.ToDto());
            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// 클라이언트가 이름과 딜러키를 등록합니다. 딜러키가 맞고 현재 딜러가 없으면 딜러로 등록됩니다.
        /// 그렇지 않으면 플레이어로 등록됩니다.
        /// </summary>
        public async Task Register(string name, string? dealerKey)
        {
            bool isDealer = !string.IsNullOrWhiteSpace(dealerKey) && dealerKey == _dealerKey;
            var player = _session.AddPlayer(Context.ConnectionId, name, isDealer);
            string role = player.IsDealer ? "Dealer" : "Player";
            Console.WriteLine($"{name} registered as {role}");
            // 클라이언트에게 역할 할당 이벤트 전송
            await Clients.Caller.SendAsync("RoleAssigned", role);
            // 전체 상태 전송
            await Clients.All.SendAsync("StateUpdated", _session.ToDto());
        }

        /// <summary>
        /// 현재 게임 상태를 요청합니다. Caller에게만 상태를 반환합니다.
        /// </summary>
        public async Task RequestState()
        {
            await Clients.Caller.SendAsync("StateUpdated", _session.ToDto());
        }

        /// <summary>
        /// 딜러만이 호출할 수 있는 메서드입니다. 게임을 시작하고 초기 카드 배분을 수행합니다.
        /// </summary>
        public async Task StartGame()
        {
            // 호출자가 딜러인지 확인
            var caller = _session.Players.FirstOrDefault(p => p.Id == Context.ConnectionId);
            if (caller == null || !caller.IsDealer)
            {
                await Clients.Caller.SendAsync("SystemMessage", "딜러만 게임을 시작할 수 있습니다.");
                return;
            }
            var message = _session.StartGame();
            if (!string.IsNullOrEmpty(message))
            {
                await Clients.Caller.SendAsync("SystemMessage", message);
            }
            else
            {
                Console.WriteLine("Game started by dealer");
                await Clients.All.SendAsync("StateUpdated", _session.ToDto());
            }
        }

        /// <summary>
        /// 현재 턴인 플레이어가 카드를 더 받습니다.
        /// </summary>
        public async Task Hit()
        {
            var error = _session.PlayerHit(Context.ConnectionId);
            if (!string.IsNullOrEmpty(error))
            {
                await Clients.Caller.SendAsync("SystemMessage", error);
            }
            // 상태를 전체에 브로드캐스트
            await Clients.All.SendAsync("StateUpdated", _session.ToDto());
        }

        /// <summary>
        /// 현재 턴인 플레이어가 스탠드를 선언합니다.
        /// </summary>
        public async Task Stand()
        {
            var error = _session.PlayerStand(Context.ConnectionId);
            if (!string.IsNullOrEmpty(error))
            {
                await Clients.Caller.SendAsync("SystemMessage", error);
            }
            await Clients.All.SendAsync("StateUpdated", _session.ToDto());
        }

        /// <summary>
        /// 딜러가 라운드를 초기화합니다. 라운드 결과를 보고 새 게임을 시작하려면 호출합니다.
        /// </summary>
        public async Task ResetRound()
        {
            var caller = _session.Players.FirstOrDefault(p => p.Id == Context.ConnectionId);
            if (caller == null || !caller.IsDealer)
            {
                await Clients.Caller.SendAsync("SystemMessage", "딜러만 라운드를 초기화할 수 있습니다.");
                return;
            }
            _session.ResetRound();
            Console.WriteLine("Round reset by dealer");
            await Clients.All.SendAsync("StateUpdated", _session.ToDto());
        }
    }
}