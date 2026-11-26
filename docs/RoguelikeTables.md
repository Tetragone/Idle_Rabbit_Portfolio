# 로그라이크 설정 컬럼

원본 운영 CSV 대신 컬럼 의미와 [가상 예시](RoguelikeExample.json)를 제공합니다. 예시의 sample_enemy·sample_boss는 가상의 ID이며 실제 에셋과 연결되지 않습니다. 전체 게임 데이터가 아니므로 예시만으로 초기화·실행할 수는 없습니다.

## 웨이브

공급 식별자는 `Tables/c_roguelike_wave`, TableType은 RoguelikeWave입니다. 바깥 Dictionary Key는 행 ID이며 각 행의 컬럼 값은 문자열입니다.

| 컬럼 | 의미 |
| --- | --- |
| stage, wave | 1부터 연속인 구성 단계·순서 |
| clear_type | KillAll 또는 Survive |
| enemy_type | Normal 또는 Boss |
| enemy_ids, enemy_counts | 세미콜론으로 구분한 ID·전체 생성 수량 |
| spawn_interval | 묶음 생성 간격, 양수 |
| repeat_spawn | o 또는 x, KillAll은 x |
| max_alive | 동시 생존 한도 |
| time_limit | 0 이상, Survive는 양수 |
| atk_multi, hp_multi | 실제 단계 기준 적 능력치에 적용하는 양수 배율 |

## 효과

공급 식별자는 `Tables/c_roguelike_effect`, TableType은 RoguelikeEffect입니다.

| 컬럼 | 의미 |
| --- | --- |
| stat | c_stat에 존재하는 능력치 ID |
| value | 양수 효과 값 |
| weight | 양수 추첨 가중치 |
| max_stack | 한 도전에서 같은 효과를 선택할 수 있는 최대 횟수 |
| icon | Upgrade Atlas의 Sprite Key, 공개본에는 원본 Sprite 없음 |

지원하는 효과는 atk_multi·hp_multi·atk_spd·cri·cridam_add입니다. 같은 증가율은 합산하고 치명타 확률은 최종 100으로 제한합니다. 이름·설명 형식은 Stat 테이블에서 조회합니다. 필요한 선택 횟수보다 전체 중첩 가능 횟수가 적으면 설정 오류입니다.

던전 연결에는 c_dungeon의 RogueDungeon 행(sid·need), 해당 Stage 행의 배경·BGM, Enemy 설정, Stat의 desc·desc_format 등도 필요합니다. 예시에 이들 원본 데이터와 리소스를 포함하지 않습니다.

Table.ReadSource를 Awake 전에 연결하면 식별자별 Dictionary를 공급할 수 있습니다. CSV 파서는 포함하지 않습니다. 가상 예시는 문서용이며 실제 Table 초기화나 Unity 실행을 검사한 데이터가 아닙니다.
