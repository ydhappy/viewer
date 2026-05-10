# 38차 - 선택 Record Info 탭 Diagnostic 연결 준비

## 목표

PAK 목록에서 record를 선택했을 때 Info 탭에 단순 요약 대신 `PakRecordDiagnostics` 기반 상세 정보를 표시한다.

## 현재 완료된 기반

이미 37차에서 다음 API가 추가되어 있다.

```csharp
Pak.IdxLoadUiBinder.BuildRecordInfo(record, pakPath, pakSize)
```

이 API는 내부적으로 다음을 호출한다.

```csharp
PakRecordDiagnostics.BuildRecordSummary(record, pakPath, pakSize)
```

## MainForm 적용 위치

`src/Viewer.App/MainForm.cs`의 `ShowPakPreview()` 메서드 초반부를 교체한다.

기존 코드:

```csharp
infoPreview.Text = string.Join(Environment.NewLine,
    $"FileName : {record.FileName}",
    $"Size     : {record.Size:N0}",
    $"Offset   : {record.Offset:N0}",
    $"Format   : {record.Format}",
    $"Extract  : {(record.CanExtract ? "YES" : "NO")}");
```

교체 코드:

```csharp
var pakPathForInfo = string.IsNullOrEmpty(_currentIdxPath)
    ? null
    : Pak.PakExtractor.ResolvePakPath(_currentIdxPath);

long? pakSizeForInfo = !string.IsNullOrEmpty(pakPathForInfo) && File.Exists(pakPathForInfo)
    ? new FileInfo(pakPathForInfo).Length
    : null;

infoPreview.Text = Pak.IdxLoadUiBinder.BuildRecordInfo(record, pakPathForInfo, pakSizeForInfo);
```

## Preview 성공 시 추가 권장

기존 preview 성공 후 append 구문:

```csharp
infoPreview.AppendText(Environment.NewLine + $"Preview  : {kind}" + Environment.NewLine + $"PakPath  : {pakPath}");
```

권장 교체:

```csharp
infoPreview.AppendText(Environment.NewLine + Environment.NewLine +
    $"Preview  : {kind}" + Environment.NewLine +
    $"ReadBytes: {data.Length:N0}");
```

## Preview 실패 시 추가 권장

기존 catch:

```csharp
catch (Exception ex)
{
    textPreview.Text = ex.Message;
    infoPreview.AppendText(Environment.NewLine + "Preview error: " + ex.Message);
    tabs.SelectedIndex = 3;
}
```

권장 교체:

```csharp
catch (Exception ex)
{
    textPreview.Text = ex.Message;
    infoPreview.AppendText(Environment.NewLine + Environment.NewLine +
        "Preview error" + Environment.NewLine +
        "=============" + Environment.NewLine +
        ex.Message);
    WriteLog($"Preview failed: {record.FileName} - compression={record.Compression}, packed={record.CompressedSize?.ToString() ?? "-"}, error={ex.Message}");
    tabs.SelectedIndex = 3;
}
```

## 기대 표시 정보

Info 탭에서 다음 정보가 표시된다.

```text
FileName
Index
Format
Offset
Unpacked Size
Compression
Compressed Size
CanExtract
PAK Path
PAK Size
Compression Hint
```

## 상태

MainForm 전체 치환은 도구 안전 검사에 차단되어 코드 직접 반영은 보류되었다. 대신 적용 위치와 교체 코드를 문서로 남겼다.
