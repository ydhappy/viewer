# 30차 - IDX Parse Result UI 연결 준비

## 목표

`IdxParseResult`를 PAK 탭 UI와 Log에 노출하기 위한 presenter 계층을 추가한다.

## 완료

- `IdxParseResultPresenter` 추가
- IDX 로드 후 Info 탭에 표시할 문자열 생성 API 추가
- IDX 로드 후 Log에 기록할 문자열 생성 API 추가
- probe-only/fallback 결과 안내 문구 생성 API 추가

## 추가 파일

```text
src/Viewer.App/Pak/IdxParseResultPresenter.cs
```

## 제공 API

```csharp
IdxParseResultPresenter.BuildLoadInfo(string idxPath, IdxParseResult result)
IdxParseResultPresenter.BuildLogMessage(string idxPath, IdxParseResult result)
IdxParseResultPresenter.BuildProbeWarning(IdxParseResult result)
```

## MainForm 연결 예정

다음 단계에서 `openIdxButton.Click` 흐름을 아래 방식으로 변경한다.

```csharp
var parseResult = Pak.IdxParser.ParseDetailed(dialog.FileName);
_currentPakRecords = parseResult.Records.ToList();
infoPreview.Text = Pak.IdxParseResultPresenter.BuildLoadInfo(dialog.FileName, parseResult);
WriteLog(Pak.IdxParseResultPresenter.BuildLogMessage(dialog.FileName, parseResult));

if (parseResult.IsProbeOnly)
{
    MessageBox.Show(this,
        Pak.IdxParseResultPresenter.BuildProbeWarning(parseResult),
        "IDX parser probe-only",
        MessageBoxButtons.OK,
        MessageBoxIcon.Warning);
}
```

## 다음 단계

31차에서 MainForm의 IDX 로드 handler를 위 presenter API에 연결한다.
