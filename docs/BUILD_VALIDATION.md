# Build Validation

## 목적

이 문서는 `viewer`의 빌드 검증 기준만 관리한다.

## GitHub Actions

```text
.github/workflows/build.yml
```

Workflow 이름:

```text
Build / Windows .NET build
```

Trigger:

```text
main push
main pull_request
manual workflow_dispatch
```

## 기준 빌드 명령

```powershell
dotnet restore .\Viewer.sln --verbosity minimal
dotnet build .\Viewer.sln -c Release --no-restore --verbosity minimal
```

## Artifact

빌드 성공 시 다음 artifact를 업로드한다.

```text
viewer-release-build
```

## 환경

```text
OS: windows-latest
SDK: .NET 10.x
TargetFramework: net10.0-windows
App Type: WinForms
```

## 최근 주의 대상

최근 다음 영역의 변경이 많으므로 빌드 실패 시 우선 확인한다.

```text
src/Viewer.App/Map/S32GridRenderPanel.cs
src/Viewer.App/Map/S32IsoTileLayout.cs
src/Viewer.App/Map/TileConversion.cs
src/Viewer.App/Map/TileResourceConverters.cs
src/Viewer.App/Pak/L1TilBlockParser.cs
src/Viewer.App/Pak/L1ImageFormatDecoder.cs
src/Viewer.App/Pak/ImageResourceDecoder.cs
src/Viewer.App/Pak/DesIdxParserStrategy.cs
src/Viewer.App/Pak/PakExtractor.cs
```

## 검증 절차

1. GitHub repository의 Actions 탭을 연다.
2. `Build` workflow를 확인한다.
3. 최신 `main` push 실행 결과를 확인한다.
4. 실패 시 `Build Release` 단계의 첫 번째 compiler error부터 수정한다.
5. 성공 시 `viewer-release-build` artifact 생성 여부를 확인한다.

## 실패 처리 원칙

- 여러 오류가 있어도 첫 번째 compiler error부터 수정한다.
- 경고는 빌드 실패 원인이 아니면 후순위로 둔다.
- Actions에서 성공하면 문서와 README에는 성공 기준만 유지한다.
