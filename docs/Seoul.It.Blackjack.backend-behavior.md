# Seoul.It.Blackjack Backend 동작 상세 문서

## 1. 문서 목적
- 이 문서는 `Seoul.It.Blackjack.Backend`가 실제 런타임에서 어떻게 동작하는지 코드 기준으로 상세 설명한다.
- 설계 의도 요약이 아니라, 현재 구현된 실행 흐름을 기준으로 작성한다.
- 대상 독자는 강사/개발자이며, 수업 설명 시 참고 가능한 수준으로 세밀하게 정리한다.

## 2. 기준 코드 범위
- `src/Seoul.It.Blackjack.Backend/*`
- `src/Seoul.It.Blackjack.Core/Contracts/*`
- `src/Seoul.It.Blackjack.Backend.Tests/*` (동작 검증 근거)

## 3. 백엔드 전체 구조

### 3.1 런타임 엔트리 포인트
- 파일: `src/Seoul.It.Blackjack.Backend/Program.cs`
- 핵심 동작:
1. ASP.NET Core `WebApplication` 생성
2. SignalR 등록
3. Swagger 등록(Development 환경)
4. 옵션/서비스 DI 등록
5. Hub 엔드포인트 `/blackjack` 매핑

### 3.2 DI 등록 구성
- 파일: `src/Seoul.It.Blackjack.Backend/Program.cs`
- 등록 목록:
1. `ConnectionRegistry` (`Singleton`)
2. `IGameRuleValidator -> GameRuleValidator` (`Singleton`)
3. `IRoundEngine -> RoundEngine` (`Singleton`)
4. `IGameStateSnapshotFactory -> GameStateSnapshotFactory` (`Singleton`)
5. `IGameCommandProcessor -> ChannelGameCommandProcessor` (`Singleton`)
6. `IGameRoomService -> GameRoomService` (`Singleton`)

### 3.3 옵션 바인딩
- 파일: `src/Seoul.It.Blackjack.Backend/Extensions/ServiceCollectionExtensions.cs`
- 바인딩:
1. `DealerOptions` <- `Dealer` 섹션
2. `GameRuleOptions` <- `GameRules` 섹션

현재 기본 `appsettings.json`에는 `Dealer.Key`만 명시되어 있고, `GameRules`는 미지정이므로 기본값을 사용한다.

## 4. 외부 계약 (SignalR)

### 4.1 서버가 받는 메서드 (`IBlackjackServer`)
- `Join(string name, string? dealerKey)`
- `Leave()`
- `StartRound()`
- `Hit()`
- `Stand()`

### 4.2 클라이언트가 받는 메서드 (`IBlackjackClient`)
- `OnStateChanged(GameState state)`
- `OnError(string code, string message)`

### 4.3 Hub 엔드포인트
- 파일: `src/Seoul.It.Blackjack.Backend/Hubs/GameSessionHub.cs`
- 경로: `GameSessionHub.Endpoint = "/blackjack"`

## 5. 상태 모델 정의

### 5.1 `GameState`
- `Phase`: `Idle` 또는 `InRound`
- `Players`: 전체 참가자 상태 리스트
- `DealerPlayerId`: 딜러 플레이어 ID
- `CurrentTurnPlayerId`: 현재 턴 플레이어 ID(없으면 빈 문자열)
- `StatusMessage`: 최근 상태 메시지

### 5.2 `PlayerState`
- `PlayerId`: 현재 구현에서는 연결 ID를 그대로 사용
- `Name`
- `IsDealer`
- `Cards`
- `Score`
- `TurnState`: `Playing`, `Standing`, `Busted`
- `Outcome`: `None`, `Win`, `Lose`, `Tie`

### 5.3 점수 규칙
- 파일: `src/Seoul.It.Blackjack.Core/Domain/Rank.cs`
- Ace = 1
- 2~10 = 숫자값
- J/Q/K = 10

## 6. 핵심 컴포넌트 책임

### 6.1 `GameSessionHub`
- 네트워크 입출력 담당
- 도메인 규칙 판단 직접 수행하지 않음
- 예외를 클라이언트 오류 이벤트로 변환
- 성공 결과를 전체 브로드캐스트

### 6.2 `GameRoomService`
- 서버 게임 상태의 단일 소유자
- 참가자 목록, 페이즈, 턴, 딜러, 상태 메시지 보유
- 각 명령 핸들러(`Join/Leave/StartRound/Hit/Stand`) 수행
- 내부적으로 큐 처리기(`IGameCommandProcessor`)에 위임하여 직렬 처리 보장

### 6.3 `ChannelGameCommandProcessor`
- `Channel<QueueItem>` 기반 큐
- 설정: `SingleReader = true`, `SingleWriter = false`
- `EnqueueAsync`로 들어온 명령을 단일 소비 루프에서 순차 실행

### 6.4 `GameRuleValidator`
- 이름 정규화/검증
- 참가 여부 검증
- 라운드 시작 권한 검증
- 플레이어 액션(Hit/Stand) 검증

### 6.5 `RoundEngine`
- 라운드 시작 배분
- Hit/Stand 처리
- 딜러 자동 진행
- 결과 계산 및 라운드 종료 판정

### 6.6 `GameStateSnapshotFactory`
- 내부 상태를 외부 전송용 `GameState`로 깊은 복사
- 카드도 새 `Card` 인스턴스로 복사

### 6.7 `ConnectionRegistry`
- 연결 ID 등록/해제/조회
- `lock` 기반 동기화

### 6.8 `Shoe`/`Deck`
- `Deck`: 한 벌(52장) 카드 시퀀스
- `Shoe`: `deckCount`만큼 합친 카드 스택, 셔플 후 `TryDraw` 제공

## 7. 내부 상태 필드 (GameRoomService)
- `_players`: 참가자 목록
- `_shoe`: 현재 라운드 카드 더미
- `_phase`: 현재 게임 페이즈 (`Idle` 기본)
- `_dealerPlayerId`: 현재 딜러 ID
- `_currentTurnPlayerId`: 현재 턴 ID
- `_statusMessage`: 최근 메시지

초기값:
- `_phase = Idle`
- `_dealerPlayerId = ""`
- `_currentTurnPlayerId = ""`
- `_statusMessage = ""`
- `_players = []`

## 8. 공통 명령 처리 파이프라인

모든 Hub 요청은 아래 공통 흐름을 따른다.

1. Hub 메서드가 호출된다.
2. Hub가 `IGameRoomService`의 비동기 메서드를 호출한다.
3. `GameRoomService`는 `GameCommand`를 생성한다.
4. `GameRoomService`는 `IGameCommandProcessor.EnqueueAsync(...)` 호출로 핸들러를 큐에 넣는다.
5. `ChannelGameCommandProcessor`의 단일 리더 루프가 핸들러를 실행한다.
6. 핸들러 결과로 `GameOperationResult`를 반환한다.
7. Hub가 결과를 브로드캐스트 정책에 맞게 전송한다.
8. 예외가 발생하면 Hub가 `GameRoomException`을 잡아 Caller에게만 `OnError`를 보낸다.

핵심 포인트:
- 동시 요청이 들어와도 상태 변경은 단일 루프에서 직렬 처리된다.
- 상태 변경 직후에는 항상 스냅샷(`GameState`)을 만들어 전송한다.

## 9. 브로드캐스트 정책 상세

### 9.1 성공 결과
- `GameOperationResult.ShouldPublishState == true`이면 `Clients.All.OnStateChanged(result.State)` 전송

### 9.2 성공 + 공지(Notice) 결과
- `result.Notice != null`이면 먼저 `Clients.All.OnError(code, message)` 전송
- 이후 `Clients.All.OnStateChanged(state)` 전송
- 대표 사례:
1. 딜러 퇴장 시 `GAME_TERMINATED`
2. 카드 부족 종료 시 `SHOE_EMPTY`

### 9.3 요청자 오류
- `GameRoomException` 발생 시 `Clients.Caller.OnError(code, message)`만 전송
- 상태 브로드캐스트 없음

### 9.4 무시되는 결과
- `CreateSilentResult()`는 `ShouldPublishState = false`
- 예: 참가하지 않은 연결의 `Leave/Disconnect`

## 10. Join 동작 상세 (`HandleJoin`)

### 10.1 검증 순서
1. 현재 페이즈가 `Idle`인지 확인
- 아니면 `GAME_IN_PROGRESS`
2. 연결이 이미 등록되어 있는지 확인
- 이미 등록되어 있으면 `ALREADY_JOINED`
3. 이름 정규화/길이 검증
- `Trim()` 후 길이 범위 검사
- 범위 밖이면 `INVALID_NAME`
4. 딜러 키 판정
- `dealerKey == DealerOptions.Key`면 딜러 요청으로 간주
5. 딜러 중복 검사
- 요청자가 딜러 요청이고 기존 딜러가 있으면 `DEALER_ALREADY_EXISTS`

### 10.2 상태 변경
- 새 `PlayerState` 생성 후 `_players` 추가
- `_connections.Add(connectionId, playerId)`
- 딜러면 `_dealerPlayerId` 설정
- `_statusMessage` 설정
1. 딜러: `"딜러가 참가했습니다."`
2. 일반: `"플레이어가 참가했습니다."`

### 10.3 결과
- 전체 상태 브로드캐스트 (`OnStateChanged`)

## 11. Leave/Disconnect 동작 상세 (`HandleLeave`)

`Leave`와 `Disconnect`는 같은 로직을 공유하며, 상태 메시지 일부만 다르다.

### 11.1 공통 처리 시작
1. `_connections.TryRemove(connectionId)` 시도
2. 실패하면 `CreateSilentResult()` 반환(브로드캐스트 없음)
3. `_players`에서 해당 플레이어 검색
4. 없으면 `CreateSilentResult()` 반환
5. 있으면 `_players.Remove(...)`

### 11.2 퇴장자가 딜러인 경우
1. `_players.Clear()`
2. `_connections.Clear()`
3. `ResetToIdle(clearDealer: true, clearCurrentTurn: true)`
4. `_statusMessage = "딜러 퇴장으로 게임이 종료되었습니다."`
5. `GameNotice("GAME_TERMINATED", "딜러가 퇴장하여 게임이 종료되었습니다.")` 포함 결과 반환

결과 전송 순서:
1. `OnError(GAME_TERMINATED)` 전체
2. `OnStateChanged(초기화 상태)` 전체

### 11.3 퇴장자가 일반 플레이어인 경우
- `InRound`이고 퇴장자가 현재 턴이면 다음 턴 재계산
- `InRound`에서 비딜러가 0명이 되면 즉시 Idle 종료
- `InRound`에서 더 이상 행동 가능한 비딜러가 없으면 `CompleteRound` 수행
- 그 외에는 상태 메시지만 갱신

메시지:
- Disconnect: `"플레이어 연결이 끊어졌습니다."`
- Leave: `"플레이어가 퇴장했습니다."`

## 12. StartRound 동작 상세 (`HandleStartRound`)

### 12.1 검증 (`EnsureCanStartRound`)
1. 참가 상태 확인 (`NOT_JOINED`)
2. 현재 `Idle` 확인 (`GAME_IN_PROGRESS`)
3. 호출자가 딜러인지 확인 (`NOT_DEALER`)
4. 인원수 확인 (`INSUFFICIENT_PLAYERS`)

### 12.2 라운드 시작
- `RoundEngine.StartRound(players, deckCount, dealerStandScore)` 호출
- 반환된 `RoundResolution` 적용
1. `Phase`
2. `CurrentTurnPlayerId`
3. `StatusMessage`
4. 필요 시 `Shoe`

### 12.3 초기 배분 규칙
- 참가자 전원 카드 2장 배분
- `Cards/Score/TurnState/Outcome` 라운드 초기화
- 행동 가능한 일반 플레이어가 있으면 `InRound`
- 없으면 즉시 `CompleteRound` 호출 후 Idle 종료

## 13. Hit 동작 상세 (`HandleHit`)

### 13.1 검증 (`ValidatePlayerAction`)
1. 참가 상태 (`NOT_JOINED`)
2. 게임 진행 상태 (`GAME_NOT_INROUND`)
3. 호출자 플레이어 조회 (`NOT_JOINED`)
4. 딜러 수동 행동 금지 (`DEALER_IS_AUTO`)
5. 턴 일치 확인 (`NOT_YOUR_TURN`)
6. 이미 종료된 플레이어 행동 금지 (`ALREADY_DONE`)

### 13.2 카드 드로우 및 턴 처리
- 현재 플레이어에게 1장 드로우
- 점수 재계산
- `Score > 21`이면 `Busted`
- `Score == 21`이면 `Standing` (즉시 행동 종료)
- 플레이어가 아직 `Playing`이면 턴 유지, 아니면 다음 플레이어로 턴 이동

### 13.3 종료 분기
- 행동 가능한 비딜러가 남아 있으면 `InRound` 유지
- 없으면 `CompleteRound` 호출

### 13.4 카드 부족 분기
- Hit 중 카드 부족이면 `EndRoundByShoeEmpty()`
- 결과: Idle + `SHOE_EMPTY` Notice

## 14. Stand 동작 상세 (`HandleStand`)

### 14.1 검증
- Hit와 동일한 `ValidatePlayerAction` 적용

### 14.2 상태 변경
- 현재 플레이어 `TurnState = Standing`
- 다음 플레이어 계산

### 14.3 종료 분기
- 행동 가능한 비딜러가 남아 있으면 `InRound` 유지
- 없으면 `CompleteRound`

## 15. 라운드 엔진 상세 (`RoundEngine`)

### 15.1 `StartRound`
1. 새 `Shoe(deckCount)` 생성
2. 전 플레이어 라운드 필드 초기화
- `Cards.Clear()`
- `Score = 0`
- `TurnState = Playing`
- `Outcome = None`
3. 전원 2장 배분
4. 첫 턴 계산 (`ResolveNextTurnPlayerId`)
5. 상태 메시지: `"라운드가 시작되었습니다."`
6. 행동 가능한 비딜러가 없으면 즉시 `CompleteRound`

### 15.2 `HandleHit`
- 메시지: `"{player.Name} 님이 Hit 했습니다."`
- 다음 턴 계산 로직 포함

### 15.3 `HandleStand`
- 메시지: `"{player.Name} 님이 Stand 했습니다."`
- 즉시 다음 턴 계산

### 15.4 `CompleteRound`
1. 딜러 조회 (`FindDealer`)
2. 딜러 자동 진행
- `Score < dealerStandScore` 이고 `Playing`이면 계속 Hit
3. 딜러가 아직 `Playing`이면 최종적으로 `Standing` 처리
4. 일반 플레이어 Outcome 계산
- 플레이어 `Busted`면 무조건 `Lose`
- 딜러 `Busted`면 플레이어 `Win`
- 그 외 점수 비교로 `Win/Lose/Tie`
5. 최종 상태
- `Phase = Idle`
- `CurrentTurnPlayerId = ""`
- `StatusMessage = "라운드가 종료되었습니다."`

### 15.5 `RecalculatePlayerState`
- 점수 합산 후 상태 변경:
1. `Score > 21` -> `Busted`
2. `Score == 21` -> `Standing`
3. 그 외는 기존 `Playing` 유지

주의:
- 현재 구현에서 `Score == 21`은 “플레이어 개인 행동 종료” 의미다.
- 즉시 라운드 전체 종료를 의미하지 않는다.

### 15.6 카드 부족 종료 (`EndRoundByShoeEmpty`)
- 반환 내용:
1. `Phase = Idle`
2. `CurrentTurnPlayerId = ""`
3. `StatusMessage = "카드가 부족해 라운드를 종료했습니다."`
4. `Notice = (SHOE_EMPTY, 동일 메시지)`

## 16. 상태 스냅샷 생성 방식

파일: `src/Seoul.It.Blackjack.Backend/Services/State/GameStateSnapshotFactory.cs`

핵심:
1. 내부 `_players`를 그대로 노출하지 않음
2. `PlayerState`를 새 객체로 복사
3. 카드 리스트도 새 리스트/새 카드 인스턴스로 복사

효과:
- 클라이언트가 수신한 상태를 변경해도 서버 내부 원본 상태는 오염되지 않는다.

## 17. 검증/예외 체계

### 17.1 예외 계층
- `GameRoomException` (base)
- `GameAuthorizationException`
- `GameRuleException`
- `GameValidationException`

### 17.2 Hub의 예외 처리
- `ExecuteAsync`에서 `GameRoomException`만 잡아서 Caller 오류로 전송
- `OnDisconnectedAsync`는 disconnect 처리 중 `GameRoomException` 발생 시 무시

## 18. 에러 코드별 발생 조건

### 18.1 규칙/권한/검증 오류
1. `GAME_IN_PROGRESS`
- InRound 상태에서 Join
- Idle이 아닌 상태에서 StartRound

2. `NOT_JOINED`
- 참가하지 않은 연결이 StartRound/Hit/Stand 요청
- 플레이어 조회 실패

3. `NOT_DEALER`
- 딜러가 아닌 연결이 StartRound 요청
- 딜러가 존재하지 않는 라운드 완료 계산 시도

4. `DEALER_IS_AUTO`
- 딜러가 Hit/Stand 직접 호출

5. `DEALER_ALREADY_EXISTS`
- 딜러가 이미 있는데 dealerKey로 딜러 Join 시도

6. `INVALID_NAME`
- Trim 후 길이 범위(기본 1~20) 위반

7. `ALREADY_JOINED`
- 같은 연결에서 Join 재호출

8. `NOT_YOUR_TURN`
- 현재 턴이 아닌 플레이어 Hit/Stand

9. `GAME_NOT_INROUND`
- 라운드 진행 중이 아닐 때 Hit/Stand

10. `ALREADY_DONE`
- 이미 Standing/Busted 상태 플레이어가 행동

11. `INSUFFICIENT_PLAYERS`
- 최소 인원 미충족 StartRound

### 18.2 시스템 공지(브로드캐스트 Notice)
12. `GAME_TERMINATED`
- 딜러 Leave/Disconnect 시 전체 공지

13. `SHOE_EMPTY`
- 라운드 진행 중 카드 부족 종료 시 전체 공지

## 19. 페이즈 및 턴 상태 전이

### 19.1 게임 페이즈
- `Idle -> InRound`: 딜러의 StartRound 성공
- `InRound -> Idle`: 라운드 완료, 카드 부족 종료, 또는 플레이어 소멸 종료
- `Idle 유지`: Join/Leave 등 대기 상태 변경

### 19.2 플레이어 턴 상태
- 시작 시 모두 `Playing`
- Hit로 `Score == 21`이면 `Standing`
- Hit로 `Score > 21`이면 `Busted`
- Stand 호출 시 `Standing`
- 한 라운드에서 `Standing/Busted -> Playing` 역전 없음

## 20. 딜러 퇴장 시나리오 (중요)

### 20.1 상태 초기화 범위
- 플레이어 목록 전체 삭제
- 연결 매핑 전체 삭제
- 페이즈 Idle
- 딜러/현재턴 ID 비움

### 20.2 네트워크 전송 순서
1. `OnError("GAME_TERMINATED", ... )` to `Clients.All`
2. `OnStateChanged(초기화된 GameState)` to `Clients.All`

### 20.3 이후 행동
- 기존 연결은 남아 있어도 서버 참가 상태는 제거됨
- 다시 플레이하려면 각 클라이언트가 `Join`을 재호출해야 함

## 21. 동시성 처리 방식

### 21.1 직렬화 보장 위치
- `ChannelGameCommandProcessor.ProcessLoopAsync`의 단일 리더

### 21.2 의미
- 동시에 여러 클라이언트가 요청해도 상태 변경은 한 번에 하나씩 처리됨
- “동시에 Hit” 같은 경우도 순서대로 처리되어 레이스 컨디션 완화

### 21.3 테스트 근거
- `ConcurrencyTests.ConcurrentHitRequests_AreSerializedAndStayConsistent`

## 22. 구성 옵션 상세

### 22.1 `DealerOptions`
- 섹션: `Dealer`
- 키: `Key`
- 역할: 딜러 인증 문자열

### 22.2 `GameRuleOptions` 기본값
- `DeckCount = 4`
- `DealerStandScore = 17`
- `MinPlayersToStart = 2`
- `MinNameLength = 1`
- `MaxNameLength = 20`

### 22.3 운영 시 오버라이드
- `GameRules` 섹션을 추가하면 기본값 대체 가능
- 현재 `appsettings.json`에는 `GameRules` 섹션이 없어 기본값 사용

## 23. 테스트로 확인된 동작 요약

다음 항목은 `Backend.Tests` 통합 테스트로 검증된다.

1. 딜러 키 일치/불일치 Join 역할 판정
2. 이름 검증 및 재Join 거부
3. 딜러 중복 금지
4. 비딜러 StartRound 금지
5. InRound Join 금지
6. 턴 위반/딜러 수동행동 금지
7. Hit 성공 시 전체 브로드캐스트
8. Stand 후 Idle 전환 및 Outcome 계산
9. 다음 라운드 시작 시 라운드 필드 초기화
10. 딜러 종료 공지 + 상태 초기화 순서/횟수 보장
11. 동시 요청 직렬 처리
12. 기본 최소 인원 규칙(2명) 적용

## 24. 구현상 주의할 점 (리팩터링 시)

1. Hub는 반드시 전송 계층에 머물러야 하며, 상태 변경은 `GameRoomService` 내부에서만 이루어져야 한다.
2. `GameOperationResult.Notice` 전송 순서(Error 먼저, State 나중)를 바꾸면 기존 테스트가 깨질 수 있다.
3. `CreateSilentResult()` 경로를 제거하면 불필요한 상태 브로드캐스트가 발생할 수 있다.
4. `GameStateSnapshotFactory`의 깊은 복사를 얕은 복사로 바꾸면 외부 변경에 의해 상태 오염 위험이 생긴다.
5. 큐 단일 소비 구조를 깨면 동시성 버그가 재발할 수 있다.

## 25. 수업 설명용 권장 순서 (백엔드 동작 이해)

1. `IBlackjackServer/IBlackjackClient` 계약을 먼저 설명한다.
2. Hub는 네트워크 어댑터, Service는 게임 룸이라는 책임 분리를 설명한다.
3. `Channel` 단일 소비 루프로 직렬 처리한다는 핵심을 설명한다.
4. `Join -> StartRound -> Hit/Stand -> CompleteRound -> Idle` 상태 흐름을 한 번에 그림으로 설명한다.
5. 마지막으로 딜러 퇴장(`GAME_TERMINATED`)과 브로드캐스트 순서를 강조한다.

## 26. 요약
- 현재 백엔드는 “단일 룸 + 단일 직렬 명령 처리 + 전체 상태 브로드캐스트” 구조다.
- 규칙 위반은 요청자에게만 오류를 보내고, 정상 상태 변화는 전체 전파한다.
- 딜러 퇴장은 게임 전체 종료 이벤트로 취급하며, 공지 후 상태를 초기화한다.
- 통합 테스트는 이 동작을 회귀 기준으로 고정하고 있다.
