# PakViewer + L1MapViewer 통합 계획

## 원칙

원본 저장소를 그대로 복사하지 않고, 기능을 분석하여 우리 `viewer` 구조에 맞게 단계적으로 흡수한다. 모든 새 파일은 UTF-8 기준으로 작성한다.

## 완료 내역 요약

### 1차 ~ 5차

- Windows Forms 기반 통합 앱 생성
- PAK/IDX, S32 Map, Log 탭 구성
- IDX 기본 파서와 S32 기본 분석기 자리 생성
- PAK 추출기와 선택 추출 흐름 추가
- classic 28-byte IDX 후보 파서 추가
- PAK 미리보기 패널 추가
- Text/Hex/Image/Special 미리보기 추가
- SPR/IMG/TIL/TBT 전용 리소스 감지와 헤더 정보 표시

### 6차 ~ 10차

- S32 좌표 추정과 지도 폴더 스캔 추가
- Tile.idx 연동 상태 표시 추가
- S32 Layer1 후보 Tile ID 샘플 파서 추가
- Render 탭 확대/축소, hover/selected tile 표시 추가
- Render PNG 저장, 선택 Tile 정보 복사, S32 분석 TXT 저장, 지도 스캔 CSV 저장 추가

### 11차 ~ 15차

- `Viewer.sln`, build/run 스크립트, README, Known Issues 문서 추가
- Tile 레코드 목록/검색 UI 추가
- Tile 변환 후보 분류와 직접 이미지 변환 구조 추가
- Tile converter registry 추가
- Tile diagnostics analyzer와 HEX preview 추가

### 16차 ~ 20차

- 문서/빌드 설정 안정화
- TIL/IMG Raw Byte 진단 이미지 변환 추가
- Tile 변환 결과 이미지 저장/복사 추가
- list.spr parser와 Sprite list catalog 추가
- PAK 탭에서 list.spr 로드 및 `.spr` 매핑 표시 추가
- Sprite 전용 탭과 `SpriteResourcePanel` 추가
- Sprite entry 검색, PAK `.spr` record 역매핑, renderer placeholder 추가

### 21차 ~ 25차

- Sprite 패널에 SPR 진단/추출/정보 저장 추가
- SPR 바이트 HEX preview 추가
- SPR header candidate analysis 추가
- frame count / direction / palette / frame bytes 후보 추정 추가
- SPR raw byte 저장 추가
- SPR Raw Preview 탭 추가
- width / offset / frame index / zoom 수동 조정 추가
- README/KNOWN_ISSUES에 Sprite/SPR 기능과 한계 반영

### 26차 완료

- PakViewer 원본에서 SPR/list.spr/이미지 처리 후보를 재탐색했으나 독립 SPR decoder 파일은 확인되지 않음
- `SpriteFrameDecodeRequest` 추가
- `SpriteFrameDecodeResult` 추가
- `ISpriteFrameDecoder` 인터페이스 추가
- `RawPreviewSpriteFrameDecoder` 추가
- `PlaceholderSpriteFrameDecoder` 추가
- `SpriteFrameDecoderRegistry` 추가
- Sprite 패널 raw preview 생성 흐름을 decoder registry 경유로 변경
- 등록 decoder 목록을 Sprite 패널 Detail에 표시
- 실제 SPR 디코더를 별도 구현체로 추가할 수 있는 구조 마련

### 27차 완료

- `GlobalUsings.cs` 추가
- WinForms / Drawing / LINQ / IO 공통 namespace 전역 using 정리
- `SpriteFrameDecodeResult`에 `IDisposable` 구현 추가
- Sprite decoder 결과 Bitmap 정리 기준 명확화
- `KNOWN_ISSUES.md`에 decoder registry와 RawPreview fallback decoder 한계 반영
- 디코더 구조 안정화 및 향후 실제 SPR decoder 교체 준비

### 28차 완료

- PakViewer 원본에서 IDX decode / CorePakTools / `_EXTB$` 관련 후보 재탐색
- 원본 검색 결과 `PakReader.cs`, `frmMain.cs`, `Backup/frmMain.cs`가 IDX/PAK 처리 후보로 확인됨
- 기존 `IdxParser` 내부에 섞여 있던 classic-28 / fallback 파싱 로직을 전략 구조로 분리
- `IdxParseContext` 추가
- `IIdxParserStrategy` 인터페이스 추가
- `Classic28IdxParserStrategy` 추가
- `ExtbHeaderProbeIdxParserStrategy` 추가
- `FallbackIdxParserStrategy` 추가
- `IdxParserStrategyRegistry` 추가
- `IdxParserUtilities` 추가
- 기존 `IdxParser.Parse()`가 strategy registry를 통해 동작하도록 변경
- classic-28 기존 동작 유지
- `_EXTB$` 확장 IDX는 실제 해석이 아닌 marker probe 단계로 명확히 분리
- README에 IDX parser registry와 `_EXTB$` probe 반영
- KNOWN_ISSUES에 IDX strategy registry와 확장/보호 IDX 한계 반영

## 현재 적용 상태

### PAK / IDX

- IDX parser registry 구조 적용 완료
- classic-28 실제 후보 파싱 지원
- `_EXTB$` marker probe 지원
- fallback binary/text 후보 표시 지원
- PAK 자동 탐색 및 선택 추출 지원
- Text/Image/Hex/Special 미리보기 지원

### Sprite / SPR

- list.spr catalog/parsing 지원
- Sprite entry 검색과 `.spr` record 역매핑 지원
- SPR 진단, HEX preview, raw byte 저장 지원
- SPR header 후보 분석 지원
- Raw Preview 수동 검증 도구 지원
- `ISpriteFrameDecoder` registry 구조 지원
- 실제 SPR 프레임 디코더는 아직 미구현

### Tile / Map

- Tile.idx 검색/진단/변환 후보 구조 지원
- TIL/IMG Raw Byte 진단 이미지 지원
- S32 Layer1 후보 샘플 렌더링 지원
- 실제 Tile 이미지 기반 S32 렌더링은 아직 미구현

## PakViewer 흡수 대상

PakViewer의 주요 흡수 대상은 다음과 같다.

- 클라이언트 폴더 선택
- `.idx` 파일 스캔
- `.idx` / `.pak` 레코드 표시
- Text/Image/Sprite/SprList/DAT/Gallery 모드
- export/update/delete/rebuild 계열 도구
- list.spr 기반 Sprite 분류
- 보호/확장 IDX decode 후보

## L1MapViewer 흡수 대상

L1MapViewer의 주요 흡수 대상은 다음과 같다.

- S32 지도 파일 로딩
- Layer1/2/3/4/5/7 다중 레이어 표시
- 줌/패닝
- Undo/Redo
- Layer4 객체 선택/삭제
- Minimap
- PNG Export
- CLI info/extract/render/benchmark

## 다음 단계

29차에서는 IDX parser registry를 UI/진단에 더 노출한다.

- PAK 탭 Info 또는 Log에 등록 IDX strategy 목록 표시
- IDX 로드 시 어떤 strategy가 선택됐는지 표시
- `IdxParseResult` 모델 추가 검토
- `_EXTB$` probe 결과를 사용자가 명확히 볼 수 있도록 표시
- 이후 보호/확장 IDX 실제 record parser 이식 준비

## 주의사항

- classic-28 외 보호/확장 IDX는 아직 실제 추출 가능한 record로 해석하지 않는다.
- `_EXTB$`는 현재 marker probe이며 실제 확장 레코드 구조 파서는 아니다.
- Raw Preview 계열 기능은 실제 렌더링이 아니라 디코더 이식 전 검증 도구다.
