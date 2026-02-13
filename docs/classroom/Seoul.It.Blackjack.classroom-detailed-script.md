# Seoul.It.Blackjack 초정밀 수업 스크립트

이 문서는 강사가 듀얼 모니터에서 그대로 읽으면서, 학생들이 직접 타이핑하도록 진행하기 위한 실전용 문서입니다.

- 대상: C# 문법을 거의 모르는 고등학생(큐/스택 기초만 아는 수준)
- 설명 원칙: 항상 **정확한 개념/원리 먼저**, 그 다음 **비유**
- 실습 원칙: 파일 단위로 끊어서 설명 -> 타이핑 -> 실행 확인 -> 질문
- 코드 원문: `docs/classroom/Seoul.It.Blackjack.complete-teaching-guide.md`의 부록 참조

---

## 1. 수업 진행 규칙 (강사용)

1. 파일 열기 전에 먼저 "이 파일의 존재 이유"를 말합니다.
2. 코드 타이핑 전 30초 동안 핵심 개념을 먼저 설명합니다.
3. 비유는 개념 설명 후에만 사용합니다.
4. 한 파일 타이핑이 끝나면 반드시 질문 1개 이상을 던집니다.
5. 20~30분마다 반드시 실행/빌드로 피드백 루프를 만듭니다.

권장 멘트:

- 개념 먼저: "먼저 정확한 개념부터 설명하겠습니다."
- 비유 전환: "이제 이해를 돕기 위해 쉬운 비유로 바꿔 보겠습니다."
- 확인 질문: "지금 설명에서 핵심이 무엇이었는지 한 문장으로 말해볼까요?"

---

## 2. 우리 프로젝트 블랙잭 규칙 초상세 명세

이 섹션은 "코드에 실제로 구현된 규칙"만 설명합니다.
즉, 일반 카지노 블랙잭과 다른 부분이 있을 수 있습니다.

### 2.1 게임 모델

정확한 개념/원리:

1. 서버는 하나의 게임 방만 운영합니다.
2. 게임 전체 상태는 `GamePhase`로 관리합니다: `Idle`, `InRound`.
3. 플레이어는 `PlayerState`로 표현합니다.
4. 딜러는 별도 클래스가 아니라 `IsDealer=true`인 플레이어입니다.

쉬운 비유:

- 한 반에서 진행하는 카드 게임 1개라고 생각하면 됩니다.
- 칠판에 현재 상태(`Idle`, `InRound`)를 적어두고 모두가 공유합니다.

### 2.2 카드/점수 규칙

정확한 개념/원리:

1. 무늬(`Suit`): Clubs, Diamonds, Hearts, Spades.
2. 숫자(`Rank`): Two~Ten, Jack, Queen, King, Ace.
3. 점수(`Rank.ToValue()`):
1. Ace = 1
2. Two~Nine = 숫자 그대로
3. Ten/Jack/Queen/King = 10
4. A(11) 가변값 규칙은 사용하지 않습니다.

쉬운 비유:

- 이번 수업에서는 A를 항상 1점짜리 카드라고 약속한 단순 버전입니다.

### 2.3 입장(Join) 규칙

정확한 개념/원리:

1. `InRound` 상태에서는 신규 입장이 불가능합니다 (`GAME_IN_PROGRESS`).
2. 같은 연결 ID가 재입장하면 거부합니다 (`ALREADY_JOINED`).
3. 이름은 `Trim()` 후 길이 1~20이어야 합니다 (`INVALID_NAME`).
4. 중복 이름은 허용됩니다.
5. `dealerKey`가 백엔드 `DealerOptions.Key`와 일치하면 딜러 요청으로 판단합니다.
6. 딜러는 한 명만 허용합니다 (`DEALER_ALREADY_EXISTS`).
7. 키가 일치하지 않으면 일반 플레이어로 입장합니다.

쉬운 비유:

- 출석부에 같은 학생번호가 두 번 등록되면 안 되고, 이름은 공백만 쓰면 안 되는 것과 같습니다.

### 2.4 라운드 시작(StartRound) 규칙

정확한 개념/원리:

1. 요청자는 반드시 입장 상태여야 합니다 (`NOT_JOINED`).
2. 게임은 `Idle` 상태여야 시작 가능합니다 (`GAME_IN_PROGRESS`).
3. 요청자는 딜러여야 합니다 (`NOT_DEALER`).
4. 총 인원은 최소 2명이어야 합니다 (`INSUFFICIENT_PLAYERS`).
5. 시작 시 Shoe를 새로 생성하고, 모든 플레이어의 라운드 필드를 초기화합니다.

쉬운 비유:

- 수업 시작 버튼은 반장(딜러)만 누를 수 있고, 학생이 혼자면 팀 게임을 시작할 수 없는 것과 같습니다.

### 2.5 초기 카드 배분 규칙

정확한 개념/원리:

1. 모든 플레이어(딜러 포함)에게 카드 2장을 배분합니다.
2. 각 플레이어의 `Cards`, `Score`, `TurnState`, `Outcome`를 라운드 시작 기준으로 갱신합니다.
3. 점수 21이면 즉시 `TurnState=Standing`으로 전환됩니다.
4. 점수 21 초과면 `TurnState=Busted`로 전환됩니다.

쉬운 비유:

- 시작 신호와 함께 모두가 기본 카드를 2장씩 받는 고정 룰입니다.

### 2.6 턴 진행 규칙

정확한 개념/원리:

1. 현재 턴은 `CurrentTurnPlayerId`로 관리합니다.
2. 턴 대상은 "딜러가 아닌 플레이어 중 TurnState가 Playing인 첫 번째 플레이어"입니다.
3. `Hit`/`Stand`는 다음 조건을 만족해야 합니다.
1. 입장 상태 (`NOT_JOINED` 아니어야 함)
2. 게임 상태 `InRound`
3. 요청자가 딜러가 아님 (`DEALER_IS_AUTO`)
4. 요청자가 현재 턴 플레이어임 (`NOT_YOUR_TURN`)
5. 요청자 TurnState가 `Playing`임 (`ALREADY_DONE`)

쉬운 비유:

- 발표 순서가 정해져 있고, 자기 차례가 아닌 학생은 발표 버튼을 누를 수 없습니다.

### 2.7 Hit 규칙

정확한 개념/원리:

1. 현재 턴 플레이어에게 카드 1장을 추가합니다.
2. 점수를 다시 계산합니다.
3. 점수 > 21 이면 `Busted`.
4. 점수 == 21 이면 `Standing`.
5. 여전히 `Playing`이면 턴 유지, 아니면 다음 플레이어로 턴 이동.
6. 일반 플레이어 중 `Playing`이 없으면 라운드 종료 절차로 이동합니다.

쉬운 비유:

- 카드 더 받기 버튼이며, 21에 도달하면 자동으로 "이제 멈춤" 상태가 됩니다.

### 2.8 Stand 규칙

정확한 개념/원리:

1. 현재 플레이어를 `Standing`으로 바꿉니다.
2. 다음 `Playing` 플레이어로 턴을 이동합니다.
3. 일반 플레이어가 모두 `Playing`이 아니면 라운드 종료 절차로 이동합니다.

쉬운 비유:

- "저는 더 안 받겠습니다"를 선언하는 버튼입니다.

### 2.9 딜러 자동 진행 규칙

정확한 개념/원리:

1. 일반 플레이어 행동이 모두 끝난 뒤에만 딜러를 자동 진행합니다.
2. 딜러 점수 < `DealerStandScore`(기본 17)면 자동 Hit 반복.
3. 딜러 점수 >= 17이면 `Standing`.
4. 딜러가 Bust면 일반 플레이어는 Bust가 아닌 한 모두 Win.

쉬운 비유:

- 딜러는 사람이 직접 버튼 누르지 않고, 시스템이 규칙대로 자동 행동합니다.

### 2.10 승패 계산 규칙

정확한 개념/원리:

일반 플레이어별 판정:

1. 플레이어가 Bust면 무조건 Lose.
2. 딜러가 Bust면 플레이어는 Win.
3. 둘 다 Bust가 아니면 점수 비교:
1. 플레이어 > 딜러: Win
2. 플레이어 < 딜러: Lose
3. 플레이어 == 딜러: Tie

쉬운 비유:

- 각 플레이어가 딜러와 1:1 비교 대결을 하는 방식입니다.

### 2.11 라운드 종료 후 상태

정확한 개념/원리:

1. 라운드 종료 시 `Phase=Idle`.
2. `CurrentTurnPlayerId`는 빈 문자열.
3. 직전 라운드의 결과(`Outcome`)는 Idle 상태에서 유지됩니다.
4. 다음 `StartRound`에서 결과/카드/점수는 다시 초기화됩니다.

쉬운 비유:

- 경기 끝난 뒤 스코어보드는 잠시 남아 있고, 다음 경기 시작 버튼을 누르면 초기화됩니다.

### 2.12 퇴장(Leave/Disconnect) 규칙

정확한 개념/원리:

1. 일반 플레이어 퇴장:
1. 해당 플레이어만 제거
2. 진행 중이고 현재 턴이 떠난 플레이어면 다음 턴 재계산
3. 진행 중인데 일반 플레이어가 0명이면 라운드 종료 후 Idle
4. 진행 중이고 플레이 가능한 일반 플레이어가 없으면 즉시 라운드 정산

2. 딜러 퇴장:
1. 전체 플레이어 목록 비움
2. 연결 매핑 비움
3. `Phase=Idle`, 턴/딜러 ID 초기화
4. 전원에게 `GAME_TERMINATED` 공지 1회
5. 초기화된 상태 전송 1회

쉬운 비유:

- 진행 담당 교사가 나가면 수업을 계속하지 않고 반 자체를 종료하는 정책입니다.

### 2.13 Shoe(카드 더미) 고갈 규칙

정확한 개념/원리:

1. 카드가 더 이상 없으면 `SHOE_EMPTY`.
2. 라운드를 종료하고 `Idle`로 전환.
3. 전원에게 공지 + 상태 전송.

쉬운 비유:

- 문제지가 다 떨어지면 그 라운드는 강제로 종료하고 다음 라운드로 넘어갑니다.

### 2.14 서버-클라이언트 전송 규칙

정확한 개념/원리:

1. 성공 처리 후 상태는 기본적으로 `Clients.All.OnStateChanged`.
2. 요청자 개인 오류는 `Clients.Caller.OnError`.
3. 전역 공지(예: `GAME_TERMINATED`, `SHOE_EMPTY`)는 `Clients.All.OnError`.
4. 딜러 종료 시 공지 1회 후 상태 1회 순서 보장.

쉬운 비유:

- 개인 피드백은 개인 채팅, 전체 공지는 단체 채팅으로 보냅니다.

### 2.15 동시성(Queue 직렬 처리) 규칙

정확한 개념/원리:

1. 모든 명령은 `ChannelGameCommandProcessor` 큐로 들어갑니다.
2. 단일 소비자(`SingleReader`)가 순서대로 처리합니다.
3. 따라서 동시에 버튼을 눌러도 상태 변경은 순차적으로 일어납니다.

쉬운 비유:

- 급식 줄에서 한 명씩 배식받는 구조와 같습니다.

### 2.16 에러 코드 13종 상세 사전

1. `GAME_IN_PROGRESS`: 진행 중이라 입장/시작 불가
2. `NOT_JOINED`: 입장하지 않은 연결
3. `NOT_DEALER`: 딜러 권한 없음
4. `DEALER_IS_AUTO`: 딜러는 수동 Hit/Stand 불가
5. `DEALER_ALREADY_EXISTS`: 딜러 중복 입장 시도
6. `INVALID_NAME`: 이름 규칙 위반(Trim 후 1~20)
7. `ALREADY_JOINED`: 같은 연결 재입장
8. `GAME_TERMINATED`: 딜러 퇴장으로 게임 종료
9. `NOT_YOUR_TURN`: 현재 턴 아님
10. `GAME_NOT_INROUND`: 라운드 진행 중 아님
11. `ALREADY_DONE`: 이미 행동 끝난 플레이어
12. `INSUFFICIENT_PLAYERS`: 시작 최소 인원 부족
13. `SHOE_EMPTY`: 카드 소진

### 2.17 일반 블랙잭과 다른 단순화 포인트

정확한 개념/원리:

1. Ace 가변 1/11 규칙 미사용
2. 스플릿/더블다운/보험 없음
3. 멀티룸 없음
4. 딜러 수동 행동 없음(완전 자동)
5. 룸 종료 정책이 강함(딜러 퇴장 = 전체 종료)

쉬운 비유:

- "교육용 축약 버전"이라고 안내하면 학생들이 규칙 차이를 혼동하지 않습니다.

---

## 3. 프로젝트 제작 순서 (수업용 확정)

1. `Core`
2. `Backend`
3. `Client`
4. `Backend.Tests` (강사 검증)
5. `Frontend`
6. `Frontend.Tests` (강사 검증)

이유:

1. 계약(Core) -> 구현(Backend) -> 소비자(Client/Frontend) 순서가 이해가 쉽습니다.
2. 테스트 프로젝트는 "완성 후 자동 검증"으로 붙입니다.

---

## 4. 프로젝트/파일 단위 상세 강의 스크립트

읽는 법:

- 각 파일마다 순서를 고정합니다.
1. 정확한 개념/원리
2. 쉬운 비유
3. 교사 발화 예시
4. 타이핑 포인트
5. 학생 확인 질문

## 4.1 공통/솔루션

### 파일: `src/Seoul.It.Blackjack.sln`

정확한 개념/원리:

1. 솔루션 파일은 여러 프로젝트를 한 번에 빌드/실행/디버깅하기 위한 상위 컨테이너입니다.
2. 프로젝트 간 참조 관계와 빌드 구성을 묶어 관리합니다.

쉬운 비유:

- 과목별 노트를 하나의 바인더에 정리한 "학기 바인더"입니다.

교사 발화 예시:

- "여러 프로젝트를 따로 열지 않게 해주는 최상위 파일이 솔루션입니다."

타이핑 포인트:

1. 프로젝트 이름과 경로가 실제 폴더와 일치하는지 확인합니다.
2. `Debug/Release` 구성 이름을 건드리지 않습니다.

학생 확인 질문:

- "솔루션이 없으면 프로젝트를 어떻게 불편하게 관리하게 될까요?"

### 파일: `src/Directory.Build.props`

정확한 개념/원리:

1. 모든 하위 프로젝트에 공통 빌드 규칙을 일괄 적용합니다.
2. 경고 수준, 분석기, 패키징 메타데이터를 통일합니다.

쉬운 비유:

- 모든 과목 공통으로 적용되는 학급 공통 규칙표입니다.

교사 발화 예시:

- "각 프로젝트마다 반복해서 적지 말고, 공통 규칙은 한 군데에서 관리합니다."

타이핑 포인트:

1. `EnableNETAnalyzers`, `AnalysisMode` 등 품질 설정을 확인합니다.
2. Debug/Release 조건부 속성의 차이를 구분해 설명합니다.

학생 확인 질문:

- "공통 규칙을 파일마다 복붙하면 어떤 유지보수 문제가 생길까요?"

---

## 4.2 Core 프로젝트

### 파일: `src/Seoul.It.Blackjack.Core/Seoul.It.Blackjack.Core.csproj`

정확한 개념/원리:

1. Core는 재사용 계약 라이브러리이므로 `netstandard2.0`을 사용합니다.
2. Backend/Client/Frontend가 공통으로 참조합니다.

쉬운 비유:

- 여러 앱이 같이 쓰는 표준 용어 사전입니다.

교사 발화 예시:

- "Core는 누구나 참조 가능한 최소 공통분모로 만듭니다."

타이핑 포인트:

1. `TargetFramework` 오타 금지.
2. `Nullable` 활성화로 null 실수를 줄입니다.

학생 확인 질문:

- "왜 Core에는 웹 서버 패키지가 들어가면 안 될까요?"

### 파일: `src/Seoul.It.Blackjack.Core/Domain/Suit.cs`

정확한 개념/원리:

1. 카드 무늬를 enum으로 고정해 타입 안정성을 높입니다.

쉬운 비유:

- 정해진 선택지 4개만 있는 객관식입니다.

교사 발화 예시:

- "무늬를 문자열로 쓰면 오타가 버그가 됩니다. enum으로 막겠습니다."

타이핑 포인트:

1. `Clubs, Diamonds, Hearts, Spades` 순서 그대로 입력.

학생 확인 질문:

- "`Spade` 오타가 왜 enum에서는 빨리 잡힐까요?"

### 파일: `src/Seoul.It.Blackjack.Core/Domain/Rank.cs`

정확한 개념/원리:

1. 카드 숫자를 enum으로 선언합니다.
2. `ToValue()` 확장 메서드로 점수 규칙을 중앙집중 관리합니다.

쉬운 비유:

- 카드 이름표와 채점표를 한 세트로 묶은 구조입니다.

교사 발화 예시:

- "점수 계산 규칙이 여기 한 곳에만 있으면, 나중에 규칙 변경도 쉽습니다."

타이핑 포인트:

1. `Rank.Ten or Rank.Jack ...` 패턴 매칭 문법 설명.
2. `ArgumentOutOfRangeException`은 안전장치라는 점 강조.

학생 확인 질문:

- "점수 로직을 여러 파일에 복붙하면 어떤 일이 생길까요?"

### 파일: `src/Seoul.It.Blackjack.Core/Domain/Card.cs`

정확한 개념/원리:

1. 카드 한 장은 `Suit + Rank` 조합입니다.
2. 생성자 주입 후 읽기 전용으로 사용합니다.

쉬운 비유:

- 주민등록증 발급 후 성별/생년월일을 바꾸지 않는 것과 비슷합니다.

교사 발화 예시:

- "카드는 생성 순간에 확정되고, 라운드 중 바뀌지 않아야 합니다."

타이핑 포인트:

1. 생성자 파라미터와 프로퍼티 이름 일치.

학생 확인 질문:

- "카드 값이 중간에 바뀌면 게임 신뢰성에 어떤 문제?"

### 파일: `src/Seoul.It.Blackjack.Core/Contracts/GamePhase.cs`

정확한 개념/원리:

1. 게임 전역 상태를 단순 2단계로 제한합니다.

쉬운 비유:

- 교실 상태가 "수업 전" 또는 "수업 중" 두 가지만 있는 것과 같습니다.

교사 발화 예시:

- "상태 수를 줄이면 버그 수가 줄어듭니다."

타이핑 포인트:

1. `Idle`, `InRound` 값 그대로 유지.

학생 확인 질문:

- "상태가 많아질수록 무엇이 어려워질까요?"

### 파일: `src/Seoul.It.Blackjack.Core/Contracts/PlayerTurnState.cs`

정확한 개념/원리:

1. 플레이어 행동 가능 여부를 분리해 표현합니다.

쉬운 비유:

- 발표 대기(Playing), 발표 완료(Standing), 규정 위반 탈락(Busted) 상태표입니다.

교사 발화 예시:

- "행동 상태와 승패는 다른 정보입니다. 분리해서 저장합니다."

타이핑 포인트:

1. `Playing`, `Standing`, `Busted` 정확 입력.

학생 확인 질문:

- "왜 Win/Lose를 턴 상태에 섞으면 안 될까요?"

### 파일: `src/Seoul.It.Blackjack.Core/Contracts/RoundOutcome.cs`

정확한 개념/원리:

1. 라운드 결과를 명확히 표현합니다.

쉬운 비유:

- 시험 결과표에서 합격/불합격/동점/미채점(None)을 구분하는 것과 같습니다.

교사 발화 예시:

- "라운드 시작 직후에는 아직 결과가 없으므로 None이 필요합니다."

타이핑 포인트:

1. `None` 기본값의 의미를 강조.

학생 확인 질문:

- "라운드 중간에 Outcome을 확정하면 어떤 오류가 생길까요?"

### 파일: `src/Seoul.It.Blackjack.Core/Contracts/PlayerState.cs`

정확한 개념/원리:

1. 한 플레이어에 필요한 화면/판정 데이터를 모두 담습니다.

쉬운 비유:

- 학생 한 명의 출석카드(이름, 역할, 점수, 현재상태)입니다.

교사 발화 예시:

- "화면을 다시 그리려면 이 정보가 빠짐없이 있어야 합니다."

타이핑 포인트:

1. `Cards`는 리스트, `TurnState`와 `Outcome` 기본값 설정 확인.

학생 확인 질문:

- "`IsDealer`가 없으면 프론트에서 어떤 표시를 못할까요?"

### 파일: `src/Seoul.It.Blackjack.Core/Contracts/GameState.cs`

정확한 개념/원리:

1. 게임 전체 스냅샷(브로드캐스트 단위)입니다.

쉬운 비유:

- 칠판에 적힌 반 전체 진행 현황판입니다.

교사 발화 예시:

- "서버는 상태가 바뀔 때마다 이 구조를 전체에게 방송합니다."

타이핑 포인트:

1. `Players`, `DealerPlayerId`, `CurrentTurnPlayerId`, `StatusMessage` 확인.

학생 확인 질문:

- "CurrentTurnPlayerId가 빠지면 버튼 활성화 판단이 가능할까요?"

### 파일: `src/Seoul.It.Blackjack.Core/Contracts/IBlackjackServer.cs`

정확한 개념/원리:

1. 클라이언트가 서버로 보내는 명령 계약입니다.

쉬운 비유:

- 리모컨 버튼 목록입니다.

교사 발화 예시:

- "서버 명령은 이 인터페이스에 적힌 것만 허용합니다."

타이핑 포인트:

1. 메서드명 오타 금지.
2. Join의 `dealerKey`는 nullable.

학생 확인 질문:

- "메서드명을 문자열 대신 인터페이스로 관리하면 장점이 뭘까요?"

### 파일: `src/Seoul.It.Blackjack.Core/Contracts/IBlackjackClient.cs`

정확한 개념/원리:

1. 서버가 클라이언트를 콜백할 때 쓰는 계약입니다.

쉬운 비유:

- 방송국(서버)이 수신기(클라이언트)에게 보내는 고정 채널 2개입니다.

교사 발화 예시:

- "상태 전송은 OnStateChanged, 오류 전송은 OnError로 분리합니다."

타이핑 포인트:

1. `OnStateChanged`, `OnError` 이름 일치가 중요.

학생 확인 질문:

- "오류도 상태와 같은 메서드로 보내면 어떤 불편이 생길까요?"

---

## 4.3 Backend 프로젝트

### 파일: `src/Seoul.It.Blackjack.Backend/Seoul.It.Blackjack.Backend.csproj`

정확한 개념/원리:

1. ASP.NET Core Web 서버 프로젝트입니다.
2. SignalR/Swagger를 사용합니다.

쉬운 비유:

- 실제 운영 서버의 설계도 파일입니다.

교사 발화 예시:

- "이 프로젝트는 브라우저와 실시간 통신하는 웹 서버입니다."

타이핑 포인트:

1. `Swashbuckle.AspNetCore` 패키지 확인.
2. Core 프로젝트 참조 확인.

학생 확인 질문:

- "서버 프로젝트가 Core를 참조해야 하는 이유는?"

### 파일: `src/Seoul.It.Blackjack.Backend/Program.cs`

정확한 개념/원리:

1. 서버 시작점이며 DI 조립을 담당합니다.
2. Hub 엔드포인트를 등록하고 앱을 실행합니다.

쉬운 비유:

- 식당 오픈 전 주방/홀/직원 배치를 모두 세팅하는 오픈 체크리스트입니다.

교사 발화 예시:

- "Program은 로직이 아니라 조립 담당입니다."

타이핑 포인트:

1. `AddSignalR`, `AddSwaggerGen`, 옵션/서비스 등록 순서.
2. `app.MapHub<GameSessionHub>(GameSessionHub.Endpoint)` 확인.

학생 확인 질문:

- "Hub 매핑을 안 하면 클라이언트가 어디로 연결할까요?"

### 파일: `src/Seoul.It.Blackjack.Backend/Extensions/ServiceCollectionExtensions.cs`

정확한 개념/원리:

1. 옵션 바인딩 코드를 확장 메서드로 분리합니다.

쉬운 비유:

- 반복되는 준비 동작을 매뉴얼 카드로 빼두는 것과 같습니다.

교사 발화 예시:

- "Program을 짧게 유지하기 위해 등록 코드를 분리합니다."

타이핑 포인트:

1. `Configure<T>(GetSection(...))` 패턴 정확히 입력.

학생 확인 질문:

- "분리하지 않으면 Program이 어떻게 변할까요?"

### 파일: `src/Seoul.It.Blackjack.Backend/appsettings.json`

정확한 개념/원리:

1. 실행 환경 설정(로그 레벨, 딜러 키 등)을 코드 밖으로 분리합니다.

쉬운 비유:

- 코드 본문이 아니라 설정표(교실 시간표)입니다.

교사 발화 예시:

- "딜러 키를 코드에 하드코딩하지 않고 설정에서 주입합니다."

타이핑 포인트:

1. `Dealer:Key` 값이 프론트 입력값과 비교 대상임을 설명.

학생 확인 질문:

- "키를 바꾸고 싶을 때 코드 수정 없이 어디만 바꾸면 될까요?"

### 파일: `src/Seoul.It.Blackjack.Backend/Properties/launchSettings.json`

정확한 개념/원리:

1. 개발 실행 프로필/포트를 정의합니다.

쉬운 비유:

- 서버 실행 시 사용하는 출입문 주소표입니다.

교사 발화 예시:

- "프론트가 연결하는 주소와 launchSettings 포트가 같아야 합니다."

타이핑 포인트:

1. `https://localhost:5000;http://localhost:5001` 확인.

학생 확인 질문:

- "주소가 틀리면 어떤 에러가 화면에 보일까요?"

### 파일: `src/Seoul.It.Blackjack.Backend/Seoul.It.Blackjack.Backend.http`

정확한 개념/원리:

1. HTTP 수동 테스트용 템플릿 파일입니다.

쉬운 비유:

- 빠른 점검용 메모지입니다.

교사 발화 예시:

- "현재 핵심은 SignalR이지만, HTTP 확인 파일도 남겨둡니다."

타이핑 포인트:

1. 기본 주소가 실제 포트와 다를 수 있음을 안내.

학생 확인 질문:

- "왜 템플릿 파일이 남아 있어도 기능과 직접 연결되지 않을까요?"

### 파일: `src/Seoul.It.Blackjack.Backend/Options/DealerOptions.cs`

정확한 개념/원리:

1. 딜러 키 설정 모델입니다.

쉬운 비유:

- 반장 인증번호를 담는 상자입니다.

교사 발화 예시:

- "딜러 여부는 이 키 일치로만 판정합니다."

타이핑 포인트:

1. `DefaultSectionName = "Dealer"` 확인.

학생 확인 질문:

- "왜 이름으로 딜러를 판정하지 않고 키로 판정할까요?"

### 파일: `src/Seoul.It.Blackjack.Backend/Options/GameRuleOptions.cs`

정확한 개념/원리:

1. 게임 룰 상수를 외부 설정 가능한 옵션으로 둡니다.

쉬운 비유:

- 학교 규칙 숫자(최소 인원, 최대 글자 수)를 적어둔 규정집입니다.

교사 발화 예시:

- "하드코딩 대신 옵션으로 빼면 정책 변경이 쉬워집니다."

타이핑 포인트:

1. 기본값: DeckCount=4, DealerStandScore=17, MinPlayersToStart=2, 이름 길이 1~20.

학생 확인 질문:

- "MinPlayersToStart를 3으로 바꾸면 어떤 동작이 달라지나요?"

### 파일: `src/Seoul.It.Blackjack.Backend/Hubs/GameSessionHub.cs`

정확한 개념/원리:

1. 네트워크 진입점입니다.
2. 규칙 판단은 서비스에 위임하고, 전송 정책만 담당합니다.

쉬운 비유:

- 문지기(허브)는 출입 통제만 하고, 판정은 심판실(서비스)이 합니다.

교사 발화 예시:

- "Hub는 얇게 유지합니다. 비즈니스 로직은 넣지 않습니다."

타이핑 포인트:

1. `Hub<IBlackjackClient>, IBlackjackServer` 구현 의미 설명.
2. `ExecuteAsync`에서 `GameRoomException`만 잡아 Caller 오류 전송.
3. `BroadcastResultAsync`의 순서: Notice(All.OnError) -> State(All.OnStateChanged).

학생 확인 질문:

- "요청 오류를 전체 방송하면 어떤 UX 문제가 생길까요?"

### 파일: `src/Seoul.It.Blackjack.Backend/Services/IGameRoomService.cs`

정확한 개념/원리:

1. 허브가 의존하는 추상 인터페이스입니다.

쉬운 비유:

- 허브가 호출할 기능 목록을 적은 계약서입니다.

교사 발화 예시:

- "허브는 구현체 이름을 몰라도 됩니다. 인터페이스만 알면 됩니다."

타이핑 포인트:

1. Join/Leave/Disconnect/StartRound/Hit/Stand 반환형 `Task<GameOperationResult>` 통일.

학생 확인 질문:

- "인터페이스를 쓰면 테스트나 교체가 왜 쉬워질까요?"

### 파일: `src/Seoul.It.Blackjack.Backend/Services/ConnectionRegistry.cs`

정확한 개념/원리:

1. 연결 ID 등록소입니다.
2. 내부 Dictionary를 lock으로 보호합니다.

쉬운 비유:

- 출석부를 한 명씩만 동시에 쓰게 잠금 장치를 건 상태입니다.

교사 발화 예시:

- "동시 접속 환경에서는 컬렉션 접근 보호가 필수입니다."

타이핑 포인트:

1. `ContainsConnection`, `Add`, `TryRemove`, `Clear` 동작 확인.
2. `TryRemove` 실패 시 `playerId=string.Empty` 처리 설명.

학생 확인 질문:

- "lock 없이 동시에 Remove하면 어떤 문제가 생길까요?"

### 파일: `src/Seoul.It.Blackjack.Backend/Services/GameRoomService.cs`

정확한 개념/원리:

1. 게임 상태 오케스트레이션의 핵심입니다.
2. 명령 생성 -> 큐 위임 -> 핸들러에서 상태 변경 패턴입니다.
3. 내부 필드로 현재 상태를 유지합니다.

쉬운 비유:

- 교실 운영 총괄 선생님 역할입니다. 요청을 받아 규칙 검사 후 상태를 갱신합니다.

교사 발화 예시:

- "이 클래스가 길어 보이지만, 실제로는 상태 조정과 흐름 제어를 담당합니다."

타이핑 포인트:

1. public 메서드는 모두 명령을 만들고 `_commandProcessor.EnqueueAsync` 호출.
2. `HandleJoin`:
1. InRound 입장 차단
2. 재입장 차단
3. 이름 정규화
4. 딜러 중복 차단
3. `HandleLeave`:
1. 딜러 퇴장 시 전체 초기화 + GAME_TERMINATED
2. 일반 플레이어 퇴장 시 턴 재계산
4. `HandleStartRound`는 validator + round engine 연동.
5. `HandleHit`/`HandleStand`는 `ValidatePlayerAction` 선행.
6. `CreateResult`는 스냅샷 팩토리 사용.
7. `CreateSilentResult`는 상태 전송 생략 케이스.

학생 확인 질문:

- "딜러 퇴장 로직이 일반 퇴장과 왜 완전히 다를까요?"

### 파일: `src/Seoul.It.Blackjack.Backend/Services/Infrastructure/IGameCommandProcessor.cs`

정확한 개념/원리:

1. 명령 직렬 처리 추상 인터페이스입니다.

쉬운 비유:

- 대기열 처리 담당자에게 전달하는 요청 양식입니다.

교사 발화 예시:

- "서비스는 큐 구현 세부를 몰라도 됩니다."

타이핑 포인트:

1. `EnqueueAsync(GameCommand, Func<GameOperationResult>)` 시그니처 의미 설명.

학생 확인 질문:

- "핸들러를 매개변수로 넘기는 이유는 무엇일까요?"

### 파일: `src/Seoul.It.Blackjack.Backend/Services/Infrastructure/ChannelGameCommandProcessor.cs`

정확한 개념/원리:

1. `Channel` 기반 비동기 큐 구현입니다.
2. 모든 명령을 단일 소비자로 직렬 처리합니다.

쉬운 비유:

- 번호표를 뽑고 창구 1개에서 순서대로 처리하는 은행 창구입니다.

교사 발화 예시:

- "여러 명이 동시에 버튼 눌러도 실제 상태 변경은 한 줄로 처리됩니다."

타이핑 포인트:

1. `Channel.CreateUnbounded` 옵션 `SingleReader=true`.
2. 생성자에서 `Task.Run(ProcessLoopAsync)`로 소비 루프 시작.
3. `TaskCompletionSource`로 호출자에게 결과 전달.
4. 예외는 `Completion.SetException(ex)`로 호출자에게 전파.

학생 확인 질문:

- "왜 결과를 바로 return하지 않고 TaskCompletionSource를 쓸까요?"

### 파일: `src/Seoul.It.Blackjack.Backend/Services/Rules/IGameRuleValidator.cs`

정확한 개념/원리:

1. 검증 책임을 서비스에서 분리하기 위한 인터페이스입니다.

쉬운 비유:

- 경기 규칙만 전문으로 보는 심판 보조표입니다.

교사 발화 예시:

- "검증 로직을 별도 타입으로 분리해 가독성을 높입니다."

타이핑 포인트:

1. 이름 검증, 입장 검증, 시작 검증, 액션 검증 메서드 구분.

학생 확인 질문:

- "검증 로직이 서비스 안에 섞이면 어떤 문제가 생기나요?"

### 파일: `src/Seoul.It.Blackjack.Backend/Services/Rules/GameRuleValidator.cs`

정확한 개념/원리:

1. 실제 규칙 검증 구현체입니다.
2. 실패 시 코드가 있는 도메인 예외를 던집니다.

쉬운 비유:

- 규정 위반 시 벌점 코드와 사유를 같이 적어주는 심판입니다.

교사 발화 예시:

- "여기서 던진 코드가 그대로 프론트 오류 코드가 됩니다."

타이핑 포인트:

1. `NormalizeName`에서 `Trim` + 길이 검증.
2. `EnsureCanStartRound`의 순서: joined -> phase -> dealer -> min players.
3. `ValidatePlayerAction`의 순서: joined -> inround -> not dealer -> turn -> playing state.

학생 확인 질문:

- "검증 순서를 바꾸면 사용자에게 보이는 오류 코드가 바뀔 수 있을까요?"

### 파일: `src/Seoul.It.Blackjack.Backend/Services/State/IGameStateSnapshotFactory.cs`

정확한 개념/원리:

1. 내부 상태를 외부 전송 DTO로 만드는 팩토리 추상화입니다.

쉬운 비유:

- 원본 장부를 복사해 공지판용 요약본을 만드는 복사 담당입니다.

교사 발화 예시:

- "직접 DTO를 만들지 않고 팩토리로 추상화하면 교체/테스트가 쉬워집니다."

타이핑 포인트:

1. 파라미터에 상태 필드가 모두 포함되는지 확인.

학생 확인 질문:

- "왜 서비스 내부 리스트를 그대로 반환하면 안 될까요?"

### 파일: `src/Seoul.It.Blackjack.Backend/Services/State/GameStateSnapshotFactory.cs`

정확한 개념/원리:

1. 상태 전송 전 깊은 복사를 수행합니다.

쉬운 비유:

- 칠판에 붙일 사본을 만들 때 원본 공책은 건드리지 않는 방식입니다.

교사 발화 예시:

- "네트워크 전송은 원본 참조를 끊는 것이 안전합니다."

타이핑 포인트:

1. `Players = players.Select(ClonePlayer).ToList()`.
2. `Cards`도 새 `Card` 객체로 복사.

학생 확인 질문:

- "깊은 복사와 얕은 복사의 차이를 예시로 말해볼까요?"

### 파일: `src/Seoul.It.Blackjack.Backend/Services/Round/IRoundEngine.cs`

정확한 개념/원리:

1. 라운드 로직 API 계약입니다.

쉬운 비유:

- 경기 진행 엔진의 버튼 목록입니다.

교사 발화 예시:

- "Start/Hit/Stand/Complete를 서비스에서 분리해 테스트 가능성을 높입니다."

타이핑 포인트:

1. `RoundResolution` 반환 구조 유지.

학생 확인 질문:

- "라운드 계산을 서비스와 분리하면 무엇이 좋아질까요?"

### 파일: `src/Seoul.It.Blackjack.Backend/Services/Round/RoundResolution.cs`

정확한 개념/원리:

1. 엔진이 서비스에 돌려주는 처리 결과 DTO입니다.

쉬운 비유:

- 판정 결과지(다음 상태/턴/메시지/공지)를 한 장으로 묶은 문서입니다.

교사 발화 예시:

- "엔진은 상태를 직접 브로드캐스트하지 않고 결과만 반환합니다."

타이핑 포인트:

1. `Phase`, `CurrentTurnPlayerId`, `StatusMessage`, `Shoe`, `Notice`.

학생 확인 질문:

- "엔진이 결과 DTO를 반환하는 방식의 장점은?"

### 파일: `src/Seoul.It.Blackjack.Backend/Services/Round/RoundEngine.cs`

정확한 개념/원리:

1. 게임 규칙 계산의 실제 구현체입니다.
2. 시작/히트/스탠드/완료 흐름을 일관되게 처리합니다.

쉬운 비유:

- 규정집만 보고 자동으로 경기 흐름을 진행하는 심판 로봇입니다.

교사 발화 예시:

- "규칙을 한 클래스에 모아두면, 수업에서 규칙 설명이 쉬워집니다."

타이핑 포인트:

1. `StartRound`:
1. Shoe 생성
2. 모든 플레이어 라운드 필드 초기화
3. 2장씩 배분
4. 첫 턴 계산
2. `HandleHit`:
1. 카드 뽑기 실패 시 `EndRoundByShoeEmpty`
2. 점수 재계산 후 턴 유지/이동
3. `HandleStand`: 상태를 Standing으로 바꾸고 턴 이동.
4. `CompleteRound`:
1. 딜러 자동 진행 (<17)
2. 결과 계산
3. Idle 전환
5. `RecalculatePlayerState`:
1. >21 Busted
2. ==21 Standing

학생 확인 질문:

- "왜 21점이면 즉시 Standing으로 바꾸나요?"

### 파일: `src/Seoul.It.Blackjack.Backend/Services/Commands/GameCommandType.cs`

정확한 개념/원리:

1. 큐 명령 종류를 enum으로 고정합니다.

쉬운 비유:

- 고객 요청 유형 코드표입니다.

교사 발화 예시:

- "명령 문자열 대신 enum을 쓰면 오타를 막을 수 있습니다."

타이핑 포인트:

1. `Join, Leave, StartRound, Hit, Stand, Disconnect`.

학생 확인 질문:

- "Disconnect를 명령 타입으로 분리한 이유는?"

### 파일: `src/Seoul.It.Blackjack.Backend/Services/Commands/GameCommand.cs`

정확한 개념/원리:

1. 큐에 들어가는 단일 요청 객체입니다.

쉬운 비유:

- 창구로 전달되는 신청서 1장입니다.

교사 발화 예시:

- "연결ID와 요청 파라미터를 한 객체에 묶어 큐로 보냅니다."

타이핑 포인트:

1. 생성자 파라미터 기본값(`name`, `dealerKey`) nullable.

학생 확인 질문:

- "Join과 Hit가 같은 타입으로 표현되려면 어떤 공통 필드가 필요할까요?"

### 파일: `src/Seoul.It.Blackjack.Backend/Services/Commands/GameNotice.cs`

정확한 개념/원리:

1. 전역 공지(코드+메시지) 전송 모델입니다.

쉬운 비유:

- 반 전체 공지 방송 문구 카드입니다.

교사 발화 예시:

- "상태 전송과 공지를 분리하면 UI 처리도 분리됩니다."

타이핑 포인트:

1. 불변 형태(`get` only) 유지.

학생 확인 질문:

- "공지와 상태를 섞으면 어떤 UI 문제가 생길까요?"

### 파일: `src/Seoul.It.Blackjack.Backend/Services/Commands/GameOperationResult.cs`

정확한 개념/원리:

1. 서비스 한 번 처리 결과를 래핑합니다.

쉬운 비유:

- 처리 결과 영수증(상태 스냅샷 + 공지 + 방송 여부 체크박스)입니다.

교사 발화 예시:

- "허브는 이 결과 객체만 보고 무엇을 방송할지 결정합니다."

타이핑 포인트:

1. `ShouldPublishState` 기본값 true.

학생 확인 질문:

- "왜 상태 전송을 끌 수 있는 플래그가 필요할까요?"

### 파일: `src/Seoul.It.Blackjack.Backend/Services/Exceptions/GameRoomException.cs`

정확한 개념/원리:

1. 게임 도메인 예외의 공통 부모입니다.
2. `Code`를 갖는 것이 핵심입니다.

쉬운 비유:

- 모든 경고문에 공통 양식(경고코드+설명)을 쓰는 것과 같습니다.

교사 발화 예시:

- "에러코드는 프론트와 약속한 프로토콜입니다."

타이핑 포인트:

1. `: base(message)`와 `Code` 저장 확인.

학생 확인 질문:

- "메시지만 있고 코드가 없으면 프론트가 어떤 점에서 어려울까요?"

### 파일: `src/Seoul.It.Blackjack.Backend/Services/Exceptions/GameAuthorizationException.cs`

정확한 개념/원리:

1. 권한 관련 예외 타입입니다.

쉬운 비유:

- 권한 없는 학생이 교사용 버튼을 눌렀을 때의 경고 유형입니다.

교사 발화 예시:

- "권한 실패는 별도 타입으로 구분해 의미를 명확히 합니다."

타이핑 포인트:

1. 생성자에서 base 전달 구조 확인.

학생 확인 질문:

- "권한 오류와 입력 오류를 왜 분리할까요?"

### 파일: `src/Seoul.It.Blackjack.Backend/Services/Exceptions/GameRuleException.cs`

정확한 개념/원리:

1. 룰 위반(턴, 상태, 진행 조건) 예외 타입입니다.

쉬운 비유:

- 게임 규칙 위반 반칙 휘슬입니다.

교사 발화 예시:

- "규칙 위반은 권한 위반과 다른 레이어로 봅니다."

타이핑 포인트:

1. GameRoomException 상속 구조 유지.

학생 확인 질문:

- "`NOT_YOUR_TURN`은 권한 문제일까요, 규칙 문제일까요?"

### 파일: `src/Seoul.It.Blackjack.Backend/Services/Exceptions/GameValidationException.cs`

정확한 개념/원리:

1. 입력값 검증 실패 예외 타입입니다.

쉬운 비유:

- 양식 작성 오류(빈칸, 길이 초과) 경고입니다.

교사 발화 예시:

- "검증 실패는 요청 자체가 유효하지 않다는 뜻입니다."

타이핑 포인트:

1. 이름/재입장 오류 코드와 연결해서 설명.

학생 확인 질문:

- "검증 예외를 분리하면 사용자 메시지 설계에 어떤 장점이 있나요?"

### 파일: `src/Seoul.It.Blackjack.Backend/Models/Deck.cs`

정확한 개념/원리:

1. 52장 덱 생성기입니다.
2. 모든 Suit x 모든 Rank 조합을 만듭니다.

쉬운 비유:

- 무늬 4개와 숫자 13개를 곱해서 전체 카드 목록을 만드는 조합표입니다.

교사 발화 예시:

- "`SelectMany`는 이중 for문을 함수형으로 표현한 문법입니다."

타이핑 포인트:

1. `Enum.GetValues<Suit>()` + `Enum.GetValues<Rank>()`.

학생 확인 질문:

- "왜 Deck는 상태 없이 정적 생성기로 두었을까요?"

### 파일: `src/Seoul.It.Blackjack.Backend/Models/Shoe.cs`

정확한 개념/원리:

1. 여러 덱을 섞어 실제 드로우를 담당합니다.
2. 내부는 `ConcurrentStack<Card>`를 사용합니다.

쉬운 비유:

- 여러 카드 뭉치를 한 상자에 넣고 섞은 뒤 위에서 꺼내는 카드통입니다.

교사 발화 예시:

- "TryDraw 패턴으로 안전하게 카드 부족을 처리합니다."

타이핑 포인트:

1. `deckCount` 반복으로 카드 추가.
2. `Random.Shared.Shuffle` 후 스택 생성.
3. `TryDraw` 실패 시 false 반환.

학생 확인 질문:

- "왜 Draw와 TryDraw를 둘 다 만들었을까요?"

---

## 4.4 Client 프로젝트

### 파일: `src/Seoul.It.Blackjack.Client/Seoul.It.Blackjack.Client.csproj`

정확한 개념/원리:

1. 재사용 라이브러리 프로젝트이며 SignalR Client를 참조합니다.

쉬운 비유:

- 다른 앱들이 가져다 쓰는 공용 리모컨 라이브러리입니다.

교사 발화 예시:

- "Client 프로젝트는 서버 코드가 아니라 연결 도구를 담습니다."

타이핑 포인트:

1. `Microsoft.AspNetCore.SignalR.Client` 버전 일치 확인.

학생 확인 질문:

- "Backend와 Client를 같은 프로젝트에 섞지 않는 이유는?"

### 파일: `src/Seoul.It.Blackjack.Client/Options/BlackjackClientOptions.cs`

정확한 개념/원리:

1. Hub URL 설정을 보관하는 옵션 객체입니다.

쉬운 비유:

- 리모컨이 어느 TV로 연결할지 적어둔 주소 메모입니다.

교사 발화 예시:

- "옵션 객체는 나중에 DI로 주입받아 사용합니다."

타이핑 포인트:

1. `HubUrl` 속성만 단순 유지.

학생 확인 질문:

- "URL 하드코딩과 옵션 분리의 차이는?"

### 파일: `src/Seoul.It.Blackjack.Client/BlackjackClient.cs`

정확한 개념/원리:

1. SignalR 연결/호출/이벤트 수신을 캡슐화한 핵심 클래스입니다.
2. `IBlackjackClient`를 구현하여 콜백 계약을 일치시킵니다.

쉬운 비유:

- 복잡한 통신 절차를 숨기고 버튼만 노출한 리모컨입니다.

교사 발화 예시:

- "앱에서는 이 클래스만 알면 서버 호출이 가능합니다."

타이핑 포인트:

1. `_connection` null 체크 후 중복 Connect 방지.
2. `_connection.On<GameState>(nameof(IBlackjackClient.OnStateChanged), ...)`.
3. 서버 호출은 `InvokeAsync(nameof(IBlackjackServer.XXX))`.
4. `EnsureConnection()`으로 사전 연결 강제.

학생 확인 질문:

- "왜 Connect 전에 Hit을 호출하면 예외를 던지게 만들었을까요?"

### 파일: `src/Seoul.It.Blackjack.Client/Extensions/ServiceCollectionExtensions.cs`

정확한 개념/원리:

1. DI 등록 확장 메서드입니다.
2. 옵션 객체와 클라이언트를 Singleton으로 등록합니다.

쉬운 비유:

- 앱 시작 시 공용 리모컨 1대를 준비해 놓는 방식입니다.

교사 발화 예시:

- "등록 시 null 방어를 넣는 습관은 매우 중요합니다."

타이핑 포인트:

1. `ArgumentNullException` 방어 코드.
2. `services.AddSingleton(options)` + `services.AddSingleton<BlackjackClient>()`.

학생 확인 질문:

- "Singleton을 쓰면 어떤 중복 연결 문제를 예방할 수 있을까요?"

---

## 4.5 Backend.Tests 프로젝트

### 파일: `src/Seoul.It.Blackjack.Backend.Tests/Seoul.It.Blackjack.Backend.Tests.csproj`

정확한 개념/원리:

1. 백엔드 통합 테스트용 프로젝트 설정입니다.

쉬운 비유:

- 채점 도구 상자를 구성하는 부품 목록입니다.

교사 발화 예시:

- "테스트 프로젝트는 운영 코드와 분리해 안전하게 검증합니다."

타이핑 포인트:

1. `Microsoft.AspNetCore.Mvc.Testing`, `SignalR.Client`, MSTest 패키지 확인.

학생 확인 질문:

- "테스트 프로젝트가 운영 프로젝트를 참조하는 이유는?"

### 파일: `src/Seoul.It.Blackjack.Backend.Tests/TestHostFactory.cs`

정확한 개념/원리:

1. 테스트용 서버 팩토리입니다.

쉬운 비유:

- 실제 체육관 대신 모형 경기장을 만들어 연습하는 방식입니다.

교사 발화 예시:

- "실제 포트를 열지 않고도 서버를 검증할 수 있습니다."

타이핑 포인트:

1. `WebApplicationFactory<Program>` 상속.
2. 개발 환경 강제(`UseEnvironment("Development")`).

학생 확인 질문:

- "실제 서버를 띄우지 않아도 통합 테스트가 가능한 이유는?"

### 파일: `src/Seoul.It.Blackjack.Backend.Tests/SignalRTestClient.cs`

정확한 개념/원리:

1. 테스트에서 사용할 SignalR 클라이언트입니다.
2. 수신 이벤트를 리스트에 기록해 검증합니다.

쉬운 비유:

- 경기 중계를 받아 로그를 기록하는 기록원입니다.

교사 발화 예시:

- "테스트는 눈으로 보는 대신 기록을 비교합니다."

타이핑 포인트:

1. Hub URL: `new(factory.Server.BaseAddress, "/blackjack")`.
2. `OnStateChanged`, `OnError` 이벤트 기록 구조.
3. 호출 메서드명은 `nameof(IBlackjackServer.XXX)`.

학생 확인 질문:

- "이벤트를 리스트에 저장하면 어떤 종류의 검증이 가능해질까요?"

### 파일: `src/Seoul.It.Blackjack.Backend.Tests/TestWaiter.cs`

정확한 개념/원리:

1. 비동기 상태 변화를 기다리는 유틸입니다.

쉬운 비유:

- 버스가 올 때까지 50ms 간격으로 정류장을 확인하는 방식입니다.

교사 발화 예시:

- "비동기 테스트는 즉시 값이 바뀐다고 가정하면 실패합니다."

타이핑 포인트:

1. 폴링 간격 50ms.
2. timeout 초과 시 `TimeoutException`.

학생 확인 질문:

- "타임아웃이 없는 대기는 왜 위험할까요?"

### 파일: `src/Seoul.It.Blackjack.Backend.Tests/JoinAndRoleTests.cs`

정확한 개념/원리:

1. 입장/딜러 권한 관련 규칙 검증.

쉬운 비유:

- 출석/반장 임명 규칙 테스트입니다.

교사 발화 예시:

- "정상/실패 케이스를 같이 만들어야 규칙이 단단해집니다."

타이핑 포인트:

1. 딜러키 일치/불일치.
2. 빈 이름 오류.
3. 딜러 중복/재입장 거부.

학생 확인 질문:

- "왜 성공 테스트만으로는 부족할까요?"

### 파일: `src/Seoul.It.Blackjack.Backend.Tests/TurnRuleTests.cs`

정확한 개념/원리:

1. 턴과 권한 정책 검증.

쉬운 비유:

- 발표 순서 규칙을 어긴 경우를 검사하는 체크리스트입니다.

교사 발화 예시:

- "현재 턴이 아닌 사람이 버튼을 누르면 반드시 거부되어야 합니다."

타이핑 포인트:

1. `NOT_DEALER`, `GAME_IN_PROGRESS`, `NOT_YOUR_TURN`, `DEALER_IS_AUTO`.
2. Hit 후 전원 상태 방송 확인.

학생 확인 질문:

- "전원 브로드캐스트 검증이 왜 필요할까요?"

### 파일: `src/Seoul.It.Blackjack.Backend.Tests/RoundCompletionTests.cs`

정확한 개념/원리:

1. 라운드 종료와 다음 라운드 초기화 검증.

쉬운 비유:

- 한 경기 종료 후 점수판 유지, 다음 경기 시작 시 리셋 확인입니다.

교사 발화 예시:

- "Idle 상태에서 직전 결과를 보여주고, 다음 시작 때 초기화되는지 봅니다."

타이핑 포인트:

1. `GamePhase.Idle` 검증.
2. 다음 라운드에서 `Outcome.None` 확인.

학생 확인 질문:

- "직전 결과를 보여주는 UX가 왜 도움이 될까요?"

### 파일: `src/Seoul.It.Blackjack.Backend.Tests/DealerTerminationTests.cs`

정확한 개념/원리:

1. 딜러 종료 정책 검증(핵심 정책).

쉬운 비유:

- 경기 진행자가 사라지면 경기 전체를 종료하는 규칙 점검입니다.

교사 발화 예시:

- "딜러 퇴장 시 GAME_TERMINATED와 초기화 상태 전송 순서가 중요합니다."

타이핑 포인트:

1. Leave/Disconnect 두 케이스 검증.
2. 이벤트 순서 검증(`OnError` -> `OnStateChanged`).

학생 확인 질문:

- "순서가 바뀌면 프론트에서 어떤 UI 꼬임이 생길까요?"

### 파일: `src/Seoul.It.Blackjack.Backend.Tests/ConcurrencyTests.cs`

정확한 개념/원리:

1. 동시 요청 직렬 처리 안정성 검증.

쉬운 비유:

- 두 명이 동시에 답안 제출해도 채점기는 한 장씩 처리되는지 확인하는 테스트입니다.

교사 발화 예시:

- "동시 Hit를 보내도 상태 일관성이 깨지지 않아야 합니다."

타이핑 포인트:

1. `Task.WhenAll(hit1, hit2)`.
2. 이후 상태 증가 + `NOT_YOUR_TURN` 검증.

학생 확인 질문:

- "이 테스트가 간헐 실패하면 어떤 컴포넌트를 먼저 점검할까요?"

### 파일: `src/Seoul.It.Blackjack.Backend.Tests/GameRuleOptionsDefaultTests.cs`

정확한 개념/원리:

1. 기본 옵션값이 실제 동작에 반영되는지 확인.

쉬운 비유:

- 학급 기본 규칙(최소 인원)이 실제로 적용되는지 확인하는 체크입니다.

교사 발화 예시:

- "딜러 혼자 시작이 막혀야 기본값이 정상입니다."

타이핑 포인트:

1. `INSUFFICIENT_PLAYERS` 코드 검증.

학생 확인 질문:

- "옵션 기본값 테스트가 왜 중요할까요?"

### 파일: `src/Seoul.It.Blackjack.Backend.Tests/ClientDiIntegrationTests.cs`

정확한 개념/원리:

1. Client DI 등록과 실제 연결 동작을 함께 검증합니다.

쉬운 비유:

- 리모컨 배치(등록)와 실제 채널 전환(연결/수신)을 한 번에 검사하는 테스트입니다.

교사 발화 예시:

- "등록만 성공해도 의미가 없습니다. 실제 이벤트 수신까지 확인해야 합니다."

타이핑 포인트:

1. Singleton 검증(`Assert.AreSame`).
2. 실제 Join 후 상태 수신 대기(`TaskCompletionSource`).

학생 확인 질문:

- "DI 테스트와 기능 테스트를 왜 한 파일에서 같이 다루나요?"

---

## 4.6 Frontend 프로젝트

### 파일: `src/Seoul.It.Blackjack.Frontend/Seoul.It.Blackjack.Frontend.csproj`

정확한 개념/원리:

1. Blazor Server 앱 프로젝트 설정입니다.
2. Client/Core 참조로 계약과 연결 기능을 사용합니다.

쉬운 비유:

- UI 교실에 통신 도구(Client)와 규칙 사전(Core)을 가져오는 준비 단계입니다.

교사 발화 예시:

- "프론트는 Core 계약과 Client 도구를 같이 참조해야 완성됩니다."

타이핑 포인트:

1. ProjectReference 두 개 확인.

학생 확인 질문:

- "Frontend가 Core를 직접 참조하는 이유는?"

### 파일: `src/Seoul.It.Blackjack.Frontend/Program.cs`

정확한 개념/원리:

1. 프론트 앱 시작점 및 서비스 등록 지점입니다.

쉬운 비유:

- UI 무대 설치(라우팅/정적파일/서비스) 체크리스트입니다.

교사 발화 예시:

- "백엔드 Program과 비슷하게, 프론트도 조립 코드가 먼저입니다."

타이핑 포인트:

1. RazorComponents + InteractiveServer 설정.
2. `AddFrontendBlackjackOptions`, `AddFrontendBlackjackClient`, `AddFrontendServices`.

학생 확인 질문:

- "서비스 등록을 빼먹으면 페이지에서 어떤 예외가 날까요?"

### 파일: `src/Seoul.It.Blackjack.Frontend/appsettings.json`

정확한 개념/원리:

1. 프론트의 Hub 접속 URL 설정입니다.

쉬운 비유:

- 프론트가 접속할 서버 주소가 적힌 지도입니다.

교사 발화 예시:

- "이 주소가 백엔드 launchSettings 포트와 맞아야 합니다."

타이핑 포인트:

1. `BlackjackClient:HubUrl` 경로 확인.

학생 확인 질문:

- "http/https가 다르면 어떤 현상이 생길 수 있을까요?"

### 파일: `src/Seoul.It.Blackjack.Frontend/_Imports.razor`

정확한 개념/원리:

1. Razor 파일 공통 using 선언입니다.

쉬운 비유:

- 모든 시험지 상단에 공통으로 인쇄되는 참고 공식 모음입니다.

교사 발화 예시:

- "매 페이지마다 같은 using을 반복하지 않게 해줍니다."

타이핑 포인트:

1. Core 계약/도메인, Extensions, Services 네임스페이스 포함.

학생 확인 질문:

- "_Imports가 없으면 페이지 코드가 어떻게 길어질까요?"

### 파일: `src/Seoul.It.Blackjack.Frontend/Options/FrontendBlackjackOptions.cs`

정확한 개념/원리:

1. 프론트 설정 모델과 기본값을 정의합니다.

쉬운 비유:

- 기본 주소가 적힌 예비 연락처 카드입니다.

교사 발화 예시:

- "설정이 비어도 동작하도록 기본값을 둡니다."

타이핑 포인트:

1. `DefaultSectionName`, `DefaultHubUrl`, `HubUrl` 기본값.

학생 확인 질문:

- "기본 URL을 http로 둔 이유를 어떻게 설명할까요?"

### 파일: `src/Seoul.It.Blackjack.Frontend/Extensions/ServiceCollectionExtensions.cs`

정확한 개념/원리:

1. 프론트 서비스 등록 일관성을 담당합니다.

쉬운 비유:

- 프론트 조립 키트 설명서입니다.

교사 발화 예시:

- "Backend 스타일과 동일하게 확장 메서드로 통일합니다."

타이핑 포인트:

1. 옵션 바인딩 후 빈 값 fallback.
2. Client DI 등록 + Frontend 서비스 등록.

학생 확인 질문:

- "등록 일관성이 있으면 팀 개발에서 어떤 장점이 있나요?"

### 파일: `src/Seoul.It.Blackjack.Frontend/Extensions/CardExtensions.cs`

정확한 개념/원리:

1. Card를 UI 이미지 경로로 변환합니다.

쉬운 비유:

- 카드 객체를 파일 주소로 번역하는 번역기입니다.

교사 발화 예시:

- "UI는 Card 객체를 바로 못 그리니 경로 문자열로 바꿔야 합니다."

타이핑 포인트:

1. 소문자 규칙 + `cards/{suit}_{rank}.svg`.

학생 확인 질문:

- "확장 메서드를 쓰는 이유는 무엇일까요?"

### 파일: `src/Seoul.It.Blackjack.Frontend/Services/FrontendEntryState.cs`

정확한 개념/원리:

1. Entry -> Table 사이 데이터 전달용 Scoped 상태입니다.

쉬운 비유:

- 두 페이지 사이에서만 쓰는 임시 쪽지함입니다.

교사 발화 예시:

- "URL 쿼리로 노출하지 않고 메모리 상태로 전달합니다."

타이핑 포인트:

1. `PlayerName`, `DealerKey` 프로퍼티.

학생 확인 질문:

- "이 상태가 없으면 Table 페이지에서 이름을 어떻게 알 수 있나요?"

### 파일: `src/Seoul.It.Blackjack.Frontend/Services/IFrontendGameSession.cs`

정확한 개념/원리:

1. UI가 게임 세션을 사용할 때의 추상 계약입니다.

쉬운 비유:

- 페이지에서 호출할 공용 리모컨 버튼 목록입니다.

교사 발화 예시:

- "페이지는 구현체를 몰라도 인터페이스만 알면 됩니다."

타이핑 포인트:

1. 이벤트 2개(`StateChanged`, `ErrorReceived`) + 명령 메서드 6개.

학생 확인 질문:

- "인터페이스가 있으면 테스트 더블 만들기가 왜 쉬울까요?"

### 파일: `src/Seoul.It.Blackjack.Frontend/Services/FrontendGameSession.cs`

정확한 개념/원리:

1. BlackjackClient와 UI를 연결하는 어댑터입니다.
2. 연결/입장 상태 플래그를 유지합니다.

쉬운 비유:

- 통신 장비를 UI 친화적인 버튼 패널로 바꿔주는 변환기입니다.

교사 발화 예시:

- "페이지는 복잡한 SignalR 세부를 몰라도 됩니다."

타이핑 포인트:

1. 생성자에서 이벤트 연결.
2. `ConnectAsync` 중복 호출 방지.
3. `GAME_TERMINATED`/`NOT_JOINED` 수신 시 `IsJoined=false`.
4. `DisposeAsync`에서 이벤트 해제.

학생 확인 질문:

- "이벤트 해제를 안 하면 어떤 메모리/중복 호출 문제가 생길까요?"

### 파일: `src/Seoul.It.Blackjack.Frontend/Components/App.razor`

정확한 개념/원리:

1. HTML 루트 문서와 Blazor 스크립트 로딩을 담당합니다.

쉬운 비유:

- 앱의 대문/무대 바닥입니다.

교사 발화 예시:

- "여기서 실제 라우팅 컴포넌트를 렌더링합니다."

타이핑 포인트:

1. `<Routes @rendermode="RenderMode.InteractiveServer" />`.

학생 확인 질문:

- "blazor.web.js가 빠지면 어떤 현상이 날까요?"

### 파일: `src/Seoul.It.Blackjack.Frontend/Components/Routes.razor`

정확한 개념/원리:

1. 라우터 설정으로 URL과 페이지를 연결합니다.

쉬운 비유:

- 교실 번호판과 교실 연결도입니다.

교사 발화 예시:

- "URL이 들어오면 어떤 컴포넌트를 보여줄지 결정합니다."

타이핑 포인트:

1. `RouteView`, `DefaultLayout`, `NotFound` 블록.

학생 확인 질문:

- "NotFound 페이지는 왜 필요할까요?"

### 파일: `src/Seoul.It.Blackjack.Frontend/Components/Layout/MainLayout.razor`

정확한 개념/원리:

1. 공통 레이아웃 래퍼입니다.

쉬운 비유:

- 모든 페이지를 담는 동일한 액자입니다.

교사 발화 예시:

- "공통 여백/폭은 레이아웃에서 잡습니다."

타이핑 포인트:

1. `@Body` 위치 의미 설명.

학생 확인 질문:

- "레이아웃 분리가 없으면 스타일 중복이 어떻게 늘어날까요?"

### 파일: `src/Seoul.It.Blackjack.Frontend/Pages/Entry.razor`

정확한 개념/원리:

1. 사용자 입력 페이지(`/`)입니다.
2. 이름 유효성 확인 후 `/table`로 이동합니다.

쉬운 비유:

- 게임 시작 전 입장 카드 작성 창구입니다.

교사 발화 예시:

- "이름이 없으면 다음 단계로 넘어가지 않게 막아야 합니다."

타이핑 포인트:

1. `@bind`로 입력값 바인딩.
2. `CanProceed` 계산 속성.
3. `GoNextAsync`에서 Trim 후 상태 저장 + 이동.

학생 확인 질문:

- "왜 Trim을 하고 저장할까요?"

### 파일: `src/Seoul.It.Blackjack.Frontend/Pages/Entry.razor.css`

정확한 개념/원리:

1. Entry 컴포넌트 전용 스타일입니다.

쉬운 비유:

- 입장 창구 전용 안내판 디자인입니다.

교사 발화 예시:

- "컴포넌트별 CSS는 스타일 충돌을 줄여줍니다."

타이핑 포인트:

1. `.entry-panel`, `.field`, `.validation` 스타일 확인.

학생 확인 질문:

- "글로벌 CSS와 컴포넌트 CSS를 왜 나눌까요?"

### 파일: `src/Seoul.It.Blackjack.Frontend/Pages/Table.razor`

정확한 개념/원리:

1. 게임 메인 화면(`/table`)입니다.
2. 첫 렌더에서 자동 Connect + Join 수행.
3. 상태 이벤트 수신 시 UI 갱신.

쉬운 비유:

- 경기장 전광판이 실시간 중계 데이터를 받아 갱신되는 구조입니다.

교사 발화 예시:

- "Entry 정보가 없으면 Table 접근을 막고 홈으로 돌려보냅니다."

타이핑 포인트:

1. `@implements IAsyncDisposable`.
2. `OnInitialized`에서 이벤트 구독.
3. `OnAfterRenderAsync(firstRender)` 자동 접속/입장.
4. `ExecuteAsync` 공통 에러 처리.
5. 카드 이미지: `@card.ToAssetPath()`.

학생 확인 질문:

- "왜 자동 Join 로직을 firstRender 한 번만 실행해야 할까요?"

### 파일: `src/Seoul.It.Blackjack.Frontend/Pages/Table.razor.css`

정확한 개념/원리:

1. Table 화면 전용 스타일입니다.

쉬운 비유:

- 경기장 화면 배치도입니다.

교사 발화 예시:

- "행동 버튼/요약/오류/카드 그리드를 분리해 읽기 쉽게 만듭니다."

타이핑 포인트:

1. `.action-row`, `.summary`, `.players`, `.cards img` 확인.

학생 확인 질문:

- "카드 이미지 크기를 고정하면 UX에 어떤 장점이 있나요?"

### 파일: `src/Seoul.It.Blackjack.Frontend/wwwroot/css/app.css`

정확한 개념/원리:

1. 앱 전체 공통 스타일입니다.

쉬운 비유:

- 학교 공통 교복 규정과 비슷한 기본 디자인 규칙입니다.

교사 발화 예시:

- "전체 배경/폰트/레이아웃 폭은 전역 CSS에서 처리합니다."

타이핑 포인트:

1. `body`, `.layout-shell` 기본 스타일 확인.

학생 확인 질문:

- "전역 스타일을 너무 많이 쓰면 어떤 부작용이 있을까요?"

### 파일 묶음: `src/Seoul.It.Blackjack.Frontend/wwwroot/cards/*.svg` (52장)

정확한 개념/원리:

1. 카드 렌더링에 필요한 정적 에셋 파일입니다.
2. 파일명 규칙은 반드시 `{suit}_{rank}.svg`입니다.

쉬운 비유:

- 카드 그림이 담긴 도서관 이미지 폴더입니다.

교사 발화 예시:

- "코드는 맞아도 파일명이 규칙과 다르면 이미지가 깨집니다."

타이핑 포인트:

1. `clubs_ace.svg` 형태 확인.
2. 52장 누락 여부 확인.

학생 확인 질문:

- "파일명 규칙이 어긋나면 어떤 카드만 안 보일 수 있을까요?"

---

## 4.7 Frontend.Tests 프로젝트

### 파일: `src/Seoul.It.Blackjack.Frontend.Tests/Seoul.It.Blackjack.Frontend.Tests.csproj`

정확한 개념/원리:

1. 프론트 테스트 프로젝트 설정 파일입니다.
2. bUnit + MSTest + TestHost 조합을 사용합니다.

쉬운 비유:

- UI 자동 채점 도구 상자 구성표입니다.

교사 발화 예시:

- "페이지 렌더 테스트와 컴포넌트 단위 테스트를 함께 지원합니다."

타이핑 포인트:

1. `bunit`, `Microsoft.AspNetCore.Mvc.Testing`, `MSTest` 패키지 확인.
2. Frontend/Core 참조 확인.

학생 확인 질문:

- "왜 Frontend.Tests가 Frontend 프로젝트를 참조해야 할까요?"

### 파일: `src/Seoul.It.Blackjack.Frontend.Tests/MSTestSettings.cs`

정확한 개념/원리:

1. 테스트 병렬 실행 설정입니다.

쉬운 비유:

- 시험 채점 시 여러 조를 동시에 채점하는 규칙입니다.

교사 발화 예시:

- "병렬 실행으로 테스트 시간을 단축합니다."

타이핑 포인트:

1. `[assembly: Parallelize(Scope = ExecutionScope.MethodLevel)]`.

학생 확인 질문:

- "병렬 실행 시 공유 상태 테스트에서 어떤 주의가 필요할까요?"

### 파일: `src/Seoul.It.Blackjack.Frontend.Tests/TestHostFactory.cs`

정확한 개념/원리:

1. 프론트 앱 인메모리 테스트 서버 생성기입니다.

쉬운 비유:

- 실제 웹서버 대신 교실에서 돌리는 미니 서버입니다.

교사 발화 예시:

- "라우팅/정적파일 테스트를 빠르게 반복할 수 있습니다."

타이핑 포인트:

1. `WebApplicationFactory<Program>` 상속 확인.

학생 확인 질문:

- "이 클래스가 없으면 페이지 GET 테스트를 어떻게 해야 할까요?"

### 파일: `src/Seoul.It.Blackjack.Frontend.Tests/PageRenderingTests.cs`

정확한 개념/원리:

1. 기본 페이지 렌더링(200 응답)을 검증합니다.

쉬운 비유:

- 건물 입구 문이 열리는지 확인하는 가장 기본 점검입니다.

교사 발화 예시:

- "기능 테스트 전에 페이지가 열리는지부터 확인합니다."

타이핑 포인트:

1. `/`, `/table` GET 요청.
2. `HttpStatusCode.OK`, `<html` 포함 검사.

학생 확인 질문:

- "페이지가 500이면 기능 테스트를 진행해도 될까요?"

### 파일: `src/Seoul.It.Blackjack.Frontend.Tests/StaticCardAssetsTests.cs`

정확한 개념/원리:

1. 카드 이미지 정적 파일 제공 여부를 검증합니다.

쉬운 비유:

- 교재 그림 파일이 실제로 배포 폴더에 있는지 점검하는 과정입니다.

교사 발화 예시:

- "카드가 안 보이는 버그를 조기에 잡는 테스트입니다."

타이핑 포인트:

1. `/cards/clubs_ace.svg` GET.
2. ContentType에 `svg` 포함 여부 확인.

학생 확인 질문:

- "코드가 맞아도 에셋 테스트가 실패하는 경우는 어떤 때일까요?"

### 파일: `src/Seoul.It.Blackjack.Frontend.Tests/CardExtensionsTests.cs`

정확한 개념/원리:

1. 카드 경로 변환 규칙 단위 테스트입니다.

쉬운 비유:

- 번역기가 정확한 문장을 출력하는지 확인하는 테스트입니다.

교사 발화 예시:

- "작은 확장 메서드도 테스트하면 파일명 규칙 회귀를 막을 수 있습니다."

타이핑 포인트:

1. `new Card(Suit.Spades, Rank.Ace)` -> `cards/spades_ace.svg` 검증.

학생 확인 질문:

- "이 테스트가 깨지면 UI에서 어떤 증상이 나타날까요?"

### 파일: `src/Seoul.It.Blackjack.Frontend.Tests/FrontendBlackjackOptionsTests.cs`

정확한 개념/원리:

1. 옵션 기본값/섹션명이 기대대로인지 검증합니다.

쉬운 비유:

- 기본 설정 공장값 점검입니다.

교사 발화 예시:

- "옵션 기본값이 바뀌면 연결 실패가 나올 수 있습니다."

타이핑 포인트:

1. `DefaultSectionName`, `DefaultHubUrl`, 인스턴스 기본값 비교.

학생 확인 질문:

- "기본값 테스트를 넣지 않으면 어떤 실수가 숨어들까요?"

### 파일: `src/Seoul.It.Blackjack.Frontend.Tests/FakeFrontendGameSession.cs`

정확한 개념/원리:

1. UI 컴포넌트 테스트용 가짜 세션 구현입니다.

쉬운 비유:

- 실제 네트워크 대신 모의 신호를 보내는 연습 리모컨입니다.

교사 발화 예시:

- "Fake 객체 덕분에 네트워크 없이도 UI 동작을 검증할 수 있습니다."

타이핑 포인트:

1. 각 명령 CallCount 증가.
2. `RaiseState`, `RaiseError`로 이벤트 수동 트리거.

학생 확인 질문:

- "Fake를 쓰면 테스트가 왜 더 빠르고 안정적일까요?"

### 파일: `src/Seoul.It.Blackjack.Frontend.Tests/EntryPageBunitTests.cs`

정확한 개념/원리:

1. Entry 페이지의 입력/버튼/이동 로직 검증입니다.

쉬운 비유:

- 입장 폼 작성 검사표입니다.

교사 발화 예시:

- "이름이 비면 다음 버튼이 막혀야 합니다."

타이핑 포인트:

1. `BunitContext` 생성.
2. `context.Render<Entry>()`.
3. `Change`, `Click` 후 상태/URI 검증.

학생 확인 질문:

- "이 테스트가 통과하면 사용자 입력 UX의 어떤 부분을 신뢰할 수 있나요?"

### 파일: `src/Seoul.It.Blackjack.Frontend.Tests/TablePageBunitTests.cs`

정확한 개념/원리:

1. Table 페이지 자동 접속/입장/렌더/버튼/오류 표시를 검증합니다.

쉬운 비유:

- 경기장 전광판 작동 점검표입니다.

교사 발화 예시:

- "Table은 상태가 많은 페이지라 테스트로 안전장치를 많이 둡니다."

타이핑 포인트:

1. EntryState 없음 -> `/` 리다이렉트.
2. 첫 렌더 자동 Connect/Join 1회.
3. `RaiseState` 후 카드 경로/상태 렌더 확인.
4. 버튼 클릭 시 세션 메서드 호출 횟수 검증.

학생 확인 질문:

- "자동 Join이 두 번 실행되면 어떤 버그가 발생할까요?"

---

## 5. 수업용 실행/검증 시나리오

1. 백엔드 실행 후 Swagger 페이지 열림 확인.
2. 프론트 실행 후 `/` Entry 화면 확인.
3. 이름 입력 후 `/table` 이동.
4. 딜러 1명 + 일반 플레이어 1명으로 시작.
5. StartRound -> Hit/Stand -> 결과 확인.
6. 딜러 퇴장 -> GAME_TERMINATED 브로드캐스트 확인.

권장 명령:

```bash
dotnet build src/Seoul.It.Blackjack.sln -m:1
dotnet test src/Seoul.It.Blackjack.Backend.Tests/Seoul.It.Blackjack.Backend.Tests.csproj -m:1
dotnet test src/Seoul.It.Blackjack.Frontend.Tests/Seoul.It.Blackjack.Frontend.Tests.csproj -m:1
```

---

## 6. 파일 커버리지 체크리스트 (누락 방지)

아래 체크리스트는 이 문서에서 설명 대상으로 다룬 코드 파일 목록입니다.

- [x] `src/Directory.Build.props`
- [x] `src/Seoul.It.Blackjack.Backend.Tests/ClientDiIntegrationTests.cs`
- [x] `src/Seoul.It.Blackjack.Backend.Tests/ConcurrencyTests.cs`
- [x] `src/Seoul.It.Blackjack.Backend.Tests/DealerTerminationTests.cs`
- [x] `src/Seoul.It.Blackjack.Backend.Tests/GameRuleOptionsDefaultTests.cs`
- [x] `src/Seoul.It.Blackjack.Backend.Tests/JoinAndRoleTests.cs`
- [x] `src/Seoul.It.Blackjack.Backend.Tests/RoundCompletionTests.cs`
- [x] `src/Seoul.It.Blackjack.Backend.Tests/Seoul.It.Blackjack.Backend.Tests.csproj`
- [x] `src/Seoul.It.Blackjack.Backend.Tests/SignalRTestClient.cs`
- [x] `src/Seoul.It.Blackjack.Backend.Tests/TestHostFactory.cs`
- [x] `src/Seoul.It.Blackjack.Backend.Tests/TestWaiter.cs`
- [x] `src/Seoul.It.Blackjack.Backend.Tests/TurnRuleTests.cs`
- [x] `src/Seoul.It.Blackjack.Backend/Extensions/ServiceCollectionExtensions.cs`
- [x] `src/Seoul.It.Blackjack.Backend/Hubs/GameSessionHub.cs`
- [x] `src/Seoul.It.Blackjack.Backend/Models/Deck.cs`
- [x] `src/Seoul.It.Blackjack.Backend/Models/Shoe.cs`
- [x] `src/Seoul.It.Blackjack.Backend/Options/DealerOptions.cs`
- [x] `src/Seoul.It.Blackjack.Backend/Options/GameRuleOptions.cs`
- [x] `src/Seoul.It.Blackjack.Backend/Program.cs`
- [x] `src/Seoul.It.Blackjack.Backend/Properties/launchSettings.json`
- [x] `src/Seoul.It.Blackjack.Backend/Seoul.It.Blackjack.Backend.csproj`
- [x] `src/Seoul.It.Blackjack.Backend/Seoul.It.Blackjack.Backend.http`
- [x] `src/Seoul.It.Blackjack.Backend/Services/Commands/GameCommand.cs`
- [x] `src/Seoul.It.Blackjack.Backend/Services/Commands/GameCommandType.cs`
- [x] `src/Seoul.It.Blackjack.Backend/Services/Commands/GameNotice.cs`
- [x] `src/Seoul.It.Blackjack.Backend/Services/Commands/GameOperationResult.cs`
- [x] `src/Seoul.It.Blackjack.Backend/Services/ConnectionRegistry.cs`
- [x] `src/Seoul.It.Blackjack.Backend/Services/Exceptions/GameAuthorizationException.cs`
- [x] `src/Seoul.It.Blackjack.Backend/Services/Exceptions/GameRoomException.cs`
- [x] `src/Seoul.It.Blackjack.Backend/Services/Exceptions/GameRuleException.cs`
- [x] `src/Seoul.It.Blackjack.Backend/Services/Exceptions/GameValidationException.cs`
- [x] `src/Seoul.It.Blackjack.Backend/Services/GameRoomService.cs`
- [x] `src/Seoul.It.Blackjack.Backend/Services/IGameRoomService.cs`
- [x] `src/Seoul.It.Blackjack.Backend/Services/Infrastructure/ChannelGameCommandProcessor.cs`
- [x] `src/Seoul.It.Blackjack.Backend/Services/Infrastructure/IGameCommandProcessor.cs`
- [x] `src/Seoul.It.Blackjack.Backend/Services/Round/IRoundEngine.cs`
- [x] `src/Seoul.It.Blackjack.Backend/Services/Round/RoundEngine.cs`
- [x] `src/Seoul.It.Blackjack.Backend/Services/Round/RoundResolution.cs`
- [x] `src/Seoul.It.Blackjack.Backend/Services/Rules/GameRuleValidator.cs`
- [x] `src/Seoul.It.Blackjack.Backend/Services/Rules/IGameRuleValidator.cs`
- [x] `src/Seoul.It.Blackjack.Backend/Services/State/GameStateSnapshotFactory.cs`
- [x] `src/Seoul.It.Blackjack.Backend/Services/State/IGameStateSnapshotFactory.cs`
- [x] `src/Seoul.It.Blackjack.Backend/appsettings.json`
- [x] `src/Seoul.It.Blackjack.Client/BlackjackClient.cs`
- [x] `src/Seoul.It.Blackjack.Client/Extensions/ServiceCollectionExtensions.cs`
- [x] `src/Seoul.It.Blackjack.Client/Options/BlackjackClientOptions.cs`
- [x] `src/Seoul.It.Blackjack.Client/Seoul.It.Blackjack.Client.csproj`
- [x] `src/Seoul.It.Blackjack.Core/Contracts/GamePhase.cs`
- [x] `src/Seoul.It.Blackjack.Core/Contracts/GameState.cs`
- [x] `src/Seoul.It.Blackjack.Core/Contracts/IBlackjackClient.cs`
- [x] `src/Seoul.It.Blackjack.Core/Contracts/IBlackjackServer.cs`
- [x] `src/Seoul.It.Blackjack.Core/Contracts/PlayerState.cs`
- [x] `src/Seoul.It.Blackjack.Core/Contracts/PlayerTurnState.cs`
- [x] `src/Seoul.It.Blackjack.Core/Contracts/RoundOutcome.cs`
- [x] `src/Seoul.It.Blackjack.Core/Domain/Card.cs`
- [x] `src/Seoul.It.Blackjack.Core/Domain/Rank.cs`
- [x] `src/Seoul.It.Blackjack.Core/Domain/Suit.cs`
- [x] `src/Seoul.It.Blackjack.Core/Seoul.It.Blackjack.Core.csproj`
- [x] `src/Seoul.It.Blackjack.Frontend.Tests/CardExtensionsTests.cs`
- [x] `src/Seoul.It.Blackjack.Frontend.Tests/EntryPageBunitTests.cs`
- [x] `src/Seoul.It.Blackjack.Frontend.Tests/FakeFrontendGameSession.cs`
- [x] `src/Seoul.It.Blackjack.Frontend.Tests/FrontendBlackjackOptionsTests.cs`
- [x] `src/Seoul.It.Blackjack.Frontend.Tests/MSTestSettings.cs`
- [x] `src/Seoul.It.Blackjack.Frontend.Tests/PageRenderingTests.cs`
- [x] `src/Seoul.It.Blackjack.Frontend.Tests/Seoul.It.Blackjack.Frontend.Tests.csproj`
- [x] `src/Seoul.It.Blackjack.Frontend.Tests/StaticCardAssetsTests.cs`
- [x] `src/Seoul.It.Blackjack.Frontend.Tests/TablePageBunitTests.cs`
- [x] `src/Seoul.It.Blackjack.Frontend.Tests/TestHostFactory.cs`
- [x] `src/Seoul.It.Blackjack.Frontend/Components/App.razor`
- [x] `src/Seoul.It.Blackjack.Frontend/Components/Layout/MainLayout.razor`
- [x] `src/Seoul.It.Blackjack.Frontend/Components/Routes.razor`
- [x] `src/Seoul.It.Blackjack.Frontend/Extensions/CardExtensions.cs`
- [x] `src/Seoul.It.Blackjack.Frontend/Extensions/ServiceCollectionExtensions.cs`
- [x] `src/Seoul.It.Blackjack.Frontend/Options/FrontendBlackjackOptions.cs`
- [x] `src/Seoul.It.Blackjack.Frontend/Pages/Entry.razor`
- [x] `src/Seoul.It.Blackjack.Frontend/Pages/Entry.razor.css`
- [x] `src/Seoul.It.Blackjack.Frontend/Pages/Table.razor`
- [x] `src/Seoul.It.Blackjack.Frontend/Pages/Table.razor.css`
- [x] `src/Seoul.It.Blackjack.Frontend/Program.cs`
- [x] `src/Seoul.It.Blackjack.Frontend/Seoul.It.Blackjack.Frontend.csproj`
- [x] `src/Seoul.It.Blackjack.Frontend/Services/FrontendEntryState.cs`
- [x] `src/Seoul.It.Blackjack.Frontend/Services/FrontendGameSession.cs`
- [x] `src/Seoul.It.Blackjack.Frontend/Services/IFrontendGameSession.cs`
- [x] `src/Seoul.It.Blackjack.Frontend/_Imports.razor`
- [x] `src/Seoul.It.Blackjack.Frontend/appsettings.json`
- [x] `src/Seoul.It.Blackjack.Frontend/wwwroot/css/app.css`
- [x] `src/Seoul.It.Blackjack.sln`

### 6.1 카드 에셋 체크리스트 (정적 파일)

아래 파일들은 코드가 아닌 SVG 에셋이지만, 수업 품질을 위해 함께 점검합니다.

- [x] `src/Seoul.It.Blackjack.Frontend/wwwroot/cards/clubs_ace.svg`
- [x] `src/Seoul.It.Blackjack.Frontend/wwwroot/cards/clubs_eight.svg`
- [x] `src/Seoul.It.Blackjack.Frontend/wwwroot/cards/clubs_five.svg`
- [x] `src/Seoul.It.Blackjack.Frontend/wwwroot/cards/clubs_four.svg`
- [x] `src/Seoul.It.Blackjack.Frontend/wwwroot/cards/clubs_jack.svg`
- [x] `src/Seoul.It.Blackjack.Frontend/wwwroot/cards/clubs_king.svg`
- [x] `src/Seoul.It.Blackjack.Frontend/wwwroot/cards/clubs_nine.svg`
- [x] `src/Seoul.It.Blackjack.Frontend/wwwroot/cards/clubs_queen.svg`
- [x] `src/Seoul.It.Blackjack.Frontend/wwwroot/cards/clubs_seven.svg`
- [x] `src/Seoul.It.Blackjack.Frontend/wwwroot/cards/clubs_six.svg`
- [x] `src/Seoul.It.Blackjack.Frontend/wwwroot/cards/clubs_ten.svg`
- [x] `src/Seoul.It.Blackjack.Frontend/wwwroot/cards/clubs_three.svg`
- [x] `src/Seoul.It.Blackjack.Frontend/wwwroot/cards/clubs_two.svg`
- [x] `src/Seoul.It.Blackjack.Frontend/wwwroot/cards/diamonds_ace.svg`
- [x] `src/Seoul.It.Blackjack.Frontend/wwwroot/cards/diamonds_eight.svg`
- [x] `src/Seoul.It.Blackjack.Frontend/wwwroot/cards/diamonds_five.svg`
- [x] `src/Seoul.It.Blackjack.Frontend/wwwroot/cards/diamonds_four.svg`
- [x] `src/Seoul.It.Blackjack.Frontend/wwwroot/cards/diamonds_jack.svg`
- [x] `src/Seoul.It.Blackjack.Frontend/wwwroot/cards/diamonds_king.svg`
- [x] `src/Seoul.It.Blackjack.Frontend/wwwroot/cards/diamonds_nine.svg`
- [x] `src/Seoul.It.Blackjack.Frontend/wwwroot/cards/diamonds_queen.svg`
- [x] `src/Seoul.It.Blackjack.Frontend/wwwroot/cards/diamonds_seven.svg`
- [x] `src/Seoul.It.Blackjack.Frontend/wwwroot/cards/diamonds_six.svg`
- [x] `src/Seoul.It.Blackjack.Frontend/wwwroot/cards/diamonds_ten.svg`
- [x] `src/Seoul.It.Blackjack.Frontend/wwwroot/cards/diamonds_three.svg`
- [x] `src/Seoul.It.Blackjack.Frontend/wwwroot/cards/diamonds_two.svg`
- [x] `src/Seoul.It.Blackjack.Frontend/wwwroot/cards/hearts_ace.svg`
- [x] `src/Seoul.It.Blackjack.Frontend/wwwroot/cards/hearts_eight.svg`
- [x] `src/Seoul.It.Blackjack.Frontend/wwwroot/cards/hearts_five.svg`
- [x] `src/Seoul.It.Blackjack.Frontend/wwwroot/cards/hearts_four.svg`
- [x] `src/Seoul.It.Blackjack.Frontend/wwwroot/cards/hearts_jack.svg`
- [x] `src/Seoul.It.Blackjack.Frontend/wwwroot/cards/hearts_king.svg`
- [x] `src/Seoul.It.Blackjack.Frontend/wwwroot/cards/hearts_nine.svg`
- [x] `src/Seoul.It.Blackjack.Frontend/wwwroot/cards/hearts_queen.svg`
- [x] `src/Seoul.It.Blackjack.Frontend/wwwroot/cards/hearts_seven.svg`
- [x] `src/Seoul.It.Blackjack.Frontend/wwwroot/cards/hearts_six.svg`
- [x] `src/Seoul.It.Blackjack.Frontend/wwwroot/cards/hearts_ten.svg`
- [x] `src/Seoul.It.Blackjack.Frontend/wwwroot/cards/hearts_three.svg`
- [x] `src/Seoul.It.Blackjack.Frontend/wwwroot/cards/hearts_two.svg`
- [x] `src/Seoul.It.Blackjack.Frontend/wwwroot/cards/spades_ace.svg`
- [x] `src/Seoul.It.Blackjack.Frontend/wwwroot/cards/spades_eight.svg`
- [x] `src/Seoul.It.Blackjack.Frontend/wwwroot/cards/spades_five.svg`
- [x] `src/Seoul.It.Blackjack.Frontend/wwwroot/cards/spades_four.svg`
- [x] `src/Seoul.It.Blackjack.Frontend/wwwroot/cards/spades_jack.svg`
- [x] `src/Seoul.It.Blackjack.Frontend/wwwroot/cards/spades_king.svg`
- [x] `src/Seoul.It.Blackjack.Frontend/wwwroot/cards/spades_nine.svg`
- [x] `src/Seoul.It.Blackjack.Frontend/wwwroot/cards/spades_queen.svg`
- [x] `src/Seoul.It.Blackjack.Frontend/wwwroot/cards/spades_seven.svg`
- [x] `src/Seoul.It.Blackjack.Frontend/wwwroot/cards/spades_six.svg`
- [x] `src/Seoul.It.Blackjack.Frontend/wwwroot/cards/spades_ten.svg`
- [x] `src/Seoul.It.Blackjack.Frontend/wwwroot/cards/spades_three.svg`
- [x] `src/Seoul.It.Blackjack.Frontend/wwwroot/cards/spades_two.svg`
