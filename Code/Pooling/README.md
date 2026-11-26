# 적 상태 초기화와 오브젝트 풀

`ObjectPool<T>`는 Queue로 대기 객체를 보관하고 HashSet으로 중복 반환을 검사합니다. 처음 사용할 개수는 코루틴에서 나누어 생성합니다. `PoolMgr`는 문자열 Key에 타입·등록 상태·풀을 연결하고 Addressables 로딩을 담당합니다.

## 호출 순서

`Spawner → PoolMgr.Get → ObjectPool.Get → 활성화·OnObtain → Eid 지정·InitUnit → Born → 전투 → Dying의 처치 통보 → Die의 풀 반환 → OnDispose` 순서입니다.

`EnemyUnit.InitUnit`은 스탯·체력·미사일 Key·이동 속도·공격 타이머를 설정한 뒤 상태를 Born으로 강제 변경합니다. `MyState.ChangeState`는 예약 상태, 상태 시간, 첫 프레임, 일회성 시간 조건 기록을 초기화합니다. 재사용 전에 남은 예약 상태가 영향을 주는 문제를 읽을 때 핵심이 되는 부분입니다.

## 보장 범위

중복 반환 검사와 모든 종류의 잘못된 반환 검사는 다릅니다. 현재 구현은 다른 풀 소유 객체 반환, 모든 초기화 상태, 비동기 로드 수명까지 완전히 보장하는 범용 라이브러리가 아닙니다. EnemyUnit 초기화·획득·반환에는 NowBuff.Reset 호출이 없습니다. 공개 준비 과정에서 원본의 이 동작을 바꾸지 않았습니다.

HP·데미지 UI와 미사일·BattleMgr·테이블 연결은 보조 코드로 남겼습니다. 프리팹·애니메이션·Addressables 콘텐츠는 제공하지 않으므로 이 자료만으로 적이 생성되지는 않습니다. 원본 통합 생명주기 테스트의 코드와 관찰용 심볼은 제외했습니다.
