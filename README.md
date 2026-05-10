# viewer

`viewer`는 `tony1223/PakViewer`와 `tony1223/L1MapViewer`의 기능을 하나의 Windows 데스크톱 뷰어로 통합하기 위한 프로젝트입니다.

원본 저장소를 그대로 복사하지 않고, 기능을 분석하여 우리 구조에 맞게 단계적으로 흡수합니다.

## 통합 대상

- `tony1223/PakViewer`: IDX/PAK 리소스 조회, 추출, 텍스트/이미지/SPR/DAT 계열 뷰어 구조 참고
- `tony1223/L1MapViewer`: S32 지도 분석, 지도 폴더 스캔, 타일/레이어 기반 지도 뷰어 구조 참고

## 현재 주요 기능

### PAK / IDX

- IDX 열기
- classic 28-byte IDX 후보 파싱
- PAK/PAK 대소문자 자동 탐색
- 추출 가능 레코드 표시
- 선택 레코드 추출
- 텍스트 미리보기
- PNG/BMP/JPG/GIF 이미지 미리보기
- 작은 바이너리 HEX 미리보기
- SPR/IMG/TIL/TBT 전용 리소스 감지 및 헤더 정보 표시

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

## 요구 환경

- Windows
- .NET SDK 10 이상
- Visual Studio 2026 이상 또는 `dotnet` CLI

## 빌드

```powershell
.\scripts\build.ps1
```

또는 직접 실행:

```powershell
dotnet restore .\Viewer.sln
dotnet build .\Viewer.sln -c Release
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
├─ scripts/
│  ├─ build.ps1
│  └─ run.ps1
├─ docs/
│  ├─ INTEGRATION.md
│  └─ KNOWN_ISSUES.md
└─ src/Viewer.App/
   ├─ Viewer.App.csproj
   ├─ Program.cs
   ├─ MainForm.cs
   ├─ Pak/
   │  ├─ IdxRecord.cs
   │  ├─ IdxParser.cs
   │  ├─ PakExtractor.cs
   │  ├─ PreviewHelper.cs
   │  └─ SpecialResourceInfo.cs
   └─ Map/
      ├─ S32Analyzer.cs
      ├─ S32Coordinate.cs
      ├─ S32GridRenderPanel.cs
      ├─ S32Info.cs
      ├─ S32LayerParser.cs
      ├─ S32LayerSample.cs
      └─ TileResourceSet.cs
```

## 문서

- 통합 진행 내역: `docs/INTEGRATION.md`
- 알려진 제한사항: `docs/KNOWN_ISSUES.md`

## 다음 개발 방향

1. 보호/암호화 IDX 처리
2. `_EXTB$` 확장 IDX 처리
3. SPR/IMG/TIL 실제 이미지 변환
4. Tile.idx 기반 실제 타일 이미지 캐시
5. S32 Layer2/3/4/5/7 파서 보강
6. 실제 타일 기반 지도 렌더링
7. 편집/저장/PNG Export 고도화
