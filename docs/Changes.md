# 원본과 공개본의 코드 차이

원본 연결을 바꾼 부분을 기록합니다. 줄바꿈 정규화만 적용한 파일은 [전체 목록](FileList.md)에 있습니다.

| 코드 | 변경 이유 |
| --- | --- |
| [Code/Battle/BattleMgr.cs](../Code/Battle/BattleMgr.cs) | 로그라이크 추가·연결 변경 반영; 원본 통합 테스트 관찰 코드 제거; 개인 검토용 질문 주석만 제외, 실행 로직 유지 |
| [Code/Battle/Missile/BaseMissile.cs](../Code/Battle/Missile/BaseMissile.cs) | 로그라이크 추가·연결 변경 반영 |
| [Code/Battle/Stage/BaseStage.cs](../Code/Battle/Stage/BaseStage.cs) | 로그라이크 추가·연결 변경 반영 |
| [Code/Battle/Stage/ChallengeStage.cs](../Code/Battle/Stage/ChallengeStage.cs) | 로그라이크 추가·연결 변경 반영 |
| [Code/Battle/Stage/ChapterBossStage.cs](../Code/Battle/Stage/ChapterBossStage.cs) | 로그라이크 추가·연결 변경 반영 |
| [Code/Battle/Stage/GoldDungeonStage.cs](../Code/Battle/Stage/GoldDungeonStage.cs) | 로그라이크 추가·연결 변경 반영 |
| [Code/Battle/Stage/IGoalStage.cs](../Code/Battle/Stage/IGoalStage.cs) | 코드 유지, 줄바꿈만 정규화; 영문 주석을 한국어로 변경 |
| [Code/Battle/Stage/IStage.cs](../Code/Battle/Stage/IStage.cs) | 로그라이크 추가·연결 변경 반영; 영문 주석을 한국어로 변경 |
| [Code/Battle/Stage/MultiSpawner.cs](../Code/Battle/Stage/MultiSpawner.cs) | 원본 통합 테스트의 관찰 코드 제거 |
| [Code/Battle/Stage/NormalStage.cs](../Code/Battle/Stage/NormalStage.cs) | 로그라이크 추가·연결 변경 반영 |
| [Code/Battle/Stage/RoguelikeStage.cs](../Code/Battle/Stage/RoguelikeStage.cs) | 로그라이크와 필요한 자체 연결 코드 추가 |
| [Code/Battle/Stage/RoguelikeWave.cs](../Code/Battle/Stage/RoguelikeWave.cs) | 로그라이크와 필요한 자체 연결 코드 추가 |
| [Code/Battle/Stage/WaveSpawner.cs](../Code/Battle/Stage/WaveSpawner.cs) | 로그라이크와 필요한 자체 연결 코드 추가 |
| [Code/Battle/Units/BaseUnit.cs](../Code/Battle/Units/BaseUnit.cs) | 로그라이크 추가·연결 변경 반영; 원본 통합 테스트 관찰 코드 제거 |
| [Code/Battle/Units/PlayerUnit.cs](../Code/Battle/Units/PlayerUnit.cs) | 로그라이크 추가·연결 변경 반영; 미사용 외부 Firearm 필드와 using 제거 유지 |
| [Code/Battle/Units/PlayerUnitSkinAdapter.cs](../Code/Battle/Units/PlayerUnitSkinAdapter.cs) | 공개용 수정: 외부 빌더 타입 대신 새 호출 계약 사용; 빌더 구현 미포함 |
| [Code/Calculator/DropCalculator.cs](../Code/Calculator/DropCalculator.cs) | 로그라이크 추가·연결 변경 반영 |
| [Code/Calculator/StatCalculator.cs](../Code/Calculator/StatCalculator.cs) | 로그라이크 추가·연결 변경 반영; 영문 주석을 한국어로 변경 |
| [Code/Common/Constant.cs](../Code/Common/Constant.cs) | 로그라이크 추가·연결 변경 반영 |
| [Code/Common/ContinueButton.cs](../Code/Common/ContinueButton.cs) | 코드 유지, 줄바꿈만 정규화; 영문 주석을 한국어로 변경 |
| [Code/Common/Enums.cs](../Code/Common/Enums.cs) | 로그라이크 추가·연결 변경 반영 |
| [Code/Common/MyState.cs](../Code/Common/MyState.cs) | 원본 통합 테스트의 관찰 코드 제거 |
| [Code/Core/AtlasMgr.cs](../Code/Core/AtlasMgr.cs) | 원본 Atlas 미포함 시 기본 이미지 표시를 위한 null 처리 추가 |
| [Code/Core/SoundMgr.cs](../Code/Core/SoundMgr.cs) | 공개용 대체: 오디오 로딩·재생을 요청 이벤트로 변경 |
| [Code/Data/SaveHelper.cs](../Code/Data/SaveHelper.cs) | 공개용 대체: 실제 파일 입출력을 제거하고 타입·Key별 메모리 JSON 저장으로 변경 |
| [Code/Data/Table.cs](../Code/Data/Table.cs) | 로그라이크 추가·연결 변경 반영; 외부 CSV 파서 대신 ReadSource 공급 함수 유지 |
| [Code/Pooling/ObjectPool.cs](../Code/Pooling/ObjectPool.cs) | 원본 통합 테스트의 관찰 코드 제거 |
| [Code/Pooling/PoolMgr.cs](../Code/Pooling/PoolMgr.cs) | 원본 통합 테스트의 관찰 코드 제거 |
| [Code/UI/InGame/Battle/ActionPopup.cs](../Code/UI/InGame/Battle/ActionPopup.cs) | 로그라이크와 필요한 자체 연결 코드 추가 |
| [Code/UI/InGame/Icon/Icon_Dungeon.cs](../Code/UI/InGame/Icon/Icon_Dungeon.cs) | 로그라이크와 필요한 자체 연결 코드 추가 |
| [Code/UI/InGame/Icon/Icon_Mob.cs](../Code/UI/InGame/Icon/Icon_Mob.cs) | 로그라이크와 필요한 자체 연결 코드 추가 |
| [Code/UI/InGame/Icon/Icon_RoguelikeEffect.cs](../Code/UI/InGame/Icon/Icon_RoguelikeEffect.cs) | 로그라이크와 필요한 자체 연결 코드 추가 |
| [Code/UI/InGame/Icon/Icon_Stage.cs](../Code/UI/InGame/Icon/Icon_Stage.cs) | 로그라이크와 필요한 자체 연결 코드 추가 |
| [Code/UI/InGame/UI_Dungeon.cs](../Code/UI/InGame/UI_Dungeon.cs) | 로그라이크와 필요한 자체 연결 코드 추가 |
| [Code/UI/InGame/UI_Game.cs](../Code/UI/InGame/UI_Game.cs) | 로그라이크와 필요한 자체 연결 코드 추가 |
| [Code/UI/InGame/UI_LobbyQuest.cs](../Code/UI/InGame/UI_LobbyQuest.cs) | 로그라이크와 필요한 자체 연결 코드 추가 |
| [Code/UI/InGame/UI_Menu.cs](../Code/UI/InGame/UI_Menu.cs) | 로그라이크와 필요한 자체 연결 코드 추가 |
| [Code/UI/InGame/UI_Quest.cs](../Code/UI/InGame/UI_Quest.cs) | 로그라이크와 필요한 자체 연결 코드 추가 |
| [Code/UI/InGame/UI_Topbar.cs](../Code/UI/InGame/UI_Topbar.cs) | 로그라이크와 필요한 자체 연결 코드 추가 |
| [Code/UI/Popup/BasePopup.cs](../Code/UI/Popup/BasePopup.cs) | 로그라이크 추가·연결 변경 반영 |
| [Code/UI/Popup/Popup_RoguelikeSelect.cs](../Code/UI/Popup/Popup_RoguelikeSelect.cs) | 로그라이크와 필요한 자체 연결 코드 추가 |
| [Code/UI/Popup/Popup_StageSelect.cs](../Code/UI/Popup/Popup_StageSelect.cs) | 로그라이크와 필요한 자체 연결 코드 추가 |
| [Code/UI/Popup/Popup_WipeOut.cs](../Code/UI/Popup/Popup_WipeOut.cs) | 로그라이크와 필요한 자체 연결 코드 추가 |

프리팹은 레이아웃과 자체 연결을 유지하면서 외부 리소스 참조를 비웠습니다. [세부 기록](PrefabChanges.json)을 참고하세요.
