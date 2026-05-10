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

현재 IDX 파서는 아직 1차 안전 파서이므로, 일부 실제 Lineage IDX에서는 레코드가 정확히 분해되지 않을 수 있다. 다음 단계에서 PakViewer의 `L1PakTools.IndexRecord` 계열 로직을 분석해 실제 포맷 처리를 보강한다.

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

3차에서는 IDX 실제 파서 보강을 우선한다.

- 고정 레코드/확장 레코드 후보 탐지
- 파일명/offset/size 추정 로직 분리
- PAK 범위 검증 강화
- 잘못 읽은 레코드 자동 제외
- 추출 가능한 레코드만 버튼 활성화

## 주의사항

- 원본은 중국어/영어 UI가 섞여 있으므로 우리 viewer는 한글 UI를 기본으로 한다.
- 모든 새 파일은 UTF-8로 작성한다.
- 대용량 기능은 한 번에 병합하지 않고 작은 커밋 단위로 진행한다.
