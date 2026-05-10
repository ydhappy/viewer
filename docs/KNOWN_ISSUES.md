# Known Issues

## IDX / PAK

- 현재 IDX 파서는 classic 28-byte 후보 구조를 우선 지원한다.
- 보호/암호화 IDX는 아직 지원하지 않는다.
- `_EXTB$` 확장 IDX는 아직 지원하지 않는다.
- SPR/IMG/TIL/TBT는 전용 감지와 헤더 표시까지만 지원한다.
- SPR/IMG/TIL 실제 이미지 변환/렌더링은 추후 구현 대상이다.

## S32 Map

- 현재 S32 파서는 파일 앞부분을 Layer1 후보로 읽는 샘플 파서다.
- 모든 S32 변형 포맷을 보장하지 않는다.
- Render 탭은 Tile ID 기반 색상 그리드이며 실제 Tile 이미지 렌더링은 아니다.
- Tile.idx/Tile.pak는 상태 연결까지만 되어 있고 실제 타일 이미지 캐시는 아직 없다.
- Layer2/3/4/5/7의 실제 의미별 렌더링은 추후 구현 대상이다.

## UI / Build

- Windows Forms 기반이므로 Windows 환경 실행을 기준으로 한다.
- TargetFramework는 `net10.0-windows`이다.
- Visual Studio 또는 .NET SDK가 필요하다.
