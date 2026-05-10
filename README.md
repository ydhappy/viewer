# viewer

`viewer`는 `tony1223/PakViewer`와 `tony1223/L1MapViewer`의 기능을 하나의 Windows 데스크톱 뷰어로 통합하는 프로젝트입니다.

현재 목표는 원본 2개 저장소의 기능을 그대로 난삽하게 합치는 것이 아니라, **빌드가 깨지지 않는 구조로 핵심 기능을 단계적으로 흡수**하는 것입니다.

## 현재 상태

- GitHub Actions Windows 빌드 성공 이력 있음
- PAK / IDX parser 구조 구축
- Classic 28-byte IDX 지원
- DES encrypted IDX 후보 지원
- `_EXTB$` 확장 IDX 후보 지원
- ExtB zlib / brotli 후보 압축 해제 지원
- Preview / extract / diagnostics 기본 구조 구축
- ImageSharp 기반 image preview fallback 추가
- Sprite/list.spr 진단 구조 구축
- Tile.idx / Tile.pak resource panel 구축
- L1 TIL block parser / sheet preview 후보 추가
- TIL 실패 시 RawByte diagnostic fallback 추가
- S32 Layer1 sample render 구축
- S32 ColorGrid / IsoTile render mode 추가
- IsoTile pan / zoom / hover / select / viewport clipping 추가

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
dotnet build .\Viewer.sln -c Release --no-restore
```

GitHub Actions:

```text
.github/workflows/build.yml
```

Actions 성공 시 `viewer-release-build` artifact를 다운로드할 수 있습니다.

## 실행

```powershell
.\scripts\run.ps1
```

또는 직접 실행:

```powershell
dotnet run --project .\src\Viewer.App\Viewer.App.csproj
```

## 주요 문서

문서가 너무 많아지지 않도록 최신 기준 문서만 유지합니다.

```text
docs/CURRENT_STATUS.md   현재 구현 상태 / 완료 기능 / 제한사항
docs/ROADMAP.md          다음 작업 우선순위
docs/BUILD_VALIDATION.md 빌드 검증 기준
```

## 다음 개발 방향

1. 최근 대량 변경 후 GitHub Actions 빌드 검증
2. S32 renderer viewport clipping 안정화
3. 실제 L1 TIL/TBT/IMG 포맷 검증
4. SPR 실제 frame decoder / palette / direction 렌더링
5. S32 Layer2/3/4/5/7 parser 흡수
6. 실제 tile cache 기반 map rendering 고도화
7. MainForm 분리 및 panel 구조 정리
