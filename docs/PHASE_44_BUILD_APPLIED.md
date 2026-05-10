# 44차 - 빌드 적용 및 자동 검증 준비

## 목표

저장소에 Windows .NET 자동 빌드 workflow를 추가하고, 로컬/원격 빌드 기준을 문서화한다.

## 완료 내용

- GitHub Actions workflow 추가
- Windows runner 기반 빌드 설정
- .NET 10 SDK setup 추가
- `dotnet restore .\Viewer.sln` 추가
- `dotnet build .\Viewer.sln -c Release --no-restore` 추가
- push / pull_request / workflow_dispatch trigger 추가
- README 빌드 섹션 갱신
- README 프로젝트 구조에 `.github/workflows/build.yml` 반영
- ExtB parser의 `IReadOnlyList<int>.IndexOf()` 컴파일 리스크 수정

## 추가 파일

```text
.github/workflows/build.yml
docs/PHASE_44_BUILD_APPLIED.md
```

## 변경 파일

```text
README.md
src/Viewer.App/Pak/ExtbIdxParserStrategy.cs
```

## 자동 빌드 조건

```text
main branch push
main branch pull_request
manual workflow_dispatch
```

## 로컬 빌드

```powershell
.\scripts\build.ps1
```

직접 실행:

```powershell
dotnet restore .\Viewer.sln
dotnet build .\Viewer.sln -c Release --no-restore
```

## 수정한 컴파일 리스크

`ExtbIdxParserStrategy.CalculateCompressedSize()`에서 `IReadOnlyList<int>.IndexOf()`를 사용하던 구조를 `FindOffsetIndex()` helper로 교체했다.

## 다음 단계

45차에서는 GitHub Actions 실행 결과를 확인하고, 실패 로그가 나오면 컴파일 오류를 순서대로 수정한다.

- workflow run 확인
- 실패 시 첫 번째 compiler error부터 수정
- MainForm 장기 분리 계획 시작
