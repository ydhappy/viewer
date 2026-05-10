# 54차 - S32 Isometric Tile Placement 준비

## 목표

S32 Layer1 renderer가 기존 color grid뿐 아니라 isometric tile placement 후보 렌더링을 사용할 수 있도록 준비한다.

## 완료 내용

- `S32IsoTileLayoutOptions` 추가
- `S32IsoTileLayout` helper 추가
- grid 좌표 → iso screen 좌표 변환 추가
- tile image target rectangle 계산 추가
- iso diamond polygon 계산 추가
- iso map bounds 계산 helper 추가
- `S32RenderMode` enum 추가
  - `ColorGrid`
  - `IsoTile`
- `S32GridRenderPanel`에 render mode 상태 추가
- 우클릭 메뉴에 render mode 전환 추가
- IsoTile mode에서 tile image draw 우선 시도
- tile image 실패 시 iso diamond color fallback 유지
- overlay와 selected tile info에 mode 표시

## 추가 파일

```text
src/Viewer.App/Map/S32IsoTileLayout.cs
```

## 변경 파일

```text
src/Viewer.App/Map/S32GridRenderPanel.cs
```

## 현재 렌더 모드

```text
ColorGrid: 기존 2D grid cell 기반 렌더링
IsoTile  : isometric 좌표 기반 tile image/diamond fallback 렌더링
```

## 사용 방법

S32 render 영역에서 우클릭한다.

```text
Render Mode: Color Grid
Render Mode: Iso Tile
```

## 제한사항

- IsoTile mode의 hover/click tile 역산은 아직 구현하지 않았다.
- pan/scroll viewport가 아직 없다.
- 실제 Lineage tile placement와 완전히 동일한지는 실데이터 검증이 필요하다.
- 현재는 Tile image를 iso target rectangle에 맞춰 그리는 단계다.

## 다음 단계

55차에서는 IsoTile mode의 navigation을 보강한다.

- pan offset 추가
- mouse drag pan
- iso 좌표 역산 후보 추가
- selected tile marker 복구
