# 공개용 UI 프리팹

이벤트·장비·Soul 화면과 연결된 UI 프리팹을 선별했습니다. 루트 프리팹에서 참조하는 자체 UI 프리팹도 포함합니다. 원본 GameObject 계층, RectTransform 배치, 버튼과 자체 스크립트의 Inspector 연결을 가능한 한 유지합니다.

외부 스프라이트·폰트·머티리얼·애니메이션은 복사하지 않고 해당 참조를 `fileID: 0`으로 비웠습니다. 원본 이미지가 없는 Unity Image는 단색 사각형을 표시합니다. TMP 텍스트는 Unity에서 기본 폰트와 머티리얼을 설정해야 합니다. 폰트 파일은 포함하지 않습니다.

원본 참조에서 어떤 항목을 비웠는지는 [PrefabChanges.json](../docs/PrefabChanges.json)에 기록합니다. 임의의 GUID를 새 에셋으로 가장하지 않습니다. 자체 코드와 선별된 중첩 프리팹은 원본 GUID를 유지하고 Unity 패키지 스크립트 참조는 남깁니다.

## 주요 화면

| 화면 | 파일 |
| --- | --- |
| 이벤트 메인 | UI/InGame/UI_Event.prefab |
| 이벤트 페이지 | UI/InGame/Event/UI_EventHuntQuest.prefab |
| 이벤트 상점 | UI/InGame/UI_Shop.prefab |
| 장비 목록·상세 | UI/InGame/UI_Equip.prefab, UI/InGame/UI_EquipSub.prefab |
| Soul 목록 | UI/InGame/SubPanel/Panel_Soul.prefab |
| Soul 필터 | UI/InGame/Obj_SoulFilter.prefab |

적·플레이어 프리팹, 스킨, 애니메이션은 제공하지 않습니다. 원본에 별도 Soul 상세 프리팹이 없으면 그 화면의 코드만 포함하며 새 프리팹을 만들어 원본처럼 소개하지 않습니다.

## Unity에서 연결할 때

새 프로젝트에 Code와 이 폴더를 넣고 필요한 Unity 패키지를 설치한 뒤 TMP 기본 폰트를 설정합니다. 프리팹 안 텍스트의 폰트·머티리얼, Image의 모양·RaycastTarget, 제거된 Animator 사용 여부를 확인합니다. 원본 설정과 이미지는 자동으로 복원되지 않습니다.

부팅·테이블·관리자 초기화와 Addressables 등록은 별도로 필요합니다. 이 자료는 실행용 샘플이 아니며 프리팹 파일의 정적 참조 정리가 화면 검증을 의미하지 않습니다.
