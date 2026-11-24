# 스테이지 역할과 전환

`IStage`는 생명주기·정보·전투 역할을 묶습니다. `IGoalStage`는 결과·타이머·진행도를 제공하고 입장 조건은 `IStageRequire`로 분리합니다. `BaseStage`는 적 목록과 스포너 접근을 공통 처리합니다.

| 구현 | 주요 역할 |
| --- | --- |
| NormalStage | 지속 사냥과 드롭 |
| ChallengeStage | 처치 수·제한 시간에 따른 결과 |
| ChapterBossStage | 단일 보스 스폰과 결과 |
| GoldDungeonStage | 처치 목표·입장권 정보·보상 |
| MultiSpawner | 지역별 적 수와 재생성 시간 관리 |
| SingleSpwaner | 단일 적 생성과 관리 |

`SingleSpwaner`는 원본 클래스명입니다. 공개본에서 이름을 변경하지 않았습니다.

`BattleMgr.ChangeBattleType → ReStartGame → 기존 적 정리 → 타입별 구현 선택 → SetInfo → 목표·입장 조건 연결 → 맵·스포너 준비 대기 → 플레이어 초기화 → 전투 재개` 순으로 읽습니다.

스테이지별 처치·클리어 처리는 DropCalculator, PlayerDataMgr, QuestMgr에 연결됩니다. 플레이어·맵 준비와 로딩 UI도 관리자가 조율합니다. 역할 분리는 유지하지만 모든 처리를 인터페이스로 대체한 독립 실행 프레임워크는 아닙니다.

원본 테이블·맵·적 리소스와 로딩 씬은 포함하지 않습니다. 기존 로직의 잠재적 오류를 공개 준비 과정에서 함께 리팩터링하지 않았습니다.
