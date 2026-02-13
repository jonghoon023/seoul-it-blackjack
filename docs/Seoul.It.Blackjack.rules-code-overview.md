# Seoul.It.Blackjack 규칙 코드 오버뷰

이 문서는 \"블랙잭 규칙이 코드에서 어떻게 구현되었는지\"를 빠르게 파악하기 위한 코드 오버뷰 문서입니다.
실제 게임 규칙 원문은 `docs/Seoul.It.Blackjack.rules.md`를 기준으로 봅니다.

## 1. 범위

1. 대상 프로젝트:
- `Seoul.It.Blackjack.Core`
- `Seoul.It.Blackjack.Backend`
- `Seoul.It.Blackjack.Client`
- `Seoul.It.Blackjack.Frontend`
2. 적용 대상:
- 게임 상태 모델
- 입장/시작/턴/행동/종료 규칙
- 브로드캐스트 정책
- 에러 코드

## 2. 핵심 용어

1. `GamePhase`
- `Idle`: 대기 상태
- `InRound`: 라운드 진행 상태

2. `PlayerTurnState`
- `Playing`: 아직 행동 가능
- `Standing`: 더 이상 행동하지 않음
- `Busted`: 21점 초과로 탈락

3. `RoundOutcome`
- `None`: 결과 미확정
- `Win`, `Lose`, `Tie`

4. `Dealer`
- 별도 타입이 아니라 `PlayerState.IsDealer == true`인 플레이어

## 3. 카드/점수 규칙

1. 카드 무늬: `Clubs`, `Diamonds`, `Hearts`, `Spades`
2. 카드 숫자: `Two`~`Ten`, `Jack`, `Queen`, `King`, `Ace`
3. 점수 규칙 (`Rank.ToValue()`):
- `Ace = 1`
- `Two..Nine = 숫자 그대로`
- `Ten/Jack/Queen/King = 10`
4. Ace의 11점 가변 규칙은 사용하지 않음

## 4. 게임 상태 기본 원칙

1. 서버는 단일 게임 룸으로 동작
2. 상태 스냅샷은 `GameState`로 전송
3. 라운드 중 참가자 상태는 `PlayerState` 목록으로 관리
4. 현재 턴은 `CurrentTurnPlayerId`로 관리

## 5. 입장(Join) 규칙

1. `InRound` 상태에서는 신규 Join 불가 (`GAME_IN_PROGRESS`)
2. 같은 연결의 재Join 불가 (`ALREADY_JOINED`)
3. 이름 규칙:
- `Trim()` 후 길이 `1~20`
- 공백/빈 문자열 불가 (`INVALID_NAME`)
- 중복 이름 허용
4. 딜러 판정:
- `dealerKey == DealerOptions.Key`이면 딜러 요청으로 간주
- 딜러는 1명만 허용 (`DEALER_ALREADY_EXISTS`)
- 키 불일치 시 일반 플레이어로 입장

## 6. 라운드 시작(StartRound) 규칙

1. 요청자는 반드시 Join 상태 (`NOT_JOINED`)
2. 현재 상태는 `Idle`이어야 함 (`GAME_IN_PROGRESS`)
3. 요청자는 반드시 딜러 (`NOT_DEALER`)
4. 최소 인원 규칙:
- 전체 플레이어 수가 `MinPlayersToStart` 이상
- 기본값은 2명 (`INSUFFICIENT_PLAYERS`)
5. 시작 시 처리:
- 새 `Shoe` 생성
- 모든 플레이어 라운드 필드 초기화 (`Cards`, `Score`, `TurnState`, `Outcome`)
- 플레이어마다 카드 2장 배분

## 7. 시작 직후 상태 계산

1. 각 플레이어 점수 재계산
2. 점수 21이면 즉시 `Standing`
3. 점수 21 초과면 즉시 `Busted`
4. 첫 턴은 "딜러가 아닌 플레이어 중 `Playing` 상태 첫 번째"로 설정
5. 일반 플레이어 중 `Playing`이 없으면 즉시 라운드 정산 단계로 이동

## 8. 턴/행동 공통 검증 규칙

`Hit`/`Stand` 요청은 아래를 모두 만족해야 함:

1. 요청자 Join 상태 (`NOT_JOINED`)
2. `GamePhase == InRound` (`GAME_NOT_INROUND`)
3. 요청자는 딜러가 아님 (`DEALER_IS_AUTO`)
4. 요청자가 현재 턴 플레이어 (`NOT_YOUR_TURN`)
5. 요청자의 `TurnState == Playing` (`ALREADY_DONE`)

## 9. Hit 규칙

1. 현재 플레이어에게 카드 1장 추가
2. 카드가 없으면 `SHOE_EMPTY` 처리로 라운드 종료
3. 점수 재계산 후 상태 변경:
- `> 21` -> `Busted`
- `== 21` -> `Standing`
- `< 21` -> `Playing` 유지
4. 행동 후:
- 플레이어가 여전히 `Playing`이면 턴 유지
- 아니면 다음 플레이어로 턴 이동
5. 일반 플레이어의 `Playing`이 모두 사라지면 딜러 자동 진행/정산

## 10. Stand 규칙

1. 현재 플레이어 `TurnState = Standing`
2. 다음 플레이어 턴으로 이동
3. 일반 플레이어 `Playing`이 없으면 딜러 자동 진행/정산

## 11. 딜러 자동 진행 규칙

1. 일반 플레이어 행동이 모두 끝난 뒤 시작
2. 딜러 점수가 `DealerStandScore` 미만이면 자동 Hit 반복
3. 기본 `DealerStandScore = 17`
4. 딜러 점수가 17 이상이면 `Standing`
5. 자동 진행 중 카드가 소진되면 `SHOE_EMPTY`로 종료

## 12. 결과 판정 규칙

일반 플레이어 기준:

1. 플레이어 `Busted` -> `Lose`
2. 딜러 `Busted`이고 플레이어 비버스트 -> `Win`
3. 양쪽 비버스트 점수 비교:
- 플레이어 > 딜러 -> `Win`
- 플레이어 < 딜러 -> `Lose`
- 플레이어 == 딜러 -> `Tie`

## 13. 라운드 종료 후 상태

1. `GamePhase = Idle`
2. `CurrentTurnPlayerId = ""`
3. 직전 라운드 결과는 Idle 상태에서 유지
4. 다음 `StartRound` 시 카드/점수/결과를 다시 초기화

## 14. 퇴장(Leave/Disconnect) 규칙

### 14.1 일반 플레이어 퇴장

1. 해당 플레이어만 제거
2. 현재 턴 플레이어가 나가면 다음 턴 재계산
3. `InRound` 중 일반 플레이어가 0명이면 라운드 종료 후 Idle
4. `InRound` 중 플레이 가능한 일반 플레이어가 없으면 즉시 정산

### 14.2 딜러 퇴장

1. 게임 즉시 종료
2. 플레이어 목록 전체 제거
3. 연결 매핑 초기화
4. 상태 초기화 (`Idle`, 턴/딜러 ID 비움)
5. 전원에게 `GAME_TERMINATED` 공지 1회
6. 전원에게 초기화 상태 `OnStateChanged` 1회

## 15. 카드 소진(Shoe Empty) 규칙

1. 카드 드로우 실패 시 `SHOE_EMPTY`
2. 라운드 즉시 종료
3. 상태를 `Idle`로 전환
4. 공지 + 상태를 전원 브로드캐스트

## 16. 브로드캐스트 정책

1. 정상 처리 상태:
- `Clients.All.OnStateChanged(state)`

2. 요청자 전용 오류:
- `Clients.Caller.OnError(code, message)`

3. 전원 공지 오류(`GameNotice`):
- `Clients.All.OnError(code, message)`

4. 딜러 종료 시 순서:
- `All.OnError("GAME_TERMINATED", ...)` 먼저
- `All.OnStateChanged(resetState)` 다음

## 17. 동시성 처리 규칙

1. 모든 명령은 채널 큐에 적재
2. 단일 소비 루프가 순차 처리
3. 동시 요청이어도 상태 변경은 직렬화 보장

## 18. 에러 코드 고정 목록

1. `GAME_IN_PROGRESS`
2. `NOT_JOINED`
3. `NOT_DEALER`
4. `DEALER_IS_AUTO`
5. `DEALER_ALREADY_EXISTS`
6. `INVALID_NAME`
7. `ALREADY_JOINED`
8. `GAME_TERMINATED`
9. `NOT_YOUR_TURN`
10. `GAME_NOT_INROUND`
11. `ALREADY_DONE`
12. `INSUFFICIENT_PLAYERS`
13. `SHOE_EMPTY`

## 19. 옵션 기본값

1. `DealerOptions.Key`: 딜러 인증 키
2. `GameRuleOptions.DeckCount = 4`
3. `GameRuleOptions.DealerStandScore = 17`
4. `GameRuleOptions.MinPlayersToStart = 2`
5. `GameRuleOptions.MinNameLength = 1`
6. `GameRuleOptions.MaxNameLength = 20`

## 20. 비적용(의도적으로 제외한) 규칙

1. 멀티룸
2. Ace 1/11 가변 계산
3. 스플릿
4. 더블다운
5. 보험
6. 딜러 수동 Hit/Stand

## 21. 수업 설명용 핵심 요약

1. "딜러 1명 + 플레이어 여러 명" 구조
2. "Start는 딜러만"
3. "Hit/Stand는 자기 턴일 때만"
4. "모든 일반 플레이어 종료 후 딜러 자동"
5. "딜러 퇴장 = 게임 전체 종료"
6. "동시 요청은 큐로 줄 세워 처리"
