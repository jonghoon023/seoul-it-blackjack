# Seoul.It.Blackjack 전체 구현 수업 가이드 (완전판)

이 문서는 "학생이 처음부터 직접 타이핑해서 완성"을 목표로 작성한 전체 강의 스크립트입니다.

- 대상: C# 문법이 익숙하지 않은 고등학생(큐/스택 기초만 알고 있음)
- 목표: 이론 설명 + 실습 타이핑으로 프로젝트 전체 구조를 이해하고 재현
- 기준 코드: 현재 `src` 폴더의 실제 코드 전체
- 중요한 원칙: 코드 누락 없이 문서에 원문 포함

---

## 1. 수업 전체 목표와 운영 방식

### 1.1 최종 도달 목표
학생들이 다음을 직접 구현하고 설명할 수 있게 합니다.

1. 공용 계약(Core) 설계 이유를 설명한다.
2. 백엔드(Backend)에서 SignalR + 게임 규칙 + 큐 직렬 처리 구조를 구현한다.
3. 클라이언트(Client) 라이브러리로 서버 호출/이벤트 수신을 구현한다.
4. 프론트엔드(Frontend)에서 Entry/Table 2페이지 흐름을 구현한다.
5. 백엔드/프론트엔드 테스트 프로젝트의 역할을 이해하고 실행한다.

### 1.2 학생 난이도에 맞춘 설명 전략

1. 처음 30~40분은 문법보다 "왜 이 파일이 필요한지"를 먼저 설명한다.
2. 코드 한 줄 입력할 때마다 "문법 + 역할"을 같이 설명한다.
3. `async/await`, DI, 이벤트, 인터페이스는 실물 코드와 연결해서 설명한다.
4. 백엔드 동시성은 "여러 학생이 동시에 버튼을 누르는 상황"으로 비유한다.
5. 테스트는 "정답 검사기"로 설명하고, 학생 타이핑 범위/강사 검증 범위를 구분한다.

### 1.3 권장 수업 흐름(시간 예시)

1. 0:00~0:20: 솔루션 구조/블랙잭 규칙(단순화 버전) 설명
2. 0:20~1:20: Core 구현
3. 1:20~3:20: Backend 구현
4. 3:20~4:10: Client 구현
5. 4:10~5:30: Frontend 구현
6. 5:30~6:10: 테스트 실행 + 실패 사례 읽기
7. 6:10~6:30: 정리/질의

참고: 수업 목표가 "Core+Backend+Client 5~6시간"이라면 Frontend는 후반 확장 세션으로 분리 가능합니다.

---

## 2. 먼저 알려줄 핵심 이론 (학생용)

### 2.1 C# 기본 구조

1. `namespace`: 파일이 속한 폴더 개념의 논리적 그룹
2. `class`: 데이터와 동작을 묶는 설계도
3. `interface`: "이 기능이 있어야 한다"는 약속(구현은 다른 클래스에서)
4. `enum`: 정해진 선택지(예: `Idle`, `InRound`)
5. `property`: `get; set;` 형태로 데이터 접근

### 2.2 비동기

1. `Task`: "나중에 끝나는 작업"
2. `async/await`: 오래 걸리는 작업(네트워크 등)을 기다리면서 프로그램 멈춤을 줄임
3. SignalR 메서드/클라이언트 호출은 대부분 `Task` 기반

### 2.3 DI(의존성 주입)

1. `builder.Services.Add...`에 "객체 생성 규칙"을 등록
2. 필요한 클래스의 생성자에서 자동 주입
3. 장점: 테스트 쉬움, 결합도 감소

### 2.4 큐 직렬 처리(Channel)

1. 사용자 요청이 동시에 와도 한 줄로 세워 순서대로 처리
2. 레이스 컨디션(동시성 버그) 감소
3. 이 프로젝트의 핵심 안정화 포인트

### 2.5 이벤트

1. 서버 -> 클라이언트 상태 전달: `OnStateChanged`, `OnError`
2. 프론트 UI는 이벤트를 받아 화면 갱신

---

## 3. 처음부터 만들 때 프로젝트 생성/작성 순서

아래 순서는 "초보 학생이 이해를 유지하며 타이핑"하기 가장 좋은 순서입니다.

1. 솔루션/공통 설정
2. `Seoul.It.Blackjack.Core`
3. `Seoul.It.Blackjack.Backend`
4. `Seoul.It.Blackjack.Client`
5. `Seoul.It.Blackjack.Backend.Tests` (강사용 검증)
6. `Seoul.It.Blackjack.Frontend`
7. `Seoul.It.Blackjack.Frontend.Tests` (강사용 검증)

이유:

1. Core 계약이 먼저 있어야 Backend/Client/Frontend가 같은 언어를 씀
2. Backend를 먼저 완성하면 게임 규칙의 기준점이 생김
3. Client는 Backend 호출 래퍼 역할이라 Backend 뒤가 자연스러움
4. Frontend는 마지막에 붙여도 구조가 명확함

---

## 4. 프로젝트별 구현 순서 + 코드 역할 + 설명 포인트

## 4.1 공통/솔루션

### 4.1.1 `src/Seoul.It.Blackjack.sln`

- 역할: 모든 프로젝트를 한 묶음으로 관리
- 설명 포인트:
1. 솔루션은 프로젝트들을 담는 상위 컨테이너
2. `Debug/Release` 구성 개념

### 4.1.2 `src/Directory.Build.props`

- 역할: 전체 프로젝트 공통 빌드 규칙
- 설명 포인트:
1. 한 파일로 analyzers/경고 정책 공통 적용
2. CI 빌드와 로컬 빌드 차이

---

## 4.2 Core 프로젝트

핵심 설명 문장: "Core는 모두가 공유하는 약속(계약)과 공통 도메인 모델입니다."

### 파일 역할

1. `src/Seoul.It.Blackjack.Core/Seoul.It.Blackjack.Core.csproj`
- 역할: netstandard2.0 라이브러리 설정

2. `src/Seoul.It.Blackjack.Core/Contracts/IBlackjackServer.cs`
- 역할: 클라이언트가 서버에 보내는 명령 계약
- 포인트: Join/Leave/StartRound/Hit/Stand

3. `src/Seoul.It.Blackjack.Core/Contracts/IBlackjackClient.cs`
- 역할: 서버가 클라이언트에 보내는 콜백 계약
- 포인트: `OnStateChanged`, `OnError`

4. `src/Seoul.It.Blackjack.Core/Contracts/GamePhase.cs`
- 역할: 게임 단계 enum (`Idle`, `InRound`)

5. `src/Seoul.It.Blackjack.Core/Contracts/PlayerTurnState.cs`
- 역할: 플레이어 턴 상태 enum (`Playing`, `Standing`, `Busted`)

6. `src/Seoul.It.Blackjack.Core/Contracts/RoundOutcome.cs`
- 역할: 라운드 결과 enum (`None`, `Win`, `Lose`, `Tie`)

7. `src/Seoul.It.Blackjack.Core/Contracts/PlayerState.cs`
- 역할: 각 플레이어 상태 DTO

8. `src/Seoul.It.Blackjack.Core/Contracts/GameState.cs`
- 역할: 전체 게임 상태 스냅샷 DTO

9. `src/Seoul.It.Blackjack.Core/Domain/Suit.cs`
- 역할: 카드 무늬 enum

10. `src/Seoul.It.Blackjack.Core/Domain/Rank.cs`
- 역할: 카드 숫자 enum + 점수 변환 확장 메서드
- 포인트: Ace=1, J/Q/K=10

11. `src/Seoul.It.Blackjack.Core/Domain/Card.cs`
- 역할: 카드 1장을 표현하는 클래스

### 학생 설명 포인트

1. "인터페이스는 약속, 클래스는 구현"
2. DTO는 화면/네트워크 전달용 데이터 상자
3. enum을 쓰면 문자열 오타를 줄일 수 있음

---

## 4.3 Backend 프로젝트

핵심 설명 문장: "Backend는 규칙 판정기 + 상태 관리자 + 브로드캐스터입니다."

### 큰 구조

1. `Hub`: SignalR 진입점
2. `GameRoomService`: 게임 상태 변경 오케스트레이션
3. `RoundEngine`: 순수 라운드 로직
4. `Validator`: 규칙/권한 체크
5. `ChannelGameCommandProcessor`: 동시 요청 직렬 처리

### 파일 역할

1. `src/Seoul.It.Blackjack.Backend/Seoul.It.Blackjack.Backend.csproj`
- 역할: ASP.NET Core Web 프로젝트 설정

2. `src/Seoul.It.Blackjack.Backend/Program.cs`
- 역할: DI 등록, SignalR/Swagger 구성, Hub 매핑

3. `src/Seoul.It.Blackjack.Backend/Extensions/ServiceCollectionExtensions.cs`
- 역할: Options 등록 헬퍼

4. `src/Seoul.It.Blackjack.Backend/appsettings.json`
- 역할: 딜러 키 등 설정

5. `src/Seoul.It.Blackjack.Backend/Properties/launchSettings.json`
- 역할: 개발 실행 포트/프로필 설정

6. `src/Seoul.It.Blackjack.Backend/Seoul.It.Blackjack.Backend.http`
- 역할: 수동 HTTP 테스트 템플릿(현재 SignalR 중심이라 참고용)

7. `src/Seoul.It.Blackjack.Backend/Options/DealerOptions.cs`
- 역할: 딜러 키 설정 모델

8. `src/Seoul.It.Blackjack.Backend/Options/GameRuleOptions.cs`
- 역할: 덱 수/최소 인원/이름 길이 등 규칙 상수 옵션화

9. `src/Seoul.It.Blackjack.Backend/Hubs/GameSessionHub.cs`
- 역할: 클라이언트 요청 수신 + 서비스 호출 + 브로드캐스트
- 포인트: 요청자 오류는 Caller, 상태는 All

10. `src/Seoul.It.Blackjack.Backend/Services/IGameRoomService.cs`
- 역할: Hub가 의존하는 서비스 인터페이스

11. `src/Seoul.It.Blackjack.Backend/Services/GameRoomService.cs`
- 역할: 게임 전체 상태를 실제로 변경하는 중심 서비스
- 포인트: Join/Leave/Disconnect/Start/Hit/Stand 처리

12. `src/Seoul.It.Blackjack.Backend/Services/ConnectionRegistry.cs`
- 역할: connectionId -> playerId 매핑

13. `src/Seoul.It.Blackjack.Backend/Services/Infrastructure/IGameCommandProcessor.cs`
- 역할: 직렬 처리 인터페이스

14. `src/Seoul.It.Blackjack.Backend/Services/Infrastructure/ChannelGameCommandProcessor.cs`
- 역할: Channel 기반 단일 소비 루프
- 포인트: 동시 요청을 순차 처리

15. `src/Seoul.It.Blackjack.Backend/Services/Rules/IGameRuleValidator.cs`
- 역할: 규칙 검증 인터페이스

16. `src/Seoul.It.Blackjack.Backend/Services/Rules/GameRuleValidator.cs`
- 역할: 이름/권한/턴 검증 구현

17. `src/Seoul.It.Blackjack.Backend/Services/Round/IRoundEngine.cs`
- 역할: 라운드 규칙 엔진 인터페이스

18. `src/Seoul.It.Blackjack.Backend/Services/Round/RoundEngine.cs`
- 역할: 카드 분배, Hit/Stand, 딜러 자동 진행, 승패 계산

19. `src/Seoul.It.Blackjack.Backend/Services/Round/RoundResolution.cs`
- 역할: 라운드 처리 결과 데이터

20. `src/Seoul.It.Blackjack.Backend/Services/State/IGameStateSnapshotFactory.cs`
- 역할: 상태 스냅샷 팩토리 인터페이스

21. `src/Seoul.It.Blackjack.Backend/Services/State/GameStateSnapshotFactory.cs`
- 역할: 내부 상태를 외부 전송용 `GameState`로 복제

22. `src/Seoul.It.Blackjack.Backend/Services/Commands/GameCommandType.cs`
- 역할: 명령 종류 enum

23. `src/Seoul.It.Blackjack.Backend/Services/Commands/GameCommand.cs`
- 역할: 큐에 들어가는 명령 데이터

24. `src/Seoul.It.Blackjack.Backend/Services/Commands/GameNotice.cs`
- 역할: 전역 공지(코드/메시지)

25. `src/Seoul.It.Blackjack.Backend/Services/Commands/GameOperationResult.cs`
- 역할: 서비스 처리 결과(상태 + 공지 + 전송 여부)

26. `src/Seoul.It.Blackjack.Backend/Services/Exceptions/GameRoomException.cs`
- 역할: 공통 게임 예외 베이스

27. `src/Seoul.It.Blackjack.Backend/Services/Exceptions/GameAuthorizationException.cs`
- 역할: 권한 예외

28. `src/Seoul.It.Blackjack.Backend/Services/Exceptions/GameRuleException.cs`
- 역할: 규칙 위반 예외

29. `src/Seoul.It.Blackjack.Backend/Services/Exceptions/GameValidationException.cs`
- 역할: 입력 검증 예외

30. `src/Seoul.It.Blackjack.Backend/Models/Deck.cs`
- 역할: 카드 52장 생성

31. `src/Seoul.It.Blackjack.Backend/Models/Shoe.cs`
- 역할: 여러 덱을 섞어 뽑기 제공(ConcurrentStack)

### 학생 설명 포인트

1. Hub는 "문지기", Service는 "판정기"
2. Channel은 "줄 세우기"
3. 게임 로직은 RoundEngine으로 분리하여 테스트/설명 용이
4. 예외 코드 문자열은 프론트와의 약속이므로 중요

---

## 4.4 Client 프로젝트

핵심 설명 문장: "Client는 SignalR 연결을 캡슐화해서 앱이 쉽게 쓰게 해주는 라이브러리입니다."

### 파일 역할

1. `src/Seoul.It.Blackjack.Client/Seoul.It.Blackjack.Client.csproj`
- 역할: netstandard2.0 + SignalR Client 패키지 설정

2. `src/Seoul.It.Blackjack.Client/BlackjackClient.cs`
- 역할: 연결/호출/이벤트 전달 구현
- 포인트: `IBlackjackClient` 구현 + `nameof(IBlackjackServer...)`

3. `src/Seoul.It.Blackjack.Client/Options/BlackjackClientOptions.cs`
- 역할: HubUrl 옵션 객체

4. `src/Seoul.It.Blackjack.Client/Extensions/ServiceCollectionExtensions.cs`
- 역할: DI 등록 확장 (`AddBlackjackClient`)

### 학생 설명 포인트

1. 라이브러리화의 의미: 재사용
2. `ConnectAsync` 이후 명령 호출 가능
3. 이벤트를 UI에서 구독해 화면 갱신

---

## 4.5 Backend.Tests 프로젝트

핵심 설명 문장: "서버가 규칙대로 동작하는지 자동 채점하는 프로젝트입니다."

### 파일 역할

1. `src/Seoul.It.Blackjack.Backend.Tests/Seoul.It.Blackjack.Backend.Tests.csproj`
- 역할: 테스트 패키지/프로젝트 참조 설정

2. `src/Seoul.It.Blackjack.Backend.Tests/TestHostFactory.cs`
- 역할: 인메모리 테스트 서버 팩토리

3. `src/Seoul.It.Blackjack.Backend.Tests/SignalRTestClient.cs`
- 역할: 테스트용 SignalR 클라이언트

4. `src/Seoul.It.Blackjack.Backend.Tests/TestWaiter.cs`
- 역할: 비동기 상태 대기 유틸

5. `src/Seoul.It.Blackjack.Backend.Tests/JoinAndRoleTests.cs`
- 역할: 입장/딜러 권한 검증

6. `src/Seoul.It.Blackjack.Backend.Tests/TurnRuleTests.cs`
- 역할: 턴/권한/브로드캐스트 검증

7. `src/Seoul.It.Blackjack.Backend.Tests/RoundCompletionTests.cs`
- 역할: 라운드 종료/다음 라운드 초기화 검증

8. `src/Seoul.It.Blackjack.Backend.Tests/DealerTerminationTests.cs`
- 역할: 딜러 퇴장 시 GAME_TERMINATED + 상태 초기화 검증

9. `src/Seoul.It.Blackjack.Backend.Tests/ConcurrencyTests.cs`
- 역할: 동시 요청 직렬 처리 검증

10. `src/Seoul.It.Blackjack.Backend.Tests/GameRuleOptionsDefaultTests.cs`
- 역할: 기본 규칙 옵션 동작 검증

11. `src/Seoul.It.Blackjack.Backend.Tests/ClientDiIntegrationTests.cs`
- 역할: Client DI 등록/실서버 연동 검증

### 학생 설명 포인트

1. "눈으로 확인"은 한계가 있어 테스트 자동화 필요
2. 테스트는 입력-기대결과의 명시적 계약
3. 비동기 테스트는 대기 유틸이 중요

---

## 4.6 Frontend 프로젝트

핵심 설명 문장: "Entry에서 사용자 정보를 받고, Table에서 게임 상태를 실시간으로 보여줍니다."

### 파일 역할

1. `src/Seoul.It.Blackjack.Frontend/Seoul.It.Blackjack.Frontend.csproj`
- 역할: Blazor Server 대화형 앱 프로젝트

2. `src/Seoul.It.Blackjack.Frontend/Program.cs`
- 역할: 프론트 DI/미들웨어 구성

3. `src/Seoul.It.Blackjack.Frontend/appsettings.json`
- 역할: HubUrl 설정

4. `src/Seoul.It.Blackjack.Frontend/_Imports.razor`
- 역할: 공통 using/import 모음

5. `src/Seoul.It.Blackjack.Frontend/Options/FrontendBlackjackOptions.cs`
- 역할: 프론트 옵션 모델

6. `src/Seoul.It.Blackjack.Frontend/Extensions/ServiceCollectionExtensions.cs`
- 역할: 옵션 바인딩 + 클라이언트 등록 + 서비스 등록

7. `src/Seoul.It.Blackjack.Frontend/Extensions/CardExtensions.cs`
- 역할: `Card -> cards/{suit}_{rank}.svg` 변환

8. `src/Seoul.It.Blackjack.Frontend/Services/FrontendEntryState.cs`
- 역할: Entry -> Table 전달용 상태 저장

9. `src/Seoul.It.Blackjack.Frontend/Services/IFrontendGameSession.cs`
- 역할: UI가 사용하는 게임 세션 인터페이스

10. `src/Seoul.It.Blackjack.Frontend/Services/FrontendGameSession.cs`
- 역할: BlackjackClient를 UI 친화적 인터페이스로 연결

11. `src/Seoul.It.Blackjack.Frontend/Components/App.razor`
- 역할: HTML 뼈대 + 라우팅 루트

12. `src/Seoul.It.Blackjack.Frontend/Components/Routes.razor`
- 역할: 라우터 구성

13. `src/Seoul.It.Blackjack.Frontend/Components/Layout/MainLayout.razor`
- 역할: 기본 레이아웃

14. `src/Seoul.It.Blackjack.Frontend/Pages/Entry.razor`
- 역할: 이름/딜러키 입력 페이지(`/`)

15. `src/Seoul.It.Blackjack.Frontend/Pages/Entry.razor.css`
- 역할: Entry 스타일

16. `src/Seoul.It.Blackjack.Frontend/Pages/Table.razor`
- 역할: 게임 테이블 페이지(`/table`), 자동 Connect+Join

17. `src/Seoul.It.Blackjack.Frontend/Pages/Table.razor.css`
- 역할: Table 스타일

18. `src/Seoul.It.Blackjack.Frontend/wwwroot/css/app.css`
- 역할: 전역 스타일

19. `src/Seoul.It.Blackjack.Frontend/wwwroot/cards/*.svg` (52장)
- 역할: 카드 이미지 에셋
- 참고: 에셋은 코드가 아니므로 부록 코드 원문에서는 제외

### 학생 설명 포인트

1. Blazor 컴포넌트: HTML + C#을 한 파일에서
2. `@bind`: 입력값 양방향 바인딩
3. 생명주기: `OnInitialized`, `OnAfterRenderAsync`, `DisposeAsync`
4. 상태 변경 시 `StateHasChanged` 필요성

---

## 4.7 Frontend.Tests 프로젝트

핵심 설명 문장: "화면 컴포넌트가 의도대로 동작하는지 빠르게 검증합니다."

### 파일 역할

1. `src/Seoul.It.Blackjack.Frontend.Tests/Seoul.It.Blackjack.Frontend.Tests.csproj`
- 역할: MSTest + bUnit + TestHost 설정

2. `src/Seoul.It.Blackjack.Frontend.Tests/MSTestSettings.cs`
- 역할: 테스트 병렬 실행 설정

3. `src/Seoul.It.Blackjack.Frontend.Tests/TestHostFactory.cs`
- 역할: 프론트 인메모리 테스트 서버

4. `src/Seoul.It.Blackjack.Frontend.Tests/PageRenderingTests.cs`
- 역할: `/`, `/table` 렌더 확인

5. `src/Seoul.It.Blackjack.Frontend.Tests/StaticCardAssetsTests.cs`
- 역할: 카드 SVG 정적 파일 제공 확인

6. `src/Seoul.It.Blackjack.Frontend.Tests/CardExtensionsTests.cs`
- 역할: 카드 경로 변환 테스트

7. `src/Seoul.It.Blackjack.Frontend.Tests/FrontendBlackjackOptionsTests.cs`
- 역할: 옵션 기본값 테스트

8. `src/Seoul.It.Blackjack.Frontend.Tests/FakeFrontendGameSession.cs`
- 역할: Table/Entry 테스트용 세션 가짜 구현

9. `src/Seoul.It.Blackjack.Frontend.Tests/EntryPageBunitTests.cs`
- 역할: Entry 페이지 버튼/네비게이션 테스트

10. `src/Seoul.It.Blackjack.Frontend.Tests/TablePageBunitTests.cs`
- 역할: Table 자동 연결/상태 렌더/버튼/오류 표시 테스트

### 학생 설명 포인트

1. bUnit은 Blazor 컴포넌트 테스트 도구
2. Fake 객체로 네트워크 없이 화면 로직 검증 가능
3. "렌더 결과 문자열 검증"은 UI 자동 검증의 기본

---

## 5. 처음부터 만들 때 실제 타이핑 순서 (강의 스크립트)

아래 순서대로 타이핑하면 학생이 혼란이 적습니다.

## 5.1 솔루션 준비

1. 솔루션 생성 및 프로젝트 껍데기 생성
2. `Directory.Build.props` 반영
3. 프로젝트 참조 연결

## 5.2 Core 타이핑

1. 도메인: `Suit`, `Rank`, `Card`
2. 상태 enum: `GamePhase`, `PlayerTurnState`, `RoundOutcome`
3. DTO: `PlayerState`, `GameState`
4. 계약: `IBlackjackServer`, `IBlackjackClient`

이론 설명 순서:

1. enum의 장점
2. 클래스/프로퍼티
3. 인터페이스 계약

## 5.3 Backend 타이핑

1. `Program`, Options, DI Extension
2. Command/Exception 타입
3. `ConnectionRegistry`, `Validator`, `SnapshotFactory`
4. `Deck`, `Shoe`
5. `RoundEngine` + `RoundResolution`
6. `GameRoomService`
7. `ChannelGameCommandProcessor`
8. `GameSessionHub`

이론 설명 순서:

1. DI 등록 흐름
2. SignalR 허브 역할
3. 큐 직렬 처리 이유
4. 게임 규칙 계산 분리(SRP)

## 5.4 Client 타이핑

1. `BlackjackClientOptions`
2. `BlackjackClient`
3. DI Extension

이론 설명 순서:

1. HubConnection 생성
2. 서버 호출(`InvokeAsync`)과 이벤트 수신(`On`)
3. `nameof`로 문자열 하드코딩 줄이기

## 5.5 Backend.Tests 타이핑

1. TestHostFactory + SignalRTestClient + TestWaiter
2. Join/Turn/Round/Dealer 종료 테스트
3. 동시성/기본 옵션/Client DI 테스트

이론 설명 순서:

1. 통합 테스트와 단위 테스트 차이
2. 비동기 테스트 대기 패턴

## 5.6 Frontend 타이핑

1. Program/Options/ServiceCollectionExtension
2. Session 서비스/EntryState
3. App/Routes/Layout
4. Entry 페이지
5. Table 페이지
6. CSS 및 카드 경로 확장

이론 설명 순서:

1. Blazor 라우팅
2. 컴포넌트 생명주기
3. 이벤트 구독/해제

## 5.7 Frontend.Tests 타이핑

1. Host 렌더링 테스트
2. 카드 에셋 테스트
3. bUnit Entry/Table 테스트

이론 설명 순서:

1. bUnit 컨텍스트
2. 렌더 후 DOM 찾기/클릭/검증

## 5.8 파일 단위 전체 타이핑 순서표

아래 순서는 강의 실습에서 그대로 읽고 진행할 수 있도록 \"파일 단위\"로 나열했습니다.

1. `src/Seoul.It.Blackjack.sln`
2. `src/Directory.Build.props`
3. `src/Seoul.It.Blackjack.Core/Seoul.It.Blackjack.Core.csproj`
4. `src/Seoul.It.Blackjack.Core/Domain/Suit.cs`
5. `src/Seoul.It.Blackjack.Core/Domain/Rank.cs`
6. `src/Seoul.It.Blackjack.Core/Domain/Card.cs`
7. `src/Seoul.It.Blackjack.Core/Contracts/GamePhase.cs`
8. `src/Seoul.It.Blackjack.Core/Contracts/PlayerTurnState.cs`
9. `src/Seoul.It.Blackjack.Core/Contracts/RoundOutcome.cs`
10. `src/Seoul.It.Blackjack.Core/Contracts/PlayerState.cs`
11. `src/Seoul.It.Blackjack.Core/Contracts/GameState.cs`
12. `src/Seoul.It.Blackjack.Core/Contracts/IBlackjackServer.cs`
13. `src/Seoul.It.Blackjack.Core/Contracts/IBlackjackClient.cs`
14. `src/Seoul.It.Blackjack.Backend/Seoul.It.Blackjack.Backend.csproj`
15. `src/Seoul.It.Blackjack.Backend/Program.cs`
16. `src/Seoul.It.Blackjack.Backend/appsettings.json`
17. `src/Seoul.It.Blackjack.Backend/Properties/launchSettings.json`
18. `src/Seoul.It.Blackjack.Backend/Extensions/ServiceCollectionExtensions.cs`
19. `src/Seoul.It.Blackjack.Backend/Options/DealerOptions.cs`
20. `src/Seoul.It.Blackjack.Backend/Options/GameRuleOptions.cs`
21. `src/Seoul.It.Blackjack.Backend/Services/Commands/GameCommandType.cs`
22. `src/Seoul.It.Blackjack.Backend/Services/Commands/GameCommand.cs`
23. `src/Seoul.It.Blackjack.Backend/Services/Commands/GameNotice.cs`
24. `src/Seoul.It.Blackjack.Backend/Services/Commands/GameOperationResult.cs`
25. `src/Seoul.It.Blackjack.Backend/Services/Exceptions/GameRoomException.cs`
26. `src/Seoul.It.Blackjack.Backend/Services/Exceptions/GameAuthorizationException.cs`
27. `src/Seoul.It.Blackjack.Backend/Services/Exceptions/GameRuleException.cs`
28. `src/Seoul.It.Blackjack.Backend/Services/Exceptions/GameValidationException.cs`
29. `src/Seoul.It.Blackjack.Backend/Models/Deck.cs`
30. `src/Seoul.It.Blackjack.Backend/Models/Shoe.cs`
31. `src/Seoul.It.Blackjack.Backend/Services/ConnectionRegistry.cs`
32. `src/Seoul.It.Blackjack.Backend/Services/IGameRoomService.cs`
33. `src/Seoul.It.Blackjack.Backend/Services/Infrastructure/IGameCommandProcessor.cs`
34. `src/Seoul.It.Blackjack.Backend/Services/Infrastructure/ChannelGameCommandProcessor.cs`
35. `src/Seoul.It.Blackjack.Backend/Services/Rules/IGameRuleValidator.cs`
36. `src/Seoul.It.Blackjack.Backend/Services/Rules/GameRuleValidator.cs`
37. `src/Seoul.It.Blackjack.Backend/Services/State/IGameStateSnapshotFactory.cs`
38. `src/Seoul.It.Blackjack.Backend/Services/State/GameStateSnapshotFactory.cs`
39. `src/Seoul.It.Blackjack.Backend/Services/Round/IRoundEngine.cs`
40. `src/Seoul.It.Blackjack.Backend/Services/Round/RoundResolution.cs`
41. `src/Seoul.It.Blackjack.Backend/Services/Round/RoundEngine.cs`
42. `src/Seoul.It.Blackjack.Backend/Services/GameRoomService.cs`
43. `src/Seoul.It.Blackjack.Backend/Hubs/GameSessionHub.cs`
44. `src/Seoul.It.Blackjack.Backend/Seoul.It.Blackjack.Backend.http`
45. `src/Seoul.It.Blackjack.Client/Seoul.It.Blackjack.Client.csproj`
46. `src/Seoul.It.Blackjack.Client/Options/BlackjackClientOptions.cs`
47. `src/Seoul.It.Blackjack.Client/BlackjackClient.cs`
48. `src/Seoul.It.Blackjack.Client/Extensions/ServiceCollectionExtensions.cs`
49. `src/Seoul.It.Blackjack.Backend.Tests/Seoul.It.Blackjack.Backend.Tests.csproj`
50. `src/Seoul.It.Blackjack.Backend.Tests/TestHostFactory.cs`
51. `src/Seoul.It.Blackjack.Backend.Tests/SignalRTestClient.cs`
52. `src/Seoul.It.Blackjack.Backend.Tests/TestWaiter.cs`
53. `src/Seoul.It.Blackjack.Backend.Tests/JoinAndRoleTests.cs`
54. `src/Seoul.It.Blackjack.Backend.Tests/TurnRuleTests.cs`
55. `src/Seoul.It.Blackjack.Backend.Tests/RoundCompletionTests.cs`
56. `src/Seoul.It.Blackjack.Backend.Tests/DealerTerminationTests.cs`
57. `src/Seoul.It.Blackjack.Backend.Tests/ConcurrencyTests.cs`
58. `src/Seoul.It.Blackjack.Backend.Tests/GameRuleOptionsDefaultTests.cs`
59. `src/Seoul.It.Blackjack.Backend.Tests/ClientDiIntegrationTests.cs`
60. `src/Seoul.It.Blackjack.Frontend/Seoul.It.Blackjack.Frontend.csproj`
61. `src/Seoul.It.Blackjack.Frontend/Program.cs`
62. `src/Seoul.It.Blackjack.Frontend/appsettings.json`
63. `src/Seoul.It.Blackjack.Frontend/_Imports.razor`
64. `src/Seoul.It.Blackjack.Frontend/Options/FrontendBlackjackOptions.cs`
65. `src/Seoul.It.Blackjack.Frontend/Extensions/ServiceCollectionExtensions.cs`
66. `src/Seoul.It.Blackjack.Frontend/Extensions/CardExtensions.cs`
67. `src/Seoul.It.Blackjack.Frontend/Services/FrontendEntryState.cs`
68. `src/Seoul.It.Blackjack.Frontend/Services/IFrontendGameSession.cs`
69. `src/Seoul.It.Blackjack.Frontend/Services/FrontendGameSession.cs`
70. `src/Seoul.It.Blackjack.Frontend/Components/App.razor`
71. `src/Seoul.It.Blackjack.Frontend/Components/Routes.razor`
72. `src/Seoul.It.Blackjack.Frontend/Components/Layout/MainLayout.razor`
73. `src/Seoul.It.Blackjack.Frontend/Pages/Entry.razor`
74. `src/Seoul.It.Blackjack.Frontend/Pages/Entry.razor.css`
75. `src/Seoul.It.Blackjack.Frontend/Pages/Table.razor`
76. `src/Seoul.It.Blackjack.Frontend/Pages/Table.razor.css`
77. `src/Seoul.It.Blackjack.Frontend/wwwroot/css/app.css`
78. `src/Seoul.It.Blackjack.Frontend.Tests/Seoul.It.Blackjack.Frontend.Tests.csproj`
79. `src/Seoul.It.Blackjack.Frontend.Tests/MSTestSettings.cs`
80. `src/Seoul.It.Blackjack.Frontend.Tests/TestHostFactory.cs`
81. `src/Seoul.It.Blackjack.Frontend.Tests/PageRenderingTests.cs`
82. `src/Seoul.It.Blackjack.Frontend.Tests/StaticCardAssetsTests.cs`
83. `src/Seoul.It.Blackjack.Frontend.Tests/CardExtensionsTests.cs`
84. `src/Seoul.It.Blackjack.Frontend.Tests/FrontendBlackjackOptionsTests.cs`
85. `src/Seoul.It.Blackjack.Frontend.Tests/FakeFrontendGameSession.cs`
86. `src/Seoul.It.Blackjack.Frontend.Tests/EntryPageBunitTests.cs`
87. `src/Seoul.It.Blackjack.Frontend.Tests/TablePageBunitTests.cs`

---

## 5.9 실전 강의 대본 (그대로 읽어도 되는 버전)

아래 내용은 \"강사가 실제로 말할 문장\" 기준으로 작성했습니다.
한 파일을 열 때마다 이 순서로 진행하면 됩니다.

### 5.9.1 Core 강의 대본

1. 파일: `src/Seoul.It.Blackjack.Core/Domain/Suit.cs`
교사 발화 예시: \"지금부터 카드 무늬를 코드로 표현합니다. 문자열로 쓰면 오타가 나기 쉬우니 enum을 씁니다.\"
타이핑 포인트: `Clubs, Diamonds, Hearts, Spades`를 정확한 철자로 입력.
학생 확인 질문: \"왜 문자열보다 enum이 안전할까요?\"

2. 파일: `src/Seoul.It.Blackjack.Core/Domain/Rank.cs`
교사 발화 예시: \"숫자/그림 카드를 enum으로 만들고, `ToValue()` 확장 메서드에서 점수를 계산합니다.\"
타이핑 포인트: `Rank.Ten or Rank.Jack or Rank.Queen or Rank.King => 10` 패턴 매칭 문법 설명.
학생 확인 질문: \"Ace를 1로 고정한 이유가 뭘까요?\"

3. 파일: `src/Seoul.It.Blackjack.Core/Domain/Card.cs`
교사 발화 예시: \"Card는 불변 객체처럼 씁니다. 생성자에서만 Suit/Rank를 설정하고 바꾸지 않습니다.\"
타이핑 포인트: `public Suit Suit { get; }`, `public Rank Rank { get; }`의 읽기 전용 프로퍼티 의미 설명.
학생 확인 질문: \"카드를 중간에 바꾸면 어떤 버그가 생길 수 있을까요?\"

4. 파일: `src/Seoul.It.Blackjack.Core/Contracts/GamePhase.cs`
교사 발화 예시: \"게임 전체는 지금 대기중인지, 라운드 진행중인지만 알면 됩니다.\"
타이핑 포인트: `Idle`, `InRound` 두 상태만 둔다.
학생 확인 질문: \"상태를 줄이면 어떤 장점이 있나요?\"

5. 파일: `src/Seoul.It.Blackjack.Core/Contracts/PlayerTurnState.cs`
교사 발화 예시: \"플레이어는 행동 가능(Playing), 멈춤(Standing), 파산(Busted) 중 하나입니다.\"
타이핑 포인트: 턴 상태와 라운드 결과를 분리해서 저장한다는 점 설명.
학생 확인 질문: \"Standing과 Win은 같은 의미일까요?\"

6. 파일: `src/Seoul.It.Blackjack.Core/Contracts/RoundOutcome.cs`
교사 발화 예시: \"결과는 라운드 끝나야 확정됩니다. 그래서 `None`이 필요합니다.\"
타이핑 포인트: `None, Win, Lose, Tie` 순서 유지.
학생 확인 질문: \"라운드 시작 직후 Outcome이 왜 None이어야 할까요?\"

7. 파일: `src/Seoul.It.Blackjack.Core/Contracts/PlayerState.cs`
교사 발화 예시: \"이 클래스는 한 사람의 현재 상태 스냅샷입니다.\"
타이핑 포인트: `PlayerId`, `Name`, `IsDealer`, `Cards`, `Score`, `TurnState`, `Outcome`를 빠짐없이 입력.
학생 확인 질문: \"딜러 여부를 bool로 둔 이유는?\"

8. 파일: `src/Seoul.It.Blackjack.Core/Contracts/GameState.cs`
교사 발화 예시: \"서버가 클라이언트에 방송할 때 보내는 전체 화면 데이터입니다.\"
타이핑 포인트: `Phase`, `Players`, `DealerPlayerId`, `CurrentTurnPlayerId`, `StatusMessage`.
학생 확인 질문: \"UI를 새로 그리려면 어떤 정보가 최소로 필요할까요?\"

9. 파일: `src/Seoul.It.Blackjack.Core/Contracts/IBlackjackServer.cs`
교사 발화 예시: \"클라이언트가 서버에 요청하는 버튼 목록이라고 생각하면 됩니다.\"
타이핑 포인트: `Join/Leave/StartRound/Hit/Stand` 시그니처를 정확히 입력.
학생 확인 질문: \"Join에 `dealerKey`가 nullable인 이유는?\"

10. 파일: `src/Seoul.It.Blackjack.Core/Contracts/IBlackjackClient.cs`
교사 발화 예시: \"이제 반대 방향입니다. 서버가 클라이언트를 호출하는 메서드입니다.\"
타이핑 포인트: `OnStateChanged`, `OnError` 이름을 서버/클라이언트 양쪽에서 동일하게 맞춘다.
학생 확인 질문: \"서버에서 이벤트를 받는 방식이 왜 필요한가요?\"

### 5.9.2 Backend 강의 대본

1. 파일: `src/Seoul.It.Blackjack.Backend/Program.cs`
교사 발화 예시: \"Program은 앱의 조립 공장입니다. 서비스 등록 후 앱을 실행합니다.\"
타이핑 포인트: `AddSignalR`, `AddSwaggerGen`, Options 등록, 서비스 등록 순서 확인.
학생 확인 질문: \"DI 등록을 빼먹으면 언제 터질까요?\"

2. 파일: `src/Seoul.It.Blackjack.Backend/Options/DealerOptions.cs`, `src/Seoul.It.Blackjack.Backend/Options/GameRuleOptions.cs`
교사 발화 예시: \"하드코딩 대신 설정으로 뺍니다. 규칙 변경 시 코드 수정 범위를 줄일 수 있습니다.\"
타이핑 포인트: 섹션 이름 상수(`DefaultSectionName`)와 기본값 유지.
학생 확인 질문: \"MinPlayersToStart를 바꾸면 어떤 기능이 바뀌나요?\"

3. 파일: `src/Seoul.It.Blackjack.Backend/Extensions/ServiceCollectionExtensions.cs`
교사 발화 예시: \"옵션 바인딩 코드를 Program 밖으로 빼서 읽기 쉽게 만듭니다.\"
타이핑 포인트: `Configure<T>(GetSection(...))` 패턴 설명.
학생 확인 질문: \"확장 메서드로 빼는 장점은?\"

4. 파일: `src/Seoul.It.Blackjack.Backend/Services/Commands/*`
교사 발화 예시: \"요청 하나를 큐에 넣을 데이터 형태와 처리 결과 형태를 정의합니다.\"
타이핑 포인트: `GameCommandType`, `GameCommand`, `GameNotice`, `GameOperationResult`의 역할 구분.
학생 확인 질문: \"Notice를 State와 분리한 이유는?\"

5. 파일: `src/Seoul.It.Blackjack.Backend/Services/Exceptions/*`
교사 발화 예시: \"예외도 타입을 나눠야, 어떤 종류의 오류인지 코드에서 명확해집니다.\"
타이핑 포인트: 모든 예외가 `GameRoomException`을 상속하고 `Code`를 유지한다.
학생 확인 질문: \"오류 메시지와 오류 코드를 둘 다 보내는 이유는?\"

6. 파일: `src/Seoul.It.Blackjack.Backend/Models/Deck.cs`, `src/Seoul.It.Blackjack.Backend/Models/Shoe.cs`
교사 발화 예시: \"Deck은 52장 생성기, Shoe는 여러 Deck을 합쳐 섞고 뽑는 도구입니다.\"
타이핑 포인트: `Enum.GetValues<T>()`, `SelectMany`, `ConcurrentStack`, `TryDraw` 설명.
학생 확인 질문: \"왜 Stack(후입선출)으로도 카드 뽑기가 가능한가요?\"

7. 파일: `src/Seoul.It.Blackjack.Backend/Services/ConnectionRegistry.cs`
교사 발화 예시: \"연결 ID와 플레이어를 매핑하는 테이블입니다. lock으로 동기화합니다.\"
타이핑 포인트: `ContainsConnection`, `Add`, `TryRemove`, `Clear`.
학생 확인 질문: \"동시에 접속/퇴장하면 lock이 왜 필요할까요?\"

8. 파일: `src/Seoul.It.Blackjack.Backend/Services/Infrastructure/ChannelGameCommandProcessor.cs`
교사 발화 예시: \"핵심입니다. 동시에 들어온 명령을 채널에 넣고 한 명의 소비자가 순서대로 처리합니다.\"
타이핑 포인트: `Channel.CreateUnbounded`, `SingleReader = true`, `TaskCompletionSource`.
학생 확인 질문: \"큐 없이 바로 처리하면 어떤 동시성 문제가 생길까요?\"

9. 파일: `src/Seoul.It.Blackjack.Backend/Services/Rules/GameRuleValidator.cs`
교사 발화 예시: \"규칙 검증을 한 곳으로 모아야 서비스가 덜 복잡해집니다.\"
타이핑 포인트: 이름 검증, 시작 권한 검증, 턴 검증 메서드 분리.
학생 확인 질문: \"검증 로직이 여기저기 흩어지면 유지보수에 어떤 문제가 있나요?\"

10. 파일: `src/Seoul.It.Blackjack.Backend/Services/State/GameStateSnapshotFactory.cs`
교사 발화 예시: \"내부 원본 상태를 그대로 보내지 않고 복사본을 만들어 전송합니다.\"
타이핑 포인트: `ClonePlayer`에서 카드 리스트까지 깊은 복사.
학생 확인 질문: \"얕은 복사를 하면 어떤 버그가 날까요?\"

11. 파일: `src/Seoul.It.Blackjack.Backend/Services/Round/RoundEngine.cs`
교사 발화 예시: \"게임 규칙 계산의 중심입니다. Start/Hit/Stand/Complete를 담당합니다.\"
타이핑 포인트: 21점 처리(Standing), Bust 처리(Busted), 딜러 자동 진행(<17 Hit), 결과 계산.
학생 확인 질문: \"왜 엔진을 별도 클래스로 분리했을까요?\"

12. 파일: `src/Seoul.It.Blackjack.Backend/Services/GameRoomService.cs`
교사 발화 예시: \"이 클래스는 상태 관리자입니다. 명령을 받아 검증하고 엔진을 호출하고 결과를 만듭니다.\"
타이핑 포인트: Join/Leave/Disconnect/StartRound/Hit/Stand 흐름, 딜러 퇴장 시 전체 초기화.
학생 확인 질문: \"딜러 퇴장 시 왜 모든 플레이어를 비우나요?\"

13. 파일: `src/Seoul.It.Blackjack.Backend/Hubs/GameSessionHub.cs`
교사 발화 예시: \"Hub는 네트워크 입구입니다. 규칙은 서비스에게 맡기고, 전송만 담당합니다.\"
타이핑 포인트: 성공 시 `Clients.All.OnStateChanged`, 예외 시 `Clients.Caller.OnError`.
학생 확인 질문: \"요청자 오류를 왜 All이 아니라 Caller에게만 보낼까요?\"

14. 파일: `src/Seoul.It.Blackjack.Backend/appsettings.json`, `src/Seoul.It.Blackjack.Backend/Properties/launchSettings.json`
교사 발화 예시: \"설정값으로 딜러 키와 개발 포트를 관리합니다.\"
타이핑 포인트: 딜러 키 값, `https://localhost:5000;http://localhost:5001` 확인.
학생 확인 질문: \"프론트 HubUrl과 백엔드 포트가 안 맞으면 무슨 일이 생길까요?\"

### 5.9.3 Client 강의 대본

1. 파일: `src/Seoul.It.Blackjack.Client/BlackjackClient.cs`
교사 발화 예시: \"이 클래스는 SignalR 연결을 감싸는 도구입니다. 앱에서는 이 객체만 쓰면 됩니다.\"
타이핑 포인트: `_connection.On<...>(nameof(IBlackjackClient.OnStateChanged), ...)`, `InvokeAsync(nameof(IBlackjackServer.Hit))`.
학생 확인 질문: \"메서드명을 문자열로 직접 쓰지 않고 `nameof`를 쓰는 이유는?\"

2. 파일: `src/Seoul.It.Blackjack.Client/Options/BlackjackClientOptions.cs`
교사 발화 예시: \"HubUrl 같은 설정값을 객체로 보관합니다.\"
타이핑 포인트: `HubUrl` 기본값/주입 흐름 설명.
학생 확인 질문: \"하드코딩 URL보다 옵션이 좋은 이유는?\"

3. 파일: `src/Seoul.It.Blackjack.Client/Extensions/ServiceCollectionExtensions.cs`
교사 발화 예시: \"DI에 클라이언트를 등록해서 어디서든 주입받아 쓰게 만듭니다.\"
타이핑 포인트: null 방어, singleton 등록.
학생 확인 질문: \"BlackjackClient를 Singleton으로 두는 이유는?\"

### 5.9.4 Backend.Tests 강의 대본

1. 파일: `src/Seoul.It.Blackjack.Backend.Tests/TestHostFactory.cs`
교사 발화 예시: \"실제 서버를 띄우지 않고 메모리에서 서버를 만들어 테스트합니다.\"
타이핑 포인트: `WebApplicationFactory<Program>`.
학생 확인 질문: \"테스트 서버를 쓰면 좋은 점은?\"

2. 파일: `src/Seoul.It.Blackjack.Backend.Tests/SignalRTestClient.cs`
교사 발화 예시: \"테스트용 플레이어입니다. 이벤트를 리스트에 기록해 검증합니다.\"
타이핑 포인트: `Events`, `States`, `Errors` 저장.
학생 확인 질문: \"이벤트 순서를 왜 별도로 기록할까요?\"

3. 파일: `src/Seoul.It.Blackjack.Backend.Tests/TestWaiter.cs`
교사 발화 예시: \"비동기 테스트는 상태가 즉시 바뀌지 않아서 기다림 유틸이 필요합니다.\"
타이핑 포인트: 폴링 방식 + 타임아웃.
학생 확인 질문: \"무한 대기를 막는 장치가 무엇인가요?\"

4. 파일: `src/Seoul.It.Blackjack.Backend.Tests/JoinAndRoleTests.cs`
교사 발화 예시: \"입장 규칙을 검증합니다. 딜러키/이름/중복 참가를 테스트합니다.\"
타이핑 포인트: 코드값 `INVALID_NAME`, `DEALER_ALREADY_EXISTS`, `ALREADY_JOINED`.
학생 확인 질문: \"왜 정상 케이스와 실패 케이스를 둘 다 테스트해야 할까요?\"

5. 파일: `src/Seoul.It.Blackjack.Backend.Tests/TurnRuleTests.cs`
교사 발화 예시: \"턴 권한과 라운드 중 입장 제한을 검증합니다.\"
타이핑 포인트: `NOT_DEALER`, `GAME_IN_PROGRESS`, `NOT_YOUR_TURN`, `DEALER_IS_AUTO`.
학생 확인 질문: \"턴 검증이 없으면 멀티플레이에서 어떤 문제가 생길까요?\"

6. 파일: `src/Seoul.It.Blackjack.Backend.Tests/RoundCompletionTests.cs`
교사 발화 예시: \"라운드 종료 후 Idle 전환과 다음 라운드 초기화를 확인합니다.\"
타이핑 포인트: `GamePhase.Idle`, 다음 라운드에서 `Outcome` 초기화 확인.
학생 확인 질문: \"이전 라운드 결과가 왜 남아있으면 안 될까요?\"

7. 파일: `src/Seoul.It.Blackjack.Backend.Tests/DealerTerminationTests.cs`
교사 발화 예시: \"딜러 퇴장 시 전체 종료 정책을 검증합니다.\"
타이핑 포인트: `GAME_TERMINATED` 1회 + 상태 초기화 1회 + 이벤트 순서 검증.
학생 확인 질문: \"순서 검증까지 하는 이유는?\"

8. 파일: `src/Seoul.It.Blackjack.Backend.Tests/ConcurrencyTests.cs`
교사 발화 예시: \"동시 요청이 들어와도 큐 덕분에 규칙이 깨지지 않는지 봅니다.\"
타이핑 포인트: `Task.WhenAll` 후 `NOT_YOUR_TURN` 검증.
학생 확인 질문: \"이 테스트가 실패하면 어디를 의심해야 할까요?\"

9. 파일: `src/Seoul.It.Blackjack.Backend.Tests/GameRuleOptionsDefaultTests.cs`
교사 발화 예시: \"기본 옵션값이 실제 게임 규칙으로 반영되는지 확인합니다.\"
타이핑 포인트: 딜러 혼자 Start 시 `INSUFFICIENT_PLAYERS`.
학생 확인 질문: \"옵션 테스트를 빼면 어떤 회귀 버그가 생길 수 있나요?\"

10. 파일: `src/Seoul.It.Blackjack.Backend.Tests/ClientDiIntegrationTests.cs`
교사 발화 예시: \"클라이언트 DI와 실제 연결/이벤트 수신이 되는지 통합 검증합니다.\"
타이핑 포인트: `AddBlackjackClient` singleton 검증 + 실제 `ConnectAsync`.
학생 확인 질문: \"단위 테스트만으로는 놓치는 부분이 무엇인가요?\"

### 5.9.5 Frontend 강의 대본

1. 파일: `src/Seoul.It.Blackjack.Frontend/Program.cs`
교사 발화 예시: \"Blazor 앱도 백엔드처럼 DI로 조립합니다.\"
타이핑 포인트: `AddFrontendBlackjackOptions`, `AddFrontendBlackjackClient`, `AddFrontendServices`.
학생 확인 질문: \"Program에서 등록한 서비스가 페이지에서 어떻게 주입될까요?\"

2. 파일: `src/Seoul.It.Blackjack.Frontend/Extensions/ServiceCollectionExtensions.cs`
교사 발화 예시: \"프론트도 백엔드처럼 확장 메서드로 등록 일관성을 유지합니다.\"
타이핑 포인트: 설정값 바인딩 + 기본 URL fallback.
학생 확인 질문: \"설정값이 비어있을 때 기본값이 필요한 이유는?\"

3. 파일: `src/Seoul.It.Blackjack.Frontend/Services/FrontendEntryState.cs`
교사 발화 예시: \"Entry 페이지에서 입력한 이름을 Table 페이지로 전달하는 임시 저장소입니다.\"
타이핑 포인트: Scoped 수명 설명.
학생 확인 질문: \"Singleton 대신 Scoped인 이유는?\"

4. 파일: `src/Seoul.It.Blackjack.Frontend/Services/FrontendGameSession.cs`
교사 발화 예시: \"UI가 쉽게 쓰도록 Client를 감싸고 상태 플래그(IsConnected/IsJoined)를 제공합니다.\"
타이핑 포인트: `ConnectAsync`, `JoinAsync`, 이벤트 브릿지, `GAME_TERMINATED` 처리.
학생 확인 질문: \"세션 클래스 없이 페이지에서 바로 BlackjackClient를 쓰면 어떤 단점이 있나요?\"

5. 파일: `src/Seoul.It.Blackjack.Frontend/Extensions/CardExtensions.cs`
교사 발화 예시: \"Card 객체를 이미지 경로 문자열로 바꿉니다.\"
타이핑 포인트: `cards/{suit}_{rank}.svg` 규칙 고정.
학생 확인 질문: \"파일명 규칙이 바뀌면 어디를 수정해야 할까요?\"

6. 파일: `src/Seoul.It.Blackjack.Frontend/Pages/Entry.razor`
교사 발화 예시: \"첫 화면입니다. 이름은 필수, 딜러키는 선택입니다.\"
타이핑 포인트: `@bind`, `CanProceed`, `GoNextAsync`, `Navigation.NavigateTo(\"/table\")`.
학생 확인 질문: \"입력값 검증을 버튼 클릭 전에 하는 이유는?\"

7. 파일: `src/Seoul.It.Blackjack.Frontend/Pages/Table.razor`
교사 발화 예시: \"Table 진입 시 자동 접속/자동 입장을 수행하고 상태 이벤트를 받아 렌더링합니다.\"
타이핑 포인트: `OnAfterRenderAsync(firstRender)`, `ConnectAsync`, `JoinAsync`, `StateChanged`/`ErrorReceived` 구독과 해제.
학생 확인 질문: \"왜 `OnInitialized`가 아니라 `OnAfterRenderAsync`에서 자동 join을 하죠?\"

8. 파일: `src/Seoul.It.Blackjack.Frontend/appsettings.json`
교사 발화 예시: \"백엔드 주소를 설정으로 관리합니다. 수업 PC가 바뀌어도 코드 수정 없이 대응 가능합니다.\"
타이핑 포인트: `https://localhost:5000/blackjack` 값 확인.
학생 확인 질문: \"포트가 다르면 어떤 오류를 보게 되나요?\"

### 5.9.6 Frontend.Tests 강의 대본

1. 파일: `src/Seoul.It.Blackjack.Frontend.Tests/PageRenderingTests.cs`
교사 발화 예시: \"최소한 페이지가 200으로 뜨는지부터 확인합니다.\"
타이핑 포인트: `/`, `/table` GET 테스트.
학생 확인 질문: \"이 테스트가 실패하면 라우팅/호스트 중 어디를 먼저 볼까요?\"

2. 파일: `src/Seoul.It.Blackjack.Frontend.Tests/StaticCardAssetsTests.cs`
교사 발화 예시: \"카드 파일이 정적으로 서빙되는지 확인합니다.\"
타이핑 포인트: `/cards/clubs_ace.svg` 요청, content-type 확인.
학생 확인 질문: \"이미지가 안 보일 때 코드와 파일 중 무엇을 먼저 의심해야 하나요?\"

3. 파일: `src/Seoul.It.Blackjack.Frontend.Tests/EntryPageBunitTests.cs`
교사 발화 예시: \"Entry 화면에서 버튼 활성화 조건과 페이지 이동을 검증합니다.\"
타이핑 포인트: `context.Render<Entry>()`, DOM 변경, 클릭, 상태/URI 검증.
학생 확인 질문: \"UI 테스트에서 실제 브라우저가 꼭 필요할까요?\"

4. 파일: `src/Seoul.It.Blackjack.Frontend.Tests/TablePageBunitTests.cs`
교사 발화 예시: \"Table 자동 연결, 버튼 호출, 상태 표시, 오류 표시를 모두 검증합니다.\"
타이핑 포인트: Fake 세션 주입, `RaiseState`, `RaiseError`, 버튼 클릭 후 호출 횟수 검증.
학생 확인 질문: \"Fake 객체로 테스트하는 장점은 무엇인가요?\"

5. 파일: `src/Seoul.It.Blackjack.Frontend.Tests/FakeFrontendGameSession.cs`
교사 발화 예시: \"진짜 서버 대신 호출 횟수와 이벤트 흐름만 확인할 수 있게 만든 테스트 전용 객체입니다.\"
타이핑 포인트: CallCount, RaiseState, RaiseError.
학생 확인 질문: \"이 클래스를 production 코드에 넣지 않는 이유는?\"

### 5.9.7 수업 중 즉시 읽는 디버깅 멘트 모음

1. 연결 실패 시:
\"지금은 문법 문제가 아니라 연결 경로 문제일 가능성이 큽니다. appsettings HubUrl과 backend 포트를 먼저 맞춰봅시다.\"

2. 딜러 시작 실패 시:
\"NOT_DEALER가 오면 정상입니다. 지금 요청 보낸 클라이언트가 딜러인지 상태창에서 먼저 확인하세요.\"

3. 턴 오류 시:
\"NOT_YOUR_TURN은 버그가 아니라 규칙입니다. CurrentTurnPlayerId와 내 PlayerId를 비교해봅시다.\"

4. 카드 소진 시:
\"SHOE_EMPTY는 예외가 아니라 설계된 종료 신호입니다. 라운드를 Idle로 되돌리는지 확인하세요.\"

5. 딜러 퇴장 시:
\"GAME_TERMINATED가 전원에게 1번, 그 다음 초기 상태가 1번 와야 합니다. 이벤트 순서를 꼭 보세요.\"

---

## 6. 학생에게 꼭 강조할 게임 규칙 요약

1. 라운드 시작은 딜러만 가능
2. 딜러 키가 맞는 사용자만 딜러
3. 딜러는 1명만 가능
4. 인게임(InRound) 중에는 신규 Join 불가
5. 점수 21이면 해당 플레이어는 자동으로 행동 종료(Standing)
6. Bust(21 초과)면 Busted
7. 일반 플레이어 모두 행동 종료 후 딜러 자동 진행
8. 라운드 종료 시 Idle
9. 딜러 퇴장 시 `GAME_TERMINATED` 후 상태 초기화

---

## 7. 학생 관점에서 어려운 지점과 설명법

1. `Task`/`async`: "오래 걸리는 일 예약"으로 비유
2. DI: "new를 직접 하지 않고 조립기가 대신 넣어준다"고 설명
3. 인터페이스: "약속서"로 설명
4. Channel: "매점 줄서기" 비유로 직렬 처리 설명
5. 이벤트: "방송 송출, 구독자 수신"으로 설명

---

## 8. 실습 중 자주 나는 오류와 즉시 점검법

1. Hub 메서드명 오타
- 점검: `nameof(IBlackjackServer...)`, `nameof(IBlackjackClient...)`

2. DI 누락
- 점검: `Program.cs`의 `Add...` 등록 확인

3. 연결 전에 명령 호출
- 점검: `ConnectAsync` 선행 여부

4. 이름 검증 실패
- 점검: 공백/길이 1~20

5. 카드 이미지 안 나옴
- 점검: `wwwroot/cards/{suit}_{rank}.svg` 파일명 일치

---

## 9. 수업 진행 시 강사용 체크리스트

1. Backend 실행 포트 확인 (`https://localhost:5000`, `http://localhost:5001`)
2. Frontend `appsettings.json` HubUrl 확인
3. 카드 svg 52장 존재 확인
4. 테스트 실행 명령 준비

권장 명령:

```bash
dotnet build src/Seoul.It.Blackjack.sln -m:1
dotnet test src/Seoul.It.Blackjack.Backend.Tests/Seoul.It.Blackjack.Backend.Tests.csproj -m:1
dotnet test src/Seoul.It.Blackjack.Frontend.Tests/Seoul.It.Blackjack.Frontend.Tests.csproj -m:1
```

---

## 10. 문서 범위와 코드 포함 기준

1. 포함 대상: `src` 하위의 모든 텍스트 코드 파일
2. 제외 대상: 카드 SVG(코드가 아닌 정적 에셋)
3. 누락 방지: 아래 부록은 파일 목록 자동 순회로 생성

---

## 11. 코드 원문 부록 (누락 없음)

아래는 `rg --files src -g '!**/*.svg' | sort` 결과의 전체 파일 원문입니다.

### `src/Directory.Build.props`
```xml
<Project>
	<PropertyGroup>
		<Company>JongHoon</Company>
		<Authors>JongHoon (jonghoon023@gmail.com)</Authors>
		<Copyright>Copyright (c) $(Company). All rights reserved.</Copyright>
		<VersionPrefix>1.0.0.0</VersionPrefix>
	</PropertyGroup>

	<PropertyGroup>
		<PackageProjectUrl>https://github.com/jonghoon023/seoul-it-blackjack</PackageProjectUrl>
		<PackageReadmeFile>README.md</PackageReadmeFile>
		<PackageLicenseFile>LICENSE.txt</PackageLicenseFile>
		<PackageRequireLicenseAcceptance>True</PackageRequireLicenseAcceptance>
		<PackageTags>$(Company)</PackageTags>
		<Description></Description>
		<RepositoryUrl>https://github.com/jonghoon023/seoul-it-blackjack.git</RepositoryUrl>
		<RepositoryType>git</RepositoryType>
		<RepositoryBranch>main</RepositoryBranch>

		<Deterministic>true</Deterministic>
		<DebugSymbols>true</DebugSymbols>
		<DebugType>portable</DebugType>
		<IncludeSymbols>true</IncludeSymbols>
		<EmbedUntrackedSources>true</EmbedUntrackedSources>
		<SymbolPackageFormat>snupkg</SymbolPackageFormat>

		<AllowedOutputExtensionsInPackageBuildOutputFolder>$(AllowedOutputExtensionsInPackageBuildOutputFolder);.pdb</AllowedOutputExtensionsInPackageBuildOutputFolder>
	</PropertyGroup>

	<PropertyGroup>
		<Deterministic>true</Deterministic>
		<GenerateDocumentationFile>true</GenerateDocumentationFile>
		<WarningLevel>5</WarningLevel>
	</PropertyGroup>

	<PropertyGroup Condition="'$(GITHUB_ACTIONS)' == 'true' Or '$(TF_BUILD)' == 'true'">
		<ContinuousIntegrationBuild>true</ContinuousIntegrationBuild>
	</PropertyGroup>

	<PropertyGroup>
		<EnableNETAnalyzers>true</EnableNETAnalyzers>
		<EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
		<AnalysisMode>All</AnalysisMode>
		<AnalysisLevel>latest</AnalysisLevel>
	</PropertyGroup>

	<ItemDefinitionGroup>
		<PackageReference>
			<PrivateAssets>compile</PrivateAssets>
		</PackageReference>
	</ItemDefinitionGroup>

	<ItemGroup Condition="$(IsPackable) == 'true'">
		<None Include="$([MSBuild]::GetPathOfFileAbove('icon.png'))" Pack="true" Visible="false" PackagePath="\" />
		<None Include="$([MSBuild]::GetPathOfFileAbove('README.md'))" Pack="true" Visible="false" PackagePath="\" />
		<None Include="$([MSBuild]::GetPathOfFileAbove('LICENSE.txt'))" Pack="true" Visible="false" PackagePath="\" />

		<PackageReference Include="Microsoft.SourceLink.GitHub" Version="8.0.0">
			<IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
			<PrivateAssets>all</PrivateAssets>
		</PackageReference>
	</ItemGroup>

	<ItemGroup>
		<AdditionalFiles Include="$([MSBuild]::GetPathOfFileAbove('stylecop.json'))" Visible="false" />
	</ItemGroup>

	<PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Debug|AnyCPU' ">
		<DefineConstants>$(DefineConstants);DEBUG;TRACE</DefineConstants>
		<Optimize>false</Optimize>
		<NullableReferenceTypes>true</NullableReferenceTypes>
		<TreatWarningsAsErrors>false</TreatWarningsAsErrors>
		<CodeAnalysisTreatWarningsAsErrors>false</CodeAnalysisTreatWarningsAsErrors>
	</PropertyGroup>

	<PropertyGroup Condition="'$(Configuration)|$(Platform)'=='Release|AnyCPU'">
		<DefineConstants>$(DefineConstants);RELEASE</DefineConstants>
		<Optimize>true</Optimize>
		<TreatWarningsAsErrors>true</TreatWarningsAsErrors>
		<TreatWarningsAsErrors>true</TreatWarningsAsErrors>
		<CodeAnalysisTreatWarningsAsErrors>true</CodeAnalysisTreatWarningsAsErrors>
	</PropertyGroup>
</Project>

```

### `src/Seoul.It.Blackjack.Backend.Tests/ClientDiIntegrationTests.cs`
```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Seoul.It.Blackjack.Client;
using Seoul.It.Blackjack.Client.Extensions;
using Seoul.It.Blackjack.Client.Options;
using Seoul.It.Blackjack.Core.Contracts;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Seoul.It.Blackjack.Backend.Tests;

[TestClass]
public class ClientDiIntegrationTests
{
    [TestMethod]
    public async Task AddBlackjackClient_RegistersSingletonAndAppliesOptions()
    {
        const string hubUrl = "http://localhost/blackjack";

        ServiceCollection services = new();
        services.AddBlackjackClient(options => options.HubUrl = hubUrl);
        await using ServiceProvider provider = services.BuildServiceProvider();

        BlackjackClient first = provider.GetRequiredService<BlackjackClient>();
        BlackjackClient second = provider.GetRequiredService<BlackjackClient>();
        BlackjackClientOptions options = provider.GetRequiredService<BlackjackClientOptions>();

        Assert.AreSame(first, second);
        Assert.AreEqual(hubUrl, options.HubUrl);
    }

    [TestMethod]
    public async Task AddBlackjackClient_ConnectJoin_ReceivesStateChanged()
    {
        using TestHostFactory factory = new();
        Uri hubUrl = new(factory.Server.BaseAddress, "/blackjack");

        ServiceCollection services = new();
        services.AddBlackjackClient(options => options.HubUrl = hubUrl.ToString());
        await using ServiceProvider provider = services.BuildServiceProvider();

        BlackjackClient client = provider.GetRequiredService<BlackjackClient>();
        BlackjackClientOptions options = provider.GetRequiredService<BlackjackClientOptions>();
        TaskCompletionSource<GameState> stateReceived = new(TaskCreationOptions.RunContinuationsAsynchronously);
        client.StateChanged += state => stateReceived.TrySetResult(state);

        await client.ConnectAsync(options.HubUrl, () => factory.Server.CreateHandler());
        await client.JoinAsync("ClientDiUser");

        Task completed = await Task.WhenAny(stateReceived.Task, Task.Delay(3000));
        Assert.AreEqual(stateReceived.Task, completed);

        GameState state = await stateReceived.Task;
        Assert.IsTrue(state.Players.Any(player => player.Name == "ClientDiUser"));
    }
}

```

### `src/Seoul.It.Blackjack.Backend.Tests/ConcurrencyTests.cs`
```csharp
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Seoul.It.Blackjack.Core.Contracts;
using System.Linq;
using System.Threading.Tasks;

namespace Seoul.It.Blackjack.Backend.Tests;

[TestClass]
public class ConcurrencyTests
{
    private const string DealerKey = "DEALER_SECRET_KEY";

    [TestMethod]
    public async Task ConcurrentHitRequests_AreSerializedAndStayConsistent()
    {
        using TestHostFactory factory = new();
        await using SignalRTestClient dealer = new(factory);
        await using SignalRTestClient first = new(factory);
        await using SignalRTestClient second = new(factory);

        await dealer.ConnectAsync();
        await first.ConnectAsync();
        await second.ConnectAsync();
        await dealer.JoinAsync("Dealer", DealerKey);
        await first.JoinAsync("First");
        await second.JoinAsync("Second");
        await dealer.StartRoundAsync();
        await TestWaiter.WaitUntilAsync(() => second.States.Count >= 4);

        int firstStatesBefore = first.States.Count;
        int secondStatesBefore = second.States.Count;

        Task hit1 = first.HitAsync();
        Task hit2 = second.HitAsync();
        await Task.WhenAll(hit1, hit2);

        await TestWaiter.WaitUntilAsync(() =>
            first.States.Count > firstStatesBefore &&
            second.States.Count > secondStatesBefore);
        await TestWaiter.WaitUntilAsync(() => second.Errors.Any(error => error.Code == "NOT_YOUR_TURN"));

        GameState state = first.States.Last();
        Assert.IsTrue(state.Players.Count >= 3);
        Assert.IsTrue(state.Players.Any(player => player.IsDealer));
    }
}

```

### `src/Seoul.It.Blackjack.Backend.Tests/DealerTerminationTests.cs`
```csharp
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Seoul.It.Blackjack.Core.Contracts;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Seoul.It.Blackjack.Backend.Tests;

[TestClass]
public class DealerTerminationTests
{
    private const string DealerKey = "DEALER_SECRET_KEY";

    [TestMethod]
    public async Task DealerLeave_BroadcastsGameTerminated_AndClearsState()
    {
        using TestHostFactory factory = new();
        await using SignalRTestClient dealer = new(factory);
        await using SignalRTestClient player = new(factory);

        await dealer.ConnectAsync();
        await player.ConnectAsync();
        await dealer.JoinAsync("Dealer", DealerKey);
        await player.JoinAsync("Player");
        await TestWaiter.WaitUntilAsync(() => player.States.Count >= 2);

        await dealer.LeaveAsync();
        await TestWaiter.WaitUntilAsync(() => player.Errors.Any(error => error.Code == "GAME_TERMINATED"));
        await TestWaiter.WaitUntilAsync(() => player.States.Count >= 3);

        GameState state = player.States.Last();
        Assert.AreEqual(GamePhase.Idle, state.Phase);
        Assert.AreEqual(0, state.Players.Count);
    }

    [TestMethod]
    public async Task DealerDisconnect_BroadcastsGameTerminated_AndClearsState()
    {
        using TestHostFactory factory = new();
        await using SignalRTestClient dealer = new(factory);
        await using SignalRTestClient player = new(factory);

        await dealer.ConnectAsync();
        await player.ConnectAsync();
        await dealer.JoinAsync("Dealer", DealerKey);
        await player.JoinAsync("Player");
        await TestWaiter.WaitUntilAsync(() => player.States.Count >= 2);

        await dealer.DisposeAsync();
        await TestWaiter.WaitUntilAsync(() => player.Errors.Any(error => error.Code == "GAME_TERMINATED"));
        await TestWaiter.WaitUntilAsync(() => player.States.Count >= 3);

        GameState state = player.States.Last();
        Assert.AreEqual(GamePhase.Idle, state.Phase);
        Assert.AreEqual(0, state.Players.Count);
    }

    [TestMethod]
    public async Task DealerLeave_BroadcastsTerminationAndResetExactlyOnce_InOrder()
    {
        using TestHostFactory factory = new();
        await using SignalRTestClient dealer = new(factory);
        await using SignalRTestClient player = new(factory);

        await dealer.ConnectAsync();
        await player.ConnectAsync();
        await dealer.JoinAsync("Dealer", DealerKey);
        await player.JoinAsync("Player");
        await TestWaiter.WaitUntilAsync(() => player.States.Count >= 2);

        int stateBefore = player.States.Count;
        int errorBefore = player.Errors.Count(value => value.Code == "GAME_TERMINATED");
        int eventBefore = player.Events.Count;

        await dealer.LeaveAsync();
        await TestWaiter.WaitUntilAsync(() =>
            player.Errors.Count(value => value.Code == "GAME_TERMINATED") == errorBefore + 1);
        await TestWaiter.WaitUntilAsync(() => player.States.Count == stateBefore + 1);

        Assert.AreEqual(errorBefore + 1, player.Errors.Count(value => value.Code == "GAME_TERMINATED"));

        List<(string Type, string? Code)> events = player.Events.Skip(eventBefore).ToList();
        Assert.AreEqual(2, events.Count);
        Assert.AreEqual(nameof(IBlackjackClient.OnError), events[0].Type);
        Assert.AreEqual("GAME_TERMINATED", events[0].Code);
        Assert.AreEqual(nameof(IBlackjackClient.OnStateChanged), events[1].Type);
    }

    [TestMethod]
    public async Task PlayerLeave_RemovesOnlyThatPlayer()
    {
        using TestHostFactory factory = new();
        await using SignalRTestClient dealer = new(factory);
        await using SignalRTestClient player = new(factory);

        await dealer.ConnectAsync();
        await player.ConnectAsync();
        await dealer.JoinAsync("Dealer", DealerKey);
        await player.JoinAsync("Player");
        await TestWaiter.WaitUntilAsync(() => dealer.States.Count >= 2);

        await player.LeaveAsync();
        await TestWaiter.WaitUntilAsync(() => dealer.States.Count >= 3);

        GameState state = dealer.States.Last();
        Assert.AreEqual(1, state.Players.Count);
        Assert.IsTrue(state.Players.Single().IsDealer);
    }

    [TestMethod]
    public async Task PlayerDisconnect_RemovesOnlyThatPlayer()
    {
        using TestHostFactory factory = new();
        await using SignalRTestClient dealer = new(factory);
        await using SignalRTestClient player = new(factory);

        await dealer.ConnectAsync();
        await player.ConnectAsync();
        await dealer.JoinAsync("Dealer", DealerKey);
        await player.JoinAsync("Player");
        await TestWaiter.WaitUntilAsync(() => dealer.States.Count >= 2);

        await player.DisposeAsync();
        await TestWaiter.WaitUntilAsync(() => dealer.States.Count >= 3);

        GameState state = dealer.States.Last();
        Assert.AreEqual(1, state.Players.Count);
        Assert.IsTrue(state.Players.Single().IsDealer);
    }
}

```

### `src/Seoul.It.Blackjack.Backend.Tests/GameRuleOptionsDefaultTests.cs`
```csharp
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;

namespace Seoul.It.Blackjack.Backend.Tests;

[TestClass]
public class GameRuleOptionsDefaultTests
{
    private const string DealerKey = "DEALER_SECRET_KEY";

    [TestMethod]
    public async Task StartRound_WithOnlyDealer_UsesDefaultMinPlayerRule()
    {
        using TestHostFactory factory = new();
        await using SignalRTestClient dealer = new(factory);

        await dealer.ConnectAsync();
        await dealer.JoinAsync("Dealer", DealerKey);
        await TestWaiter.WaitUntilAsync(() => dealer.States.Count >= 1);

        await dealer.StartRoundAsync();
        await TestWaiter.WaitUntilAsync(() => dealer.Errors.Count >= 1);

        Assert.AreEqual("INSUFFICIENT_PLAYERS", dealer.Errors.Last().Code);
    }
}

```

### `src/Seoul.It.Blackjack.Backend.Tests/JoinAndRoleTests.cs`
```csharp
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Seoul.It.Blackjack.Core.Contracts;
using System.Linq;
using System.Threading.Tasks;

namespace Seoul.It.Blackjack.Backend.Tests;

[TestClass]
public class JoinAndRoleTests
{
    private const string DealerKey = "DEALER_SECRET_KEY";

    [TestMethod]
    public async Task Join_WithDealerKey_AssignsDealerRole()
    {
        using TestHostFactory factory = new();
        await using SignalRTestClient dealer = new(factory);

        await dealer.ConnectAsync();
        await dealer.JoinAsync("Dealer", DealerKey);
        await TestWaiter.WaitUntilAsync(() => dealer.States.Count >= 1);

        GameState state = dealer.States.Last();
        Assert.AreEqual(1, state.Players.Count);
        Assert.IsTrue(state.Players.Single().IsDealer);
    }

    [TestMethod]
    public async Task Join_WithInvalidDealerKey_JoinsAsPlayer()
    {
        using TestHostFactory factory = new();
        await using SignalRTestClient player = new(factory);

        await player.ConnectAsync();
        await player.JoinAsync("Player", "WRONG_KEY");
        await TestWaiter.WaitUntilAsync(() => player.States.Count >= 1);

        GameState state = player.States.Last();
        Assert.IsFalse(state.Players.Single().IsDealer);
    }

    [TestMethod]
    public async Task Join_WithEmptyName_ReturnsInvalidNameError()
    {
        using TestHostFactory factory = new();
        await using SignalRTestClient client = new(factory);

        await client.ConnectAsync();
        await client.JoinAsync("   ");
        await TestWaiter.WaitUntilAsync(() => client.Errors.Count >= 1);

        Assert.AreEqual("INVALID_NAME", client.Errors.Last().Code);
    }

    [TestMethod]
    public async Task Join_WhenDealerAlreadyExists_ReturnsDealerAlreadyExistsError()
    {
        using TestHostFactory factory = new();
        await using SignalRTestClient dealer = new(factory);
        await using SignalRTestClient another = new(factory);

        await dealer.ConnectAsync();
        await another.ConnectAsync();
        await dealer.JoinAsync("Dealer", DealerKey);
        await TestWaiter.WaitUntilAsync(() => dealer.States.Count >= 1);

        await another.JoinAsync("AnotherDealer", DealerKey);
        await TestWaiter.WaitUntilAsync(() => another.Errors.Count >= 1);

        Assert.AreEqual("DEALER_ALREADY_EXISTS", another.Errors.Last().Code);
    }

    [TestMethod]
    public async Task Join_WhenSameConnectionJoinsTwice_ReturnsAlreadyJoinedError()
    {
        using TestHostFactory factory = new();
        await using SignalRTestClient client = new(factory);

        await client.ConnectAsync();
        await client.JoinAsync("Player");
        await TestWaiter.WaitUntilAsync(() => client.States.Count >= 1);

        await client.JoinAsync("PlayerAgain");
        await TestWaiter.WaitUntilAsync(() => client.Errors.Count >= 1);

        Assert.AreEqual("ALREADY_JOINED", client.Errors.Last().Code);
    }
}

```

### `src/Seoul.It.Blackjack.Backend.Tests/RoundCompletionTests.cs`
```csharp
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Seoul.It.Blackjack.Core.Contracts;
using System.Linq;
using System.Threading.Tasks;

namespace Seoul.It.Blackjack.Backend.Tests;

[TestClass]
public class RoundCompletionTests
{
    private const string DealerKey = "DEALER_SECRET_KEY";

    [TestMethod]
    public async Task Stand_CompletesRound_AndMovesToIdle()
    {
        using TestHostFactory factory = new();
        await using SignalRTestClient dealer = new(factory);
        await using SignalRTestClient player = new(factory);

        await dealer.ConnectAsync();
        await player.ConnectAsync();
        await dealer.JoinAsync("Dealer", DealerKey);
        await player.JoinAsync("Player");
        await dealer.StartRoundAsync();
        await TestWaiter.WaitUntilAsync(() => player.States.Count >= 3);

        await player.StandAsync();
        await TestWaiter.WaitUntilAsync(() => player.States.Count >= 4);

        GameState state = player.States.Last();
        Assert.AreEqual(GamePhase.Idle, state.Phase);
        Assert.IsTrue(string.IsNullOrEmpty(state.CurrentTurnPlayerId));
        Assert.AreEqual(2, state.Players.Count);
        Assert.AreNotEqual(
            RoundOutcome.None,
            state.Players.Single(value => !value.IsDealer).Outcome);
    }

    [TestMethod]
    public async Task NextRound_ResetsRoundFields_ButKeepsIdleSnapshotBeforeStart()
    {
        using TestHostFactory factory = new();
        await using SignalRTestClient dealer = new(factory);
        await using SignalRTestClient player = new(factory);

        await dealer.ConnectAsync();
        await player.ConnectAsync();
        await dealer.JoinAsync("Dealer", DealerKey);
        await player.JoinAsync("Player");
        await dealer.StartRoundAsync();
        await TestWaiter.WaitUntilAsync(() => player.States.Count >= 3);

        await player.StandAsync();
        await TestWaiter.WaitUntilAsync(() => player.States.Count >= 4);

        GameState idleState = player.States.Last();
        PlayerState idlePlayer = idleState.Players.Single(value => !value.IsDealer);
        Assert.IsTrue(idlePlayer.Cards.Count >= 2);
        Assert.AreNotEqual(RoundOutcome.None, idlePlayer.Outcome);

        await dealer.StartRoundAsync();
        await TestWaiter.WaitUntilAsync(() => player.States.Count >= 5);

        GameState nextRoundState = player.States.Last();
        PlayerState nextRoundPlayer = nextRoundState.Players.Single(value => !value.IsDealer);
        Assert.AreEqual(GamePhase.InRound, nextRoundState.Phase);
        Assert.AreEqual(2, nextRoundPlayer.Cards.Count);
        Assert.AreEqual(RoundOutcome.None, nextRoundPlayer.Outcome);
    }
}

```

### `src/Seoul.It.Blackjack.Backend.Tests/Seoul.It.Blackjack.Backend.Tests.csproj`
```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="8.0.18" />
    <PackageReference Include="Microsoft.AspNetCore.SignalR.Client" Version="8.0.18" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.12.0" />
    <PackageReference Include="MSTest.TestAdapter" Version="3.7.3" />
    <PackageReference Include="MSTest.TestFramework" Version="3.7.3" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\Seoul.It.Blackjack.Backend\Seoul.It.Blackjack.Backend.csproj" />
    <ProjectReference Include="..\Seoul.It.Blackjack.Client\Seoul.It.Blackjack.Client.csproj" />
  </ItemGroup>

</Project>

```

### `src/Seoul.It.Blackjack.Backend.Tests/SignalRTestClient.cs`
```csharp
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.TestHost;
using Seoul.It.Blackjack.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Seoul.It.Blackjack.Backend.Tests;

internal sealed class SignalRTestClient : IAsyncDisposable
{
    private readonly HubConnection _connection;

    public SignalRTestClient(TestHostFactory factory)
    {
        Uri hubUrl = new(factory.Server.BaseAddress, "/blackjack");
        _connection = new HubConnectionBuilder()
            .WithUrl(
                hubUrl,
                options => options.HttpMessageHandlerFactory = _ => factory.Server.CreateHandler())
            .Build();

        _connection.On<GameState>(nameof(IBlackjackClient.OnStateChanged), state =>
        {
            Events.Add((nameof(IBlackjackClient.OnStateChanged), null));
            States.Add(state);
        });
        _connection.On<string, string>(nameof(IBlackjackClient.OnError), (code, message) =>
        {
            Events.Add((nameof(IBlackjackClient.OnError), code));
            Errors.Add((code, message));
        });
    }

    public List<GameState> States { get; } = new();

    public List<(string Code, string Message)> Errors { get; } = new();

    public List<(string Type, string? Code)> Events { get; } = new();

    public Task ConnectAsync() => _connection.StartAsync();

    public Task JoinAsync(string name, string? dealerKey = null) => _connection.InvokeAsync(nameof(IBlackjackServer.Join), name, dealerKey);

    public Task LeaveAsync() => _connection.InvokeAsync(nameof(IBlackjackServer.Leave));

    public Task StartRoundAsync() => _connection.InvokeAsync(nameof(IBlackjackServer.StartRound));

    public Task HitAsync() => _connection.InvokeAsync(nameof(IBlackjackServer.Hit));

    public Task StandAsync() => _connection.InvokeAsync(nameof(IBlackjackServer.Stand));

    public ValueTask DisposeAsync() => _connection.DisposeAsync();
}

```

### `src/Seoul.It.Blackjack.Backend.Tests/TestHostFactory.cs`
```csharp
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Seoul.It.Blackjack.Backend;

namespace Seoul.It.Blackjack.Backend.Tests;

internal sealed class TestHostFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
    }
}

```

### `src/Seoul.It.Blackjack.Backend.Tests/TestWaiter.cs`
```csharp
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

```

### `src/Seoul.It.Blackjack.Backend.Tests/TurnRuleTests.cs`
```csharp
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;

namespace Seoul.It.Blackjack.Backend.Tests;

[TestClass]
public class TurnRuleTests
{
    private const string DealerKey = "DEALER_SECRET_KEY";

    [TestMethod]
    public async Task StartRound_ByNonDealer_ReturnsNotDealer()
    {
        using TestHostFactory factory = new();
        await using SignalRTestClient dealer = new(factory);
        await using SignalRTestClient player = new(factory);

        await dealer.ConnectAsync();
        await player.ConnectAsync();
        await dealer.JoinAsync("Dealer", DealerKey);
        await player.JoinAsync("Player");
        await TestWaiter.WaitUntilAsync(() => player.States.Count >= 2);

        await player.StartRoundAsync();
        await TestWaiter.WaitUntilAsync(() => player.Errors.Count >= 1);

        Assert.AreEqual("NOT_DEALER", player.Errors.Last().Code);
    }

    [TestMethod]
    public async Task Join_InRound_ReturnsGameInProgress()
    {
        using TestHostFactory factory = new();
        await using SignalRTestClient dealer = new(factory);
        await using SignalRTestClient player = new(factory);
        await using SignalRTestClient lateJoiner = new(factory);

        await dealer.ConnectAsync();
        await player.ConnectAsync();
        await lateJoiner.ConnectAsync();
        await dealer.JoinAsync("Dealer", DealerKey);
        await player.JoinAsync("Player");
        await dealer.StartRoundAsync();
        await TestWaiter.WaitUntilAsync(() => dealer.States.Count >= 3);

        await lateJoiner.JoinAsync("Late");
        await TestWaiter.WaitUntilAsync(() => lateJoiner.Errors.Count >= 1);

        Assert.AreEqual("GAME_IN_PROGRESS", lateJoiner.Errors.Last().Code);
    }

    [TestMethod]
    public async Task Hit_OutOfTurn_ReturnsNotYourTurn()
    {
        using TestHostFactory factory = new();
        await using SignalRTestClient dealer = new(factory);
        await using SignalRTestClient first = new(factory);
        await using SignalRTestClient second = new(factory);

        await dealer.ConnectAsync();
        await first.ConnectAsync();
        await second.ConnectAsync();
        await dealer.JoinAsync("Dealer", DealerKey);
        await first.JoinAsync("First");
        await second.JoinAsync("Second");
        await dealer.StartRoundAsync();
        await TestWaiter.WaitUntilAsync(() => second.States.Count >= 4);

        await second.HitAsync();
        await TestWaiter.WaitUntilAsync(() => second.Errors.Count >= 1);

        Assert.AreEqual("NOT_YOUR_TURN", second.Errors.Last().Code);
    }

    [TestMethod]
    public async Task Hit_ByDealer_ReturnsDealerIsAuto()
    {
        using TestHostFactory factory = new();
        await using SignalRTestClient dealer = new(factory);
        await using SignalRTestClient player = new(factory);

        await dealer.ConnectAsync();
        await player.ConnectAsync();
        await dealer.JoinAsync("Dealer", DealerKey);
        await player.JoinAsync("Player");
        await dealer.StartRoundAsync();
        await TestWaiter.WaitUntilAsync(() => dealer.States.Count >= 3);

        await dealer.HitAsync();
        await TestWaiter.WaitUntilAsync(() => dealer.Errors.Count >= 1);

        Assert.AreEqual("DEALER_IS_AUTO", dealer.Errors.Last().Code);
    }

    [TestMethod]
    public async Task Hit_BroadcastsStateToAllClients()
    {
        using TestHostFactory factory = new();
        await using SignalRTestClient dealer = new(factory);
        await using SignalRTestClient player = new(factory);
        await using SignalRTestClient observer = new(factory);

        await dealer.ConnectAsync();
        await player.ConnectAsync();
        await observer.ConnectAsync();
        await dealer.JoinAsync("Dealer", DealerKey);
        await player.JoinAsync("Player");
        await observer.JoinAsync("Observer");
        await dealer.StartRoundAsync();
        await TestWaiter.WaitUntilAsync(() => observer.States.Count >= 4);

        int dealerBefore = dealer.States.Count;
        int playerBefore = player.States.Count;
        int observerBefore = observer.States.Count;

        await player.HitAsync();
        await TestWaiter.WaitUntilAsync(() =>
            dealer.States.Count > dealerBefore &&
            player.States.Count > playerBefore &&
            observer.States.Count > observerBefore);
    }
}

```

### `src/Seoul.It.Blackjack.Backend/Extensions/ServiceCollectionExtensions.cs`
```csharp
using Seoul.It.Blackjack.Backend.Options;

namespace Seoul.It.Blackjack.Backend.Extensions;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDealerOptions(this IServiceCollection services, IConfiguration configuration)
    {
        return services.Configure<DealerOptions>(configuration.GetSection(DealerOptions.DefaultSectionName));
    }

    public static IServiceCollection AddGameRuleOptions(this IServiceCollection services, IConfiguration configuration)
    {
        return services.Configure<GameRuleOptions>(configuration.GetSection(GameRuleOptions.DefaultSectionName));
    }
}

```

### `src/Seoul.It.Blackjack.Backend/Hubs/GameSessionHub.cs`
```csharp
using Microsoft.AspNetCore.SignalR;
using Seoul.It.Blackjack.Backend.Services;
using Seoul.It.Blackjack.Backend.Services.Commands;
using Seoul.It.Blackjack.Backend.Services.Exceptions;
using Seoul.It.Blackjack.Core.Contracts;
using System;
using System.Threading.Tasks;

namespace Seoul.It.Blackjack.Backend.Hubs;

internal sealed class GameSessionHub(IGameRoomService room) : Hub<IBlackjackClient>, IBlackjackServer
{
    public const string Endpoint = "/blackjack";

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            GameOperationResult result = await room.DisconnectAsync(Context.ConnectionId);
            await BroadcastResultAsync(result);
        }
        catch (GameRoomException)
        {
            // 연결 종료 중 에러는 무시한다.
        }

        await base.OnDisconnectedAsync(exception);
    }

    public Task Join(string name, string? dealerKey) =>
        ExecuteAsync(() => room.JoinAsync(Context.ConnectionId, name, dealerKey));

    public Task Leave() =>
        ExecuteAsync(() => room.LeaveAsync(Context.ConnectionId));

    public Task StartRound() =>
        ExecuteAsync(() => room.StartRoundAsync(Context.ConnectionId));

    public Task Hit() =>
        ExecuteAsync(() => room.HitAsync(Context.ConnectionId));

    public Task Stand() =>
        ExecuteAsync(() => room.StandAsync(Context.ConnectionId));

    private async Task ExecuteAsync(Func<Task<GameOperationResult>> action)
    {
        try
        {
            GameOperationResult result = await action();
            await BroadcastResultAsync(result);
        }
        catch (GameRoomException ex)
        {
            await Clients.Caller.OnError(ex.Code, ex.Message);
        }
    }

    private async Task BroadcastResultAsync(GameOperationResult result)
    {
        if (result.Notice is not null)
        {
            await Clients.All.OnError(result.Notice.Code, result.Notice.Message);
        }

        if (result.ShouldPublishState)
        {
            await Clients.All.OnStateChanged(result.State);
        }
    }
}

```

### `src/Seoul.It.Blackjack.Backend/Models/Deck.cs`
```csharp
using Seoul.It.Blackjack.Core.Domain;

namespace Seoul.It.Blackjack.Backend.Models;

internal sealed class Deck
{
    public static IEnumerable<Card> Cards => Enum.GetValues<Suit>()
        .SelectMany(suit => Enum.GetValues<Rank>().Select(rank => new Card(suit, rank)));
}

```

### `src/Seoul.It.Blackjack.Backend/Models/Shoe.cs`
```csharp
using Seoul.It.Blackjack.Core.Domain;
using System.Collections.Concurrent;

namespace Seoul.It.Blackjack.Backend.Models;

internal sealed class Shoe
{
    private readonly ConcurrentStack<Card> _cards;

    public Shoe(int deckCount)
    {
        List<Card> cards = [];
        for (int i = 0; i < deckCount; i++)
        {
            cards.AddRange(Deck.Cards);
        }

        Card[] cardArray = cards.ToArray();
        Random.Shared.Shuffle(cardArray);
        _cards = new ConcurrentStack<Card>(cardArray);
    }

    public Card Draw()
    {
        return TryDraw(out Card card)
            ? card
            : throw new InvalidOperationException("Shoe 에 카드가 더 이상 없습니다.");
    }

    public bool TryDraw(out Card card)
    {
        if (_cards.TryPop(out Card? popped) && popped is not null)
        {
            card = popped;
            return true;
        }

        card = null!;
        return false;
    }
}

```

### `src/Seoul.It.Blackjack.Backend/Options/DealerOptions.cs`
```csharp
namespace Seoul.It.Blackjack.Backend.Options;

public sealed class DealerOptions
{
    public const string DefaultSectionName = "Dealer";

    public string? Key { get; set; }
}

```

### `src/Seoul.It.Blackjack.Backend/Options/GameRuleOptions.cs`
```csharp
namespace Seoul.It.Blackjack.Backend.Options;

internal sealed class GameRuleOptions
{
    public const string DefaultSectionName = "GameRules";

    public int DeckCount { get; set; } = 4;

    public int DealerStandScore { get; set; } = 17;

    public int MinPlayersToStart { get; set; } = 2;

    public int MinNameLength { get; set; } = 1;

    public int MaxNameLength { get; set; } = 20;
}

```

### `src/Seoul.It.Blackjack.Backend/Program.cs`
```csharp
using Seoul.It.Blackjack.Backend.Extensions;
using Seoul.It.Blackjack.Backend.Hubs;
using Seoul.It.Blackjack.Backend.Services;
using Seoul.It.Blackjack.Backend.Services.Infrastructure;
using Seoul.It.Blackjack.Backend.Services.Round;
using Seoul.It.Blackjack.Backend.Services.Rules;
using Seoul.It.Blackjack.Backend.Services.State;

namespace Seoul.It.Blackjack.Backend;

public partial class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddSignalR();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddDealerOptions(builder.Configuration);
        builder.Services.AddGameRuleOptions(builder.Configuration);
        builder.Services.AddSingleton<ConnectionRegistry>();
        builder.Services.AddSingleton<IGameRuleValidator, GameRuleValidator>();
        builder.Services.AddSingleton<IRoundEngine, RoundEngine>();
        builder.Services.AddSingleton<IGameStateSnapshotFactory, GameStateSnapshotFactory>();
        builder.Services.AddSingleton<IGameCommandProcessor, ChannelGameCommandProcessor>();
        builder.Services.AddSingleton<IGameRoomService, GameRoomService>();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.MapHub<GameSessionHub>(GameSessionHub.Endpoint);
        app.Run();
    }
}

```

### `src/Seoul.It.Blackjack.Backend/Properties/launchSettings.json`
```json
{
  "$schema": "http://json.schemastore.org/launchsettings.json",
  "iisSettings": {
    "windowsAuthentication": false,
    "anonymousAuthentication": true,
    "iisExpress": {
      "applicationUrl": "http://localhost:17271",
      "sslPort": 44345
    }
  },
  "profiles": {
    "http": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "launchUrl": "swagger",
      "applicationUrl": "http://localhost:5001",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    "https": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "launchUrl": "swagger",
      "applicationUrl": "https://localhost:5000;http://localhost:5001",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    "IIS Express": {
      "commandName": "IISExpress",
      "launchBrowser": true,
      "launchUrl": "swagger",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

### `src/Seoul.It.Blackjack.Backend/Seoul.It.Blackjack.Backend.csproj`
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Swashbuckle.AspNetCore" Version="6.6.2" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\Seoul.It.Blackjack.Core\Seoul.It.Blackjack.Core.csproj" />
  </ItemGroup>

</Project>

```

### `src/Seoul.It.Blackjack.Backend/Seoul.It.Blackjack.Backend.http`
```http
@Seoul.It.Blackjack.Backend_HostAddress = http://localhost:5223

GET {{Seoul.It.Blackjack.Backend_HostAddress}}/weatherforecast/
Accept: application/json

###

```

### `src/Seoul.It.Blackjack.Backend/Services/Commands/GameCommand.cs`
```csharp
namespace Seoul.It.Blackjack.Backend.Services.Commands;

internal sealed class GameCommand
{
    public GameCommand(
        GameCommandType type,
        string connectionId,
        string? name = null,
        string? dealerKey = null)
    {
        Type = type;
        ConnectionId = connectionId;
        Name = name;
        DealerKey = dealerKey;
    }

    public GameCommandType Type { get; }

    public string ConnectionId { get; }

    public string? Name { get; }

    public string? DealerKey { get; }
}

```

### `src/Seoul.It.Blackjack.Backend/Services/Commands/GameCommandType.cs`
```csharp
namespace Seoul.It.Blackjack.Backend.Services.Commands;

internal enum GameCommandType
{
    Join,
    Leave,
    StartRound,
    Hit,
    Stand,
    Disconnect
}

```

### `src/Seoul.It.Blackjack.Backend/Services/Commands/GameNotice.cs`
```csharp
namespace Seoul.It.Blackjack.Backend.Services.Commands;

internal sealed class GameNotice
{
    public GameNotice(string code, string message)
    {
        Code = code;
        Message = message;
    }

    public string Code { get; }

    public string Message { get; }
}

```

### `src/Seoul.It.Blackjack.Backend/Services/Commands/GameOperationResult.cs`
```csharp
using Seoul.It.Blackjack.Core.Contracts;

namespace Seoul.It.Blackjack.Backend.Services.Commands;

internal sealed class GameOperationResult
{
    public GameState State { get; set; } = new();

    public bool ShouldPublishState { get; set; } = true;

    public GameNotice? Notice { get; set; }
}

```

### `src/Seoul.It.Blackjack.Backend/Services/ConnectionRegistry.cs`
```csharp
using System.Collections.Generic;

namespace Seoul.It.Blackjack.Backend.Services;

internal sealed class ConnectionRegistry
{
    private readonly object _sync = new();
    private readonly Dictionary<string, string> _connectionToPlayer = new();

    public bool ContainsConnection(string connectionId)
    {
        lock (_sync)
        {
            return _connectionToPlayer.ContainsKey(connectionId);
        }
    }

    public void Add(string connectionId, string playerId)
    {
        lock (_sync)
        {
            _connectionToPlayer[connectionId] = playerId;
        }
    }

    public bool TryRemove(string connectionId, out string playerId)
    {
        lock (_sync)
        {
            if (_connectionToPlayer.TryGetValue(connectionId, out string? id))
            {
                _connectionToPlayer.Remove(connectionId);
                playerId = id;
                return true;
            }

            playerId = string.Empty;
            return false;
        }
    }

    public void Clear()
    {
        lock (_sync)
        {
            _connectionToPlayer.Clear();
        }
    }
}

```

### `src/Seoul.It.Blackjack.Backend/Services/Exceptions/GameAuthorizationException.cs`
```csharp
namespace Seoul.It.Blackjack.Backend.Services.Exceptions;

internal sealed class GameAuthorizationException : GameRoomException
{
    public GameAuthorizationException(string code, string message)
        : base(code, message)
    {
    }
}

```

### `src/Seoul.It.Blackjack.Backend/Services/Exceptions/GameRoomException.cs`
```csharp
using System;

namespace Seoul.It.Blackjack.Backend.Services.Exceptions;

internal class GameRoomException : Exception
{
    public GameRoomException(string code, string message)
        : base(message)
    {
        Code = code;
    }

    public string Code { get; }
}

```

### `src/Seoul.It.Blackjack.Backend/Services/Exceptions/GameRuleException.cs`
```csharp
namespace Seoul.It.Blackjack.Backend.Services.Exceptions;

internal sealed class GameRuleException : GameRoomException
{
    public GameRuleException(string code, string message)
        : base(code, message)
    {
    }
}

```

### `src/Seoul.It.Blackjack.Backend/Services/Exceptions/GameValidationException.cs`
```csharp
namespace Seoul.It.Blackjack.Backend.Services.Exceptions;

internal sealed class GameValidationException : GameRoomException
{
    public GameValidationException(string code, string message)
        : base(code, message)
    {
    }
}

```

### `src/Seoul.It.Blackjack.Backend/Services/GameRoomService.cs`
```csharp
using Microsoft.Extensions.Options;
using Seoul.It.Blackjack.Backend.Models;
using Seoul.It.Blackjack.Backend.Options;
using Seoul.It.Blackjack.Backend.Services.Commands;
using Seoul.It.Blackjack.Backend.Services.Exceptions;
using Seoul.It.Blackjack.Backend.Services.Infrastructure;
using Seoul.It.Blackjack.Backend.Services.Round;
using Seoul.It.Blackjack.Backend.Services.Rules;
using Seoul.It.Blackjack.Backend.Services.State;
using Seoul.It.Blackjack.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Seoul.It.Blackjack.Backend.Services;

internal sealed class GameRoomService : IGameRoomService
{
    private readonly DealerOptions _dealerOptions;
    private readonly GameRuleOptions _gameRuleOptions;
    private readonly ConnectionRegistry _connections;
    private readonly IGameRuleValidator _validator;
    private readonly IRoundEngine _roundEngine;
    private readonly IGameStateSnapshotFactory _snapshotFactory;
    private readonly IGameCommandProcessor _commandProcessor;
    private readonly List<PlayerState> _players = new();
    private Shoe? _shoe;
    private GamePhase _phase = GamePhase.Idle;
    private string _dealerPlayerId = string.Empty;
    private string _currentTurnPlayerId = string.Empty;
    private string _statusMessage = string.Empty;

    public GameRoomService(
        IOptions<DealerOptions> dealerOptions,
        IOptions<GameRuleOptions> gameRuleOptions,
        ConnectionRegistry connections,
        IGameRuleValidator validator,
        IRoundEngine roundEngine,
        IGameStateSnapshotFactory snapshotFactory,
        IGameCommandProcessor commandProcessor)
    {
        _dealerOptions = dealerOptions.Value;
        _gameRuleOptions = gameRuleOptions.Value;
        _connections = connections;
        _validator = validator;
        _roundEngine = roundEngine;
        _snapshotFactory = snapshotFactory;
        _commandProcessor = commandProcessor;
    }

    public Task<GameOperationResult> JoinAsync(string connectionId, string name, string? dealerKey)
    {
        GameCommand command = new(GameCommandType.Join, connectionId, name, dealerKey);
        return _commandProcessor.EnqueueAsync(command, () => HandleJoin(command));
    }

    public Task<GameOperationResult> LeaveAsync(string connectionId)
    {
        GameCommand command = new(GameCommandType.Leave, connectionId);
        return _commandProcessor.EnqueueAsync(command, () => HandleLeave(command, false));
    }

    public Task<GameOperationResult> DisconnectAsync(string connectionId)
    {
        GameCommand command = new(GameCommandType.Disconnect, connectionId);
        return _commandProcessor.EnqueueAsync(command, () => HandleLeave(command, true));
    }

    public Task<GameOperationResult> StartRoundAsync(string connectionId)
    {
        GameCommand command = new(GameCommandType.StartRound, connectionId);
        return _commandProcessor.EnqueueAsync(command, () => HandleStartRound(command));
    }

    public Task<GameOperationResult> HitAsync(string connectionId)
    {
        GameCommand command = new(GameCommandType.Hit, connectionId);
        return _commandProcessor.EnqueueAsync(command, () => HandleHit(command));
    }

    public Task<GameOperationResult> StandAsync(string connectionId)
    {
        GameCommand command = new(GameCommandType.Stand, connectionId);
        return _commandProcessor.EnqueueAsync(command, () => HandleStand(command));
    }

    private GameOperationResult HandleJoin(GameCommand command)
    {
        if (_phase != GamePhase.Idle)
        {
            throw new GameRuleException("GAME_IN_PROGRESS", "게임이 진행 중이라 참가할 수 없습니다.");
        }

        if (_connections.ContainsConnection(command.ConnectionId))
        {
            throw new GameValidationException("ALREADY_JOINED", "이미 참가한 연결입니다.");
        }

        string normalizedName = _validator.NormalizeName(
            command.Name,
            _gameRuleOptions.MinNameLength,
            _gameRuleOptions.MaxNameLength);
        bool requestedDealer = IsDealerKeyMatched(command.DealerKey);
        bool hasDealer = _players.Any(player => player.IsDealer);
        if (requestedDealer && hasDealer)
        {
            throw new GameRuleException("DEALER_ALREADY_EXISTS", "이미 딜러가 존재합니다.");
        }

        PlayerState player = new()
        {
            PlayerId = command.ConnectionId,
            Name = normalizedName,
            IsDealer = requestedDealer,
            Score = 0,
            TurnState = PlayerTurnState.Playing,
            Outcome = RoundOutcome.None,
        };
        _players.Add(player);
        _connections.Add(command.ConnectionId, player.PlayerId);
        if (player.IsDealer)
        {
            _dealerPlayerId = player.PlayerId;
        }

        _statusMessage = player.IsDealer ? "딜러가 참가했습니다." : "플레이어가 참가했습니다.";
        return CreateResult();
    }

    private GameOperationResult HandleLeave(GameCommand command, bool isDisconnect)
    {
        if (!_connections.TryRemove(command.ConnectionId, out string _))
        {
            return CreateSilentResult();
        }

        PlayerState? leavingPlayer = _players.SingleOrDefault(player => player.PlayerId == command.ConnectionId);
        if (leavingPlayer is null)
        {
            return CreateSilentResult();
        }

        _players.Remove(leavingPlayer);
        if (leavingPlayer.IsDealer)
        {
            _players.Clear();
            _connections.Clear();
            ResetToIdle(clearDealer: true, clearCurrentTurn: true);
            _statusMessage = "딜러 퇴장으로 게임이 종료되었습니다.";
            return CreateResult(new GameNotice("GAME_TERMINATED", "딜러가 퇴장하여 게임이 종료되었습니다."));
        }

        if (_phase == GamePhase.InRound && _currentTurnPlayerId == leavingPlayer.PlayerId)
        {
            _currentTurnPlayerId = _roundEngine.ResolveNextTurnPlayerId(_players);
        }

        if (_phase == GamePhase.InRound && CountNonDealerPlayers() == 0)
        {
            ResetToIdle(clearDealer: false, clearCurrentTurn: true);
            _statusMessage = "플레이어가 없어 라운드를 종료했습니다.";
            return CreateResult();
        }

        if (_phase == GamePhase.InRound && !_roundEngine.HasPlayableNonDealer(_players))
        {
            if (_shoe is null)
            {
                throw new InvalidOperationException("Shoe is not initialized.");
            }

            RoundResolution resolution = _roundEngine.CompleteRound(
                _players,
                _shoe,
                _gameRuleOptions.DealerStandScore);
            ApplyRoundResolution(resolution);
            return CreateResult(resolution.Notice);
        }

        _statusMessage = isDisconnect ? "플레이어 연결이 끊어졌습니다." : "플레이어가 퇴장했습니다.";

        return CreateResult();
    }

    private GameOperationResult HandleStartRound(GameCommand command)
    {
        _validator.EnsureCanStartRound(
            _connections,
            _phase,
            _dealerPlayerId,
            command.ConnectionId,
            _players.Count,
            _gameRuleOptions.MinPlayersToStart);

        RoundResolution startResolution = _roundEngine.StartRound(
            _players,
            _gameRuleOptions.DeckCount,
            _gameRuleOptions.DealerStandScore);
        ApplyRoundResolution(startResolution);
        return CreateResult(startResolution.Notice);
    }

    private GameOperationResult HandleHit(GameCommand command)
    {
        PlayerState player = ValidatePlayerAction(command);
        if (_shoe is null)
        {
            throw new InvalidOperationException("Shoe is not initialized.");
        }

        RoundResolution hitResolution = _roundEngine.HandleHit(
            _players,
            _shoe,
            _currentTurnPlayerId,
            player,
            _gameRuleOptions.DealerStandScore);
        ApplyRoundResolution(hitResolution);
        return CreateResult(hitResolution.Notice);
    }

    private GameOperationResult HandleStand(GameCommand command)
    {
        PlayerState player = ValidatePlayerAction(command);
        if (_shoe is null)
        {
            throw new InvalidOperationException("Shoe is not initialized.");
        }

        RoundResolution standResolution = _roundEngine.HandleStand(
            _players,
            _shoe,
            player,
            _gameRuleOptions.DealerStandScore);
        ApplyRoundResolution(standResolution);
        return CreateResult(standResolution.Notice);
    }

    private int CountNonDealerPlayers() => _players.Count(player => !player.IsDealer);

    private bool IsDealerKeyMatched(string? dealerKey) => !string.IsNullOrEmpty(_dealerOptions.Key) && dealerKey == _dealerOptions.Key;

    private PlayerState ValidatePlayerAction(GameCommand command)
    {
        return _validator.ValidatePlayerAction(
            _connections,
            _phase,
            _players,
            command.ConnectionId,
            _currentTurnPlayerId);
    }

    private void ResetToIdle(bool clearDealer, bool clearCurrentTurn)
    {
        _phase = GamePhase.Idle;
        if (clearDealer)
        {
            _dealerPlayerId = string.Empty;
        }

        if (clearCurrentTurn)
        {
            _currentTurnPlayerId = string.Empty;
        }
    }

    private void ApplyRoundResolution(RoundResolution resolution)
    {
        _phase = resolution.Phase;
        _currentTurnPlayerId = resolution.CurrentTurnPlayerId;
        _statusMessage = resolution.StatusMessage;
        if (resolution.Shoe is not null)
        {
            _shoe = resolution.Shoe;
        }
    }

    private GameOperationResult CreateResult(GameNotice? notice = null) => new()
    {
        State = _snapshotFactory.Create(
            _phase,
            _dealerPlayerId,
            _currentTurnPlayerId,
            _statusMessage,
            _players),
        Notice = notice,
    };

    private GameOperationResult CreateSilentResult() => new()
    {
        State = _snapshotFactory.Create(
            _phase,
            _dealerPlayerId,
            _currentTurnPlayerId,
            _statusMessage,
            _players),
        ShouldPublishState = false,
    };
}

```

### `src/Seoul.It.Blackjack.Backend/Services/IGameRoomService.cs`
```csharp
using Seoul.It.Blackjack.Backend.Services.Commands;
using System.Threading.Tasks;

namespace Seoul.It.Blackjack.Backend.Services;

internal interface IGameRoomService
{
    Task<GameOperationResult> JoinAsync(string connectionId, string name, string? dealerKey);

    Task<GameOperationResult> LeaveAsync(string connectionId);

    Task<GameOperationResult> DisconnectAsync(string connectionId);

    Task<GameOperationResult> StartRoundAsync(string connectionId);

    Task<GameOperationResult> HitAsync(string connectionId);

    Task<GameOperationResult> StandAsync(string connectionId);
}

```

### `src/Seoul.It.Blackjack.Backend/Services/Infrastructure/ChannelGameCommandProcessor.cs`
```csharp
using Seoul.It.Blackjack.Backend.Services.Commands;
using System;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Seoul.It.Blackjack.Backend.Services.Infrastructure;

internal sealed class ChannelGameCommandProcessor : IGameCommandProcessor
{
    private readonly Channel<QueueItem> _queue = Channel.CreateUnbounded<QueueItem>(new UnboundedChannelOptions
    {
        SingleReader = true,
        SingleWriter = false,
    });

    public ChannelGameCommandProcessor()
    {
        _ = Task.Run(ProcessLoopAsync);
    }

    public Task<GameOperationResult> EnqueueAsync(GameCommand command, Func<GameOperationResult> handler)
    {
        QueueItem item = new(command, handler);
        _queue.Writer.TryWrite(item);
        return item.Completion.Task;
    }

    private async Task ProcessLoopAsync()
    {
        await foreach (QueueItem item in _queue.Reader.ReadAllAsync())
        {
            try
            {
                GameOperationResult result = item.Handler();
                item.Completion.SetResult(result);
            }
            catch (Exception ex)
            {
                item.Completion.SetException(ex);
            }
        }
    }

    private sealed class QueueItem
    {
        public QueueItem(GameCommand command, Func<GameOperationResult> handler)
        {
            Command = command;
            Handler = handler;
        }

        public GameCommand Command { get; }

        public Func<GameOperationResult> Handler { get; }

        public TaskCompletionSource<GameOperationResult> Completion { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
    }
}

```

### `src/Seoul.It.Blackjack.Backend/Services/Infrastructure/IGameCommandProcessor.cs`
```csharp
using Seoul.It.Blackjack.Backend.Services.Commands;
using System;
using System.Threading.Tasks;

namespace Seoul.It.Blackjack.Backend.Services.Infrastructure;

internal interface IGameCommandProcessor
{
    Task<GameOperationResult> EnqueueAsync(GameCommand command, Func<GameOperationResult> handler);
}

```

### `src/Seoul.It.Blackjack.Backend/Services/Round/IRoundEngine.cs`
```csharp
using Seoul.It.Blackjack.Backend.Models;
using Seoul.It.Blackjack.Core.Contracts;
using System.Collections.Generic;

namespace Seoul.It.Blackjack.Backend.Services.Round;

internal interface IRoundEngine
{
    RoundResolution StartRound(List<PlayerState> players, int deckCount, int dealerStandScore);

    RoundResolution HandleHit(
        List<PlayerState> players,
        Shoe shoe,
        string currentTurnPlayerId,
        PlayerState player,
        int dealerStandScore);

    RoundResolution HandleStand(
        List<PlayerState> players,
        Shoe shoe,
        PlayerState player,
        int dealerStandScore);

    RoundResolution CompleteRound(List<PlayerState> players, Shoe shoe, int dealerStandScore);

    string ResolveNextTurnPlayerId(IReadOnlyCollection<PlayerState> players);

    bool HasPlayableNonDealer(IReadOnlyCollection<PlayerState> players);
}

```

### `src/Seoul.It.Blackjack.Backend/Services/Round/RoundEngine.cs`
```csharp
using Seoul.It.Blackjack.Backend.Models;
using Seoul.It.Blackjack.Backend.Services.Commands;
using Seoul.It.Blackjack.Backend.Services.Exceptions;
using Seoul.It.Blackjack.Core.Contracts;
using Seoul.It.Blackjack.Core.Domain;
using System.Collections.Generic;
using System.Linq;

namespace Seoul.It.Blackjack.Backend.Services.Round;

internal sealed class RoundEngine : IRoundEngine
{
    public RoundResolution StartRound(List<PlayerState> players, int deckCount, int dealerStandScore)
    {
        Shoe shoe = new(deckCount);
        foreach (PlayerState player in players)
        {
            player.Cards.Clear();
            player.Score = 0;
            player.TurnState = PlayerTurnState.Playing;
            player.Outcome = RoundOutcome.None;
        }

        foreach (PlayerState player in players)
        {
            DrawCardTo(player, shoe);
            DrawCardTo(player, shoe);
        }

        string currentTurnPlayerId = ResolveNextTurnPlayerId(players);
        RoundResolution inRound = new()
        {
            Phase = GamePhase.InRound,
            CurrentTurnPlayerId = currentTurnPlayerId,
            StatusMessage = "라운드가 시작되었습니다.",
            Shoe = shoe,
        };

        if (HasPlayableNonDealer(players))
        {
            return inRound;
        }

        RoundResolution completed = CompleteRound(players, shoe, dealerStandScore);
        completed.Shoe = shoe;
        return completed;
    }

    public RoundResolution HandleHit(
        List<PlayerState> players,
        Shoe shoe,
        string currentTurnPlayerId,
        PlayerState player,
        int dealerStandScore)
    {
        if (!TryDrawCardTo(player, shoe))
        {
            return EndRoundByShoeEmpty();
        }

        string nextTurnPlayerId = player.TurnState == PlayerTurnState.Playing
            ? currentTurnPlayerId
            : ResolveNextTurnPlayerId(players);
        RoundResolution inRound = new()
        {
            Phase = GamePhase.InRound,
            CurrentTurnPlayerId = nextTurnPlayerId,
            StatusMessage = $"{player.Name} 님이 Hit 했습니다.",
        };

        if (HasPlayableNonDealer(players))
        {
            return inRound;
        }

        return CompleteRound(players, shoe, dealerStandScore);
    }

    public RoundResolution HandleStand(
        List<PlayerState> players,
        Shoe shoe,
        PlayerState player,
        int dealerStandScore)
    {
        player.TurnState = PlayerTurnState.Standing;
        string nextTurnPlayerId = ResolveNextTurnPlayerId(players);
        RoundResolution inRound = new()
        {
            Phase = GamePhase.InRound,
            CurrentTurnPlayerId = nextTurnPlayerId,
            StatusMessage = $"{player.Name} 님이 Stand 했습니다.",
        };

        if (HasPlayableNonDealer(players))
        {
            return inRound;
        }

        return CompleteRound(players, shoe, dealerStandScore);
    }

    public RoundResolution CompleteRound(List<PlayerState> players, Shoe shoe, int dealerStandScore)
    {
        PlayerState dealer = FindDealer(players);
        while (dealer.Score < dealerStandScore && dealer.TurnState == PlayerTurnState.Playing)
        {
            if (!TryDrawCardTo(dealer, shoe))
            {
                return EndRoundByShoeEmpty();
            }
        }

        if (dealer.TurnState == PlayerTurnState.Playing)
        {
            dealer.TurnState = PlayerTurnState.Standing;
        }

        foreach (PlayerState player in players.Where(player => !player.IsDealer))
        {
            if (player.TurnState == PlayerTurnState.Busted)
            {
                player.Outcome = RoundOutcome.Lose;
                continue;
            }

            if (dealer.TurnState == PlayerTurnState.Busted)
            {
                player.Outcome = RoundOutcome.Win;
                continue;
            }

            if (player.Score > dealer.Score)
            {
                player.Outcome = RoundOutcome.Win;
            }
            else if (player.Score < dealer.Score)
            {
                player.Outcome = RoundOutcome.Lose;
            }
            else
            {
                player.Outcome = RoundOutcome.Tie;
            }
        }

        return new RoundResolution
        {
            Phase = GamePhase.Idle,
            CurrentTurnPlayerId = string.Empty,
            StatusMessage = "라운드가 종료되었습니다.",
        };
    }

    public string ResolveNextTurnPlayerId(IReadOnlyCollection<PlayerState> players)
    {
        PlayerState? next = players.FirstOrDefault(player =>
            !player.IsDealer &&
            player.TurnState == PlayerTurnState.Playing);
        return next?.PlayerId ?? string.Empty;
    }

    public bool HasPlayableNonDealer(IReadOnlyCollection<PlayerState> players)
    {
        return players.Any(player =>
            !player.IsDealer &&
            player.TurnState == PlayerTurnState.Playing);
    }

    private static PlayerState FindDealer(IReadOnlyCollection<PlayerState> players)
    {
        PlayerState? dealer = players.SingleOrDefault(player => player.IsDealer);
        if (dealer is null)
        {
            throw new GameAuthorizationException("NOT_DEALER", "딜러가 존재하지 않습니다.");
        }

        return dealer;
    }

    private static void DrawCardTo(PlayerState player, Shoe shoe)
    {
        if (!TryDrawCardTo(player, shoe))
        {
            throw new GameRuleException("SHOE_EMPTY", "카드가 부족해 라운드를 종료합니다.");
        }
    }

    private static bool TryDrawCardTo(PlayerState player, Shoe shoe)
    {
        if (!shoe.TryDraw(out Card card))
        {
            return false;
        }

        player.Cards.Add(card);
        RecalculatePlayerState(player);
        return true;
    }

    private static void RecalculatePlayerState(PlayerState player)
    {
        player.Score = player.Cards.Sum(card => card.Rank.ToValue());
        if (player.Score > 21)
        {
            player.TurnState = PlayerTurnState.Busted;
        }
        else if (player.Score == 21)
        {
            player.TurnState = PlayerTurnState.Standing;
        }
        else if (player.TurnState == PlayerTurnState.Playing)
        {
            player.TurnState = PlayerTurnState.Playing;
        }
    }

    private static RoundResolution EndRoundByShoeEmpty()
    {
        return new RoundResolution
        {
            Phase = GamePhase.Idle,
            CurrentTurnPlayerId = string.Empty,
            StatusMessage = "카드가 부족해 라운드를 종료했습니다.",
            Notice = new GameNotice("SHOE_EMPTY", "카드가 부족해 라운드를 종료했습니다."),
        };
    }
}

```

### `src/Seoul.It.Blackjack.Backend/Services/Round/RoundResolution.cs`
```csharp
using Seoul.It.Blackjack.Backend.Models;
using Seoul.It.Blackjack.Backend.Services.Commands;
using Seoul.It.Blackjack.Core.Contracts;

namespace Seoul.It.Blackjack.Backend.Services.Round;

internal sealed class RoundResolution
{
    public GamePhase Phase { get; set; } = GamePhase.InRound;

    public string CurrentTurnPlayerId { get; set; } = string.Empty;

    public string StatusMessage { get; set; } = string.Empty;

    public Shoe? Shoe { get; set; }

    public GameNotice? Notice { get; set; }
}

```

### `src/Seoul.It.Blackjack.Backend/Services/Rules/GameRuleValidator.cs`
```csharp
using Seoul.It.Blackjack.Backend.Services.Exceptions;
using Seoul.It.Blackjack.Core.Contracts;
using System.Collections.Generic;
using System.Linq;

namespace Seoul.It.Blackjack.Backend.Services.Rules;

internal sealed class GameRuleValidator : IGameRuleValidator
{
    public string NormalizeName(string? name, int minNameLength, int maxNameLength)
    {
        string normalized = (name ?? string.Empty).Trim();
        if (normalized.Length < minNameLength || normalized.Length > maxNameLength)
        {
            throw new GameValidationException("INVALID_NAME", "이름은 1~20자여야 합니다.");
        }

        return normalized;
    }

    public void EnsureJoined(ConnectionRegistry connections, string connectionId)
    {
        if (!connections.ContainsConnection(connectionId))
        {
            throw new GameValidationException("NOT_JOINED", "먼저 참가해야 합니다.");
        }
    }

    public void EnsureCanStartRound(
        ConnectionRegistry connections,
        GamePhase phase,
        string dealerPlayerId,
        string connectionId,
        int playerCount,
        int minPlayersToStart)
    {
        EnsureJoined(connections, connectionId);
        if (phase != GamePhase.Idle)
        {
            throw new GameRuleException("GAME_IN_PROGRESS", "이미 게임이 진행 중입니다.");
        }

        if (string.IsNullOrEmpty(dealerPlayerId) || dealerPlayerId != connectionId)
        {
            throw new GameAuthorizationException("NOT_DEALER", "딜러만 라운드를 시작할 수 있습니다.");
        }

        if (playerCount < minPlayersToStart)
        {
            throw new GameRuleException("INSUFFICIENT_PLAYERS", "라운드를 시작하려면 최소 2명이 필요합니다.");
        }
    }

    public PlayerState FindPlayer(IReadOnlyCollection<PlayerState> players, string playerId)
    {
        PlayerState? player = players.SingleOrDefault(value => value.PlayerId == playerId);
        if (player is null)
        {
            throw new GameValidationException("NOT_JOINED", "먼저 참가해야 합니다.");
        }

        return player;
    }

    public PlayerState FindDealer(IReadOnlyCollection<PlayerState> players)
    {
        PlayerState? dealer = players.SingleOrDefault(player => player.IsDealer);
        if (dealer is null)
        {
            throw new GameAuthorizationException("NOT_DEALER", "딜러가 존재하지 않습니다.");
        }

        return dealer;
    }

    public PlayerState ValidatePlayerAction(
        ConnectionRegistry connections,
        GamePhase phase,
        IReadOnlyCollection<PlayerState> players,
        string connectionId,
        string currentTurnPlayerId)
    {
        EnsureJoined(connections, connectionId);
        if (phase != GamePhase.InRound)
        {
            throw new GameRuleException("GAME_NOT_INROUND", "게임이 진행 중이 아닙니다.");
        }

        PlayerState player = FindPlayer(players, connectionId);
        if (player.IsDealer)
        {
            throw new GameRuleException("DEALER_IS_AUTO", "딜러는 자동으로 진행됩니다.");
        }

        if (currentTurnPlayerId != player.PlayerId)
        {
            throw new GameRuleException("NOT_YOUR_TURN", "현재 턴의 플레이어가 아닙니다.");
        }

        if (player.TurnState != PlayerTurnState.Playing)
        {
            throw new GameRuleException("ALREADY_DONE", "이미 행동이 끝난 플레이어입니다.");
        }

        return player;
    }
}

```

### `src/Seoul.It.Blackjack.Backend/Services/Rules/IGameRuleValidator.cs`
```csharp
using Seoul.It.Blackjack.Core.Contracts;
using System.Collections.Generic;

namespace Seoul.It.Blackjack.Backend.Services.Rules;

internal interface IGameRuleValidator
{
    string NormalizeName(string? name, int minNameLength, int maxNameLength);

    void EnsureJoined(ConnectionRegistry connections, string connectionId);

    void EnsureCanStartRound(
        ConnectionRegistry connections,
        GamePhase phase,
        string dealerPlayerId,
        string connectionId,
        int playerCount,
        int minPlayersToStart);

    PlayerState FindPlayer(IReadOnlyCollection<PlayerState> players, string playerId);

    PlayerState FindDealer(IReadOnlyCollection<PlayerState> players);

    PlayerState ValidatePlayerAction(
        ConnectionRegistry connections,
        GamePhase phase,
        IReadOnlyCollection<PlayerState> players,
        string connectionId,
        string currentTurnPlayerId);
}

```

### `src/Seoul.It.Blackjack.Backend/Services/State/GameStateSnapshotFactory.cs`
```csharp
using Seoul.It.Blackjack.Core.Contracts;
using Seoul.It.Blackjack.Core.Domain;
using System.Collections.Generic;
using System.Linq;

namespace Seoul.It.Blackjack.Backend.Services.State;

internal sealed class GameStateSnapshotFactory : IGameStateSnapshotFactory
{
    public GameState Create(
        GamePhase phase,
        string dealerPlayerId,
        string currentTurnPlayerId,
        string statusMessage,
        IReadOnlyCollection<PlayerState> players)
    {
        return new GameState
        {
            Phase = phase,
            DealerPlayerId = dealerPlayerId,
            CurrentTurnPlayerId = currentTurnPlayerId,
            StatusMessage = statusMessage,
            Players = players.Select(ClonePlayer).ToList(),
        };
    }

    private static PlayerState ClonePlayer(PlayerState source)
    {
        return new PlayerState
        {
            PlayerId = source.PlayerId,
            Name = source.Name,
            IsDealer = source.IsDealer,
            Cards = source.Cards.Select(card => new Card(card.Suit, card.Rank)).ToList(),
            Score = source.Score,
            TurnState = source.TurnState,
            Outcome = source.Outcome,
        };
    }
}

```

### `src/Seoul.It.Blackjack.Backend/Services/State/IGameStateSnapshotFactory.cs`
```csharp
using Seoul.It.Blackjack.Core.Contracts;
using System.Collections.Generic;

namespace Seoul.It.Blackjack.Backend.Services.State;

internal interface IGameStateSnapshotFactory
{
    GameState Create(
        GamePhase phase,
        string dealerPlayerId,
        string currentTurnPlayerId,
        string statusMessage,
        IReadOnlyCollection<PlayerState> players);
}

```

### `src/Seoul.It.Blackjack.Backend/appsettings.json`
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Dealer": {
    "Key": "DEALER_SECRET_KEY"
  }
}

```

### `src/Seoul.It.Blackjack.Client/BlackjackClient.cs`
```csharp
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Seoul.It.Blackjack.Core.Contracts;

namespace Seoul.It.Blackjack.Client;

public sealed class BlackjackClient : IAsyncDisposable, IBlackjackClient
{
    private HubConnection? _connection;

    public event Action<GameState>? StateChanged;

    public event Action<string, string>? Error;

    public async Task ConnectAsync(string url)
    {
        await ConnectAsync(url, null);
    }

    public async Task ConnectAsync(string url, Func<HttpMessageHandler>? createMessageHandler)
    {
        if (_connection is not null)
        {
            return;
        }

        IHubConnectionBuilder builder = new HubConnectionBuilder();
        if (createMessageHandler is null)
        {
            builder = builder.WithUrl(url);
        }
        else
        {
            builder = builder.WithUrl(url, options => options.HttpMessageHandlerFactory = _ => createMessageHandler());
        }

        _connection = builder.Build();

        _connection.On<GameState>(nameof(IBlackjackClient.OnStateChanged), OnStateChanged);
        _connection.On<string, string>(nameof(IBlackjackClient.OnError), OnError);
        await _connection.StartAsync();
    }

    public Task JoinAsync(string name, string? dealerKey = null)
    {
        return EnsureConnection().InvokeAsync(nameof(IBlackjackServer.Join), name, dealerKey);
    }

    public Task LeaveAsync()
    {
        return EnsureConnection().InvokeAsync(nameof(IBlackjackServer.Leave));
    }

    public Task StartRoundAsync()
    {
        return EnsureConnection().InvokeAsync(nameof(IBlackjackServer.StartRound));
    }

    public Task HitAsync()
    {
        return EnsureConnection().InvokeAsync(nameof(IBlackjackServer.Hit));
    }

    public Task StandAsync()
    {
        return EnsureConnection().InvokeAsync(nameof(IBlackjackServer.Stand));
    }

    public Task OnStateChanged(GameState state)
    {
        StateChanged?.Invoke(state);
        return Task.CompletedTask;
    }

    public Task OnError(string code, string message)
    {
        Error?.Invoke(code, message);
        return Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
        {
            await _connection.DisposeAsync();
            _connection = null;
        }
    }

    private HubConnection EnsureConnection()
    {
        return _connection ?? throw new InvalidOperationException("먼저 ConnectAsync를 호출해야 합니다.");
    }
}

```

### `src/Seoul.It.Blackjack.Client/Extensions/ServiceCollectionExtensions.cs`
```csharp
using System;
using Microsoft.Extensions.DependencyInjection;
using Seoul.It.Blackjack.Client.Options;

namespace Seoul.It.Blackjack.Client.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBlackjackClient(
        this IServiceCollection services,
        Action<BlackjackClientOptions> configure)
    {
        if (services is null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (configure is null)
        {
            throw new ArgumentNullException(nameof(configure));
        }

        BlackjackClientOptions options = new();
        configure(options);

        services.AddSingleton(options);
        services.AddSingleton<BlackjackClient>();
        return services;
    }
}

```

### `src/Seoul.It.Blackjack.Client/Options/BlackjackClientOptions.cs`
```csharp
namespace Seoul.It.Blackjack.Client.Options;

public sealed class BlackjackClientOptions
{
    public string HubUrl { get; set; } = string.Empty;
}

```

### `src/Seoul.It.Blackjack.Client/Seoul.It.Blackjack.Client.csproj`
```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" Version="8.0.2" />
    <PackageReference Include="Microsoft.AspNetCore.SignalR.Client" Version="8.0.18" />
    <PackageReference Include="Microsoft.Bcl.AsyncInterfaces" Version="8.0.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\Seoul.It.Blackjack.Core\Seoul.It.Blackjack.Core.csproj" />
  </ItemGroup>

</Project>

```

### `src/Seoul.It.Blackjack.Core/Contracts/GamePhase.cs`
```csharp
namespace Seoul.It.Blackjack.Core.Contracts;

public enum GamePhase
{
    Idle,
    InRound
}

```

### `src/Seoul.It.Blackjack.Core/Contracts/GameState.cs`
```csharp
using System.Collections.Generic;

namespace Seoul.It.Blackjack.Core.Contracts;

public sealed class GameState
{
    public GamePhase Phase { get; set; } = GamePhase.Idle;

    public List<PlayerState> Players { get; set; } = new List<PlayerState>();

    public string DealerPlayerId { get; set; } = string.Empty;

    public string CurrentTurnPlayerId { get; set; } = string.Empty;

    public string StatusMessage { get; set; } = string.Empty;
}

```

### `src/Seoul.It.Blackjack.Core/Contracts/IBlackjackClient.cs`
```csharp
using System.Threading.Tasks;

namespace Seoul.It.Blackjack.Core.Contracts;

public interface IBlackjackClient
{
    Task OnStateChanged(GameState state);

    Task OnError(string code, string message);
}

```

### `src/Seoul.It.Blackjack.Core/Contracts/IBlackjackServer.cs`
```csharp
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

```

### `src/Seoul.It.Blackjack.Core/Contracts/PlayerState.cs`
```csharp
using Seoul.It.Blackjack.Core.Domain;
using System.Collections.Generic;

namespace Seoul.It.Blackjack.Core.Contracts;

public sealed class PlayerState
{
    public string PlayerId { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public bool IsDealer { get; set; }

    public List<Card> Cards { get; set; } = new List<Card>();

    public int Score { get; set; }

    public PlayerTurnState TurnState { get; set; } = PlayerTurnState.Playing;

    public RoundOutcome Outcome { get; set; } = RoundOutcome.None;
}

```

### `src/Seoul.It.Blackjack.Core/Contracts/PlayerTurnState.cs`
```csharp
namespace Seoul.It.Blackjack.Core.Contracts;

public enum PlayerTurnState
{
    Playing,
    Standing,
    Busted
}

```

### `src/Seoul.It.Blackjack.Core/Contracts/RoundOutcome.cs`
```csharp
namespace Seoul.It.Blackjack.Core.Contracts;

public enum RoundOutcome
{
    None,
    Win,
    Lose,
    Tie
}

```

### `src/Seoul.It.Blackjack.Core/Domain/Card.cs`
```csharp
namespace Seoul.It.Blackjack.Core.Domain;

public sealed class Card
{
    public Card(Suit suit, Rank rank)
    {
        Suit = suit;
        Rank = rank;
    }

    public Suit Suit { get; }

    public Rank Rank { get; }
}

```

### `src/Seoul.It.Blackjack.Core/Domain/Rank.cs`
```csharp
using System;

namespace Seoul.It.Blackjack.Core.Domain;

public enum Rank
{
    Two,
    Three,
    Four,
    Five,
    Six,
    Seven,
    Eight,
    Nine,
    Ten,
    Jack,
    Queen,
    King,
    Ace
}

public static class RankExtension
{
    public static int ToValue(this Rank rank) => rank switch
    {
        Rank.Ace => 1,
        Rank.Two => 2,
        Rank.Three => 3,
        Rank.Four => 4,
        Rank.Five => 5,
        Rank.Six => 6,
        Rank.Seven => 7,
        Rank.Eight => 8,
        Rank.Nine => 9,
        Rank.Ten or Rank.Jack or Rank.Queen or Rank.King => 10,
        _ => throw new ArgumentOutOfRangeException(nameof(rank), rank, "Unknown card rank."),
    };
}

```

### `src/Seoul.It.Blackjack.Core/Domain/Suit.cs`
```csharp
namespace Seoul.It.Blackjack.Core.Domain;

public enum Suit
{
    Clubs,
    Diamonds,
    Hearts,
    Spades
}
```

### `src/Seoul.It.Blackjack.Core/Seoul.It.Blackjack.Core.csproj`
```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
  </PropertyGroup>

</Project>

```

### `src/Seoul.It.Blackjack.Frontend.Tests/CardExtensionsTests.cs`
```csharp
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Seoul.It.Blackjack.Core.Domain;
using Seoul.It.Blackjack.Frontend.Extensions;

namespace Seoul.It.Blackjack.Frontend.Tests;

[TestClass]
public sealed class CardExtensionsTests
{
    [TestMethod]
    public void ToAssetPath_ShouldReturnExpectedRelativePath()
    {
        Card card = new(Suit.Spades, Rank.Ace);

        string path = card.ToAssetPath();

        Assert.AreEqual("cards/spades_ace.svg", path);
    }
}

```

### `src/Seoul.It.Blackjack.Frontend.Tests/EntryPageBunitTests.cs`
```csharp
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Seoul.It.Blackjack.Frontend.Pages;
using Seoul.It.Blackjack.Frontend.Services;
using System;
using BunitContext = Bunit.BunitContext;

namespace Seoul.It.Blackjack.Frontend.Tests;

[TestClass]
public sealed class EntryPageBunitTests
{
    [TestMethod]
    public void NextButtonIsDisabledWhenNameIsBlank()
    {
        using BunitContext context = new();
        context.Services.AddScoped(_ => new FrontendEntryState());

        IRenderedComponent<Entry> component = context.Render<Entry>();

        Assert.IsTrue(component.Find("#next").HasAttribute("disabled"));
    }

    [TestMethod]
    public void NextButtonStoresEntryStateAndNavigatesToTable()
    {
        using BunitContext context = new();
        context.Services.AddScoped(_ => new FrontendEntryState());

        IRenderedComponent<Entry> component = context.Render<Entry>();
        component.Find("#playerName").Change("Alice");
        component.Find("#dealerKey").Change("DEALER_SECRET_KEY");
        component.Find("#next").Click();

        FrontendEntryState state = context.Services.GetRequiredService<FrontendEntryState>();
        NavigationManager navigation = context.Services.GetRequiredService<NavigationManager>();

        Assert.AreEqual("Alice", state.PlayerName);
        Assert.AreEqual("DEALER_SECRET_KEY", state.DealerKey);
        Assert.IsTrue(navigation.Uri.EndsWith("/table", StringComparison.OrdinalIgnoreCase));
    }
}

```

### `src/Seoul.It.Blackjack.Frontend.Tests/FakeFrontendGameSession.cs`
```csharp
using Seoul.It.Blackjack.Core.Contracts;
using Seoul.It.Blackjack.Frontend.Services;
using System;
using System.Threading.Tasks;

namespace Seoul.It.Blackjack.Frontend.Tests;

internal sealed class FakeFrontendGameSession : IFrontendGameSession, IDisposable
{
    public event Action<GameState>? StateChanged;

    public event Action<string, string>? ErrorReceived;

    public bool IsConnected { get; private set; }

    public bool IsJoined { get; private set; }

    public int ConnectCallCount { get; private set; }

    public int JoinCallCount { get; private set; }

    public int LeaveCallCount { get; private set; }

    public int StartRoundCallCount { get; private set; }

    public int HitCallCount { get; private set; }

    public int StandCallCount { get; private set; }

    public string LastJoinName { get; private set; } = string.Empty;

    public string? LastJoinDealerKey { get; private set; }

    public Task ConnectAsync()
    {
        ConnectCallCount++;
        IsConnected = true;
        return Task.CompletedTask;
    }

    public Task JoinAsync(string name, string? dealerKey)
    {
        JoinCallCount++;
        LastJoinName = name;
        LastJoinDealerKey = dealerKey;
        IsJoined = true;
        return Task.CompletedTask;
    }

    public Task LeaveAsync()
    {
        LeaveCallCount++;
        IsJoined = false;
        return Task.CompletedTask;
    }

    public Task StartRoundAsync()
    {
        StartRoundCallCount++;
        return Task.CompletedTask;
    }

    public Task HitAsync()
    {
        HitCallCount++;
        return Task.CompletedTask;
    }

    public Task StandAsync()
    {
        StandCallCount++;
        return Task.CompletedTask;
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    public void Dispose()
    {
    }

    public void RaiseState(GameState state)
    {
        StateChanged?.Invoke(state);
    }

    public void RaiseError(string code, string message)
    {
        ErrorReceived?.Invoke(code, message);
    }
}

```

### `src/Seoul.It.Blackjack.Frontend.Tests/FrontendBlackjackOptionsTests.cs`
```csharp
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Seoul.It.Blackjack.Frontend.Options;

namespace Seoul.It.Blackjack.Frontend.Tests;

[TestClass]
public sealed class FrontendBlackjackOptionsTests
{
    [TestMethod]
    public void Defaults_ShouldMatchExpectedValues()
    {
        FrontendBlackjackOptions options = new();

        Assert.AreEqual("BlackjackClient", FrontendBlackjackOptions.DefaultSectionName);
        Assert.AreEqual("http://localhost:5000/blackjack", FrontendBlackjackOptions.DefaultHubUrl);
        Assert.AreEqual("http://localhost:5000/blackjack", options.HubUrl);
    }
}

```

### `src/Seoul.It.Blackjack.Frontend.Tests/MSTestSettings.cs`
```csharp
﻿[assembly: Parallelize(Scope = ExecutionScope.MethodLevel)]

```

### `src/Seoul.It.Blackjack.Frontend.Tests/PageRenderingTests.cs`
```csharp
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Threading.Tasks;

namespace Seoul.It.Blackjack.Frontend.Tests;

[TestClass]
public sealed class PageRenderingTests
{
    [TestMethod]
    public async Task RootPage_ShouldRespondSuccessfully()
    {
        using TestHostFactory factory = new();
        using HttpClient client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = true,
        });

        HttpResponseMessage response = await client.GetAsync("/");
        string body = await response.Content.ReadAsStringAsync();

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        StringAssert.Contains(body, "<html");
    }

    [TestMethod]
    public async Task TablePage_ShouldRespondSuccessfully()
    {
        using TestHostFactory factory = new();
        using HttpClient client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = true,
        });

        HttpResponseMessage response = await client.GetAsync("/table");
        string body = await response.Content.ReadAsStringAsync();

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        StringAssert.Contains(body, "<html");
    }
}

```

### `src/Seoul.It.Blackjack.Frontend.Tests/Seoul.It.Blackjack.Frontend.Tests.csproj`
```xml
﻿<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <LangVersion>latest</LangVersion>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="bunit" Version="2.5.3" />
    <PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="8.0.18" />
    <PackageReference Include="MSTest" Version="4.0.1" />
  </ItemGroup>

  <ItemGroup>
    <Using Include="Microsoft.VisualStudio.TestTools.UnitTesting" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\Seoul.It.Blackjack.Frontend\Seoul.It.Blackjack.Frontend.csproj" />
    <ProjectReference Include="..\Seoul.It.Blackjack.Core\Seoul.It.Blackjack.Core.csproj" />
  </ItemGroup>

</Project>

```

### `src/Seoul.It.Blackjack.Frontend.Tests/StaticCardAssetsTests.cs`
```csharp
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Threading.Tasks;

namespace Seoul.It.Blackjack.Frontend.Tests;

[TestClass]
public sealed class StaticCardAssetsTests
{
    [TestMethod]
    public async Task CardSvg_ShouldBeServedFromWwwroot()
    {
        using TestHostFactory factory = new();
        using HttpClient client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = true,
        });

        HttpResponseMessage response = await client.GetAsync("/cards/clubs_ace.svg");
        string body = await response.Content.ReadAsStringAsync();

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.IsTrue(response.Content.Headers.ContentType?.MediaType?.Contains("svg", System.StringComparison.OrdinalIgnoreCase) ?? false);
        StringAssert.Contains(body, "<svg");
    }
}

```

### `src/Seoul.It.Blackjack.Frontend.Tests/TablePageBunitTests.cs`
```csharp
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Seoul.It.Blackjack.Core.Contracts;
using Seoul.It.Blackjack.Core.Domain;
using Seoul.It.Blackjack.Frontend.Pages;
using Seoul.It.Blackjack.Frontend.Services;
using System;
using System.Collections.Generic;
using BunitContext = Bunit.BunitContext;

namespace Seoul.It.Blackjack.Frontend.Tests;

[TestClass]
public sealed class TablePageBunitTests
{
    [TestMethod]
    public void TableRedirectsToRootWhenEntryStateIsMissing()
    {
        using BunitContext context = new();
        FakeFrontendGameSession session = new();
        context.Services.AddScoped<IFrontendGameSession>(_ => session);
        context.Services.AddScoped(_ => new FrontendEntryState());

        IRenderedComponent<Table> component = context.Render<Table>();
        NavigationManager navigation = context.Services.GetRequiredService<NavigationManager>();

        component.WaitForAssertion(() =>
        {
            Assert.IsTrue(navigation.Uri.EndsWith("/", StringComparison.Ordinal));
        });
        Assert.AreEqual(0, session.ConnectCallCount);
        Assert.AreEqual(0, session.JoinCallCount);
    }

    [TestMethod]
    public void TableAutoConnectsAndJoinsOnFirstRender()
    {
        using BunitContext context = new();
        FakeFrontendGameSession session = new();
        context.Services.AddScoped<IFrontendGameSession>(_ => session);
        context.Services.AddScoped(_ => new FrontendEntryState
        {
            PlayerName = "Alice",
            DealerKey = "DEALER_SECRET_KEY",
        });

        IRenderedComponent<Table> component = context.Render<Table>();

        component.WaitForAssertion(() =>
        {
            Assert.AreEqual(1, session.ConnectCallCount);
            Assert.AreEqual(1, session.JoinCallCount);
        });

        Assert.AreEqual("Alice", session.LastJoinName);
        Assert.AreEqual("DEALER_SECRET_KEY", session.LastJoinDealerKey);
    }

    [TestMethod]
    public void TableRendersStateAndHandlesButtonsAndErrors()
    {
        using BunitContext context = new();
        FakeFrontendGameSession session = new();
        context.Services.AddScoped<IFrontendGameSession>(_ => session);
        context.Services.AddScoped(_ => new FrontendEntryState
        {
            PlayerName = "Alice",
            DealerKey = string.Empty,
        });

        IRenderedComponent<Table> component = context.Render<Table>();
        component.WaitForAssertion(() => Assert.AreEqual(1, session.JoinCallCount));

        session.RaiseState(new GameState
        {
            Phase = GamePhase.InRound,
            DealerPlayerId = "dealer-1",
            CurrentTurnPlayerId = "player-1",
            StatusMessage = "라운드가 시작되었습니다.",
            Players = new List<PlayerState>
            {
                new()
                {
                    PlayerId = "player-1",
                    Name = "Alice",
                    IsDealer = false,
                    Score = 11,
                    TurnState = PlayerTurnState.Playing,
                    Outcome = RoundOutcome.None,
                    Cards = new List<Card>
                    {
                        new(Suit.Spades, Rank.Ace),
                        new(Suit.Hearts, Rank.Ten),
                    },
                },
            },
        });

        component.WaitForAssertion(() =>
        {
            Assert.AreEqual("Phase: InRound", component.Find("[data-testid='phase']").TextContent.Trim());
            StringAssert.Contains(component.Markup, "cards/spades_ace.svg");
            StringAssert.Contains(component.Markup, "cards/hearts_ten.svg");
        });

        component.Find("#startRound").Click();
        component.Find("#hit").Click();
        component.Find("#stand").Click();
        component.Find("#leave").Click();

        Assert.AreEqual(1, session.StartRoundCallCount);
        Assert.AreEqual(1, session.HitCallCount);
        Assert.AreEqual(1, session.StandCallCount);
        Assert.AreEqual(1, session.LeaveCallCount);

        session.RaiseError("NOT_DEALER", "딜러만 라운드를 시작할 수 있습니다.");

        component.WaitForAssertion(() =>
        {
            StringAssert.Contains(component.Markup, "NOT_DEALER");
        });
    }
}

```

### `src/Seoul.It.Blackjack.Frontend.Tests/TestHostFactory.cs`
```csharp
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Seoul.It.Blackjack.Frontend;

namespace Seoul.It.Blackjack.Frontend.Tests;

internal sealed class TestHostFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
    }
}

```

### `src/Seoul.It.Blackjack.Frontend/Components/App.razor`
```razor
<!DOCTYPE html>
<html lang="ko">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <base href="/" />
    <link rel="stylesheet" href="css/app.css" />
    <HeadOutlet @rendermode="RenderMode.InteractiveServer" />
</head>
<body>
    <Routes @rendermode="RenderMode.InteractiveServer" />
    <script src="_framework/blazor.web.js"></script>
</body>
</html>

```

### `src/Seoul.It.Blackjack.Frontend/Components/Layout/MainLayout.razor`
```razor
@inherits LayoutComponentBase

<div class="layout-shell">
    @Body
</div>

```

### `src/Seoul.It.Blackjack.Frontend/Components/Routes.razor`
```razor
<Router AppAssembly="typeof(Program).Assembly">
    <Found Context="routeData">
        <RouteView RouteData="routeData" DefaultLayout="typeof(Seoul.It.Blackjack.Frontend.Components.Layout.MainLayout)" />
        <FocusOnNavigate RouteData="routeData" Selector="h1" />
    </Found>
    <NotFound>
        <LayoutView Layout="typeof(Seoul.It.Blackjack.Frontend.Components.Layout.MainLayout)">
            <p>페이지를 찾을 수 없습니다.</p>
        </LayoutView>
    </NotFound>
</Router>

```

### `src/Seoul.It.Blackjack.Frontend/Extensions/CardExtensions.cs`
```csharp
using Seoul.It.Blackjack.Core.Domain;

namespace Seoul.It.Blackjack.Frontend.Extensions;

public static class CardExtensions
{
    public static string ToAssetPath(this Card card)
    {
        string suit = card.Suit.ToString().ToLowerInvariant();
        string rank = card.Rank.ToString().ToLowerInvariant();
        return $"cards/{suit}_{rank}.svg";
    }
}

```

### `src/Seoul.It.Blackjack.Frontend/Extensions/ServiceCollectionExtensions.cs`
```csharp
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Seoul.It.Blackjack.Client.Extensions;
using Seoul.It.Blackjack.Frontend.Options;
using Seoul.It.Blackjack.Frontend.Services;

namespace Seoul.It.Blackjack.Frontend.Extensions;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFrontendBlackjackOptions(this IServiceCollection services, IConfiguration configuration)
    {
        return services.Configure<FrontendBlackjackOptions>(
            configuration.GetSection(FrontendBlackjackOptions.DefaultSectionName));
    }

    public static IServiceCollection AddFrontendBlackjackClient(this IServiceCollection services, IConfiguration configuration)
    {
        FrontendBlackjackOptions options = new();
        configuration
            .GetSection(FrontendBlackjackOptions.DefaultSectionName)
            .Bind(options);
        if (string.IsNullOrWhiteSpace(options.HubUrl))
        {
            options.HubUrl = FrontendBlackjackOptions.DefaultHubUrl;
        }

        return services.AddBlackjackClient(clientOptions =>
        {
            clientOptions.HubUrl = options.HubUrl;
        });
    }

    public static IServiceCollection AddFrontendServices(this IServiceCollection services)
    {
        services.AddScoped<FrontendEntryState>();
        services.AddScoped<IFrontendGameSession, FrontendGameSession>();
        return services;
    }
}

```

### `src/Seoul.It.Blackjack.Frontend/Options/FrontendBlackjackOptions.cs`
```csharp
namespace Seoul.It.Blackjack.Frontend.Options;

public sealed class FrontendBlackjackOptions
{
    public const string DefaultSectionName = "BlackjackClient";

    public const string DefaultHubUrl = "http://localhost:5000/blackjack";

    public string HubUrl { get; set; } = DefaultHubUrl;
}

```

### `src/Seoul.It.Blackjack.Frontend/Pages/Entry.razor`
```razor
@page "/"
@inject FrontendEntryState EntryState
@inject NavigationManager Navigation

<h1>입장 정보 입력</h1>

<section class="entry-panel">
    <div class="field">
        <label for="playerName">사용자 이름</label>
        <input id="playerName" @bind="_playerName" />
    </div>

    <div class="field">
        <label for="dealerKey">딜러 키 (옵션)</label>
        <input id="dealerKey" @bind="_dealerKey" />
    </div>

    @if (!string.IsNullOrWhiteSpace(_validationMessage))
    {
        <p class="validation">@_validationMessage</p>
    }

    <button id="next" @onclick="GoNextAsync" disabled="@(!CanProceed)">다음</button>
</section>

@code {
    private string _playerName = string.Empty;
    private string _dealerKey = string.Empty;
    private string _validationMessage = string.Empty;

    private bool CanProceed => !string.IsNullOrWhiteSpace(_playerName);

    protected override void OnInitialized()
    {
        _playerName = EntryState.PlayerName;
        _dealerKey = EntryState.DealerKey;
    }

    private Task GoNextAsync()
    {
        if (!CanProceed)
        {
            _validationMessage = "이름을 입력해 주세요.";
            return Task.CompletedTask;
        }

        EntryState.PlayerName = _playerName.Trim();
        EntryState.DealerKey = _dealerKey.Trim();
        _validationMessage = string.Empty;
        Navigation.NavigateTo("/table");
        return Task.CompletedTask;
    }
}

```

### `src/Seoul.It.Blackjack.Frontend/Pages/Entry.razor.css`
```css
.entry-panel {
    max-width: 420px;
    padding: 16px;
    border: 1px solid #ddd;
    border-radius: 8px;
    background: #fff;
}

.field {
    margin-bottom: 12px;
}

label {
    display: block;
    margin-bottom: 6px;
    font-weight: 600;
}

input {
    width: 100%;
    box-sizing: border-box;
    padding: 8px;
}

.validation {
    color: #b00020;
}

```

### `src/Seoul.It.Blackjack.Frontend/Pages/Table.razor`
```razor
@page "/table"
@implements IAsyncDisposable
@inject IFrontendGameSession Session
@inject FrontendEntryState EntryState
@inject NavigationManager Navigation

<h1>블랙잭 테이블</h1>

@if (_isAutoJoining)
{
    <p data-testid="auto-joining">자동 접속/입장 중입니다...</p>
}

<section class="action-row">
    <button id="leave" @onclick="LeaveAsync" disabled="@(!Session.IsJoined)">Leave</button>
    <button id="startRound" @onclick="StartRoundAsync" disabled="@(!Session.IsJoined)">StartRound</button>
    <button id="hit" @onclick="HitAsync" disabled="@(!CanHitOrStand)">Hit</button>
    <button id="stand" @onclick="StandAsync" disabled="@(!CanHitOrStand)">Stand</button>
</section>

@if (_state is not null)
{
    <section class="summary">
        <p data-testid="phase">Phase: @_state.Phase</p>
        <p>Dealer: @_state.DealerPlayerId</p>
        <p>CurrentTurn: @_state.CurrentTurnPlayerId</p>
        <p>Status: @_state.StatusMessage</p>
    </section>

    <section class="players">
        @foreach (PlayerState player in _state.Players)
        {
            <article class="player">
                <h3>@player.Name (@(player.IsDealer ? "Dealer" : "Player"))</h3>
                <p>Score: @player.Score</p>
                <p>TurnState: @player.TurnState</p>
                <p>Outcome: @player.Outcome</p>
                <div class="cards">
                    @foreach (Card card in player.Cards)
                    {
                        <img src="@card.ToAssetPath()" alt="card" />
                    }
                </div>
            </article>
        }
    </section>
}

@if (_errors.Count > 0)
{
    <section class="errors">
        <h3>오류</h3>
        <ul>
            @foreach ((string Code, string Message) error in _errors)
            {
                <li>@error.Code: @error.Message</li>
            }
        </ul>
    </section>
}

@code {
    private readonly List<(string Code, string Message)> _errors = [];
    private GameState? _state;
    private bool _autoJoinRequested;
    private bool _isAutoJoining;

    private bool CanHitOrStand =>
        Session.IsJoined &&
        _state is not null &&
        _state.Phase == GamePhase.InRound;

    protected override void OnInitialized()
    {
        Session.StateChanged += OnStateChanged;
        Session.ErrorReceived += OnErrorReceived;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender || _autoJoinRequested)
        {
            return;
        }

        _autoJoinRequested = true;

        if (string.IsNullOrWhiteSpace(EntryState.PlayerName))
        {
            Navigation.NavigateTo("/");
            return;
        }

        _isAutoJoining = true;
        await InvokeAsync(StateHasChanged);

        try
        {
            await Session.ConnectAsync();
            await Session.JoinAsync(EntryState.PlayerName, NormalizeDealerKey(EntryState.DealerKey));
        }
        catch (Exception ex)
        {
            _errors.Insert(0, ("AUTO_JOIN_FAILED", ex.Message));
        }

        _isAutoJoining = false;
        await InvokeAsync(StateHasChanged);
    }

    public ValueTask DisposeAsync()
    {
        Session.StateChanged -= OnStateChanged;
        Session.ErrorReceived -= OnErrorReceived;
        return ValueTask.CompletedTask;
    }

    private static string? NormalizeDealerKey(string value)
    {
        string normalized = value.Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }

    private Task LeaveAsync() => ExecuteAsync(() => Session.LeaveAsync(), "LEAVE_FAILED");

    private Task StartRoundAsync() => ExecuteAsync(() => Session.StartRoundAsync(), "START_FAILED");

    private Task HitAsync() => ExecuteAsync(() => Session.HitAsync(), "HIT_FAILED");

    private Task StandAsync() => ExecuteAsync(() => Session.StandAsync(), "STAND_FAILED");

    private async Task ExecuteAsync(Func<Task> action, string fallbackCode)
    {
        try
        {
            await action();
        }
        catch (Exception ex)
        {
            _errors.Insert(0, (fallbackCode, ex.Message));
            await InvokeAsync(StateHasChanged);
        }
    }

    private void OnStateChanged(GameState state)
    {
        _state = state;
        _ = InvokeAsync(StateHasChanged);
    }

    private void OnErrorReceived(string code, string message)
    {
        _errors.Insert(0, (code, message));
        if (_errors.Count > 10)
        {
            _errors.RemoveAt(_errors.Count - 1);
        }

        _ = InvokeAsync(StateHasChanged);
    }
}

```

### `src/Seoul.It.Blackjack.Frontend/Pages/Table.razor.css`
```css
.action-row {
    display: flex;
    gap: 8px;
    margin-bottom: 16px;
}

.summary,
.errors {
    background: #fff;
    border: 1px solid #ddd;
    border-radius: 8px;
    padding: 12px;
    margin-bottom: 16px;
}

.players {
    display: grid;
    gap: 12px;
}

.player {
    background: #fff;
    border: 1px solid #ddd;
    border-radius: 8px;
    padding: 12px;
}

.cards {
    display: flex;
    flex-wrap: wrap;
    gap: 6px;
}

.cards img {
    width: 72px;
    height: auto;
}

.errors ul {
    margin: 0;
    padding-left: 18px;
}

```

### `src/Seoul.It.Blackjack.Frontend/Program.cs`
```csharp
using Seoul.It.Blackjack.Frontend.Components;
using Seoul.It.Blackjack.Frontend.Extensions;

namespace Seoul.It.Blackjack.Frontend;

public partial class Program
{
    private static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        builder.Services.AddFrontendBlackjackOptions(builder.Configuration);
        builder.Services.AddFrontendBlackjackClient(builder.Configuration);
        builder.Services.AddFrontendServices();

        WebApplication app = builder.Build();

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseAntiforgery();

        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        app.Run();
    }
}

```

### `src/Seoul.It.Blackjack.Frontend/Seoul.It.Blackjack.Frontend.csproj`
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\Seoul.It.Blackjack.Client\Seoul.It.Blackjack.Client.csproj" />
    <ProjectReference Include="..\Seoul.It.Blackjack.Core\Seoul.It.Blackjack.Core.csproj" />
  </ItemGroup>

</Project>

```

### `src/Seoul.It.Blackjack.Frontend/Services/FrontendEntryState.cs`
```csharp
namespace Seoul.It.Blackjack.Frontend.Services;

public sealed class FrontendEntryState
{
    public string PlayerName { get; set; } = string.Empty;

    public string DealerKey { get; set; } = string.Empty;
}

```

### `src/Seoul.It.Blackjack.Frontend/Services/FrontendGameSession.cs`
```csharp
using Seoul.It.Blackjack.Client;
using Seoul.It.Blackjack.Client.Options;
using Seoul.It.Blackjack.Core.Contracts;
using System;
using System.Threading.Tasks;

namespace Seoul.It.Blackjack.Frontend.Services;

public sealed class FrontendGameSession : IFrontendGameSession
{
    private readonly BlackjackClient _client;
    private readonly BlackjackClientOptions _options;

    public FrontendGameSession(BlackjackClient client, BlackjackClientOptions options)
    {
        _client = client;
        _options = options;
        _client.StateChanged += HandleStateChanged;
        _client.Error += HandleError;
    }

    public event Action<GameState>? StateChanged;

    public event Action<string, string>? ErrorReceived;

    public bool IsConnected { get; private set; }

    public bool IsJoined { get; private set; }

    public async Task ConnectAsync()
    {
        if (IsConnected)
        {
            return;
        }

        await _client.ConnectAsync(_options.HubUrl);
        IsConnected = true;
    }

    public async Task JoinAsync(string name, string? dealerKey)
    {
        await _client.JoinAsync(name, dealerKey);
        IsJoined = true;
    }

    public async Task LeaveAsync()
    {
        await _client.LeaveAsync();
        IsJoined = false;
    }

    public Task StartRoundAsync() => _client.StartRoundAsync();

    public Task HitAsync() => _client.HitAsync();

    public Task StandAsync() => _client.StandAsync();

    public ValueTask DisposeAsync()
    {
        _client.StateChanged -= HandleStateChanged;
        _client.Error -= HandleError;
        return ValueTask.CompletedTask;
    }

    private void HandleStateChanged(GameState state)
    {
        StateChanged?.Invoke(state);
    }

    private void HandleError(string code, string message)
    {
        if (code == "GAME_TERMINATED" || code == "NOT_JOINED")
        {
            IsJoined = false;
        }

        ErrorReceived?.Invoke(code, message);
    }
}

```

### `src/Seoul.It.Blackjack.Frontend/Services/IFrontendGameSession.cs`
```csharp
using Seoul.It.Blackjack.Core.Contracts;
using System;
using System.Threading.Tasks;

namespace Seoul.It.Blackjack.Frontend.Services;

public interface IFrontendGameSession : IAsyncDisposable
{
    event Action<GameState>? StateChanged;

    event Action<string, string>? ErrorReceived;

    bool IsConnected { get; }

    bool IsJoined { get; }

    Task ConnectAsync();

    Task JoinAsync(string name, string? dealerKey);

    Task LeaveAsync();

    Task StartRoundAsync();

    Task HitAsync();

    Task StandAsync();
}

```

### `src/Seoul.It.Blackjack.Frontend/_Imports.razor`
```razor
@using System.Linq
@using Seoul.It.Blackjack.Core.Contracts
@using Seoul.It.Blackjack.Core.Domain
@using Seoul.It.Blackjack.Frontend.Extensions
@using Seoul.It.Blackjack.Frontend.Services
@using Microsoft.AspNetCore.Components
@using Microsoft.AspNetCore.Components.Routing
@using Microsoft.AspNetCore.Components.Web

```

### `src/Seoul.It.Blackjack.Frontend/appsettings.json`
```json
{
  "BlackjackClient": {
    "HubUrl": "https://localhost:5000/blackjack"
  }
}
```

### `src/Seoul.It.Blackjack.Frontend/wwwroot/css/app.css`
```css
body {
    margin: 0;
    padding: 0;
    font-family: "Segoe UI", Tahoma, Geneva, Verdana, sans-serif;
    background: #f5f5f5;
}

.layout-shell {
    max-width: 1100px;
    margin: 0 auto;
    padding: 24px;
}

```

### `src/Seoul.It.Blackjack.sln`
```text
﻿
Microsoft Visual Studio Solution File, Format Version 12.00
# Visual Studio Version 18
VisualStudioVersion = 18.2.11415.280
MinimumVisualStudioVersion = 10.0.40219.1
Project("{2150E333-8FDC-42A3-9474-1A3956D46DE8}") = "items", "items", "{A735A56C-0C9E-4887-BBBE-92CA5AAED017}"
	ProjectSection(SolutionItems) = preProject
		.editorconfig = .editorconfig
		Directory.Build.props = Directory.Build.props
	EndProjectSection
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "Seoul.It.Blackjack.Core", "Seoul.It.Blackjack.Core\Seoul.It.Blackjack.Core.csproj", "{DAA3465F-7890-4F91-8E66-55E4A8933906}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "Seoul.It.Blackjack.Backend", "Seoul.It.Blackjack.Backend\Seoul.It.Blackjack.Backend.csproj", "{341D2A21-A2BD-45E5-9DC8-A8098CF37D75}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "Seoul.It.Blackjack.Backend.Tests", "Seoul.It.Blackjack.Backend.Tests\Seoul.It.Blackjack.Backend.Tests.csproj", "{6301CF2D-2AEA-458D-A170-9C8D5F135CB1}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "Seoul.It.Blackjack.Client", "Seoul.It.Blackjack.Client\Seoul.It.Blackjack.Client.csproj", "{8D18BCD6-B37F-4B85-8024-4517CEBC385A}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "Seoul.It.Blackjack.Frontend", "Seoul.It.Blackjack.Frontend\Seoul.It.Blackjack.Frontend.csproj", "{5FB398C9-599C-49FF-ABD6-ECB2224C639B}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "Seoul.It.Blackjack.Frontend.Tests", "Seoul.It.Blackjack.Frontend.Tests\Seoul.It.Blackjack.Frontend.Tests.csproj", "{BE488585-6ACD-402C-A4F2-9419AB2DE7DC}"
EndProject
Global
	GlobalSection(SolutionConfigurationPlatforms) = preSolution
		Debug|Any CPU = Debug|Any CPU
		Debug|x64 = Debug|x64
		Debug|x86 = Debug|x86
		Release|Any CPU = Release|Any CPU
		Release|x64 = Release|x64
		Release|x86 = Release|x86
	EndGlobalSection
	GlobalSection(ProjectConfigurationPlatforms) = postSolution
		{DAA3465F-7890-4F91-8E66-55E4A8933906}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{DAA3465F-7890-4F91-8E66-55E4A8933906}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{DAA3465F-7890-4F91-8E66-55E4A8933906}.Debug|x64.ActiveCfg = Debug|Any CPU
		{DAA3465F-7890-4F91-8E66-55E4A8933906}.Debug|x64.Build.0 = Debug|Any CPU
		{DAA3465F-7890-4F91-8E66-55E4A8933906}.Debug|x86.ActiveCfg = Debug|Any CPU
		{DAA3465F-7890-4F91-8E66-55E4A8933906}.Debug|x86.Build.0 = Debug|Any CPU
		{DAA3465F-7890-4F91-8E66-55E4A8933906}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{DAA3465F-7890-4F91-8E66-55E4A8933906}.Release|Any CPU.Build.0 = Release|Any CPU
		{DAA3465F-7890-4F91-8E66-55E4A8933906}.Release|x64.ActiveCfg = Release|Any CPU
		{DAA3465F-7890-4F91-8E66-55E4A8933906}.Release|x64.Build.0 = Release|Any CPU
		{DAA3465F-7890-4F91-8E66-55E4A8933906}.Release|x86.ActiveCfg = Release|Any CPU
		{DAA3465F-7890-4F91-8E66-55E4A8933906}.Release|x86.Build.0 = Release|Any CPU
		{341D2A21-A2BD-45E5-9DC8-A8098CF37D75}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{341D2A21-A2BD-45E5-9DC8-A8098CF37D75}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{341D2A21-A2BD-45E5-9DC8-A8098CF37D75}.Debug|x64.ActiveCfg = Debug|Any CPU
		{341D2A21-A2BD-45E5-9DC8-A8098CF37D75}.Debug|x64.Build.0 = Debug|Any CPU
		{341D2A21-A2BD-45E5-9DC8-A8098CF37D75}.Debug|x86.ActiveCfg = Debug|Any CPU
		{341D2A21-A2BD-45E5-9DC8-A8098CF37D75}.Debug|x86.Build.0 = Debug|Any CPU
		{341D2A21-A2BD-45E5-9DC8-A8098CF37D75}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{341D2A21-A2BD-45E5-9DC8-A8098CF37D75}.Release|Any CPU.Build.0 = Release|Any CPU
		{341D2A21-A2BD-45E5-9DC8-A8098CF37D75}.Release|x64.ActiveCfg = Release|Any CPU
		{341D2A21-A2BD-45E5-9DC8-A8098CF37D75}.Release|x64.Build.0 = Release|Any CPU
		{341D2A21-A2BD-45E5-9DC8-A8098CF37D75}.Release|x86.ActiveCfg = Release|Any CPU
		{341D2A21-A2BD-45E5-9DC8-A8098CF37D75}.Release|x86.Build.0 = Release|Any CPU
		{6301CF2D-2AEA-458D-A170-9C8D5F135CB1}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{6301CF2D-2AEA-458D-A170-9C8D5F135CB1}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{6301CF2D-2AEA-458D-A170-9C8D5F135CB1}.Debug|x64.ActiveCfg = Debug|Any CPU
		{6301CF2D-2AEA-458D-A170-9C8D5F135CB1}.Debug|x64.Build.0 = Debug|Any CPU
		{6301CF2D-2AEA-458D-A170-9C8D5F135CB1}.Debug|x86.ActiveCfg = Debug|Any CPU
		{6301CF2D-2AEA-458D-A170-9C8D5F135CB1}.Debug|x86.Build.0 = Debug|Any CPU
		{6301CF2D-2AEA-458D-A170-9C8D5F135CB1}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{6301CF2D-2AEA-458D-A170-9C8D5F135CB1}.Release|Any CPU.Build.0 = Release|Any CPU
		{6301CF2D-2AEA-458D-A170-9C8D5F135CB1}.Release|x64.ActiveCfg = Release|Any CPU
		{6301CF2D-2AEA-458D-A170-9C8D5F135CB1}.Release|x64.Build.0 = Release|Any CPU
		{6301CF2D-2AEA-458D-A170-9C8D5F135CB1}.Release|x86.ActiveCfg = Release|Any CPU
		{6301CF2D-2AEA-458D-A170-9C8D5F135CB1}.Release|x86.Build.0 = Release|Any CPU
		{8D18BCD6-B37F-4B85-8024-4517CEBC385A}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{8D18BCD6-B37F-4B85-8024-4517CEBC385A}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{8D18BCD6-B37F-4B85-8024-4517CEBC385A}.Debug|x64.ActiveCfg = Debug|Any CPU
		{8D18BCD6-B37F-4B85-8024-4517CEBC385A}.Debug|x64.Build.0 = Debug|Any CPU
		{8D18BCD6-B37F-4B85-8024-4517CEBC385A}.Debug|x86.ActiveCfg = Debug|Any CPU
		{8D18BCD6-B37F-4B85-8024-4517CEBC385A}.Debug|x86.Build.0 = Debug|Any CPU
		{8D18BCD6-B37F-4B85-8024-4517CEBC385A}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{8D18BCD6-B37F-4B85-8024-4517CEBC385A}.Release|Any CPU.Build.0 = Release|Any CPU
		{8D18BCD6-B37F-4B85-8024-4517CEBC385A}.Release|x64.ActiveCfg = Release|Any CPU
		{8D18BCD6-B37F-4B85-8024-4517CEBC385A}.Release|x64.Build.0 = Release|Any CPU
		{8D18BCD6-B37F-4B85-8024-4517CEBC385A}.Release|x86.ActiveCfg = Release|Any CPU
		{8D18BCD6-B37F-4B85-8024-4517CEBC385A}.Release|x86.Build.0 = Release|Any CPU
		{5FB398C9-599C-49FF-ABD6-ECB2224C639B}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{5FB398C9-599C-49FF-ABD6-ECB2224C639B}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{5FB398C9-599C-49FF-ABD6-ECB2224C639B}.Debug|x64.ActiveCfg = Debug|Any CPU
		{5FB398C9-599C-49FF-ABD6-ECB2224C639B}.Debug|x64.Build.0 = Debug|Any CPU
		{5FB398C9-599C-49FF-ABD6-ECB2224C639B}.Debug|x86.ActiveCfg = Debug|Any CPU
		{5FB398C9-599C-49FF-ABD6-ECB2224C639B}.Debug|x86.Build.0 = Debug|Any CPU
		{5FB398C9-599C-49FF-ABD6-ECB2224C639B}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{5FB398C9-599C-49FF-ABD6-ECB2224C639B}.Release|Any CPU.Build.0 = Release|Any CPU
		{5FB398C9-599C-49FF-ABD6-ECB2224C639B}.Release|x64.ActiveCfg = Release|Any CPU
		{5FB398C9-599C-49FF-ABD6-ECB2224C639B}.Release|x64.Build.0 = Release|Any CPU
		{5FB398C9-599C-49FF-ABD6-ECB2224C639B}.Release|x86.ActiveCfg = Release|Any CPU
		{5FB398C9-599C-49FF-ABD6-ECB2224C639B}.Release|x86.Build.0 = Release|Any CPU
		{BE488585-6ACD-402C-A4F2-9419AB2DE7DC}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{BE488585-6ACD-402C-A4F2-9419AB2DE7DC}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{BE488585-6ACD-402C-A4F2-9419AB2DE7DC}.Debug|x64.ActiveCfg = Debug|Any CPU
		{BE488585-6ACD-402C-A4F2-9419AB2DE7DC}.Debug|x64.Build.0 = Debug|Any CPU
		{BE488585-6ACD-402C-A4F2-9419AB2DE7DC}.Debug|x86.ActiveCfg = Debug|Any CPU
		{BE488585-6ACD-402C-A4F2-9419AB2DE7DC}.Debug|x86.Build.0 = Debug|Any CPU
		{BE488585-6ACD-402C-A4F2-9419AB2DE7DC}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{BE488585-6ACD-402C-A4F2-9419AB2DE7DC}.Release|Any CPU.Build.0 = Release|Any CPU
		{BE488585-6ACD-402C-A4F2-9419AB2DE7DC}.Release|x64.ActiveCfg = Release|Any CPU
		{BE488585-6ACD-402C-A4F2-9419AB2DE7DC}.Release|x64.Build.0 = Release|Any CPU
		{BE488585-6ACD-402C-A4F2-9419AB2DE7DC}.Release|x86.ActiveCfg = Release|Any CPU
		{BE488585-6ACD-402C-A4F2-9419AB2DE7DC}.Release|x86.Build.0 = Release|Any CPU
	EndGlobalSection
	GlobalSection(SolutionProperties) = preSolution
		HideSolutionNode = FALSE
	EndGlobalSection
	GlobalSection(ExtensibilityGlobals) = postSolution
		SolutionGuid = {56F63AE9-F907-46D3-B9F5-304077E96C9E}
	EndGlobalSection
EndGlobal

```
