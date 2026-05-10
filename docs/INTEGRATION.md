# PakViewer + L1MapViewer 통합 계획

## 원칙

원본 저장소를 그대로 복사하지 않고, 기능을 분석하여 우리 `viewer` 구조에 맞게 단계적으로 흡수한다.

## 1차 완료

- Windows Forms 기반 통합 앱 생성
- PAK/IDX 탭 추가
- S32 Map 탭 추가
- Log 탭 추가
- IDX 기본 파서 자리 생성
- S32 기본 분석기 자리 생성

## 2차 완료

- `PakExtractor` 추가
- `.idx` 경로 기준 `.pak` / `.PAK` 자동 탐색
- GUI `선택 추출` 버튼 연결
- ListView 선택 레코드 기반 추출 흐름 추가
- 추출 성공/실패 로그 기록
- 경로 조작 방지를 위한 안전 파일명 처리 추가

## 3차 완료

- PakViewer의 `L1PakTools.IndexRecord` 구조를 참고하여 classic 28-byte IDX 후보 파서 추가
- classic record 구조: `offset(4) + filename(20) + size(4)`
- PAK 파일 크기 기준으로 추출 가능 여부 검증
- `IdxRecord`에 `CanExtract`, `Format` 메타데이터 추가
- GUI 목록에 `Extract`, `Format` 컬럼 추가
- 추출 가능한 레코드가 선택된 경우에만 `선택 추출` 활성화
- 추출 불가 레코드는 건너뛰고 로그 기록

## 4차 완료

- `PakExtractor.ReadBytes` 추가
- 선택 레코드를 추출 없이 메모리로 읽는 구조 추가
- `PreviewHelper` 추가
- 확장자/시그니처 기반 PreviewKind 판별
- 텍스트 미리보기 추가
- PNG/BMP/JPG/GIF 이미지 미리보기 추가
- 작은 바이너리 파일용 HEX 미리보기 추가
- PAK 탭을 좌측 목록 / 우측 Preview 패널 구조로 개편
- Preview 패널에 Text/Hex, Image, Info 탭 추가

## 5차 완료

- `SpecialResourceInfo` 추가
- `SpecialResourceAnalyzer` 추가
- `.spr`, `.img`, `.til`, `.tbt` 확장자 감지 추가
- `PreviewKind.Special` 추가
- Preview 패널에 `Special` 탭 추가
- 전용 리소스 선택 시 종류, 확장자, 크기, 헤더 HEX 정보 표시
- SPR/IMG/TIL/TBT 렌더링 연결을 위한 자리 구성

## 6차 완료

- `S32Coordinate` 추가
- `S32Info`에 파일명, 좌표, 레이어 후보 요약 추가
- S32 파일명 기반 좌표 추정 추가
- S32 폴더 스캔 기능 추가
- 지도 목록 테이블 추가
- S32 Map 탭을 좌측 목록 / 우측 Preview 패널 구조로 개편
- Preview 패널에 Info / Render 탭 추가
- Tile.idx 연동 전용 렌더링 자리 구성

## 7차 완료

- `TileResourceSet` 추가
- Tile.idx 선택 기능 추가
- Tile.idx 기준 Tile.pak 자동 탐색 상태 표시
- Tile.idx 레코드 수 / 추출 가능 레코드 수 표시
- `S32GridRenderPanel` 추가
- Render 탭에 임시 Iso Grid 렌더링 추가
- 선택된 S32 정보와 Tile 리소스 상태를 렌더 오버레이에 표시
- Map Preview 탭에 Tile 상태 탭 추가

## 8차 완료

- `S32LayerSample` 추가
- `S32LayerParser` 추가
- S32 파일 앞부분에서 Layer1 후보 Tile ID 샘플 읽기 추가
- Render 탭에서 Layer1 Tile ID 기반 색상 그리드 표시
- Layer1 샘플이 없을 경우 기존 임시 Iso Grid로 fallback
- Render 오버레이에 Layer1 샘플 개수/읽은 바이트 표시

## 9차 완료

- Render 탭 확대/축소 배율 추가
- 마우스 휠 기반 줌 추가
- 툴바 확대/축소/100% 버튼 추가
- Layer1 색상 그리드 셀 크기 조절 추가
- 마우스 위치 기준 Tile 좌표/Tile ID 표시
- 클릭한 Tile 좌표/Tile ID 선택 표시
- Hover Tile은 흰색 테두리, Selected Tile은 노란색 테두리로 표시

## 10차 완료

- Render 화면 PNG 저장 기능 추가
- 현재 Hover/Selected Tile 정보 클립보드 복사 기능 추가
- S32 분석 결과 텍스트 저장 기능 추가
- 지도 폴더 스캔 결과 CSV 저장 기능 추가
- CSV UTF-8 저장 및 기본 escape 처리 추가
- Render 패널 스냅샷/선택 Tile 정보 API 추가

현재 적용된 classic-28 파서는 보호/암호화 IDX나 확장 IDX를 완전히 처리하지 않는다. 보호 IDX는 PakViewer의 CorePakTools/Decode 계열 로직이 필요하므로 별도 단계에서 흡수한다.

## PakViewer 흡수 대상

PakViewer의 `frmMain.cs`는 다음 역할을 포함한다.

- 클라이언트 폴더 선택
- `.idx` 파일 스캔
- `.idx` / `.pak` 레코드 표시
- Text/Image/Sprite/SprList/DAT/Gallery 모드
- export/update/delete/rebuild 계열 도구
- list.spr 기반 Sprite 분류

우리 viewer에서는 다음 순서로 이식한다.

1. IDX 레코드 파서 정교화
2. PAK 추출기 추가
3. Text/Image 미리보기
4. SPR/IMG/TIL 뷰어
5. DAT/Gallery 모드
6. Rebuild/Update 기능

## L1MapViewer 흡수 대상

L1MapViewer README 기준 핵심 기능은 다음과 같다.

- S32 지도 파일 로딩
- Layer1/2/3/4/5/7 다중 레이어 표시
- 줌/패닝
- Undo/Redo
- Layer4 객체 선택/삭제
- Minimap
- PNG Export
- CLI info/extract/render/benchmark

우리 viewer에서는 다음 순서로 이식한다.

1. S32 파일 구조 파서
2. 지도 폴더 스캔 및 좌표 인식
3. Tile.idx 연동
4. 기본 렌더링
5. 레이어 ON/OFF
6. 편집/저장
7. PNG Export

## 다음 단계

11차에서는 안정화와 빌드 편의 기능을 보강한다.

- 프로젝트 솔루션 파일 추가
- 빌드/실행 스크립트 추가
- README 실행 방법 갱신
- known issues 정리
- 이후 Tile 이미지 캐시/실제 타일 렌더러 연결

## 주의사항

- 원본은 중국어/영어 UI가 섞여 있으므로 우리 viewer는 한글 UI를 기본으로 한다.
- 모든 새 파일은 UTF-8로 작성한다.
- 대용량 기능은 한 번에 병합하지 않고 작은 커밋 단위로 진행한다.
