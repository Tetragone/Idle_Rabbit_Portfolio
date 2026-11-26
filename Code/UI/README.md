# 아이콘 Key와 Virtualized List

## Key에 따른 책임

장비 ID는 테이블의 장비 종류를, Soul Key는 보유 객체를 식별합니다. Sprite Key는 표시할 리소스를 식별합니다. 이 세 종류의 Key를 동일한 것으로 취급하지 않습니다.

`UI_Equip.Awake`는 장비 테이블 Key로 아이콘을 만들고 `Icon_Equip.Init`은 해당 ID의 이미지·등급·레벨을 표시합니다. 클릭 시 부모가 선택 Key를 관리하고 상세 UI에 ID를 전달합니다. 데이터 변경 알림은 활성 화면의 표시를 갱신합니다.

`AtlasMgr.GetSprite(AtlasType, key)`는 Atlas 종류와 Sprite Key로 조회하고 캐시합니다. 원본 Atlas와 이미지는 없으며 공개본에서 Atlas가 미연결이면 null을 반환합니다. 누락된 Sprite 결과를 캐시하지 않는 원본 동작은 유지합니다.

이벤트 UI는 이벤트 ID, 퀘스트 ID, 상품 ID로 조회합니다. 재화와 수량은 `Icon_Currency`, `Icon_SimpleItem`을 사용합니다. 이벤트·장비·Soul의 목록·상세·필터 코드와 관련 팝업을 함께 읽을 수 있습니다.

## Virtualized List

`UI_VirtualList`는 고정 크기 셀의 세로 리스트·그리드입니다. 전체 데이터 수로 Content 높이를 계산하고 Viewport 주변 셀만 생성·재사용합니다. 가변 높이, 가로 스크롤, 여러 종류의 셀 프리팹은 지원하지 않습니다.

`Init`의 생성 콜백은 셀 인스턴스마다 한 번, 연결 콜백은 데이터 인덱스가 바뀌거나 갱신 요청이 있을 때 호출합니다. 연결 콜백은 이전 이미지·텍스트·선택 상태를 모두 덮어써야 합니다. 활성 프리팹도 데이터 연결 전에 OnEnable이 호출되지 않도록 비활성 부모 아래에서 생성합니다.

`SetItemCount`는 개수와 위치 초기화 여부를 설정하고 다음 LateUpdate에 최종 상태를 연결합니다. `RefreshItem`, `RefreshVisibleItems`는 표시 중인 셀만 갱신합니다. `CreatedCellCount`와 `VisibleCellCount`는 재사용 확인을 위한 값이며 실제 성능 측정 결과는 아닙니다.

## Soul 적용 사례

`UI_Soul.InitList`가 생성·연결 콜백을 등록합니다. `RefreshIcon`은 조건별 보유 Key를 목록으로 확정합니다. 선택·분해 대상은 셀 객체가 아닌 Key로 보관하고, 재사용되는 Icon_Soul에 상태를 다시 적용합니다. 상세보기 닫기 구독은 교체와 비활성화 시 해제합니다.

UI_Soul은 대량 아이콘을 넣기 위한 임시 적용 화면입니다. 완성된 게임 화면으로 소개하지 않습니다. 개별 Soul 이미지 조회는 원본부터 주석 처리되어 있으며 공개본에서 완성한 기능으로 주장하지 않습니다.

`Tests/Editor/VirtualListChecks`는 별도 Preview Scene에서 경계 개수·재사용·위치 보정·이벤트 해제를 검사하는 코드입니다. 실제 UI 화면과 Profiler 성능은 별도 측정 대상입니다.
