# Build Validation

## GitHub Actions

자동 빌드 workflow가 추가되어 있다.

```text
.github/workflows/build.yml
```

## Trigger

이 파일은 build workflow push trigger 검증용 기록이다.

## Expected command

```powershell
dotnet restore .\Viewer.sln
dotnet build .\Viewer.sln -c Release --no-restore
```

## Notes

- Windows runner 기준
- .NET 10 SDK 기준
- WinForms target: `net10.0-windows`
