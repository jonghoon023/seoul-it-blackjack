# Seoul.It.Blackjack Frontend 구현 계획서 (검토용)

## 1. 문서 목적
- 이 문서는 `Seoul.It.Blackjack.Frontend` 프로젝트를 새로 만들기 전에, 구현 범위/순서/검증 기준을 확정하기 위한 검토 문서입니다.
- 현재 목표는 "바로 코드 작성"이 아니라, "실제 구현 가능한 세부 계획"을 명확히 정리하는 것입니다.
- 본 문서는 `docs/Seoul.It.Blackjack.design.md`를 기준으로 작성합니다.

## 2. 확정 목표
1. `src/Seoul.It.Blackjack.Frontend` 프로젝트를 신규 생성합니다.
2. Frontend는 게임 규칙을 계산하지 않고, Backend 상태(`GameState`)를 그대로 렌더링합니다.
3. 통신은 `Seoul.It.Blackjack.Client`를 사용합니다.
4. 카드 SVG 에셋은 제공된 ZIP을 Frontend 정적 경로에 압축 해제해 배치합니다.
5. 작업은 변경 묶음 기준 `300줄 이내`로 분할하고, 각 묶음 착수 전 승인 절차를 유지합니다.
6. 페이지는 2개로 분리합니다.
1. 첫 페이지: 사용자 이름 + 딜러 키(옵션) 입력
2. 두 번째 페이지: 블랙잭 테이블 화면

## 3. 범위/비범위

### 3.1 범위
- Frontend 프로젝트 생성
- 2페이지 구성(입력 페이지 + 테이블 페이지)
- 입력 페이지에서 사용자 이름/딜러 키를 받고 `다음` 버튼으로 테이블 페이지 이동
- 테이블 페이지에서 Join/Leave/StartRound/Hit/Stand 제공
- 상태/에러 메시지 렌더링
- 카드 이미지 렌더링
- 최소 스타일링(가독성 중심)

### 3.2 비범위
- 게임 규칙 재구현
- 멀티룸
- 고급 애니메이션/복잡한 UI 프레임워크
- 인증/권한 시스템 추가
- 자동 재연결/세션 복구

## 4. 기술 선택 (권장안)

### 4.1 권장안
- `Blazor Web App (.NET 8)` + Interactive Server 모드

### 4.2 권장 이유
1. 수업/타이핑 관점에서 구조가 단순합니다.
2. C#만으로 화면/이벤트를 설명하기 좋습니다.
3. 현재 `Seoul.It.Blackjack.Client`를 그대로 참조하기 쉽습니다.

### 4.3 대안(비권장)
- Blazor WebAssembly 단독: 번들/초기 설정/배포 설명량 증가
- JS 프레임워크(React/Vue 등): 현재 학습 흐름(C# 중심)과 분리됨

## 5. 프로젝트 구조 제안

아래는 생성 후 목표 구조입니다.

```text
src/
  Seoul.It.Blackjack.Frontend/
    Components/
      App.razor
      Layout/
        MainLayout.razor
    Pages/
      Entry.razor
      Table.razor
    Services/
      FrontendGameSession.cs
      FrontendEntryState.cs
    Extensions/
      CardExtensions.cs
    wwwroot/
      css/
        app.css
      cards/
        clubs_ace.svg
        ... (총 52장)
    Program.cs
    Seoul.It.Blackjack.Frontend.csproj
```

## 6. 카드 이미지 ZIP 배치 정책 (중요)

### 6.1 배치 원칙
- 수업에서는 카드 SVG ZIP을 제공받아 지정 경로에 압축 해제합니다.
- 압축 해제 경로: `src/Seoul.It.Blackjack.Frontend/wwwroot/cards`
- 수동 파일 이동(`mv`)은 수업 필수 단계로 두지 않습니다.

### 6.2 파일명 규칙
- 이미 현재 파일명은 `{suit}_{rank}.svg` 규칙으로 정리되어 있습니다.
- 예시:
1. `clubs_ace.svg`
2. `diamonds_ten.svg`
3. `hearts_three.svg`
4. `spades_queen.svg`

### 6.3 렌더링 경로 규칙
- 카드 1장의 이미지 URL은 아래 규칙으로 계산합니다.
- `cards/{suit}_{rank}.svg`
- `suit`: `Card.Suit.ToString().ToLowerInvariant()`
- `rank`: `Card.Rank.ToString().ToLowerInvariant()`

권장 구현: 확장 함수
```csharp
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

사용 예시:
```razor
<img src="@card.ToAssetPath()" alt="card" />
```

### 6.4 검증 기준
1. Frontend 실행 후 네트워크 요청에서 `/cards/...svg` 200 응답 확인
2. 플레이어/딜러 카드 이미지 모두 정상 렌더링 확인
3. `wwwroot/cards`에 52장 SVG가 존재하는지 확인

## 7. UI 요구사항 상세

### 7.1 페이지 1: 입장 정보 입력 (`Entry.razor`)
- 필드:
1. 사용자 이름
2. 딜러 키(선택)
- 버튼:
1. 다음
- 동작:
1. 이름 공백/빈 문자열은 프론트 1차 검증
2. 다음 클릭 시 `FrontendEntryState`에 값 저장
3. 테이블 페이지(`/table`)로 이동

### 7.2 페이지 간 데이터 전달 정책
- `QueryString`에 딜러 키를 노출하지 않습니다.
- `Scoped` 상태 서비스(`FrontendEntryState`)를 사용해 입력값을 전달합니다.
- 테이블 페이지 진입 시 상태 서비스 값을 읽어 Join 요청에 사용합니다.

### 7.3 페이지 2: 블랙잭 테이블 (`Table.razor`)
- 연결/세션 버튼:
1. Connect
2. Join
3. Leave
- 게임 액션 버튼:
1. StartRound (딜러가 눌러야 성공)
2. Hit
3. Stand

### 7.4 게임 상태 요약 영역
- 현재 `Phase`
- 딜러 ID
- 현재 턴 플레이어 ID
- `StatusMessage`
- 최근 오류(`code: message`) 1~N개

### 7.5 플레이어 목록/카드 영역
- 각 플레이어 카드:
1. Name
2. IsDealer 여부
3. Score
4. TurnState
5. Outcome
6. 카드 이미지 목록

### 7.6 버튼 활성화 정책(프론트)
- 프론트는 UX 편의상 비활성화를 제공하되, 권한의 최종 판정은 백엔드가 수행합니다.
- 예시:
1. Entry 페이지: 이름 비어 있으면 `다음` 비활성
2. Table 페이지 연결 전: Join/Leave/Start/Hit/Stand 비활성
3. Join 전: Start/Hit/Stand 비활성
4. InRound가 아니면 Hit/Stand 비활성

## 8. 프론트 상태 관리 전략

### 8.1 최소 상태 모델
- `bool IsConnected`
- `bool IsJoined` (클라이언트 로컬 플래그)
- `GameState CurrentState`
- `List<(string Code, string Message)> Errors`
- `string BackendHubUrl`
- `FrontendEntryState`
1. `PlayerName`
2. `DealerKey`

### 8.2 상태 갱신 원칙
1. 서버 이벤트 `OnStateChanged` 수신 시 `CurrentState`를 통째로 교체
2. 서버 이벤트 `OnError` 수신 시 오류 목록에 추가
3. 로컬 계산으로 점수/턴/승패를 재판정하지 않음
4. Entry 입력값은 `FrontendEntryState`를 통해 Table 페이지에서 재사용

### 8.3 스레드/렌더링
- 이벤트 콜백에서 `InvokeAsync(StateHasChanged)`를 사용해 UI 갱신 안전성 확보

## 9. Frontend 서비스 계층 제안

### 9.1 `FrontendGameSession` (신규)
역할:
1. `BlackjackClient`를 캡슐화
2. 연결/입장/행동 메서드 제공
3. UI가 쓰기 쉬운 이벤트 제공

메서드 후보:
1. `ConnectAsync(string url)`
2. `JoinAsync(string name, string? dealerKey)`
3. `LeaveAsync()`
4. `StartRoundAsync()`
5. `HitAsync()`
6. `StandAsync()`

이벤트 후보:
1. `event Action<GameState>? StateChanged`
2. `event Action<string, string>? ErrorReceived`

### 9.2 이유
- 페이지 코드비하인드 비대화를 줄입니다.
- 수업에서는 "서비스가 통신, 페이지가 표시"라는 역할 분리를 설명하기 쉽습니다.

## 10. `Program.cs` 구성 계획

1. `AddRazorComponents().AddInteractiveServerComponents()` 등록
2. `AddBlackjackClient(options => options.HubUrl = "...")` 등록
3. `FrontendGameSession` 등록 (`Scoped` 권장)

주의:
- `BlackjackClient`는 현재 `Singleton` 등록 정책을 그대로 유지합니다.
- 실제 연결 시작은 자동이 아니라 `ConnectAsync(...)` 명시 호출입니다.

## 11. 사용자 동작 시나리오 (정상 흐름)

1. 사용자가 Entry 페이지에서 이름/딜러 키(선택) 입력
2. `다음` 클릭으로 Table 페이지 이동
3. Table 페이지에서 Connect 클릭
4. Connect 성공 후 Join 클릭
5. 참가 성공 시 `OnStateChanged`로 플레이어 목록 갱신
6. 딜러가 StartRound 클릭
7. 상태가 InRound로 전환되고 카드/턴 표시
8. 플레이어가 Hit/Stand 수행
9. 각 행동마다 전체 상태가 갱신됨
10. 라운드 종료 후 Idle 전환, 결과 표시 유지
11. 딜러가 다시 StartRound 시 다음 라운드 시작

## 12. 오류 시나리오 (백엔드 권위)

프론트는 아래 오류를 단순 표시만 합니다.

1. `INVALID_NAME`
2. `ALREADY_JOINED`
3. `DEALER_ALREADY_EXISTS`
4. `NOT_DEALER`
5. `GAME_IN_PROGRESS`
6. `GAME_NOT_INROUND`
7. `NOT_YOUR_TURN`
8. `ALREADY_DONE`
9. `INSUFFICIENT_PLAYERS`
10. `DEALER_IS_AUTO`
11. `GAME_TERMINATED`
12. `SHOE_EMPTY`
13. `NOT_JOINED`

## 13. 구현 작업 묶음 계획 (300줄 기준, 승인 게이트 포함)

### 묶음 A (문서 정합화, 예상 180줄)
대상:
- `docs/Seoul.It.Blackjack.design.md`
- `docs/Seoul.It.Blackjack.frontend-implementation-plan.md` (본 문서)

작업:
1. Frontend 기술 선택과 카드 ZIP 배치 정책 확정 문구 반영
2. 수업 범위/강사용 범위 분리 문구 정리

검증:
- 문서 내 정책 충돌 0건

승인 게이트:
- 묶음 A 코드 변경 전 승인

### 묶음 B (프로젝트 생성, 예상 260줄)
대상:
- 신규 `src/Seoul.It.Blackjack.Frontend/*`
- 수정 `src/Seoul.It.Blackjack.sln`

작업:
1. Frontend 프로젝트 생성
2. Client/Core 참조 추가
3. 기본 페이지/레이아웃 구성

검증:
- `dotnet build src/Seoul.It.Blackjack.sln --no-restore -v minimal`

승인 게이트:
- 묶음 B 착수 전 승인

### 묶음 C (클라이언트 연결/입장 UI, 예상 280줄)
대상:
- `Program.cs`
- `Pages/Entry.razor`
- `Pages/Table.razor`
- `Services/FrontendGameSession.cs` (신규)
- `Services/FrontendEntryState.cs` (신규)

작업:
1. `AddBlackjackClient` 연동
2. Entry 페이지(이름/딜러키 + 다음) 구현
3. Table 페이지 Connect/Join/Leave 동작 연결
4. 에러 표시 영역 추가

검증:
- 수동 시나리오: Entry 입력 -> 다음 -> Connect -> Join -> Leave

승인 게이트:
- 묶음 C 착수 전 승인

### 묶음 D (게임 상태/액션 UI, 예상 290줄)
대상:
- `Pages/Table.razor`
- 필요 시 `Pages/Table.razor.css`

작업:
1. StartRound/Hit/Stand 버튼 연동
2. `GameState`/`PlayerState` 렌더링
3. 상태 메시지/턴/결과 표시

검증:
- 수동 시나리오: StartRound -> Hit/Stand -> Idle 전환

승인 게이트:
- 묶음 D 착수 전 승인

### 묶음 E (카드 이미지 배치 및 렌더링, 예상 200줄)
대상:
- ZIP 압축 해제: `src/Seoul.It.Blackjack.Frontend/wwwroot/cards`
- `Table.razor` 카드 이미지 바인딩

작업:
1. 제공된 카드 ZIP 압축 해제(강사 사전 준비 또는 수업 시작 전 배치)
2. `{suit}_{rank}.svg` 경로 계산 함수 적용
3. 카드 이미지 UI 확인

검증:
1. 52장 파일 존재
2. 브라우저에서 카드 이미지 로딩 확인
3. `wwwroot/cards` 경로에서 상대 URL(`cards/...svg`) 접근 확인

승인 게이트:
- 묶음 E 착수 전 승인

### 묶음 F (마무리/검증, 예상 120줄)
대상:
- README 또는 docs

작업:
1. 실행 방법 정리
2. 기본 점검 체크리스트 반영

검증:
- `dotnet build src/Seoul.It.Blackjack.sln --no-restore -v minimal`

승인 게이트:
- 묶음 F 착수 전 승인

## 14. 검증 체크리스트

### 14.1 기능 체크
1. Entry 페이지에서 이름/딜러 키 입력 후 `다음` 이동이 동작한다.
2. Table 페이지에서 Connect 성공/실패가 구분 표시된다.
3. Join 성공 후 플레이어 목록이 갱신된다.
4. 비딜러 StartRound 시 오류가 보인다.
5. Hit/Stand 성공 시 상태가 전원 기준으로 갱신된다.
6. 라운드 종료 후 Idle 상태가 보인다.
7. 딜러 퇴장 시 `GAME_TERMINATED`가 표시된다.

### 14.2 카드 체크
1. 모든 카드가 깨지지 않고 표시된다.
2. 파일명 매핑 오류가 없다.
3. 숫자 랭크(Two~Ten)와 Face 카드가 모두 정상 표시된다.

### 14.3 빌드 체크
1. 솔루션 빌드 성공
2. 기존 Backend/Client 기능 회귀 없음

## 15. 수업 진행 관점 가이드

### 15.1 학생과 함께 타이핑할 범위
1. Frontend 프로젝트 생성
2. Entry 페이지 기본 UI(입력 + 다음)
3. Table 페이지 기본 UI(연결/행동 버튼)
4. Client 연결/버튼 이벤트
5. 상태 렌더링
6. 카드 이미지 렌더링

### 15.2 강사용 사전 준비 권장
1. Backend 실행 상태 미리 준비
2. 딜러/플레이어 2개 브라우저 탭 시나리오 준비
3. 카드 이미지 경로 함수 샘플 준비

## 16. 수용 기준 (Frontend 단계)
1. `Seoul.It.Blackjack.Frontend` 프로젝트가 솔루션에 포함된다.
2. Frontend가 2페이지(`Entry`, `Table`)로 분리된다.
3. Entry에서 입력 후 Table로 이동할 수 있다.
4. Table에서 Connect/Join/Leave/StartRound/Hit/Stand를 실행할 수 있다.
5. `GameState` 기반 렌더링이 동작한다.
6. 카드 SVG 52장이 Frontend `wwwroot/cards`에서 제공된다.
7. 루트 `cards` 폴더 의존이 제거된다.

## 17. 명시적 가정
1. Frontend는 Backend와 별도 프로세스로 실행될 수 있다.
2. CORS/주소 문제는 로컬 개발 기준(`localhost`)으로 해결한다.
3. 재연결 복구는 이번 범위에 포함하지 않는다.
4. 기준 문서는 `docs/Seoul.It.Blackjack.design.md`이다.

## 18. 검토 요청 포인트
아래 5가지만 확정해 주시면 바로 구현 단계로 전환할 수 있습니다.

1. Frontend 기술 선택: `Blazor Web App (Interactive Server)` 확정 여부
2. 라우트 경로: `Entry(/)` + `Table(/table)`로 확정 여부
3. Entry -> Table 전달 방식: `Scoped 상태 서비스` 확정 여부
4. 카드 ZIP 배치 시점: 프로젝트 생성 직후(B 묶음 직후) 또는 카드 렌더링 시점(E 묶음)
5. 실행 기본 URL: Frontend 기본값에 Backend Hub URL(`http://localhost:5000/blackjack`)을 넣을지
