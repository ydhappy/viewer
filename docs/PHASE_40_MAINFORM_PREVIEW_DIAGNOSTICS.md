# 40차 - MainForm Preview Diagnostics 연결

## 목표

39차에서 추가한 `PakPreviewDiagnosticsPresenter`를 `MainForm.ShowPakPreview()`에 직접 연결한다.

## 완료 내용

- `ShowPakPreview()` 초기 Info 표시를 `PakPreviewDiagnosticsPresenter.BuildInitialInfo()`로 변경
- preview 성공 시 `BuildPreviewSuccessInfo()` 결과를 Info 탭에 append
- preview 실패 시 `BuildPreviewFailureInfo()` 결과를 Info 탭에 append
- preview 실패 시 `BuildPreviewFailureLog()` 결과를 Log에 기록
- 기존 간단 FileName/Size/Offset/Format/Extract 표시를 detailed diagnostics 기반으로 교체

## 변경 파일

```text
src/Viewer.App/MainForm.cs
```

## 변경 후 흐름

```text
Record 선택
 → ShowPakPreview()
 → BuildInitialInfo(record, _currentIdxPath)
 → PakExtractor.ReadBytes()
 → PreviewKind 판별
 → 성공: BuildPreviewSuccessInfo(kind, data.Length)
 → 실패: BuildPreviewFailureInfo(record, ex, _currentIdxPath) + BuildPreviewFailureLog(record, ex)
```

## Info 탭 표시 강화

선택 record에서 다음 정보를 즉시 확인할 수 있다.

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
Preview Result
ReadBytes
Preview error
Inner diagnostics
```

## 기대 효과

ExtB 압축 record preview/extract 실패 시 어떤 compression/packed-size/offset 조합에서 실패했는지 UI와 Log에서 바로 추적할 수 있다.

## 다음 단계

41차에서는 실제 빌드 실패 가능성이 있는 부분을 점검한다.

- MainForm 전체 치환 후 using/namespace 영향 확인
- `PakPreviewDiagnosticsPresenter` nullable/string 처리 점검
- ExtB compression 지원 관련 .NET API 사용 가능성 확인
- 문서에 40차 반영
