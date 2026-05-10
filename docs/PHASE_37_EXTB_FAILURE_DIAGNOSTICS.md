# 37차 - ExtB Preview/Extract 실패 진단 강화

## 목표

ExtB 압축 entry preview/extract 실패 시 원인을 빠르게 추적할 수 있도록 record metadata를 예외 메시지와 UI helper에 포함한다.

## 완료 내용

- `PakRecordDiagnostics` 추가
- record diagnostic summary 생성 기능 추가
- failure message 생성 기능 추가
- compression hint 생성 기능 추가
- `PakExtractor.ReadBytes()` 주요 실패 지점에 record diagnostics 포함
- PAK 파일 없음 / 크기 오류 / offset 오류 / packed size 오류 / 범위 초과 오류 메시지 강화
- PAK read 실패 시 inner exception 포함
- zlib/brotli 압축 해제 실패 시 record metadata와 inner exception 포함
- `IdxLoadUiBinder.BuildRecordInfo()` 추가

## 추가 파일

```text
src/Viewer.App/Pak/PakRecordDiagnostics.cs
```

## 변경 파일

```text
src/Viewer.App/Pak/PakExtractor.cs
src/Viewer.App/Pak/IdxLoadUiBinder.cs
```

## 진단에 포함되는 정보

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
Inner Exception
```

## 기대 효과

압축 preview/extract 실패 시 단순히 "압축 해제 실패"로 끝나지 않고, 어떤 record의 어떤 compression/offset/packed size 조합에서 실패했는지 바로 확인할 수 있다.

## 다음 단계

38차에서는 선택 record Info 탭에도 `IdxLoadUiBinder.BuildRecordInfo()`를 직접 연결한다.

- MainForm `ShowPakPreview()`의 기본 infoPreview를 diagnostic summary로 교체
- preview 실패 시 Text/Info 탭 모두에 diagnostics 표시
- Log에도 실패 record metadata 일부 기록
