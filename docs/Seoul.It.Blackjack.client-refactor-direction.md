# Seoul.It.Blackjack.Client 개선 방향 문서 (래핑 없음, 수업 난이도 유지)

## 1. 문서 목적
- 본 문서는 `BlackjackClient` 개선 방향을 확정하기 위한 검토 문서입니다.
- 목적은 다음 3가지를 동시에 만족하는 것입니다.
1. 아이들이 이해하기 쉬운 구조
2. 계약 중심(Interface-first) 설계
3. 기존 게임 동작/테스트 시나리오 불변

- 본 문서는 `examples` 폴더의 스타일을 참고하되, 아래는 명시적으로 제외합니다.
1. XML 주석 대량 추가
2. 과도한 프레임워크/인프라 도입

---

## 2. examples 에서 채택할 핵심 원칙
`examples/IMiddlewareApiClient.cs`, `examples/MiddlewareApiClient.cs`, `examples/IServiceCollectionExtension.cs` 기준으로 아래 원칙만 채택합니다.

1. 계약 먼저 정의하고 구현체가 계약을 따른다.
2. 문자열 하드코딩보다 `nameof(...)` 기반 호출을 우선한다.
3. 공개 API는 단순한 비동기 메서드로 유지한다.
4. 구현체 내부는 예측 가능한 패턴으로 통일한다.

---

## 3. 현재 상태 진단
현재 코드 기준 주요 지점:

1. `IBlackjackClient` 계약 파일명 문제는 해결됨
   - `src/Seoul.It.Blackjack.Core/Contracts/ㅊ.cs` 제거 완료
   - `src/Seoul.It.Blackjack.Core/Contracts/IBlackjackClient.cs` 단일 유지
2. `BlackjackClient`는 `IBlackjackClient`를 직접 구현함
   - 경로: `src/Seoul.It.Blackjack.Client/BlackjackClient.cs`
3. SignalR 이벤트/서버 호출은 `nameof(...)` 기반으로 정리됨
   - `nameof(IBlackjackClient.OnStateChanged)`, `nameof(IBlackjackClient.OnError)`, `nameof(IBlackjackServer.Join)` 등
4. 현재 남은 경고는 구조/계약과 무관한 코드 분석 경고(CA1003, CA1054 등) 중심임

---

## 4. 목표 설계 (핵심)

## 4.1 계약 메서드명 변경
`IBlackjackClient`를 다음처럼 변경합니다.

- 기존:
1. `Task StateChanged(GameState state)`
2. `Task Error(string code, string message)`

- 변경:
1. `Task OnStateChanged(GameState state)`
2. `Task OnError(string code, string message)`

이유:
1. 이벤트 수신 핸들러라는 의미가 명확해짐
2. `Error` 키워드 충돌 성격을 완화
3. `BlackjackClient`가 인터페이스 구현 시 이벤트명 충돌을 피하기 쉬움

## 4.2 `BlackjackClient`는 `IBlackjackClient` 구현
`BlackjackClient` 선언을 아래처럼 변경합니다.

1. `public sealed class BlackjackClient : IAsyncDisposable, IBlackjackClient`

핵심 규칙:
1. 래핑/어댑터 추가하지 않음
2. 인터페이스 메서드는 이벤트 전달만 수행
3. 메서드 본문은 1~3줄로 단순 유지

예시 형태:
1. `OnStateChanged` -> 이벤트 호출 후 `Task.CompletedTask`
2. `OnError` -> 이벤트 호출 후 `Task.CompletedTask`

## 4.3 이벤트/호출 문자열 제거 (`nameof`)
문자열 리터럴 호출을 계약 기반 `nameof`로 변경합니다.

1. `_connection.On<GameState>(nameof(IBlackjackClient.OnStateChanged), OnStateChanged);`
2. `_connection.On<string, string>(nameof(IBlackjackClient.OnError), OnError);`
3. `InvokeAsync(nameof(IBlackjackServer.Join), ...)`
4. `InvokeAsync(nameof(IBlackjackServer.Leave))` 등

효과:
1. 오타 리스크 감소
2. 메서드명 변경 시 컴파일 타임 추적 가능
3. 수업 중 "계약과 구현 연결" 설명이 쉬워짐

---

## 5. 수업 난이도 관점 설명

## 5.1 아이들이 어려워할 수 있는 지점
1. 이벤트와 메서드의 차이
2. SignalR 콜백 등록
3. 계약 인터페이스의 역할

## 5.2 이 설계가 쉬운 이유
1. `OnStateChanged`, `OnError` 이름이 수신 핸들러임을 바로 설명함
2. 이벤트 전달 로직이 메서드 안에서 직선형(조건/분기 거의 없음)
3. `nameof`로 문자열 암기 부담 감소

## 5.3 수업용 설명 스크립트 (짧은 버전)
1. "서버가 보내는 메시지는 `IBlackjackClient`라는 약속으로 받습니다."
2. "받은 메시지는 `On...` 메서드에서 바로 이벤트로 전달합니다."
3. "서버에 보낼 때도 `IBlackjackServer` 약속 이름을 그대로 씁니다."

---

## 6. 변경 영향 범위

## 6.1 Core
1. 파일명/중복 정리 완료:
   - `src/Seoul.It.Blackjack.Core/Contracts/ㅊ.cs` 삭제
   - `src/Seoul.It.Blackjack.Core/Contracts/IBlackjackClient.cs` 유지
2. 인터페이스 메서드명 변경:
   - `StateChanged` -> `OnStateChanged`
   - `Error` -> `OnError`

## 6.2 Backend
1. `GameSessionHub`의 클라이언트 호출부 변경
   - `Clients.All.StateChanged(...)` -> `Clients.All.OnStateChanged(...)`
   - `Clients.Caller.Error(...)` -> `Clients.Caller.OnError(...)`
   - 공지 전송도 동일하게 `OnError` 사용

## 6.3 Client
1. `BlackjackClient`가 `IBlackjackClient` 구현
2. 이벤트 등록 및 서버 메서드 호출을 `nameof` 기반으로 변경
3. 공개 이벤트명은 `StateChanged`, `Error`를 유지

## 6.4 Tests
1. `SignalRTestClient` 이벤트 수신 문자열 변경
   - `"StateChanged"` -> `nameof(IBlackjackClient.OnStateChanged)`
   - `"Error"` -> `nameof(IBlackjackClient.OnError)`
2. 이벤트 추적 문자열(`Events.Add`)도 새 이름 기준으로 동기화

## 6.5 Docs
1. `docs/Seoul.It.Blackjack.design.md` 계약 섹션 메서드명 동기화
2. 필요 시 `teaching-simplification-proposal.md` 용어 동기화

---

## 7. 구현 묶음 계획 (300줄 제한 준수)

## 묶음 A (예상 80줄)
대상:
1. `src/Seoul.It.Blackjack.Core/Contracts/ㅊ.cs` (삭제)
2. `src/Seoul.It.Blackjack.Core/Contracts/IBlackjackClient.cs` (유지)

작업:
1. 파일명 정리
2. 인터페이스 메서드명 `OnStateChanged`, `OnError`로 변경

검증:
1. `dotnet build src/Seoul.It.Blackjack.sln`

## 묶음 B (예상 120줄)
대상:
1. `src/Seoul.It.Blackjack.Client/BlackjackClient.cs`

작업:
1. `IBlackjackClient` 구현
2. `OnStateChanged`, `OnError` 구현
3. `nameof(IBlackjackClient...)`, `nameof(IBlackjackServer...)` 적용

검증:
1. `dotnet build src/Seoul.It.Blackjack.sln`

## 묶음 C (예상 100줄)
대상:
1. `src/Seoul.It.Blackjack.Backend/Hubs/GameSessionHub.cs`
2. `src/Seoul.It.Blackjack.Backend.Tests/SignalRTestClient.cs`
3. 영향 받는 테스트 1~2개

작업:
1. Hub 전송 메서드명 동기화
2. 테스트 이벤트 구독 문자열 동기화

검증:
1. `dotnet test src/Seoul.It.Blackjack.Backend.Tests/Seoul.It.Blackjack.Backend.Tests.csproj -m:1`

## 묶음 D (예상 60줄)
대상:
1. `docs/Seoul.It.Blackjack.design.md`
2. 필요 시 `docs/Seoul.It.Blackjack.teaching-simplification-proposal.md`

작업:
1. 계약 명세 용어 동기화

검증:
1. 문서 용어 충돌 0건 확인

---

## 8. 수용 기준
1. 전체 빌드 성공 (`Core + Backend + Client`)
2. Backend 통합 테스트 전체 통과
3. `BlackjackClient`는 `IBlackjackClient`를 직접 구현
4. SignalR 문자열 리터럴이 계약 메서드명 영역에서 제거됨
5. 수업 설명 시 "계약 이름 = 실제 호출 이름"이 성립함

---

## 9. 명시적 가정
1. XML 주석은 이번 작업 범위에서 제외한다.
2. 래핑/어댑터 패턴은 도입하지 않는다.
3. 공개 계약 변경(`IBlackjackClient` 메서드명)으로 인한 내부 참조 수정은 이번 범위에 포함한다.
4. 기존 게임 규칙/에러코드/브로드캐스트 정책은 변경하지 않는다.

---

## 10. 결정 완료 항목
1. `StateChanged/Error` -> `OnStateChanged/OnError` 명칭 변경 적용
2. `BlackjackClient : IBlackjackClient` 직접 구현 적용
3. 공개 이벤트 이름은 `StateChanged`, `Error` 유지
