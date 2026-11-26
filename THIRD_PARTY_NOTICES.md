# 외부 코드와 리소스 처리

## CSV Reader

원본 프로젝트는 웹에 공개된 외부 CSV Reader 코드를 사용했습니다. 해당 코드는 이 자료에 포함하지 않습니다. 출처 안내는 재배포 허가나 본인 작성 주장과 구분합니다.

- 참고한 게시글: https://hungry2s.tistory.com/215
- 원본으로 추정되는 저장소: https://github.com/tikonen/blog/tree/master/csvreader

`Table`의 파서 호출은 공개본에서 공급 함수로 대체했습니다. 원본의 정규식이나 CSV 파싱 코드를 복사하지 않았습니다.

## 제외한 외부 구현

`NewPlayerUnitBuilder`, `NewSpriteLayer`, `NewSpriteCollection`, PixelFantasy의 스크립트·스킨·애니메이션·컨트롤러, 기타 Asset Store 원본은 포함하지 않습니다. `PlayerUnitSkinAdapter`는 작성자의 코드이며 외부 빌더 구현 대신 공개본의 작은 계약을 참조합니다.

## 이미지·폰트·프리팹

원본 스프라이트, Atlas, NEXON Kart Gothic 폰트와 생성된 SDF, 오디오, 외부 머티리얼·텍스처는 포함하지 않습니다. UI 프리팹의 해당 참조는 비웠습니다. 별도 폰트 바이너리도 배포하지 않습니다. 실제 Unity 사용 시 도입 환경에서 사용 권한이 있는 TMP 기본 폰트를 설정해야 합니다.

보존한 Unity 패키지 스크립트 GUID는 패키지 설치를 요구하는 참조이며 패키지 코드의 재배포가 아닙니다. Unity 엔진·패키지의 라이선스는 각 제공자의 조건을 따릅니다.

이 자료 자체에 재배포 라이선스를 새로 부여하지 않았습니다. 외부 에셋의 재배포 권한이 있는 자료라고 해석해서는 안 됩니다.
