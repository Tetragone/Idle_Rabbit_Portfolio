# 추가 구현

## 버프

`Code/Battle/Buff.cs`는 버프·디버프 종류별 값과 남은 시간을 관리합니다. Update에서 만료된 항목을 모아 제거하고, 전투 계산은 IsContain과 GetValue로 조회합니다. 별도 오브젝트 생성보다 기존 유닛의 상태에 값을 보관하는 구조입니다.

## 스탯 집계

`Code/Core/PowerMgr.cs`는 장비·스킬·강화 데이터 변경 알림을 받아 StatCalculator로 다시 집계합니다. `StatCalculator.UpdateStatValue`는 가산과 배율을 구분하고 CalculatePlayer·CalculateBattlePower로 실제 전투 스탯과 표시용 전투력을 계산합니다. 데이터 변경과 UI 표시를 알림으로 연결하는 흐름을 읽을 수 있습니다.

## 연속 입력

`Code/Common/ContinueButton.cs`는 포인터 누름·해제·영역 이탈과 반복 호출을 처리합니다. 일정 호출 수 이후 반복 간격을 줄이고 종료 시 누적 처리를 적용하도록 onHold와 onClickEnd를 제공합니다. UI_EquipSub에서 누르는 동안 강화 횟수를 모으고 종료 시 반영하는 사용 사례를 볼 수 있습니다.

세 기능은 작성자가 직접 만든 코드로 설명한 대상입니다. 공개 준비 과정에서 기능 자체를 새로 구현하거나 기존 오류를 포괄적으로 수정하지 않았습니다.
