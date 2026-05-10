# 50차 - TIL 전체 파일 Block Parser 추가

## 목표

기존 첫 block 후보 렌더링에서 한 단계 확장하여 `.til` 리소스 전체를 여러 block 후보로 분리하고 sheet preview로 표시한다.

## 완료 내용

- `L1TilBlock` 모델 추가
- `L1TilBlockParser` 추가
- simple diamond block length 후보 계산 추가
- segmented block length 후보 계산 추가
- 최대 block 수 제한 추가
- block type별 summary 생성 추가
- `L1TilTileResourceConverter`가 `L1TilBlockParser.ParseBlocks()`를 사용하도록 변경
- block이 1개이면 단일 block preview 표시
- block이 여러 개이면 sheet preview 표시
- conversion result message에 TIL parse summary 포함

## 추가 파일

```text
src/Viewer.App/Pak/L1TilBlockParser.cs
```

## 변경 파일

```text
src/Viewer.App/Map/TileResourceConverters.cs
```

## 현재 TIL preview 흐름

```text
TileResourceSet
 → TileResourceClassifier: .til
 → L1TilTileResourceConverter
 → PakExtractor.ReadBytes()
 → L1TilBlockParser.ParseBlocks()
 → 1 block: L1ImageFormatDecoder.RenderTilBlock()
 → multiple blocks: L1ImageFormatDecoder.RenderTilSheet()
```

## 제한사항

- block length 계산은 원본 ImageConvert 구조 기반의 후보 parser다.
- 실제 모든 클라이언트의 TIL 변형을 보장하지 않는다.
- Tile ID와 TIL 내부 block index 매핑은 아직 확정 전이다.
- S32 renderer에 실제 tile cache를 완전 연결한 단계는 아니다.

## 다음 단계

51차에서는 TIL preview 진단 정보를 UI에서 더 잘 보이게 보강한다.

- block count / parsed bytes / type distribution 표시
- Tile panel save/copy flow 확인
- 실패 시 raw diagnostic fallback 검토
