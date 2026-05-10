# 56차 - S32 Viewport Clipping / Render 성능 보강

## 목표

S32 renderer가 모든 tile을 무조건 그리지 않고, 화면 밖 tile은 skip하여 성능을 개선한다.

## 완료 내용

- `S32GridRenderPanel`에 viewport render counter 추가
  - visible tile count
  - drawn tile count
  - skipped tile count
- `OnPaint()`에서 visible clip bounds 전달
- ColorGrid mode에서 화면 밖 cell skip
- IsoTile mode에서 diamond/image target bounds 기준 화면 밖 tile skip
- overlay에 viewport counter 표시
- selected tile info copy에 viewport counter 표시
- render counter reset helper 추가
- polygon bounds helper 추가

## 변경 파일

```text
src/Viewer.App/Map/S32GridRenderPanel.cs
```

## 현재 render skip 흐름

```text
ColorGrid:
cell rectangle 계산
 → clip bounds와 교차하지 않으면 skip
 → 교차하면 draw

IsoTile:
diamond polygon bounds + image target bounds 계산
 → clip bounds와 교차하지 않으면 skip
 → 교차하면 draw
```

## Overlay 표시

```text
Viewport: visible=..., drawn=..., skipped=...
```

## 기대 효과

- Pan/Zoom 상태에서 화면 밖 tile draw call 감소
- IsoTile mode에서 향후 대형 map render 최적화 기반 마련
- 실제 tile image render 시 불필요한 conversion attempt 감소

## 다음 단계

57차에서는 build 검증을 수행한다.

- GitHub Actions 결과 확인
- 실패 시 첫 compiler error부터 수정
- 최근 대량 변경 S32GridRenderPanel 컴파일 안정화
