# 공개본에서 제외한 항목

| 대상 | 처리 이유 |
| --- | --- |
| CSVReader.cs | 외부 구현이며 재배포 조건을 확정하지 않고 출처 설명만 유지 |
| NewPlayerUnitBuilder, NewSpriteLayer, NewSpriteCollection | 작성자가 만든 코드가 아니므로 제외 |
| PixelFantasy 전체와 기타 Asset Store 폴더 | 외부 스크립트·스킨·텍스처·애니메이션 등 원본 에셋 제외 |
| Assets/Design, Atlas, 원본 이미지·폰트·SDF·오디오 | 외부 에셋의 복사·가공본이 섞여 있어 일괄 제외 |
| 적·플레이어 프리팹 | 외부 스킨과 컨트롤러 의존 |
| Scenes, Resources/Tables, Local | 원본 씬·운영 데이터·Localization 설정 제외 |
| AddressableAssetsData | 원본 콘텐츠 주소와 빌드 설정 제외 |
| ProjectSettings, Packages 원본 파일 | 원본 계정·클라우드 연결 설정과 전체 패키지 설정 제외 |
| EnemyLifecycleTests | 원본 전체 게임과 연결된 통합 관찰 코드·설정·로그 제외 |
| Library, Temp, obj, Logs, build, UserSettings | 생성 결과와 로컬 환경 정보 제외 |
| MemoryCaptures, ProfilerCaptures | 캡처 바이너리와 실행 데이터 제외 |
| .git, IDE 설정, 로컬 기록 | 기존 저장소 이력과 개인 환경 제외 |

UI 프리팹에서 위 리소스를 가리키던 참조는 원본 파일을 가져오는 대신 비웠습니다. 원본 경로 이름은 변경 내역 설명에만 남을 수 있으며 해당 파일의 내용은 포함하지 않습니다.

라이선스가 불명확한 코드를 공개된 웹 코드라는 이유만으로 포함하지 않습니다. 라이선스를 임의로 선택하거나 Git 저장소를 초기화하지 않았습니다.
