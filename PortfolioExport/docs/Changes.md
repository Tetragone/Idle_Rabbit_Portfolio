# 원본과 공개본의 코드 차이

원본 연결을 바꾼 부분을 기록합니다. 줄바꿈 정규화만 적용한 파일은 [전체 목록](FileList.md)에 있습니다.

| 코드 | 변경 이유 |
| --- | --- |
| [Code/Battle/BattleMgr.cs](../Code/Battle/BattleMgr.cs) | 원본 통합 테스트의 관찰 코드 제거 |
| [Code/Battle/Stage/IGoalStage.cs](../Code/Battle/Stage/IGoalStage.cs) | 코드 유지, 줄바꿈만 정규화; 영문 주석을 한국어로 변경 |
| [Code/Battle/Stage/IStage.cs](../Code/Battle/Stage/IStage.cs) | 코드 유지, 줄바꿈만 정규화; 영문 주석을 한국어로 변경 |
| [Code/Battle/Stage/MultiSpawner.cs](../Code/Battle/Stage/MultiSpawner.cs) | 원본 통합 테스트의 관찰 코드 제거 |
| [Code/Battle/Units/BaseUnit.cs](../Code/Battle/Units/BaseUnit.cs) | 원본 통합 테스트의 관찰 코드 제거 |
| [Code/Battle/Units/PlayerUnit.cs](../Code/Battle/Units/PlayerUnit.cs) | 사용하지 않는 외부 Firearm 타입과 using 제거 |
| [Code/Battle/Units/PlayerUnitSkinAdapter.cs](../Code/Battle/Units/PlayerUnitSkinAdapter.cs) | 공개용 수정: 외부 빌더 타입 대신 새 호출 계약 사용; 빌더 구현 미포함 |
| [Code/Calculator/StatCalculator.cs](../Code/Calculator/StatCalculator.cs) | 코드 유지, 줄바꿈만 정규화; 영문 주석을 한국어로 변경 |
| [Code/Common/ContinueButton.cs](../Code/Common/ContinueButton.cs) | 코드 유지, 줄바꿈만 정규화; 영문 주석을 한국어로 변경 |
| [Code/Common/MyState.cs](../Code/Common/MyState.cs) | 원본 통합 테스트의 관찰 코드 제거 |
| [Code/Core/AtlasMgr.cs](../Code/Core/AtlasMgr.cs) | 원본 Atlas 미포함 시 기본 이미지 표시를 위한 null 처리 추가 |
| [Code/Core/SoundMgr.cs](../Code/Core/SoundMgr.cs) | 공개용 대체: 오디오 로딩·재생을 요청 이벤트로 변경 |
| [Code/Data/SaveHelper.cs](../Code/Data/SaveHelper.cs) | 공개용 대체: 실제 파일 입출력을 제거하고 타입·Key별 메모리 JSON 저장으로 변경 |
| [Code/Data/Table.cs](../Code/Data/Table.cs) | 외부 CSVReader 호출을 명시적인 테이블 공급 함수로 대체 |
| [Code/Pooling/ObjectPool.cs](../Code/Pooling/ObjectPool.cs) | 원본 통합 테스트의 관찰 코드 제거 |
| [Code/Pooling/PoolMgr.cs](../Code/Pooling/PoolMgr.cs) | 원본 통합 테스트의 관찰 코드 제거 |

프리팹은 레이아웃과 자체 연결을 유지하면서 외부 리소스 참조를 비웠습니다. [세부 기록](PrefabChanges.json)을 참고하세요.
