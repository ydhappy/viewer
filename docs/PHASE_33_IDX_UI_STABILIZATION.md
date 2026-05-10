# 33차 - IDX UI 연결 안정화

## 목표

32차에서 반영된 `IdxLoadUiBinder` 기반 MainForm 연결 상태를 확인하고 README를 최신 상태로 갱신한다.

## 확인 결과

`src/Viewer.App/MainForm.cs`의 IDX 로드 흐름은 다음 구조로 변경되었다.

```csharp
var state = Pak.IdxLoadUiBinder.Load(dialog.FileName);
_currentIdxPath = dialog.FileName;
_currentPakRecords = state.Records.ToList();
Pak.IdxLoadUiBinder.FillListView(list, state.Records);
_spritePanel.SetRecords(_currentPakRecords, _currentIdxPath);
infoPreview.Text = state.InfoText;
WriteLog(state.LogText);
```

probe-only/fallback 결과에 대해서는 다음 흐름으로 안내한다.

```csharp
if (!string.IsNullOrWhiteSpace(state.ProbeWarning))
{
    MessageBox.Show(this, state.ProbeWarning, "IDX parser probe-only", MessageBoxButtons.OK, MessageBoxIcon.Warning);
}
```

## 완료 내용

- MainForm에서 `IdxLoadUiBinder.Load()` 사용 확인
- MainForm에서 `IdxLoadUiBinder.FillListView()` 사용 확인
- IDX 로드 직후 Info 탭에 parse result 표시 확인
- IDX 로드 직후 Log에 strategy/probeOnly/records/extractable 기록 확인
- probe-only/fallback 결과 안내 메시지 표시 확인
- README에 `IdxLoadUiBinder`, `IdxParseResultPresenter` 파일 구조 반영
- README에 PAK 탭 UI 연결 상태 반영

## 현재 IDX 로드 흐름

```text
IDX 열기
 → IdxLoadUiBinder.Load()
 → IdxParser.ParseDetailed()
 → IdxParseResult 생성
 → IdxLoadUiBinder.FillListView()
 → Info 탭에 상세 결과 표시
 → Log 기록
 → probe-only/fallback이면 안내 메시지 표시
```

## 다음 단계

34차에서는 `_EXTB$` 확장 IDX의 실제 record parser 후보를 준비한다.

- PakViewer 원본 `PakReader.cs` 상세 분석
- `_EXTB$` 주변 헤더/record 구조 후보 정리
- `ExtbIdxParserStrategy` skeleton 추가
- probe-only 전략과 실제 parser 전략 분리
