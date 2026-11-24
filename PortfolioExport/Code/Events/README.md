# 데이터 기반 이벤트와 퀘스트

기간·보상·획득 한도는 테이블에서 읽고, 제공할 기능의 구성은 코드에서 결정합니다. `EventMgr.Factories`에 등록된 ID가 구현 객체를 생성합니다. CSV 행만 추가하면 모든 종류의 이벤트가 자동 생성되는 구조는 아닙니다.

## 읽는 흐름

1. `EventSchedule`: 시작 전, 획득 기간, 교환 전용 기간, 종료의 경계.
2. `GameEvent<T>`와 `EventData`: 데이터 복사, 시작 시각을 기준으로 한 회차 초기화.
3. `EventInterfaces`: 재화·온라인 사냥·오프라인 사냥·상점의 선택 기능.
4. `EventMgr`: 테이블 팩토리, 외부 조회·구매·처치 알림 전달.
5. `CurrencyExchangeEvent`: 재화 잔액, 사냥 한도, 퀘스트, 구매 기록과 지급 거절 처리.
6. `EventHuntRule`, `EventShopRule`: 설정 검증과 지급·구매 제한 계산.
7. `../Quests/QuestMgr.cs`, `QuestModule.cs`: 공통·이벤트 퀘스트 등록, 행동 보고, 수령 처리.

적 처치는 `BattleMgr.DieEnemy → EventMgr.OnEnemyKilled`로 연결됩니다. 퀘스트 행동은 `QuestMgr.ReportAction`으로 전달합니다. 저장자는 `LocalDataMgr`에서 이벤트 ID별 읽기·쓰기 함수를 등록하고 저장 데이터 적용 후 기간을 갱신합니다.

## 표시와 시간 정책

`UI_Event`는 페이지를 최초 선택할 때 생성하고 이후 재사용합니다. `UI_EventPage`가 공통 정보를, `UI_EventHuntQuest`가 사냥·퀘스트를 표시합니다. `UI_Shop`과 `Icon_Shop`에서 이벤트 상품을 표시합니다. UI는 구체 이벤트 구현 대신 관리자 API를 사용합니다.

시간은 한국 시간 기준 초 단위입니다. 종료 시각은 포함하지 않습니다. 시작 시각이 새 회차의 기준이며 종료 시각이 바뀌었다고 동일 회차 데이터를 지우지는 않습니다. 오프라인 보상은 기간 교집합을 날짜별로 나누고 일일·전체 한도와 소수점 잔여량을 적용합니다. 일반 재화와 이벤트 재화가 여러 파일에 저장될 때의 원자적 거래까지 보장하는 구조는 아닙니다.

## 공개본 연결과 검사

테이블과 외부 CSV Reader는 포함하지 않습니다. `Table.ReadSource`가 설정을 공급하고 SaveHelper는 메모리만 사용합니다. 실제 저장을 검증하는 샘플이 아닙니다.

[검사 코드](../Tests/Editor)의 EventFoundationChecks, EventHuntChecks, EventCurrencyChecks, EventShopChecks, EventOfflineChecks, QuestChecks에서 경계·한도·복사·중복 정산 검사 코드를 읽을 수 있습니다. Unity 실행과 실제 화면·저장은 별도 검증 대상입니다.
