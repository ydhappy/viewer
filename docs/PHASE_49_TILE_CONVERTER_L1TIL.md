# 49차 - TileResourceConverters에 L1 TIL 연결

## 목표

48차에서 추가한 `L1ImageFormatDecoder`를 실제 Tile resource conversion pipeline에 연결한다.

## 완료 내용

- `L1TilTileResourceConverter` 추가
- `.til` 후보에 대해 L1 TIL 24x24 block preview 시도
- TIL resource 크기 검증 추가
- preview 실패 시 명확한 메시지 반환
- `TileResourceConverterRegistry`에 `L1TilTileResourceConverter` 등록
- `RawByteDiagnosticTileResourceConverter`보다 L1 TIL converter를 먼저 시도하도록 순서 조정
- DirectImage converter가 `ImageResourceDecoder`를 사용하도록 변경
- TileResourceClassifier의 direct image 확장자 후보 확장
  - `.tga`
  - `.targa`
  - `.tif`
  - `.tiff`
  - `.webp`
- `.til` candidate description 갱신

## 변경 파일

```text
src/Viewer.App/Map/TileResourceConverters.cs
src/Viewer.App/Map/TileConversion.cs
```

## 현재 Tile converter 순서

```text
DirectImage
L1TIL
RawByteDiagnostic
SPR placeholder
TBT placeholder
Text placeholder
Unsupported
```

## 한계

- 현재는 전체 `.til` 파일 parser가 아니라 첫 block 후보 렌더링이다.
- TIL block split 구조는 아직 확정하지 않았다.
- Tile.idx의 tile id와 `.til` 내부 block id 매핑은 아직 고도화 전이다.
- 실제 map renderer에 tile image cache가 완전 연결된 단계는 아니다.

## 다음 단계

50차에서는 `.til` 전체 파일에서 block 후보를 분리하는 parser를 추가한다.

- TIL block length 후보 계산
- block type별 expected byte count 후보
- 여러 block sheet preview
- S32 renderer와 tile cache 연결 준비
