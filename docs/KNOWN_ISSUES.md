# Known Issues

## IDX / PAK

- 현재 IDX 파서는 classic 28-byte 후보 구조를 우선 지원한다.
- 보호/암호화 IDX는 아직 지원하지 않는다.
- `_EXTB$` 확장 IDX는 아직 지원하지 않는다.
- IDX 포맷이 다른 클라이언트에서는 목록/offset/size가 정확하지 않을 수 있다.

## Sprite / SPR

- list.spr 파서는 텍스트/CSV/탭/공백 구분 후보를 관대한 방식으로 읽는다.
- list.spr 실제 포맷이 다른 경우 Sprite ID / 이름 / 그룹 / 액션 매핑이 부정확할 수 있다.
- `.spr` record 역매핑은 `record.Index`, 숫자 파일명, 파일명 이름 match 후보 기반이다.
- SPR header analysis는 실제 구조 확정이 아니라 frame count / direction / palette / frame bytes 후보 추정이다.
- SPR Raw Preview는 실제 SPR 렌더링이 아니라 후보 payload 회색조 시각화이다.
- Raw Preview의 width / offset / frame index / zoom 수동 조정은 디코더 이식 전 검증 보조 도구이다.
- SPR 실제 프레임 디코더, 팔레트 적용, 방향별 렌더링은 아직 구현되지 않았다.
- Raw Preview PNG 저장은 현재 화면에 표시된 후보 시각화 이미지만 저장한다.

## Tile Resource

- PNG/BMP/JPG/JPEG/GIF 같은 일반 이미지 포맷만 직접 변환을 시도한다.
- TIL/IMG는 Raw Byte 진단 이미지 변환을 지원하지만, 이는 실제 타일 이미지 렌더링이 아니다.
- Raw Byte 진단 이미지는 데이터 패턴 확인용 회색조 시각화이다.
- Raw Byte 진단 이미지는 최대 1MB 리소스까지만 변환을 시도한다.
- 변환 결과 이미지 저장/복사는 현재 Tile 패널 Image 탭에 표시된 이미지에 한정된다.
- SPR/TBT는 변환기 pipeline과 진단 기능만 준비되어 있다.
- TIL/IMG/SPR 실제 포맷 기반 이미지 변환/렌더링은 아직 구현되지 않았다.
- Tile ID와 IDX 레코드 매핑은 현재 `record.Index` 또는 파일명 숫자 후보 기반이다.
- Tile ID 매핑 규칙이 다른 클라이언트에서는 검색 결과가 정확하지 않을 수 있다.
- HEX 진단은 포맷 분석 보조 기능이며 실제 구조 해석을 보장하지 않는다.

## S32 Map

- 현재 S32 파서는 파일 앞부분을 Layer1 후보로 읽는 샘플 파서다.
- 모든 S32 변형 포맷을 보장하지 않는다.
- Render 탭은 Tile ID 기반 색상 그리드이며 실제 Tile 이미지 렌더링은 아니다.
- Tile.idx/Tile.pak는 상태/검색/진단 연결까지만 되어 있고 실제 타일 이미지 캐시는 일반 이미지 포맷 및 Raw Byte 진단 이미지에 한정된다.
- Layer2/3/4/5/7의 실제 의미별 렌더링은 추후 구현 대상이다.

## UI / Build

- Windows Forms 기반이므로 Windows 환경 실행을 기준으로 한다.
- TargetFramework는 `net10.0-windows`이다.
- Visual Studio 또는 .NET SDK가 필요하다.
- 현재 저장소에서는 원격 환경에서 실제 `dotnet build` 실행 검증을 수행하지 않았다.
