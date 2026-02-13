# Seoul.It.Blackjack 설계 문서

## 1. 문서 목적
- 본 문서는 MVP를 완성하기 위한 최종 구현 기준 문서이다.
- 현재 코드는 전면 수정 가능하며, 구현은 반드시 아래 순서를 따른다.
1. `Backend + Core` 완성
2. `Client + Frontend` 구현
- 이 문서는 이후 세부 구현 계획과 작업 승인 기준 문서로 사용한다.

## 2. 개발 순서(고정)
1. 1단계: `Seoul.It.Blackjack.Core`, `Seoul.It.Blackjack.Backend` 완성
2. 2단계: `Seoul.It.Blackjack.Client`, `Seoul.It.Blackjack.Frontend` 구현

## 3. 교육/시간 제약(고정)
- 대상은 고등학생이며, 설명을 들으며 따라 타이핑해도 완성 가능한 난이도를 유지한다.
- 목표 시간은 `Core + Backend + Client` 기준 `5~6시간` 이내 완성이다.
- 따라서 복잡한 기능은 추가하지 않는다.
- 구현 판단은 이 설계 문서를 기준으로 한다.
- 코드 변경은 가능한 한 한 번의 작업(변경 묶음) 기준 `300줄` 이내로 유지한다.
- 수업 실습은 생산 코드 중심으로 진행한다. 테스트 프로젝트는 강사/개발자 검증용으로 분리한다.

## 4. 공통 원칙
- MVP 범위만 구현한다.
- 상태 전송용 DTO를 추가하지 않는다. Core 계약 모델을 그대로 전송한다.
- Hub는 상태를 직접 변경하지 않는다. 모든 상태 변경은 단일 큐 소비 루프에서만 수행한다.
- Queue(`Channel`) 기반 직렬 처리 구조는 유지한다. 단, 큐 인프라는 `GameRoomService` 외부 컴포넌트로 분리할 수 있다.
- 잘못된 요청은 요청자에게만 `Error(code, message)`를 보낸다.
- 상태가 변경되면 `StateChanged(GameState)`를 전체 브로드캐스트한다.
- 특히 `Hit/Stand` 성공 시에는 반드시 `Clients.All`로 상태를 전송한다.
- Disconnect는 즉시 Leave와 동일하게 처리한다(복구 없음).

## 5. 1단계 설계: Core + Backend 완성

### 5.1 목표
- 한 라운드가 끝까지 진행되는 서버 권위(Server-authoritative) 블랙잭 게임을 완성한다.
- 1단계 완료 시, Client/Frontend 없이도 SignalR 계약 기준으로 동작이 검증되어야 한다.

### 5.2 Core 설계

#### 5.2.1 타겟 프레임워크
- `Seoul.It.Blackjack.Core`: `netstandard2.0`

#### 5.2.2 도메인
- `Card(Suit, Rank)` 유지
- `Suit`: Clubs, Diamonds, Hearts, Spades
- `Rank`: Ace, Two..Ten, Jack, Queen, King
- 점수 규칙:
  - Ace = 1
  - J/Q/K = 10

#### 5.2.3 계약 모델(전송 모델)
- `GamePhase`: Idle, InRound
- `RoundOutcome`: None, Win, Lose, Tie
- `PlayerTurnState`: Playing, Standing, Busted
- `PlayerState`
  - PlayerId
  - Name
  - IsDealer
  - Cards
  - Score
  - TurnState
  - Outcome
- `GameState`
  - Phase
  - Players
  - DealerPlayerId
  - CurrentTurnPlayerId
  - StatusMessage

#### 5.2.4 SignalR 계약 인터페이스
- 서버 수신(`IBlackjackServer`)
  - `Join(string name, string? dealerKey)`
  - `Leave()`
  - `StartRound()`
  - `Hit()`
  - `Stand()`
- 클라이언트 수신(`IBlackjackClient`)
  - `StateChanged(GameState state)`
  - `Error(string code, string message)`

### 5.3 Backend 설계

#### 5.3.1 타겟 프레임워크
- `Seoul.It.Blackjack.Backend`: `net8.0`

#### 5.3.2 책임 분리
- `GameSessionHub`
  - 요청 검증 최소화
  - 명령 enqueue
  - 직접 상태 변경 금지
  - 네트워크 입출력 계층이며, 게임방 자체가 아니다.
- `GameRoomService`
  - 단일 상태 저장소
  - 단일 큐 소비 루프
  - 게임 규칙/턴/점수/결과 계산
  - 실제 게임방 역할을 담당한다.
- `ConnectionRegistry`
  - `connectionId <-> playerId` 매핑 관리

#### 5.3.3 상태 변경 구조(필수)
- `Channel<GameCommand>` 사용
- `SingleReader = true`, `SingleWriter = false`
- 모든 명령은 큐에 들어가고, 단일 소비 루프에서 순차 실행된다.
- 따라서 동시 요청 상황에서도 상태 경합을 제거한다.

#### 5.3.4 내부 게임 데이터
- 방은 1개만 존재한다.
- 덱은 상태 모델에 넣지 않는다.
- 라운드 시작 시:
1. 52장 카드 생성
2. 4덱(총 208장) 구성
3. 셔플(Fisher-Yates)
4. Draw는 내부 리스트에서 수행

#### 5.3.5 게임 규칙
- `name`은 `Trim()` 후 길이 `1~20`만 허용한다. 조건 불만족 시 `INVALID_NAME`.
- `name` 중복은 허용한다.
- 같은 연결에서 `Join`을 다시 호출하면 거부한다(`ALREADY_JOINED`).
- 딜러 선정은 `DealerOptions.Key` 기반으로 처리한다.
- `dealerKey`가 서버 키와 같고 현재 딜러가 없으면 딜러로 입장한다.
- 한 룸에는 딜러가 반드시 최대 1명만 존재한다.
- `dealerKey`가 서버 키와 같지만 이미 딜러가 있으면 Join을 거부한다(`DEALER_ALREADY_EXISTS`).
- `dealerKey`가 없거나 서버 키와 다르면 에러 없이 일반 플레이어로 입장한다.
- Join 가능 Phase: Idle
- InRound Join 시 `GAME_IN_PROGRESS`
- StartRound는 딜러만 가능하고, Phase가 Idle일 때만 가능하다.
- 라운드 시작 시 참가자 전원 2장 배분
- 일반 플레이어만 턴 진행(Join 순서)
- 플레이어는 Hit/Stand만 가능
- 플레이어 점수가 정확히 `21`이 되면 해당 플레이어는 즉시 행동 종료 상태가 된다(라운드 전체 종료 아님).
- 블랙잭 표시는 별도 `IsBlackjack` 필드 없이 `Score == 21`로 판단한다.
- Bust 또는 Stand 시 턴 종료
- 일반 플레이어 턴 종료 후 딜러 자동 진행
  - Score < 17: Hit 반복
  - Score >= 17: Stand
  - > 21: Bust
- 결과 계산 후 즉시 Idle로 전환한다.
- Idle 상태에서는 직전 라운드의 카드/Outcome을 유지하고, 다음 `StartRound`에서 라운드 관련 필드를 초기화한다.
- 카드 드로우 중 덱이 비면 `SHOE_EMPTY`로 라운드를 종료하고 Idle로 전환한다(자동 재셔플 없음).
- 모든 상태 변경 결과는 `Clients.All.StateChanged`로 전송한다.

#### 5.3.6 퇴장/끊김 규칙
- Leave와 Disconnect 동일 처리
- 딜러 퇴장 시:
  - 즉시 게임 종료로 처리한다.
  - 접속 중인 플레이어를 전원 강제 퇴장 처리한다.
  - Players 목록과 ConnectionRegistry 매핑을 모두 초기화한다.
  - 모든 접속자에게 `Error("GAME_TERMINATED", "딜러가 퇴장하여 게임이 종료되었습니다.")`를 1회 전송한다.
  - 초기화된 상태를 `StateChanged`로 1회 브로드캐스트한다.
  - 게임 상태는 Idle로 리셋되고, 이후 새로 Join해야 게임을 다시 시작할 수 있다.
- 일반 플레이어 퇴장 시:
  - 목록에서 제거
  - 현재 턴 플레이어가 퇴장한 경우 다음 턴으로 즉시 이동
  - 딜러만 남으면 즉시 Idle로 전환한다.

#### 5.3.7 에러 코드
- `GAME_IN_PROGRESS`
- `NOT_JOINED`
- `NOT_DEALER`
- `DEALER_IS_AUTO`
- `DEALER_ALREADY_EXISTS`
- `INVALID_NAME`
- `ALREADY_JOINED`
- `GAME_TERMINATED`
- `NOT_YOUR_TURN`
- `GAME_NOT_INROUND`
- `ALREADY_DONE`
- `INSUFFICIENT_PLAYERS`
- `SHOE_EMPTY`

#### 5.3.8 확장 대비 구조 원칙(단일룸 유지)
- MVP에서는 멀티룸을 구현하지 않는다.
- 하지만 구조는 `Hub != Room` 원칙을 유지한다.
- 현재는 `GameRoomService`를 단일 인스턴스로 두고, Hub는 이 인스턴스에 요청 전달만 수행한다.
- 추후 확장 과제로 멀티룸을 넣을 때는 Hub를 크게 바꾸지 않고 `RoomManager(roomId -> GameRoomService)`를 추가하는 방식으로 확장한다.
- 이 원칙은 수업 난이도를 낮추면서도, 확장 가능한 설계 사고를 학습하는 목적을 가진다.

#### 5.3.9 예외 처리 원칙(커스텀 예외)
- 공통 베이스 커스텀 예외 클래스를 만든다. 예: `GameRoomException(string code, string message)`.
- 필요 시 목적별 파생 예외를 사용한다. 예: `ValidationException`, `GameRuleException`, `AuthorizationException`.
- Hub에서는 커스텀 예외의 `code/message`를 읽어 요청자에게 `Error(code, message)`를 전송한다.
- 예상 가능한 규칙 위반은 커스텀 예외로 처리하고, 예기치 못한 시스템 예외와 구분한다.

### 5.4 1단계 완료 기준
1. 2명 이상 Join 가능(딜러 + 플레이어)
2. `name`은 `Trim()` 후 `1~20`만 허용하고, 중복 이름은 허용
3. 같은 연결의 재 Join은 `ALREADY_JOINED`로 거부
4. `DealerOptions.Key`가 일치하는 사용자만 딜러 지정되고, 딜러는 1명만 유지
5. 딜러 StartRound 호출 시 전원 2장 배분
6. 플레이어 턴 Hit/Stand 동작
7. 점수 `21` 도달 시 해당 플레이어만 즉시 행동 종료(라운드 계속 진행)
8. Hit/Stand 결과가 모든 접속 클라이언트에 즉시 반영
9. 결과 계산 후 자동 Idle 전환
10. Idle에서 직전 결과 표시 유지, 다음 StartRound에서 초기화
11. 규칙 위반 요청은 요청자에게만 Error 전송
12. 딜러 Disconnect/Leave 시 `GAME_TERMINATED` 통지 + 전원 강제 퇴장 + 상태 초기화
13. 덱 소진 시 `SHOE_EMPTY`로 라운드 종료 후 Idle 전환
14. 동시 요청에서도 상태 무결성 유지

### 5.5 통합 테스트 전략(개발자 검증용)
- 별도 테스트 프로젝트를 생성해 서버 게임 로직을 검증한다.
- 테스트 프로젝트는 수업 진행 대상이 아니라, 개발자 검증용으로만 사용한다.
- 통합 테스트까지 포함한다(실제 Hub/SignalR 경로 검증).
- 가능한 많은 시나리오를 자동 테스트로 검증한다.
1. 딜러 키 일치/불일치 Join
2. 이름 검증(`Trim`, 길이 `1~20`, 중복 허용)
3. 같은 연결 재 Join 거부(`ALREADY_JOINED`)
4. 딜러 중복 Join 거부
5. InRound Join 거부
6. StartRound 권한 검증(딜러/비딜러)
7. Hit/Stand 턴 검증
8. 점수 21 즉시 행동 종료(라운드 지속) 검증
9. Hit/Stand 후 전원 브로드캐스트 검증
10. Bust 처리
11. 딜러 자동 진행(<17 Hit, >=17 Stand)
12. 승/패/무 결과 계산 및 Idle 전환 검증
13. Idle 상태 결과 유지 + 다음 StartRound 초기화 검증
14. 일반 Leave/Disconnect 처리
15. 딜러 Leave/Disconnect 시 `GAME_TERMINATED` + 전원 강제 퇴장 + 단일 상태 브로드캐스트 검증
16. 덱 소진 시 `SHOE_EMPTY` 처리 및 Idle 전환 검증
17. 동시 요청 직렬 처리

## 6. 2단계 설계: Client + Frontend 구현

### 6.1 목표
- 프론트엔드가 게임 규칙을 직접 구현하지 않도록, Client SDK가 Hub 통신을 캡슐화한다.
- Frontend는 상태 표시와 사용자 입력에 집중한다.

### 6.2 Client 설계

#### 6.2.1 타겟 프레임워크
- `Seoul.It.Blackjack.Client`: `netstandard2.0`

#### 6.2.2 역할
- SignalR 연결/재연결 관리(복구는 미지원)
- 서버 메서드 호출 API 제공
- 서버 이벤트를 로컬 이벤트로 전달

#### 6.2.3 API
- `ConnectAsync(string url)`
- `JoinAsync(string name, string? dealerKey = null)`
- `LeaveAsync()`
- `StartRoundAsync()`
- `HitAsync()`
- `StandAsync()`

#### 6.2.4 이벤트
- `StateChanged(GameState state)`
- `Error(string code, string message)`

### 6.3 Frontend 설계

#### 6.3.1 타겟 프레임워크
- `Seoul.It.Blackjack.Frontend`: `net8.0`(Blazor)

#### 6.3.2 화면 구성
- 접속/입장 영역
- 플레이어 목록 및 딜러 표시
- 현재 턴 표시
- 내 액션(Hit/Stand)
- 딜러 StartRound 버튼
- 상태 메시지/오류 메시지
- 라운드 종료 결과 표시

#### 6.3.3 상태 처리
- Frontend는 `GameState`를 그대로 렌더링한다.
- 점수/승패/턴 판정 로직은 Backend를 신뢰한다.

### 6.4 2단계 완료 기준
1. Frontend에서 Join/Leave 가능
2. 딜러 StartRound 가능
3. 플레이어 Hit/Stand 가능
4. 상태 갱신과 오류 메시지 UI 반영
5. 라운드 결과 표시

## 7. 구현 및 승인 절차
- 작업은 반드시 세부 계획 수립 후 승인받아 진행한다.
- 이번 문서 확정 후, 다음 작업은 1단계(`Core + Backend`) 세부 계획을 작성하고 승인받아 시작한다.
- 코드 변경량이 한 번의 작업 기준 `300줄`을 초과할 것으로 예상되면, 작업 시작 전에 반드시 사용자 동의를 먼저 받는다.
