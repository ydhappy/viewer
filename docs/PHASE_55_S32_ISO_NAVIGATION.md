# 55차 - S32 IsoTile Navigation 보강

## 목표

IsoTile render mode에서 화면좌표와 타일좌표를 연결하고, pan 이동 및 hover/select marker를 사용할 수 있게 한다.

## 완료 내용

- `S32IsoTileLayout.TryFromScreenCandidate()` 추가
- 화면 좌표 → iso tile 후보 역산 추가
- diamond 내부 hit-test 추가
- scaled tile width/height helper 추가
- `S32GridRenderPanel`에 iso pan offset 추가
- middle mouse drag pan 추가
- pan reset 메뉴 추가
- IsoTile mode hover tile 계산 추가
- IsoTile mode selected tile 계산 추가
- IsoTile mode hover/selected marker 표시 추가
- overlay에 pan offset 표시
- selected tile info copy에 pan offset 표시

## 변경 파일

```text
src/Viewer.App/Map/S32IsoTileLayout.cs
src/Viewer.App/Map/S32GridRenderPanel.cs
```

## 사용 방법

```text
우클릭 → Render Mode: Iso Tile
마우스 휠 → zoom in/out
마우스 가운데 버튼 drag → pan
우클릭 → Pan Reset
마우스 hover → tile 좌표 후보 표시
마우스 좌클릭 → selected tile 지정
```

## 현재 IsoTile navigation 흐름

```text
MouseMove
 → TryGetTileAt()
 → S32IsoTileLayout.TryFromScreenCandidate()
 → 화면좌표를 iso tile 후보로 역산
 → diamond hit-test 통과 시 hover tile 지정

Middle Mouse Drag
 → _isoPanOffset 갱신
 → Invalidate()
```

## 제한사항

- 정확한 Lineage 클라이언트 기준 tile origin/height는 실데이터 검증 필요
- Layer2/3/4 object overlay는 아직 없음
- viewport clipping 최적화는 아직 없음
- IsoTile mode는 현재 모든 Layer1 cell을 순회한다

## 다음 단계

56차에서는 viewport clipping과 render 성능 보강을 진행한다.

- 화면에 보이는 tile 후보만 draw
- iso map bounds 기반 clipping
- draw call count overlay 표시
- 대형 map 렌더링 최적화
