# 작성 기여

아래 분류는 프로젝트 작성자가 설명한 작성 방식에 근거합니다. Git 커밋 작성자만으로 AI 사용 여부나 단독 작성 여부를 추정하지 않습니다.

| 구분 | 대상 | 설명 |
| --- | --- | --- |
| 직접 설계·구현 | AtlasMgr, Table, MyState | 작성자가 직접 설계하고 구현 |
| AI 정보를 참고한 직접 구현 | ObjectPool, PoolMgr, BaseUnitAnimation | AI에 질문한 정보를 참고하여 대부분 직접 구현 |
| AI 생성 기반 적용 | MySpriteUtility, NumberFormatter | 필요한 기능을 설명하고 생성된 코드를 일부 수정하거나 테스트하여 적용 |
| 직접 구현 후 AI 수정 | Singleton, SingletonAllTime | 직접 구현한 기반 코드에 AI의 부분 수정 반영 |
| 직접 구현 | PlayerUnitSkinAdapter | 외부 스킨 빌더와 플레이어 사이의 적용 단계 |
| 직접 구현 | 버프, 스탯 집계, 연속 입력 | Buff, PowerMgr·StatCalculator, ContinueButton |
| AI 생성 기반 구현 | 로그라이크 탐험 던전과 관련 변경 | 작성자가 기능·정책·수정 방향을 구체적으로 설명하고, AI 생성 결과를 확인·조정하며 반영 |

이벤트·퀘스트·Virtualized List와 최근 변경에는 Codex 도움을 받은 코드가 포함됩니다. 해당 기능 전체를 AI 없이 단독 작성한 코드로 소개하지 않습니다. 위 표 밖의 보조 파일에는 파일별 작성 비율을 임의로 부여하지 않았습니다.

표의 직접 작성 분류는 기존 기반 코드의 출발점을 설명합니다. StatCalculator 등 기존 파일에 추가된 로그라이크 처리까지 모두 직접 작성했다는 뜻은 아닙니다. 신규 RoguelikeStage·RoguelikeWave·WaveSpawner·선택 UI와 관련 기존 코드의 변경분은 AI 생성 기반으로 분류합니다. 게임 정책과 수정 요구를 제시한 기여와 실제 코드 생성 방식을 구분합니다.

공개 준비 과정의 `Table.ReadSource`, 메모리 SaveHelper, 사운드 알림, 스킨 빌더 계약, Atlas 미연결 처리, 프리팹 참조 정리, 검사 도구와 문서는 Codex가 공개용으로 작성·수정했습니다. 이는 원본 게임에서 사용한 구현과 구분해야 합니다. [변경 기록](SourceManifest.json)의 파일별 사유를 참고하세요.

외부 CSV Reader, NewPlayerUnitBuilder, NewSpriteLayer, NewSpriteCollection 및 외부 리소스는 본인의 구현으로 소개하지 않으며 제공하지 않습니다.
