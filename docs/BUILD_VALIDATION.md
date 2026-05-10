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

## Workflow hardening

최근 workflow는 다음 항목을 포함하도록 보강했다.

```text
- timeout-minutes: 20
- dotnet --info
- dotnet --list-sdks
- restore/build --verbosity minimal
- failure/success 관계없이 bin output listing
```

## Latest validation attempt

- workflow hardening commit: `95b9cf93a8e251734ec4847119c470a4ec613e5b`
- commit status query returned an empty status list at check time.
- commit workflow run query returned an empty workflow run list at check time.

## Manual check path

GitHub repository page:

```text
Actions > Build > Windows .NET build
```

If the workflow fails, inspect the first compiler error in the `Build Release` step and fix from the first error downward.

## Next action

If Actions does not show a run after this workflow file is present on `main`, check repository Actions settings first.

```text
Settings > Actions > General > Actions permissions
```

Actions must be enabled for this repository.
