# 의존성과 공개본 연결

## 자료의 성격

코드 열람용 선별본입니다. `Code`는 원본의 하위 폴더 구조를 유지합니다. 기능 사이의 실제 호출을 읽을 수 있도록 데이터·계산·팝업 보조 코드를 포함했지만 부팅 씬과 전체 설정은 포함하지 않습니다. 컴파일·실행 가능한 샘플이라고 보장하지 않습니다.

원본에서 사용한 주요 패키지는 uGUI 2.0.0(TMP 포함), Addressables 2.9.1, 2D Animation 14.0.3입니다. 보조 UI에 Input System 또는 Localization 컴포넌트가 있으면 해당 패키지도 필요합니다. 실제 보존한 패키지 스크립트 GUID는 [PackageReferences.json](PackageReferences.json)을 참고하세요. 패키지 소스와 DLL은 복사하지 않았습니다.

## 유지한 연결

- `PoolMgr`, `Preloader`, `PopupMgr`, `Map`의 Addressables API 호출은 유지합니다. 원본 콘텐츠와 주소 등록은 제공하지 않으므로 실제 로딩은 준비되어 있지 않습니다.
- 데이터 관리자, 보상 계산, 구매와 팝업의 호출 관계를 유지합니다. 운영 CSV 값은 제공하지 않습니다.
- `TimeMgr`의 한국 시간 기준과 변환을 유지합니다. 값은 `DateTime.Ticks / 10,000,000`이며 Unix timestamp가 아닙니다. `AppliedStartUtcTicks`도 이름과 달리 이 초 단위 값을 보관합니다.
- 프리팹에서 자체 스크립트와 선별된 중첩 프리팹의 GUID는 유지합니다. `.meta`는 연결용 GUID만 남겼습니다.

## 대체한 연결

| 파일 | 공개본 처리 | 제한 |
| --- | --- | --- |
| Data/Table.cs | `ReadSource`로 설정 공급 함수를 받음 | Awake 이전에 연결해야 하며 외부 CSV 파서는 없음 |
| Data/SaveHelper.cs | 타입별·경로별 메모리 JSON 저장 | 디스크 저장·재접속 유지·저장 복구를 제공하지 않음 |
| Core/SoundMgr.cs | 사운드 요청 이벤트 전달 | 실제 재생과 오디오 로딩을 제공하지 않음 |
| Core/AtlasMgr.cs | Atlas가 없으면 null 반환 | Unity Image의 기본 사각형 표시, 원본 아이콘 모양 없음 |
| Battle/Units/PlayerUnitSkinAdapter.cs | 직접 작성한 빌더 계약으로 호출 | 실제 빌더 구현 없음, 연결하지 않고 호출하면 예외 |
| Battle/Units/PlayerUnit.cs | 사용하지 않는 외부 Firearm 필드 제거 | 외부 스킨 빌더 구현은 여전히 제공하지 않음 |

테이블 공급 함수의 결과 형식은 `Dictionary<string, Dictionary<string, string>>`입니다. 바깥 Key는 행 ID, 안쪽 Key는 컬럼명입니다. `TableName`의 식별자를 받아 해당 설정을 반환합니다. 공개본에는 운영 값이나 모든 테이블을 채우는 샘플이 없습니다.

## 초기화 순서

`Table.ReadSource` 연결 → 테이블 준비 → QuestMgr·EventMgr 초기화 → 저장 데이터 적용 → 기간·회차 갱신 → UI 활성화 순서입니다. `LocalDataMgr.InitDatas`, `LoadQuestData`, `LoadEventData`에서 실제 연결을 읽을 수 있습니다. 원본 Title 씬은 포함하지 않습니다.

부팅, 인스턴스 생성, Inspector 연결, 테이블 공급, Addressables 준비 없이 임의의 UI 프리팹을 켜면 동작하지 않습니다. 공개본에서 원본 게임의 시작 절차까지 새로 구성하지 않았습니다.

## 로그라이크 추가 의존성

`RoguelikeStage`는 `BaseStage`, `IGoalStage`, `IStageRequire`를 사용하고 `WaveSpawner`는 기존 `IEnemySpawner`·`Preloader`·`PoolMgr`에 연결됩니다. 선택은 `GachaCalculator.GachaIdx`, 보상은 `DropCalculator`·`CurrencyReward`, 영구 기록과 퀘스트는 `PlayerDataMgr`·`QuestMgr`에 연결합니다.

`BattleMgr.IsCombatPaused`를 BaseUnit·PlayerUnit·BaseMissile이 사용합니다. 웨이브 회수 알림은 BaseMissile에서 구독·해제합니다. 선택 효과는 PlayerUnit이 기본 스탯에서 StatCalculator를 호출해 계산합니다. `IStageCombat.DieEnemy`는 처리 여부를 bool로 반환하며 기존 스테이지 구현도 같은 계약으로 갱신했습니다.

`UI_Game`은 전투 정보를 표시하고 팝업의 비동기 로딩 이후 스테이지·웨이브·활성 상태를 다시 검사합니다. `UI_Dungeon`은 진입, `UI_Menu`는 상태별 메뉴, `ActionPopup`은 결과 표시와 실패 비용을 처리합니다. 관련 보조 코드도 포함했지만 해당 화면을 실행할 씬은 제공하지 않습니다.

신규 설정의 컬럼과 가상 예시는 [로그라이크 테이블](RoguelikeTables.md)을 참고하세요. 기존 공개용 `Table.ReadSource`, 메모리 저장, 사운드 알림과 스킨 빌더 계약은 그대로 유지합니다. 원본 CSV Reader와 스킨 빌더 구현을 다시 가져오지 않았습니다.
