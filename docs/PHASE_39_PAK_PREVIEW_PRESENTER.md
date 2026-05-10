# 39차 - PAK Preview Diagnostics Presenter

## 목표

MainForm 직접 치환 리스크를 줄이기 위해 PAK preview 진단 문자열 생성을 별도 presenter로 분리한다.

## 추가 파일

```text
src/Viewer.App/Pak/PakPreviewDiagnosticsPresenter.cs
```

## 완료 내용

- `PakPreviewDiagnosticsPresenter` 추가
- 선택 record 초기 Info 표시용 API 추가
- preview 성공 결과 표시 API 추가
- preview 실패 진단 메시지 생성 API 추가
- preview 실패 Log 메시지 생성 API 추가
- IDX path 기준 PAK path / PAK size 계산 helper 추가
- README 프로젝트 구조에 presenter 파일 반영

## 제공 API

```csharp
PakPreviewDiagnosticsPresenter.BuildInitialInfo(record, currentIdxPath)
PakPreviewDiagnosticsPresenter.BuildPreviewSuccessInfo(kind, readBytes)
PakPreviewDiagnosticsPresenter.BuildPreviewFailureInfo(record, exception, currentIdxPath)
PakPreviewDiagnosticsPresenter.BuildPreviewFailureLog(record, exception)
```

## MainForm 최소 연결 예시

`ShowPakPreview()` 초반:

```csharp
infoPreview.Text = Pak.PakPreviewDiagnosticsPresenter.BuildInitialInfo(record, _currentIdxPath);
```

preview 성공 후:

```csharp
infoPreview.AppendText(Environment.NewLine + Environment.NewLine +
    Pak.PakPreviewDiagnosticsPresenter.BuildPreviewSuccessInfo(kind, data.Length));
```

preview 실패 catch:

```csharp
textPreview.Text = ex.Message;
infoPreview.AppendText(Environment.NewLine + Environment.NewLine +
    Pak.PakPreviewDiagnosticsPresenter.BuildPreviewFailureInfo(record, ex, _currentIdxPath));
WriteLog(Pak.PakPreviewDiagnosticsPresenter.BuildPreviewFailureLog(record, ex));
tabs.SelectedIndex = 3;
```

## 기대 효과

- MainForm 수정량 최소화
- 압축 record preview 실패 시 file/offset/size/compression/packed size를 일관된 형식으로 표시
- 향후 PAK preview UI를 별도 panel로 분리하기 쉬워짐

## 다음 단계

40차에서는 MainForm의 `ShowPakPreview()`에 위 presenter 호출만 작은 범위로 연결한다.
