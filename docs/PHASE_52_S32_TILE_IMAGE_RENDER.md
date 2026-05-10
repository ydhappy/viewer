# 52차 - S32 Renderer Tile Image Cache 연결 시작

## 목표

S32 Layer1 render에서 Tile ID 색상 grid만 표시하던 구조를 확장하여, Tile.idx/Tile.pak가 로드되어 있고 tile image 변환이 가능하면 실제 tile image를 우선 렌더링한다.

## 완료 내용

- `S32GridRenderPanel`에 `DefaultTileImageCache` 추가
- Tile resource가 로드된 경우 tile image render 우선 시도
- tile image 변환 성공 시 grid cell에 실제 image draw
- 변환 실패/미지원/미로딩 시 기존 Tile ID color grid fallback 유지
- 한 번의 Paint에서 tile image 변환 시도 최대 512개로 제한
- cell size가 8 이상일 때만 tile image render 시도
- overlay에 tile image render attempts/success 표시
- selected tile info copy에도 tile image attempts/success 포함
- nearest-neighbor interpolation 설정

## 변경 파일

```text
src/Viewer.App/Map/S32GridRenderPanel.cs
```

## 현재 render 흐름

```text
S32Layer1 tile id
 → TileResourceSet loaded?
 → cell size >= 8?
 → attempts < 512?
 → DefaultTileImageCache.GetTileImage(tileId)
 → success + image: draw image
 → otherwise: draw color fallback
```

## 성능 보호

전체 64x128 grid는 8,192 cells이다. 모든 tile을 첫 Paint에서 변환하면 UI가 멈출 수 있으므로 현재는 다음 제한을 둔다.

```text
MaxTileImageDrawAttemptsPerPaint = 512
cellSize >= 8 only
```

## 제한사항

- 아직 virtual viewport/pan 기반 렌더링은 아니다.
- 현재는 grid cell rectangle에 image를 맞춰 그린다.
- L1 isometric diamond 정확도는 다음 단계 대상이다.
- Tile cache clear/refresh 정책은 아직 단순하다.

## 다음 단계

53차에서는 S32 renderer 성능/UX를 보강한다.

- tile image render on/off toggle
- max attempts 조정
- cache status 표시
- 실제 isometric tile placement 준비
