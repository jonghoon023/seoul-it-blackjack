using Seoul.It.Blackjack.Backend.Domains;
using Seoul.It.Blackjack.Backend.Dto;
using Seoul.It.Blackjack.Backend.DTO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Seoul.It.Blackjack.Backend.Domain
{
    /// <summary>
    /// 현재 서버에서 하나만 존재하는 게임 세션을 관리하는 클래스입니다.
    /// 플레이어 목록, 덱, 게임 단계 등을 저장하고 게임 로직을 수행합니다.
    /// </summary>
    public class GameSession
    {
        public GamePhase Phase { get; private set; } = GamePhase.Waiting;
        public List<Player> Players { get; } = new List<Player>();
        private Deck? _deck;
        private int _currentPlayerIndex;
        private bool _dealerHoleRevealed;
        public string LastAction { get; private set; } = string.Empty;

        /// <summary>
        /// 새로운 플레이어를 추가합니다. 같은 연결 ID가 이미 존재하면 추가되지 않습니다.
        /// </summary>
        public Player AddPlayer(string connectionId, string name, bool isDealer)
        {
            // 이미 존재하는 경우 반환
            var existing = Players.FirstOrDefault(p => p.Id == connectionId);
            if (existing != null) return existing;

            // 이미 딜러가 존재한다면 두 번째 딜러는 일반 플레이어로 간주
            if (isDealer && Players.Any(p => p.IsDealer))
            {
                isDealer = false;
            }

            var player = new Player(connectionId, name, isDealer);
            Players.Add(player);
            return player;
        }

        /// <summary>
        /// 플레이어를 제거합니다. 딜러가 나가면 딜러 자리를 비워둡니다.
        /// </summary>
        public void RemovePlayer(string connectionId)
        {
            var player = Players.FirstOrDefault(p => p.Id == connectionId);
            if (player == null) return;

            int removedIndex = Players.IndexOf(player);
            Players.RemoveAt(removedIndex);

            // 현재 턴 인덱스 조정
            if (Phase == GamePhase.PlayersTurn && player.IsTurn)
            {
                // 플레이어가 턴 중에 나갔을 경우 다음 플레이어로 넘깁니다.
                NextPlayerOrDealer();
            }
            else if (Phase == GamePhase.PlayersTurn && removedIndex < _currentPlayerIndex)
            {
                _currentPlayerIndex--;
            }

            // 딜러가 나가면 Phase 초기화
            if (player.IsDealer)
            {
                // 딜러가 나가면 게임을 초기상태로 되돌립니다.
                Phase = GamePhase.Waiting;
                LastAction = string.Empty;
                _deck = null;
                _dealerHoleRevealed = false;
                // 플레이어들의 정보 초기화
                foreach (var pl in Players)
                {
                    pl.Cards.Clear();
                    pl.IsTurn = false;
                    pl.IsBusted = false;
                    pl.Result = null;
                }
            }
        }

        /// <summary>
        /// 라운드를 초기화합니다. 현재 플레이어 명단은 유지하지만 카드와 상태를 초기화합니다.
        /// </summary>
        public void ResetRound()
        {
            Phase = GamePhase.Waiting;
            LastAction = string.Empty;
            _deck = null;
            _dealerHoleRevealed = false;
            foreach (var player in Players)
            {
                player.Cards.Clear();
                player.IsTurn = false;
                player.IsBusted = false;
                player.Result = null;
            }
        }

        /// <summary>
        /// 게임을 시작합니다. 덱을 생성하고 각자 카드 두 장을 배분한 뒤
        /// 첫 번째 플레이어부터 턴을 시작합니다.
        /// </summary>
        public string StartGame()
        {
            // 플레이어가 두 명 이상(딜러 포함) 있어야 게임을 시작할 수 있습니다.
            if (!Players.Any(p => !p.IsDealer) || !Players.Any(p => p.IsDealer))
            {
                return "딜러와 플레이어가 모두 연결되어 있어야 게임을 시작할 수 있습니다.";
            }
            // 덱 생성 및 셔플
            _deck = new Deck();
            // 상태 초기화
            Phase = GamePhase.PlayersTurn;
            LastAction = "Game Started";
            _dealerHoleRevealed = false;
            _currentPlayerIndex = 0;

            foreach (var player in Players)
            {
                player.Cards.Clear();
                player.IsTurn = false;
                player.IsBusted = false;
                player.Result = null;
            }

            // 카드 두 장씩 배분
            foreach (var player in Players)
            {
                // 딜러와 플레이어 구분 없이 각각 두 장씩 받습니다.
                player.Cards.Add(_deck!.Draw());
                player.Cards.Add(_deck!.Draw());
            }

            // 첫 번째 턴은 일반 플레이어들 중 리스트 순서대로 진행합니다.
            var firstPlayerIndex = Players.FindIndex(p => !p.IsDealer);
            if (firstPlayerIndex >= 0)
            {
                _currentPlayerIndex = firstPlayerIndex;
                Players[_currentPlayerIndex].IsTurn = true;
            }

            return string.Empty;
        }

        /// <summary>
        /// 현재 턴인 플레이어가 카드를 한 장 더 받습니다. 버스트 시 자동으로 턴을 넘깁니다.
        /// </summary>
        public string PlayerHit(string connectionId)
        {
            var player = Players.FirstOrDefault(p => p.Id == connectionId);
            if (player == null || !player.IsTurn) return "현재 턴이 아닙니다.";
            if (Phase != GamePhase.PlayersTurn) return "플레이어 턴이 아닙니다.";
            if (_deck == null) return "게임이 시작되지 않았습니다.";

            // 카드 한 장 뽑기
            var card = _deck.Draw();
            player.Cards.Add(card);

            // 점수 계산 후 버스트 여부 확인
            var score = CalculateScore(player.Cards);
            if (score > 21)
            {
                player.IsBusted = true;
                player.IsTurn = false;
                LastAction = $"{player.Name} Hit ({card}) and Busted";
                // 다음 플레이어로 넘어감
                NextPlayerOrDealer();
            }
            else
            {
                LastAction = $"{player.Name} Hit ({card})";
            }
            return string.Empty;
        }

        /// <summary>
        /// 현재 턴인 플레이어가 더 이상 카드를 받지 않고 턴을 마칩니다.
        /// </summary>
        public string PlayerStand(string connectionId)
        {
            var player = Players.FirstOrDefault(p => p.Id == connectionId);
            if (player == null || !player.IsTurn) return "현재 턴이 아닙니다.";
            if (Phase != GamePhase.PlayersTurn) return "플레이어 턴이 아닙니다.";

            player.IsTurn = false;
            LastAction = $"{player.Name} Stand";
            // 다음 플레이어 또는 딜러로 이동
            NextPlayerOrDealer();
            return string.Empty;
        }

        /// <summary>
        /// 플레이어 턴이 끝났을 때 다음 차례를 결정합니다. 모든 플레이어가 끝나면 딜러가 진행합니다.
        /// </summary>
        private void NextPlayerOrDealer()
        {
            // 현재 플레이어 인덱스 이후로 버스트하지 않은 다음 일반 플레이어를 찾습니다.
            for (int i = _currentPlayerIndex + 1; i < Players.Count; i++)
            {
                if (!Players[i].IsDealer && !Players[i].IsBusted)
                {
                    _currentPlayerIndex = i;
                    Players[_currentPlayerIndex].IsTurn = true;
                    Phase = GamePhase.PlayersTurn;
                    return;
                }
            }

            // 더 이상 차례를 진행할 플레이어가 없으면 딜러 차례
            _dealerHoleRevealed = true;
            Phase = GamePhase.DealerTurn;
            DealerAutoPlay();
        }

        /// <summary>
        /// 딜러가 자동으로 카드를 받는 로직을 처리합니다.
        /// 블랙잭 규칙에 따라 16 이하면 Hit, 17 이상이면 Stand합니다.
        /// Soft 17에서도 스탠드(S17)합니다.
        /// </summary>
        private void DealerAutoPlay()
        {
            var dealer = Players.First(p => p.IsDealer);
            // 딜러 차례 시작 안내
            LastAction = "Dealer Turn";
            // 딜러는 Hit/Stand를 자동으로 수행합니다.
            while (true)
            {
                // 점수가 16 이하이면 한 장 더, 17 이상이면 멈춥니다.
                var score = CalculateScoreInternal(dealer.Cards);
                if (score >= 17)
                {
                    break;
                }
                var card = _deck!.Draw();
                dealer.Cards.Add(card);
                LastAction = $"Dealer Hit ({card})";
            }
            // 딜러 점수 계산 후 버스트 여부 확인
            var dealerScore = CalculateScore(dealer.Cards);
            if (dealerScore > 21)
            {
                dealer.IsBusted = true;
            }
            // 딜러 차례가 끝나면 결과를 정산합니다.
            EvaluateResults();
            Phase = GamePhase.Finished;
        }

        /// <summary>
        /// 승패를 판정하고 각 플레이어의 Result 속성을 채웁니다.
        /// </summary>
        private void EvaluateResults()
        {
            var dealer = Players.First(p => p.IsDealer);
            var dealerScore = CalculateScore(dealer.Cards);
            foreach (var player in Players.Where(p => !p.IsDealer))
            {
                var playerScore = CalculateScore(player.Cards);
                if (player.IsBusted)
                {
                    // 플레이어가 이미 버스트면 패배
                    player.Result = "Lose";
                }
                else if (dealer.IsBusted)
                {
                    // 딜러가 버스트면 승리
                    player.Result = "Win";
                }
                else if (playerScore > dealerScore)
                {
                    player.Result = "Win";
                }
                else if (playerScore < dealerScore)
                {
                    player.Result = "Lose";
                }
                else
                {
                    player.Result = "Push";
                }
            }
        }

        /// <summary>
        /// 카드 목록의 점수를 계산합니다. 에이스는 1 또는 11로 계산하여 가장 유리한 점수가 되도록 조정합니다.
        /// Soft 17 여부를 별도로 고려하지 않고, 점수가 17 이상이면 딜러는 스탠드하는 규칙을 따릅니다.
        /// </summary>
        public int CalculateScore(List<Card> cards)
        {
            return CalculateScoreInternal(cards);
        }

        /// <summary>
        /// 카드 목록의 점수를 계산하고 소프트 여부를 함께 반환합니다.
        /// 블랙잭에서 에이스는 1 또는 11 중 유리한 쪽으로 계산됩니다.
        /// 예를 들어 A와 6이 있는 경우 기본 합은 11(A) + 6 = 17이며 에이스를 11로 세더라도 21이 넘지 않으므로
        /// 이 상황을 "소프트 17"이라고 부르고, 딜러는 S17 규칙에 따라 스탠드합니다.
        /// A, 9, 8과 같은 손에서 처음 합계는 11+9+8=28로 21을 넘습니다. 이 경우 A를 1로 내려 1+9+8=18로 계산합니다.
        /// </summary>
        // Soft 17과 같은 복잡한 규칙을 구분할 필요 없이, 점수 계산만 수행하여 반환합니다.
        // 딜러는 점수가 17 이상이면 스탠드하므로 Soft 여부를 따로 확인하지 않습니다.
        private int CalculateScoreInternal(List<Card> cards)
        {
            int total = 0;
            int aceCount = 0;
            foreach (var card in cards)
            {
                switch (card.Rank)
                {
                    case "J":
                    case "Q":
                    case "K":
                        total += 10;
                        break;
                    case "A":
                        total += 11;
                        aceCount++;
                        break;
                    default:
                        total += int.Parse(card.Rank);
                        break;
                }
            }
            // 21을 초과하면 에이스를 11에서 1로 줄여 총합을 낮춥니다.
            while (total > 21 && aceCount > 0)
            {
                total -= 10;
                aceCount--;
            }
            return total;
        }

        /// <summary>
        /// GameStateDto 형태로 현재 세션의 상태를 변환합니다.
        /// 딜러의 Hole 카드는 _dealerHoleRevealed 플래그에 따라 공개 여부를 결정합니다.
        /// </summary>
        public GameStateDto ToDto()
        {
            var dto = new GameStateDto
            {
                Phase = Phase.ToString(),
                Message = BuildMessage(),
                LastAction = LastAction
            };
            // 딜러 정보 채우기
            var dealer = Players.FirstOrDefault(p => p.IsDealer);
            if (dealer != null)
            {
                var dealerDto = new DealerDto
                {
                    Score = CalculateScore(dealer.Cards),
                    IsHoleRevealed = _dealerHoleRevealed
                };
                // 홀카드 공개 여부에 따라 카드 문자열 리스트를 생성
                if (_dealerHoleRevealed || Phase == GamePhase.Finished)
                {
                    dealerDto.Cards = dealer.Cards.Select(c => c.ToString()).ToList();
                }
                else
                {
                    if (dealer.Cards.Count > 0)
                    {
                        // 첫 번째 카드는 공개, 두 번째 카드는 ?? 표시
                        dealerDto.Cards.Add(dealer.Cards[0].ToString());
                        dealerDto.Cards.Add("??");
                    }
                }
                dto.Dealer = dealerDto;
            }
            // 플레이어 정보 채우기
            foreach (var player in Players.Where(p => !p.IsDealer))
            {
                var pDto = new PlayerDto
                {
                    Id = player.Id,
                    Name = player.Name,
                    Cards = player.Cards.Select(c => c.ToString()).ToList(),
                    Score = CalculateScore(player.Cards),
                    IsTurn = player.IsTurn,
                    IsBusted = player.IsBusted,
                    Result = player.Result
                };
                dto.Players.Add(pDto);
            }
            return dto;
        }

        /// <summary>
        /// 현재 게임 상태를 설명하는 간단한 메시지를 생성합니다.
        /// </summary>
        private string BuildMessage()
        {
            switch (Phase)
            {
                case GamePhase.Waiting:
                    return "딜러와 플레이어가 모두 참여하면 게임을 시작할 수 있습니다.";
                case GamePhase.PlayersTurn:
                    var current = Players.ElementAtOrDefault(_currentPlayerIndex);
                    return current != null ? $"{current.Name}님의 차례입니다." : "플레이어 차례";
                case GamePhase.DealerTurn:
                    return "딜러가 카드를 받고 있습니다.";
                case GamePhase.Finished:
                    return "라운드가 종료되었습니다. ResetRound를 호출해 새로운 라운드를 시작하세요.";
                default:
                    return string.Empty;
            }
        }
    }
}