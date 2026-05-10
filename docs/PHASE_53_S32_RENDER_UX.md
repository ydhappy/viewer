# 53차 - S32 Renderer 성능/UX 보강

## 목표

S32 renderer가 tile image cache를 사용하기 시작하면서 생길 수 있는 성능 문제를 사용자가 직접 제어할 수 있게 한다.

## 완료 내용

- `S32GridRenderPanel`에 우클릭 context menu 추가
- Tile image render on/off toggle 추가
- tile image render attempt limit 조정 기능 추가
- limit 낮추기: -256
- limit 올리기: +256
- limit 기본값 복원
- Zoom 100% reset 메뉴 추가
- overlay에 render enabled / attempts / success / limit 표시
- selected tile info copy에도 enabled / limit / attempts / success 표시
- 기존 MainForm 수정 없이 panel 자체에서 제어 가능하도록 구현

## 변경 파일

```text
src/Viewer.App/Map/S32GridRenderPanel.cs
```

## 기본값

```text
Tile image render enabled: true
Default attempt limit     : 512
Min attempt limit         : 0
Max attempt limit         : 4096
```

## 사용 방법

S32 Render 영역에서 우클릭한다.

```text
Tile Image Render 켜기/끄기
Tile Image Limit 낮추기
Tile Image Limit 올리기
Tile Image Limit 기본값
Zoom 100%
```

## 기대 효과

- tile image conversion이 무거운 환경에서도 renderer를 멈추지 않고 조절할 수 있다.
- 변환 품질/성능 균형을 사용자가 직접 맞출 수 있다.
- 색상 grid fallback은 계속 유지된다.

## 다음 단계

54차에서는 실제 isometric tile placement 준비를 시작한다.

- 24x24 tile image를 grid cell 대신 iso diamond 좌표에 배치
- Layer1 row/column 기준 screen 좌표 계산 helper 추가
- 기존 color grid와 iso render mode 전환 준비
