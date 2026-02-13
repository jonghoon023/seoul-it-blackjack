# Seoul.It.Blackjack.Backend.Tests 설명 문서

## 1. 문서 목적
- 이 문서는 `src/Seoul.It.Blackjack.Backend.Tests`가 실제로 무엇을 테스트하는지 명확히 설명한다.
- 수업 대상(학생) 관점이 아니라, 강사/개발자 검증 관점에서 테스트 범위와 한계를 정리한다.

## 2. 테스트 프로젝트 성격
- 테스트 프레임워크: `MSTest`
- 실행 방식: `WebApplicationFactory<Program>` 기반 통합 테스트
- 통신 경로: 실제 SignalR Hub 엔드포인트(`/blackjack`)를 통해 검증
- 검증 대상: Hub + 서비스 + 상태 전송 흐름(블랙박스 관점)

참고 파일:
- `src/Seoul.It.Blackjack.Backend.Tests/Seoul.It.Blackjack.Backend.Tests.csproj`
- `src/Seoul.It.Blackjack.Backend.Tests/TestHostFactory.cs`
- `src/Seoul.It.Blackjack.Backend.Tests/SignalRTestClient.cs`
- `src/Seoul.It.Blackjack.Backend.Tests/TestWaiter.cs`

## 3. 테스트 인프라가 하는 일
1. `TestHostFactory`
- 백엔드 앱을 테스트 서버로 띄운다.
- 환경은 `Development`로 설정한다.

2. `SignalRTestClient`
- 테스트 코드에서 실제 클라이언트처럼 Hub에 접속한다.
- `Join/Leave/StartRound/Hit/Stand`를 실제 메서드명(`nameof(IBlackjackServer...)`)으로 호출한다.
- 서버가 보낸 `OnStateChanged`, `OnError`를 수신하여 `States`, `Errors`, `Events`에 기록한다.

3. `TestWaiter`
- 비동기 이벤트 기반 테스트에서 상태 도착을 기다린다.
- 기본 타임아웃은 3초이며, 조건 미충족 시 `TimeoutException`을 발생시킨다.

## 4. 현재 테스트 개수(실제 디스커버리 기준)
- 총 21개
- 확인 명령:
```bash
dotnet test src/Seoul.It.Blackjack.Backend.Tests/Seoul.It.Blackjack.Backend.Tests.csproj --no-build --list-tests -v minimal
```

## 5. 클래스별 실제 검증 내용

### 5.1 `JoinAndRoleTests` (5개)
1. `Join_WithDealerKey_AssignsDealerRole`
- 딜러 키가 맞으면 `IsDealer=true`로 입장되는지 검증

2. `Join_WithInvalidDealerKey_JoinsAsPlayer`
- 딜러 키가 틀리면 일반 플레이어로 입장되는지 검증

3. `Join_WithEmptyName_ReturnsInvalidNameError`
- 공백 이름 입력 시 `INVALID_NAME` 오류 반환 검증

4. `Join_WhenDealerAlreadyExists_ReturnsDealerAlreadyExistsError`
- 딜러가 이미 있을 때 딜러 재입장 시 `DEALER_ALREADY_EXISTS` 검증

5. `Join_WhenSameConnectionJoinsTwice_ReturnsAlreadyJoinedError`
- 같은 연결 재입장 시 `ALREADY_JOINED` 검증

### 5.2 `TurnRuleTests` (5개)
1. `StartRound_ByNonDealer_ReturnsNotDealer`
- 비딜러 시작 요청 시 `NOT_DEALER` 검증

2. `Join_InRound_ReturnsGameInProgress`
- 라운드 진행 중 입장 요청 시 `GAME_IN_PROGRESS` 검증

3. `Hit_OutOfTurn_ReturnsNotYourTurn`
- 자기 턴이 아닐 때 Hit 요청 시 `NOT_YOUR_TURN` 검증

4. `Hit_ByDealer_ReturnsDealerIsAuto`
- 딜러 수동 Hit 요청 시 `DEALER_IS_AUTO` 검증

5. `Hit_BroadcastsStateToAllClients`
- 플레이어 Hit 성공 시 딜러/행동자/옵저버 모두 상태 업데이트를 받는지 검증

### 5.3 `RoundCompletionTests` (2개)
1. `Stand_CompletesRound_AndMovesToIdle`
- Stand 후 라운드가 끝나고 `GamePhase.Idle`로 전환되는지 검증
- 현재 턴이 비워지고, 플레이어 Outcome이 `None`이 아니게 되는지 검증

2. `NextRound_ResetsRoundFields_ButKeepsIdleSnapshotBeforeStart`
- Idle 시 직전 라운드 카드/결과가 남아있는지 확인
- 다음 StartRound 시 카드/결과가 새 라운드 기준으로 초기화되는지 검증

### 5.4 `DealerTerminationTests` (5개)
1. `DealerLeave_BroadcastsGameTerminated_AndClearsState`
- 딜러 Leave 시 `GAME_TERMINATED` 전송 + 상태 초기화(Idle, Players=0) 검증

2. `DealerDisconnect_BroadcastsGameTerminated_AndClearsState`
- 딜러 Disconnect 시 동일 동작 검증

3. `DealerLeave_BroadcastsTerminationAndResetExactlyOnce_InOrder`
- 딜러 종료 시
  - `OnError(GAME_TERMINATED)` 1회
  - `OnStateChanged(초기화 상태)` 1회
  - 순서가 Error -> StateChanged 인지 검증

4. `PlayerLeave_RemovesOnlyThatPlayer`
- 일반 플레이어 Leave는 해당 플레이어만 제거되는지 검증

5. `PlayerDisconnect_RemovesOnlyThatPlayer`
- 일반 플레이어 Disconnect도 동일하게 처리되는지 검증

### 5.5 `ConcurrencyTests` (1개)
1. `ConcurrentHitRequests_AreSerializedAndStayConsistent`
- 두 플레이어가 거의 동시에 Hit 요청할 때 직렬 처리되는지 검증
- 한쪽은 상태 갱신, 다른 한쪽은 `NOT_YOUR_TURN`을 받는 시나리오를 통해 무결성 확인

### 5.6 `GameRuleOptionsDefaultTests` (1개)
1. `StartRound_WithOnlyDealer_UsesDefaultMinPlayerRule`
- 딜러만 있는 상태에서 StartRound 시 `INSUFFICIENT_PLAYERS` 검증
- 기본 옵션(최소 인원 규칙)이 실제로 적용되는지 확인

### 5.7 `ClientDiIntegrationTests` (2개)
1. `AddBlackjackClient_RegistersSingletonAndAppliesOptions`
- `AddBlackjackClient` 등록 후 `BlackjackClient` 해석 가능 여부 검증
- `BlackjackClientOptions.HubUrl` 값 반영 검증
- `Singleton` 수명(`GetRequiredService` 2회 해석 시 동일 인스턴스) 검증

2. `AddBlackjackClient_ConnectJoin_ReceivesStateChanged`
- DI로 해석한 `BlackjackClient`가 실제 Hub(`/blackjack`)에 연결 가능한지 검증
- Join 후 `StateChanged` 이벤트 수신 검증
- 테스트 서버 핸들러 주입 경로(`ConnectAsync` 오버로드)로 통합 경로를 검증

## 6. 이 테스트가 보장해주는 것
1. 입장/권한/턴 검증의 핵심 에러코드가 실제 Hub 경로에서 올바르게 반환된다.
2. 상태 변경이 SignalR 이벤트(`OnStateChanged`)로 브로드캐스트된다.
3. 딜러 종료 시 종료 알림과 상태 초기화 브로드캐스트 정책이 보장된다.
4. 동시 요청 상황에서도 단일 직렬 처리 의도가 깨지지 않는다.
5. Client SDK DI 등록 진입점(`AddBlackjackClient`)이 실제 서버 통신 경로와 함께 동작한다.

## 7. 현재 문서 기준 미검증 또는 약한 검증 영역
1. 모든 에러코드 13종을 각각 독립적으로 전부 검증하지는 않는다.
2. 덱 소진(`SHOE_EMPTY`) 시나리오는 현재 통합 테스트에 직접 포함되어 있지 않다.
3. 점수 21 도달 시 즉시 행동 종료(라운드 지속) 시나리오는 직접 검증 테스트가 없다.
4. 메시지 문자열 본문(한글 문구)까지 엄격 비교하지 않고, 코드 중심으로 검증한다.

## 8. 실행 명령
```bash
dotnet test src/Seoul.It.Blackjack.Backend.Tests/Seoul.It.Blackjack.Backend.Tests.csproj -m:1 -v minimal
```

## 9. 요약
- `Seoul.It.Blackjack.Backend.Tests`는 단위 테스트보다 "실제 Hub 통신 기반 통합 테스트"에 가깝다.
- 핵심 게임 흐름(입장, 권한, 턴, 라운드 종료, 딜러 종료, 동시성)과 Client DI 경로를 21개 시나리오로 회귀 검증한다.
- 따라서 백엔드 리팩터링 시 외부 동작 불변 여부를 판단하는 1차 안전망 역할을 한다.
