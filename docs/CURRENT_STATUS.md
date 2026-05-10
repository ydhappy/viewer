# Current Status

## 프로젝트 목적

`ydhappy/viewer`는 `tony1223/PakViewer`와 `tony1223/L1MapViewer`의 기능을 하나의 Windows WinForms viewer로 통합하는 프로젝트다.

원본 2개를 단순 복사하지 않고, 우리 구조에 맞게 parser / converter / renderer / diagnostics 계층으로 나누어 흡수 중이다.

## 현재 빌드 상태

- GitHub Actions Windows build workflow 적용
- 이전 수동 확인에서 빌드 성공 확인
- 최근 S32 renderer / documentation 변경 후 재검증 필요

빌드 명령:

```powershell
dotnet restore .\Viewer.sln
dotnet build .\Viewer.sln -c Release --no-restore
```

## 현재 구현된 주요 기능

### PAK / IDX

- IDX open / PAK 자동 탐색
- parser strategy registry
- Classic 28-byte IDX parser
- DES encrypted IDX 후보 parser
- `_EXTB$` 확장 IDX parser
- ExtB offset 기반 packed size 계산
- zlib / brotli 후보 압축 해제
- 압축 실패 유형별 diagnostics
- preview success/failure presenter
- extract / text preview / hex preview / image preview

### Image Decode

- System.Drawing 기본 image decode
- ImageSharp fallback image decode
- PNG/BMP/JPG/JPEG/GIF/TGA/TIFF/WEBP 후보 감지
- direct image tile converter

### Sprite

- list.spr parser
- sprite entry search
- PAK `.spr` record mapping
- SPR raw byte preview
- SPR header 후보 분석
- raw grayscale preview 기반

### Tile Resource

- Tile.idx / Tile.pak resource set
- tile id lookup
- converter registry
- DirectImage converter
- L1TIL converter
- RawByteDiagnostic converter
- TIL 실패 시 raw diagnostic fallback
- TIL block parser
- TIL block/sheet preview 후보
- Tile resource diagnostics

### S32 Map

- S32 file open
- folder scan
- coordinate 후보 추정
- Layer1 sample parser
- ColorGrid render mode
- IsoTile render mode
- tile image cache 우선 render 후보
- color fallback 유지
- zoom in/out/reset
- IsoTile pan with middle mouse drag
- IsoTile hover/select tile picking 후보
- viewport clipping / skipped count
- render PNG snapshot

## 최근 핵심 변경 요약

- 원본 PakViewer의 DES IDX 개념 흡수
- ExtB compression auto-detect 흡수
- ImageSharp 기반 preview fallback 추가
- L1 TIL block parser / renderer 후보 추가
- Tile converter fallback 구조 추가
- S32 renderer에 tile image cache 연결
- S32 IsoTile render mode / navigation / viewport clipping 추가
- 문서 구조를 최신 상태 중심으로 축약

## 현재 제한사항

- 실제 Lineage 클라이언트별 IDX/PAK/TIL/S32 변형은 샘플 검증 필요
- L1 IMG RLE decoder는 아직 미완성
- TBT metadata parser는 아직 placeholder
- SPR 실제 frame/palette/direction 렌더링은 아직 raw preview 수준
- S32 Layer2/3/4/5/7 parser는 아직 미흡
- IsoTile 좌표계는 후보 구현이며 실데이터 기준 보정 필요
- MainForm이 아직 크고, 장기적으로 panel/controller 분리가 필요

## 유지 문서

```text
README.md
CURRENT_STATUS.md
ROADMAP.md
BUILD_VALIDATION.md
```

`PHASE_*` 문서는 누적 기록용이었으나 최신 개발에서는 위 문서 중심으로 관리한다.
