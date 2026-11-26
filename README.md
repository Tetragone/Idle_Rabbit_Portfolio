# Idle Rabbit — 핵심 코드 포트폴리오

Unity 방치형 게임에서 사용한 이벤트, UI, 오브젝트 풀, 스테이지 관리와 로그라이크 탐험 던전 코드를 선별한 자료입니다. 전체 게임이나 실행 가능한 Unity 프로젝트는 제공하지 않습니다. 원본 프로젝트의 Unity 버전은 `6000.4.0f1`입니다.

이 자료는 원본에서 사용한 구조와 호출 흐름을 읽기 위한 것입니다. 씬, 운영 테이블, 외부 에셋, Addressables 등록 설정은 포함하지 않습니다. `Code`와 `Prefabs`를 Unity에 넣는 것만으로 게임이 실행되지는 않습니다.

## 읽는 순서

| 기능 | 시작 파일 | 설명 |
| --- | --- | --- |
| 데이터 기반 이벤트 | [EventMgr](Code/Events/EventMgr.cs), [CurrencyExchangeEvent](Code/Events/CurrencyExchangeEvent.cs) | [이벤트와 퀘스트](Code/Events/README.md) |
| Virtualized List | [UI_VirtualList](Code/UI/UI_VirtualList.cs) | [셀 재사용과 Soul 적용](Code/UI/README.md) |
| 적 상태 초기화와 풀 | [EnemyUnit](Code/Battle/Units/EnemyUnit.cs), [ObjectPool](Code/Pooling/ObjectPool.cs) | [획득·초기화·반환](Code/Pooling/README.md) |
| 스테이지 | [BattleMgr](Code/Battle/BattleMgr.cs), [IStage](Code/Battle/Stage/IStage.cs) | [역할과 전환](Code/Battle/Stage/README.md) |
| 로그라이크 탐험 던전 | [RoguelikeStage](Code/Battle/Stage/RoguelikeStage.cs), [WaveSpawner](Code/Battle/Stage/WaveSpawner.cs) | [웨이브·효과 선택·정산](Code/Battle/ROGUELIKE_README.md) |
| Key 기반 아이콘 | [AtlasMgr](Code/Core/AtlasMgr.cs), [Icon_Equip](Code/UI/InGame/Icon/Icon_Equip.cs) | [장비·이벤트·Soul UI](Code/UI/README.md) |
| 플레이어 스킨 적용 단계 | [PlayerUnitSkinAdapter](Code/Battle/Units/PlayerUnitSkinAdapter.cs) | [외부 빌더와의 경계](Code/Battle/Units/README.md) |
| 버프·스탯·연속 입력 | [Buff](Code/Battle/Buff.cs), [PowerMgr](Code/Core/PowerMgr.cs), [ContinueButton](Code/Common/ContinueButton.cs) | [추가 구현](docs/Additional.md) |

각 기능에서 호출하는 데이터 관리자와 팝업 등 보조 코드도 선별했습니다. 보조 파일이 많아도 위 시작 파일부터 읽으면 기능별 흐름을 따라갈 수 있습니다. 뽑기·스킬 관련 일부 파일은 장비·이벤트 상점과 스탯 집계의 의존성으로 포함했으며 별도 대표 기능으로 소개하지 않습니다.

로그라이크의 신규 파일뿐 아니라 기존 전투·스탯·스테이지 계약의 변경도 함께 포함합니다. 던전 진입과 결과 화면, 공통 메뉴를 읽는 데 필요한 보조 코드도 연결했습니다. 새 콘텐츠의 작성 방식은 요구사항과 수정 방향을 구체적으로 제시하고 결과를 확인·조정하며 적용한 AI 생성 기반 구현입니다.

## 공개본의 변경 범위

- 외부 `CSVReader`를 제외하고 `Table.ReadSource`에 테이블 공급 함수를 연결하도록 변경했습니다.
- 실제 파일 저장은 메모리 안의 JSON 복사본으로 대체했습니다. 종료 후 데이터는 유지되지 않습니다.
- 사운드 재생은 호출 알림으로 대체하고 오디오 파일을 포함하지 않습니다.
- 외부 스킨 빌더는 계약만 남겼습니다. 실제 빌더와 적·플레이어 스킨은 제공하지 않습니다.
- 원본 통합 테스트의 관찰 코드를 제거했습니다. 핵심 상태 전이와 풀 동작은 유지합니다.
- 프리팹은 레이아웃을 유지하고 외부 이미지·폰트·머티리얼·애니메이션 참조를 비웠습니다. 이미지는 기본 사각형, 텍스트는 도입 환경의 TMP 기본 폰트를 사용하도록 준비해야 합니다.

상세 연결 조건은 [의존성과 대체 코드](docs/Dependencies.md), 프리팹 구성은 [프리팹 안내](Prefabs/README.md)를 참고하세요. [전체 파일 목록](docs/FileList.md), [코드 변경 이유](docs/Changes.md), [제외 목록](docs/Excluded.md), [원본 대응 경로와 해시](docs/SourceManifest.json)도 함께 제공합니다.

## 작성 기여와 이용 범위

[작성 기여](docs/Authorship.md)에 직접 작성, AI 참고, AI 생성 기반, AI 수정 반영을 구분했습니다. 공개본에서 새로 만든 연결 코드도 원본 구현과 구분합니다.

이 자료에는 MIT 등의 재배포 라이선스를 임의로 부여하지 않았습니다. 코드 열람을 위한 공개 자료이며, 제3자 패키지에는 해당 패키지의 조건이 별도로 적용됩니다. [외부 코드·리소스 처리](THIRD_PARTY_NOTICES.md)를 참고하세요.

## 검사 안내

`Code/Tests/Editor`에는 원본의 이벤트·퀘스트·리스트 검사 코드를 포함했습니다. 실행 환경을 구성한 뒤 Unity 메뉴에서 실행하는 코드이며, 코드가 포함되어 있다는 사실이 검사 통과를 의미하지 않습니다. 실제 씬과 Inspector 연결, 폰트, Addressables, 파일 저장을 입증하는 자료도 아닙니다.

`python Tools/check_export.py`는 공개 폴더의 파일 종류, 제외 파일, GUID 참조, 기록된 해시를 정적으로 점검합니다. Unity 컴파일이나 화면 동작 검사는 수행하지 않습니다.

이번 공개본 준비에서 수행한 검사와 남은 확인 사항은 [작업 보고](REVIEW.md)에 분리했습니다.
