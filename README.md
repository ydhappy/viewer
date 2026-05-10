# viewer

`viewer`는 `tony1223/PakViewer`와 `tony1223/L1MapViewer`의 기능을 하나의 Windows 데스크톱 뷰어로 통합하기 위한 프로젝트입니다.

원본 저장소를 그대로 복사하지 않고, 기능을 분석하여 우리 구조에 맞게 단계적으로 흡수합니다.

## 통합 대상

- `tony1223/PakViewer`: IDX/PAK 리소스 조회, 추출, 텍스트/이미지/SPR/DAT 계열 뷰어 구조 참고
- `tony1223/L1MapViewer`: S32 지도 분석, 지도 폴더 스캔, 타일/레이어 기반 지도 뷰어 구조 참고

## 현재 주요 기능

### PAK / IDX

- IDX 열기
- `IIdxParserStrategy` 기반 IDX parser registry
- `IdxParseResult` 기반 상세 parse 결과 모델
- `IdxLoadUiBinder` 기반 PAK 탭 UI 연결
- `PakPreviewDiagnosticsPresenter` 기반 preview 진단 메시지 생성
- IDX 로드 직후 Info 탭에 strategy / probe 여부 / message 표시
- IDX 로드 직후 Log에 strategy / probeOnly / records / extractable 기록
- probe-only/fallback 결과 안내 표시
- classic 28-byte IDX 후보 파싱
- `_EXTB$` 확장 IDX parser
- `_EXTB$` 구조: 16-byte header + 128-byte entries 후보 지원
- `_EXTB$` entry 후보: filename 8~119, PAK offset 120, uncompressed size 124
- `_EXTB$` offset 정렬 기반 compressed size 계산
- `_EXTB$` compression metadata 저장
- `_EXTB$` compression 0 raw read 지원
- `_EXTB$` compression 1 zlib 해제 후보 지원
- `_EXTB$` compression 2 brotli 해제 후보 지원
- fallback binary/text 후보 파싱
- PAK/PAK 대소문자 자동 탐색
- 추출 가능 레코드 표시
- 선택 레코드 추출
- 텍스트 미리보기
- PNG/BMP/JPG/GIF 이미지 미리보기
- 작은 바이너리 HEX 미리보기
- SPR/IMG/TIL/TBT 전용 리소스 감지 및 헤더 정보 표시
- list.spr 열기 및 `.spr` 리소스 매핑 표시

### Sprite

- Sprite 전용 탭
- list.spr entry 목록 표시
- Sprite ID / 이름 / 그룹 / 액션 검색
- list.spr entry와 PAK `.spr` record 역매핑
- 매핑된 `.spr` 리소스 개별 추출
- Sprite 매핑/진단 정보 TXT 저장
- SPR 바이트 HEX preview
- SPR 헤더 후보 분석
- Frame Count / Direction Count / Palette Size / Frame Bytes 후보 추정
- `ISpriteFrameDecoder` 기반 SPR frame decoder registry
- RawPreview fallback decoder
- SPR raw byte 저장
- 후보 payload 회색조 Raw Preview 표시
- Raw Preview width / offset / frame index / zoom 수동 조정
- Raw Preview PNG 저장

### S32 Map

- S32 파일 열기
- S32 지도 폴더 스캔
- 파일명 기반 좌표 추정
- Layer 후보 요약
- Tile.idx 열기 및 Tile.pak 상태 표시
- Layer1 후보 Tile ID 샘플 읽기
- Tile ID 기반 색상 그리드 렌더링
- 확대/축소
- 마우스 Hover Tile ID 표시
- 클릭 선택 Tile ID 표시
- Render PNG 저장
- 선택 Tile 정보 복사
- S32 분석 TXT 저장
- 지도 스캔 CSV 저장

### Tile Resource

- Tile.idx 레코드 목록 표시
- Tile ID 검색
- Tile 레코드 상세 정보 표시
- 확장자 기반 변환 후보 분류
- DirectImage/TIL/IMG/SPR/TBT/Text 변환기 등록 구조
- PNG/BMP/JPG/JPEG/GIF 직접 이미지 변환 및 미리보기
- TIL/IMG Raw Byte 진단 이미지 변환
- TIL/IMG/SPR/TBT 변환기 placeholder 및 실패 사유 표시
- 선택 리소스 헤더/HEX 진단
- 변환 결과 이미지 PNG 저장/복사
- 등록 변환기 목록 표시

## 요구 환경

- Windows
- .NET SDK 10 이상
- Visual Studio 2026 이상 또는 `dotnet` CLI

## 빌드

GitHub Actions 자동 빌드가 적용되어 있습니다.

```text
.github/workflows/build.yml
```

자동 빌드는 `main` 브랜치 push, pull request, 수동 실행(`workflow_dispatch`)에서 Windows 환경으로 수행됩니다.

로컬 빌드:

```powershell
.\scripts\build.ps1
```

또는 직접 실행:

```powershell
dotnet restore .\Viewer.sln
dotnet build .\Viewer.sln -c Release --no-restore
```

## 실행

```powershell
.\scripts\run.ps1
```

또는 직접 실행:

```powershell
dotnet run --project .\src\Viewer.App\Viewer.App.csproj
```

## 프로젝트 구조

```text
viewer/
├─ Viewer.sln
├─ README.md
├─ .github/
│  └─ workflows/
│     └─ build.yml
├─ scripts/
│  ├─ build.ps1
│  └─ run.ps1
├─ docs/
│  ├─ INTEGRATION.md
│  └─ KNOWN_ISSUES.md
└─ src/Viewer.App/
   ├─ Viewer.App.csproj
   ├─ GlobalUsings.cs
   ├─ Program.cs
   ├─ MainForm.cs
   ├─ Pak/
   │  ├─ ExtbIdxParserStrategy.cs
   │  ├─ IdxLoadUiBinder.cs
   │  ├─ IdxParseResultPresenter.cs
   │  ├─ IdxParserStrategy.cs
   │  ├─ IdxRecord.cs
   │  ├─ IdxParser.cs
   │  ├─ PakExtractor.cs
   │  ├─ PakPreviewDiagnosticsPresenter.cs
   │  ├─ PakRecordDiagnostics.cs
   │  ├─ PreviewHelper.cs
   │  ├─ SpecialResourceInfo.cs
   │  ├─ SpriteFrameDecoder.cs
   │  ├─ SpriteHeaderAnalysis.cs
   │  ├─ SpriteListEntry.cs
   │  ├─ SpriteListParser.cs
   │  ├─ SpriteRawPreview.cs
   │  └─ SpriteResourcePanel.cs
   └─ Map/
      ├─ ITileImageCache.cs
      ├─ S32Analyzer.cs
      ├─ S32Coordinate.cs
      ├─ S32GridRenderPanel.cs
      ├─ S32Info.cs
      ├─ S32LayerParser.cs
      ├─ S32LayerSample.cs
      ├─ TileConversion.cs
      ├─ TileResourceConverters.cs
      ├─ TileResourceDiagnostics.cs
      ├─ TileResourcePanel.cs
      └─ TileResourceSet.cs
```

## 문서

- 통합 진행 내역: `docs/INTEGRATION.md`
- 알려진 제한사항: `docs/KNOWN_ISSUES.md`

## 다음 개발 방향

1. 보호/암호화 IDX 처리
2. `_EXTB$` 압축 entry 실데이터 검증
3. TIL/IMG 실제 이미지 변환
4. SPR 실제 프레임 디코더/팔레트/방향별 렌더링
5. Tile.idx 기반 실제 타일 이미지 캐시
6. S32 Layer2/3/4/5/7 파서 보강
7. 실제 타일 기반 지도 렌더링
8. 편집/저장/PNG Export 고도화
