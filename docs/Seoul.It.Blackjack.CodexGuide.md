# Seoul.It.Blackjack — Codex 구현 지침서 (MVP / 교육용 / 7시간 내)

이 문서는 Codex가 **읽고 바로 구현을 시작**할 수 있도록 만든 **상세 지침서**입니다.  
최우선 목표는 **문법을 모르는 아이들과 함께 만들어도 될 만큼 단순하게**, 그리고 **Core/Backend/Client를 7시간 내 구현 가능한 범위**로 고정하는 것입니다.

---

## 0) 절대 규칙(가장 중요)

### 0.1 범위 고정(MVP)
이번 구현은 “게임 한 판이 끝까지 진행되는 흐름”만 만듭니다.

**반드시 구현**
- SignalR로 Join/Leave 가능
- 단일 방(방 1개 고정)
- 딜러(사용자) 1명 지정
- StartRound로 라운드 시작(초기 카드 배분)
- 플레이어 액션: Hit / Stand
- 딜러는 **서버 자동 진행**
- 라운드 종료 및 결과 표시
- Disconnect(끊김) = 즉시 퇴장(복구 없음)
- 상태 변화 시 전원 브로드캐스트
- 잘못된 요청은 요청자에게만 Error(code, message)

**절대 구현하지 않음(Non-goals)**
- 계정/로그인/인증(JWT 등)
- DB 저장/통계/랭킹/리플레이
- 베팅/칩/경제
- 타이머/턴 제한
- Split/Double/Insurance/Surrender 등 고급 룰
- 재접속 상태 복구(요구사항상 없음)

### 0.2 DTO 변환 금지(복잡도 방지)
- Backend에서 `CardDto`, `GameStateDto` 같은 **전송용 DTO 클래스**를 만들지 않습니다.
- Core의 모델(`Card`, `GameState` 등)을 **그대로 전송 계약(Contract)** 으로 사용합니다.
- 따라서 “변환 코드(Mapper)”도 만들지 않습니다.

> 단, **그대로 보내도 안전하도록** 상태 모델에 “보내면 안 되는 정보”를 애초에 넣지 않습니다(아래 5절 참고).

### 0.3 쓰레드 세이프는 “구조로” 보장
- Hub가 상태를 직접 바꾸면 안 됩니다.
- 모든 게임 상태 변경은 **단일 처리 루프 1곳**에서만 일어나야 합니다.
- 동시 요청은 “명령 큐로 직렬 처리”하여 레이스 컨디션을 제거합니다.

---

## 1) 프로젝트 구성 및 타겟 프레임워크(고정)

### 1.1 프로젝트
1. `Seoul.It.Blackjack.Backend`
2. `Seoul.It.Blackjack.Core`
3. `Seoul.It.Blackjack.Client`
4. `Seoul.It.Blackjack.Frontend` (이번 문서에서는 Core/Backend/Client 중심)

### 1.2 타겟 프레임워크(고정)
- `Seoul.It.Blackjack.Core` : **.NET Standard 2.0**
- `Seoul.It.Blackjack.Client` : **.NET Standard 2.0**
- `Seoul.It.Blackjack.Backend` : **.NET 8**
- `Seoul.It.Blackjack.Frontend` : (Blazor) **.NET 8**

### 1.3 참조(의존성) 방향(권장)
- Backend → Core
- Client → Core
- Frontend → Client

> Core/Client는 netstandard2.0이므로 **.NET 8 전용 타입/기능**을 사용하지 않습니다.

---

## 2) 현재 보유 코드(확정 기반)

### 2.1 Core(확정)
- `Card.cs` : `public record Card(Suit Suit, Rank Rank);`
- `Suit.cs` : Clubs / Diamonds / Hearts / Spades
- `Rank.cs` : Ace, Two..Ten, Jack, Queen, King
  - `RankExtension.ToValue()`에서 **Ace는 1점 고정**
  - Ten/Jack/Queen/King는 10점

> 참고(정리): `RankExtension.ToValue()` 구현에 Jack이 중복으로 들어있다면(의미상 문제는 작지만) 깔끔하게 한 번만 두는 것을 권장합니다.

### 2.2 Backend(확정)
- `Deck.cs` : `Deck.Cards`로 52장 조합을 생성(`IEnumerable<Card>`)

> 이 구성은 “덱은 서버만 알고, 상태는 Core 모델로 전송” 구조와 잘 맞습니다.

---

## 3) 게임 룰(고정)

### 3.1 덱/카드
- 표준 52장 카드 기준 **4덱(총 208장)**
- 조커 없음
- 라운드 시작(StartRound)마다 **새 덱(208장) 생성 + 셔플**해서 사용(가장 단순)
- 카드 공개 정책: **모두 공개** (숨김 카드 없음)

### 3.2 점수
- Ace는 **반드시 1점**
- J/Q/K는 10점
- 합계 **21 초과 → Bust(즉시 패배 상태)**
- 합계 **21 → “블랙잭” 라벨(표시용)**  
  (베팅/배당 없음이므로 특별한 지급 로직은 없습니다.)

### 3.3 액션
- 플레이어 액션은 **Hit / Stand만**
- 자신의 턴에만 Hit/Stand 가능
- Hit 후 Bust가 되면 해당 플레이어는 더 이상 행동 불가(Stand처럼 종료 처리)

### 3.4 딜러(사용자) + 자동 진행(확정)
- 딜러는 “사용자(자리/권한)”로 존재하지만, **딜러의 Hit/Stand 행동은 서버가 자동으로 수행**합니다.
- 딜러 자동 진행 규칙:
  - 딜러 점수 < 17 이면 Hit(카드 1장 추가)
  - 반복
  - 딜러 점수 ≥ 17 이면 Stand로 종료
  - 21 초과면 Bust로 종료

- 딜러가 클라이언트에서 Hit/Stand를 호출하면:
  - 상태는 변하지 않고,
  - 요청자에게만 `Error("DEALER_IS_AUTO", "딜러는 자동으로 진행됩니다.")` 전송

### 3.5 라운드 시작(확정)
- `StartRound()` 호출 시:
  - 현재 참가 중인 모든 플레이어(딜러 포함)에게 **2장씩 배분**
- 이후 턴 진행 순서:
  - **딜러 제외** 일반 플레이어만 순서대로 진행
  - 순서는 **Join 순서**를 사용(가장 단순)

### 3.6 라운드 종료
- 모든 일반 플레이어가 Stand 또는 Bust 상태가 되면:
  1) 서버가 딜러 자동 진행 실행
  2) 결과 계산(승/패/무)
  3) Phase = Ended
  4) 상태 브로드캐스트

### 3.7 승패(베팅 없음)
- 플레이어 Bust → 플레이어 패
- 딜러 Bust → Bust하지 않은 플레이어는 승
- 둘 다 Bust가 아니면 점수 비교:
  - playerScore > dealerScore → 승
  - playerScore < dealerScore → 패
  - 같으면 무승부

---

## 4) 단일 방 / 접속 / 퇴장 규칙(고정)

### 4.1 방은 하나
- 방 id, 방 목록 등 없음
- 상태 변경 시 기본적으로 **전원에게 브로드캐스트**

### 4.2 Join 정책(확정)
- **InRound 중에는 Join 불허**
- Join 허용 Phase:
  - Waiting, Ended
- InRound에서 Join 시도:
  - 상태 변경 없음
  - 요청자에게만 `Error("GAME_IN_PROGRESS", "게임이 진행 중이라 참가할 수 없습니다.")`

### 4.3 딜러 선정(확정)
- **첫 번째로 Join한 플레이어가 딜러**가 됩니다.
- 딜러는 사용자지만 행동은 자동(위 3.4 규칙)

### 4.4 Leave / Disconnect(복구 없음)
- `Leave()` 또는 `OnDisconnectedAsync`는 동일하게 “퇴장” 처리
- 퇴장 시:
  - Players 목록에서 제거
  - ConnectionRegistry에서 매핑 제거

### 4.5 딜러 퇴장 정책(확정)
- 딜러가 나가면(Leave/Disconnect):
  - 진행 중 라운드가 있든 없든 **즉시 Waiting으로 리셋**
  - 남은 플레이어가 있으면 **Join 순서 기준 첫 번째를 새 딜러**로 지정
  - 상태 브로드캐스트

### 4.6 일반 플레이어 퇴장 정책(확정)
- 라운드 중 일반 플레이어가 나가면:
  - 해당 플레이어 제거
  - 그 플레이어가 현재 턴이면 즉시 다음 플레이어로 턴 이동
- 남은 플레이어가 너무 적어 라운드가 의미 없으면(예: 딜러만 남음):
  - Waiting으로 리셋(단순 정책)

---

## 5) “DTO 없이 전송”을 위한 상태 모델 설계(핵심)

DTO 변환을 하지 않으므로, Core 모델은 **그 자체로 전송 계약**입니다.  
따라서 **클라이언트가 보면 안 되는 정보는 상태 모델에 절대 넣지 않습니다.**

### 5.1 절대 상태에 넣지 말 것
- **덱(Deck) / 남은 카드 목록 / 다음 카드 순서**
  - 이 정보는 클라이언트에 불필요하며, 포함하면 “그대로 전송” 방식과 충돌합니다.
  - 덱은 Backend의 `GameRoomService` 내부 필드로만 유지합니다.

### 5.2 Core에 추가로 필요한 최소 모델(권장)
> 아래는 “권장 최소”입니다. 교육용/7시간 MVP라 파일 수를 줄이고 싶으면 합쳐도 됩니다.

#### GamePhase (enum)
- Waiting / InRound / Ended

#### RoundOutcome (enum)
- None / Win / Lose / Tie

#### PlayerState (class)
- PlayerId (string)
- DisplayName (string)
- IsDealer (bool)
- Cards (List<Card>)  ← 공개이므로 그대로 보냄
- Score (int)         ← 서버가 매 처리 후 계산해 저장(클라이언트 단순화)
- IsBlackjack (bool)  ← Score == 21 (표시용)
- IsStanding (bool)
- IsBusted (bool)
- Outcome (RoundOutcome) ← Ended에서만 채움(Waiting/InRound에서는 None)

#### GameState (class)
- Phase (GamePhase)
- Players (List<PlayerState>)
- DealerPlayerId (string)
- CurrentTurnPlayerId (string)  ← 딜러 자동이므로 “일반 플레이어 턴”만 의미
- StatusMessage (string)        ← 교육용/디버깅용(선택)

> 주의: `GameState`에는 Deck/RemainingCards 같은 필드를 넣지 않습니다.

### 5.3 Score 업데이트 규칙(명확히)
- Join/Leave로 Players가 바뀌거나,
- Hit로 카드가 추가되거나,
- StartRound로 카드가 배분되거나,
- 딜러 자동 진행으로 카드가 추가될 때마다,
  - 해당 플레이어의 Score/IsBlackjack/IsBusted를 **서버가 즉시 갱신**합니다.
- Score 계산은 Core의 `RankExtension.ToValue()`를 사용하며 Ace는 항상 1점입니다.

---

## 6) SignalR 계약(고정)

### 6.1 Hub 메서드(클라 → 서버)
- `Join(string displayName)`
- `Leave()`
- `StartRound()`  // 딜러만 가능
- `Hit()`
- `Stand()`

### 6.2 서버 이벤트(서버 → 클라)
- `StateChanged(GameState state)`  // 전원 브로드캐스트
- `Error(string code, string message)` // 요청자에게만

### 6.3 이벤트 전송 정책(단순화)
- 모든 명령 처리 후 `StateChanged`를 1회 전송
- 딜러 자동 진행은 내부적으로 여러 번 Hit 될 수 있으나, MVP에서는:
  - 자동 진행이 끝난 뒤 **최종 상태 1회만 전송**(가장 단순)
  - (추후 확장 시 카드 1장마다 전송 가능)

---

## 7) Backend 구현 설계(단순 + 쓰레드 세이프)

### 7.1 핵심: Hub는 enqueue만
- Hub는 상태를 직접 수정하지 않습니다.
- Hub는 요청을 “명령”으로 만들어 **큐에 넣기만** 합니다.

### 7.2 권장 구성(최소 파일)
- `Hubs/GameHub.cs`
- `Services/GameRoomService.cs`  ← 상태 + 명령 큐 소비 루프(핵심)
- `Services/ConnectionRegistry.cs` ← connectionId ↔ playerId
- `Program.cs`

### 7.3 쓰레드 세이프 구현 방식(고정)
- `System.Threading.Channels` 사용 (Backend .NET 8)
- `Channel<GameCommand>`를 만들고,
  - Hub는 `WriteAsync`로 enqueue
  - GameRoomService는 **단일 소비 루프**에서만 상태 변경

> 교육용 주석 예시:  
> `// 요청은 큐에 줄을 세우고, 상태는 한 곳에서만 바꿔서 안전합니다.`

### 7.4 Deck(Backend) 구현 규칙
- Backend의 기존 `Deck.Cards`(52장 생성)를 이용해 라운드마다 다음을 수행:
  1) 52장 생성
  2) 이를 4번 반복하여 208장 리스트 생성
  3) 셔플(Fisher-Yates)
  4) Draw 시 리스트 끝에서 Pop(또는 index 사용)

- Deck/남은카드는 **GameRoomService 내부 필드**로만 둡니다.
  - `private List<Card> _shoe;` 같은 형태로 유지

### 7.5 명령 모델(Backend 내부용)
- DTO 변환을 없앴으므로, 명령은 Backend 내부에서만 단순하게 둡니다.
- 예: `GameCommandType`(Join/Leave/StartRound/Hit/Stand)
- `GameCommand` 필드(권장):
  - Type
  - ConnectionId
  - PlayerId(Join 전에는 없을 수 있음)
  - DisplayName(Join에서만)

> 명령 타입은 Core에 두어도 되고 Backend에만 두어도 됩니다.  
> 7시간 MVP에서는 Backend에만 두는 편이 더 단순합니다.

---

## 8) 유효성 검사(에러 코드 포함)

### 8.1 공통 에러 코드(권장)
- `GAME_IN_PROGRESS` : InRound 중 Join 불가
- `NOT_JOINED` : Join하지 않은 상태에서 행동 시도
- `NOT_DEALER` : 딜러만 가능한 StartRound 시도
- `DEALER_IS_AUTO` : 딜러는 자동 진행이라 Hit/Stand 불가
- `NOT_YOUR_TURN` : 내 턴이 아닌데 Hit/Stand
- `GAME_NOT_INROUND` : InRound가 아닌데 Hit/Stand
- `ALREADY_DONE` : 이미 Stand/Bust인데 Hit/Stand
- `INSUFFICIENT_PLAYERS` : StartRound 최소 인원 부족(딜러 포함 2명 미만)

### 8.2 StartRound 유효성
- 딜러만 가능
- 참가 인원(딜러 포함) 2명 이상 권장
- Phase가 InRound면 StartRound 불가

### 8.3 Hit/Stand 유효성
- Phase == InRound 에서만 가능
- 현재 턴 플레이어만 가능
- 딜러는 무조건 `DEALER_IS_AUTO`
- 이미 Stand/Bust면 `ALREADY_DONE`

---

## 9) Client(.NET Standard 2.0) 구현 지침(프론트가 쉽게)

### 9.1 목표
Frontend가 Hub 세부를 몰라도 되도록 `BlackjackClient` 하나로 감쌉니다.

### 9.2 필수 API (Task 기반)
- `Task ConnectAsync(string url)`
- `Task JoinAsync(string displayName)`
- `Task LeaveAsync()`
- `Task StartRoundAsync()`
- `Task HitAsync()`
- `Task StandAsync()`

### 9.3 이벤트
- `event Action<GameState> StateChanged;`
- `event Action<string, string> Error;`

### 9.4 재접속 정책(복구 없음)
- 끊겼다가 다시 연결하더라도 상태 복구 로직은 구현하지 않습니다.
- 사용자가 다시 Join해야 합니다.

---

## 10) 완료 기준(Acceptance Criteria)

다음이 모두 되면 MVP 구현 완료입니다.

1) 최소 2명이 Join 가능(딜러 + 플레이어)
2) 첫 Join 사용자가 딜러로 지정됨
3) 딜러가 StartRound 호출 → 모두 2장씩 받음
4) 일반 플레이어가 자신의 턴에 Hit/Stand 가능
5) 플레이어 턴 종료 후 딜러 자동 진행(<17 Hit 반복, ≥17 Stand)
6) 라운드 Ended 상태가 되고, 플레이어별 Outcome이 채워짐
7) InRound 중 Join 시 `GAME_IN_PROGRESS` 에러
8) 딜러 Hit/Stand 시 `DEALER_IS_AUTO` 에러
9) Disconnect는 즉시 Leave처럼 처리
10) 동시 요청에도 상태가 꼬이지 않음(큐 직렬 처리)

---

## 11) 코드 스타일(교육용)
- 복잡한 패턴 금지(과한 제네릭/리플렉션/복잡한 아키텍처 금지)
- 메서드는 짧게(가능하면 30줄 이하)
- 주석은 한국어로 핵심만 짧게
- `record`/`init` 남발 금지(호환성과 설명을 위해 단순 POCO 선호)
- “왜 이렇게 하는지” 한 줄 설명 주석을 권장

---

## 12) 구현 순서(권장)
1) **Core**: GamePhase, RoundOutcome, PlayerState, GameState 추가 + 점수 갱신 헬퍼
2) **Backend**: GameRoomService(상태+채널 소비 루프) + Deck(4덱/셔플/드로우)
3) **Backend**: GameHub(Enqueue만) + ConnectionRegistry + Program DI
4) **Client**: BlackjackClient(연결 + 메서드 호출 + 이벤트 바인딩)
5) 최소 시나리오로 수동 테스트(2명 Join → StartRound → Hit/Stand → Ended)

---
