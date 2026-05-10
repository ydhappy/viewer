# Roadmap

## 최우선

### 1. 빌드 검증

최근 변경량이 크므로 가장 먼저 GitHub Actions 빌드 결과를 확인한다.

확인 대상:

```text
S32GridRenderPanel.cs
S32IsoTileLayout.cs
TileResourceConverters.cs
TileConversion.cs
L1TilBlockParser.cs
L1ImageFormatDecoder.cs
ImageResourceDecoder.cs
DesIdxParserStrategy.cs
PakExtractor.cs
```

실패 시 첫 번째 compiler error부터 수정한다.

## 2. S32 renderer 안정화

현재 IsoTile mode는 후보 구현이다.

다음 작업:

- viewport clipping 실동작 확인
- hover/select tile picking 보정
- iso origin / tile width / tile height 실데이터 보정
- tile image draw target 보정
- pan/zoom UX 점검

## 3. L1 Tile format 보강

현재 TIL은 block parser 후보 단계다.

다음 작업:

- 실제 `.til` 샘플 기준 block split 검증
- simple diamond block type 검증
- segmented block type 검증
- TBT metadata parser 추가
- Tile ID ↔ TIL block index 매핑 검증
- S32 renderer와 tile cache 매핑 보정

## 4. IMG / SPR 흡수

아직 미흡한 원본 PakViewer 영역이다.

다음 작업:

- L1 IMG RLE decoder
- RGB555/RGB565 변환 검증
- SPR frame table parser
- palette parser
- direction/action frame rendering
- sprite animation preview

## 5. S32 Layer 확장

현재 Layer1 sample 중심이다.

다음 작업:

- Layer2 parser
- Layer3 parser
- Layer4 object parser
- Layer5 parser
- Layer7 parser
- object overlay
- object select/delete 후보 기능

## 6. 구조 정리

기능 흡수 후 유지보수성을 위해 구조를 정리한다.

대상:

- MainForm 분리
- PakPanel 분리
- MapPanel 분리
- SpritePanel 분리
- command/service 계층 정리
- diagnostics presenter 정리

## 문서 정책

앞으로는 차수별 `PHASE_*` 문서를 계속 늘리지 않는다.

유지 문서:

```text
README.md
CURRENT_STATUS.md
ROADMAP.md
BUILD_VALIDATION.md
```

차수 기록은 필요한 경우 `CURRENT_STATUS.md`의 최근 변경 요약에 반영한다.
