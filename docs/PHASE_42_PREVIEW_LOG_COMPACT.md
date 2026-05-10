# 42차 - Preview 실패 Log Compact 처리

## 목표

ExtB 압축 preview/extract 실패 시 상세 진단은 Info 탭에 유지하되, Log 탭에는 한 줄 요약만 기록한다.

## 완료 내용

- `PakPreviewDiagnosticsPresenter.BuildPreviewFailureLog()`의 error 메시지를 compact 처리
- multi-line exception message는 첫 줄만 사용
- Log error 길이를 최대 240자로 제한
- 상세 record diagnostics는 기존처럼 `BuildPreviewFailureInfo()`를 통해 Info 탭에 표시

## 변경 파일

```text
src/Viewer.App/Pak/PakPreviewDiagnosticsPresenter.cs
```

## 동작 원칙

```text
Info 탭: 상세 진단 전체 표시
Log 탭 : file/compression/packed/error 1줄 요약 표시
```

## 이유

`PakExtractor`는 실패 시 record metadata와 inner exception을 포함한 긴 메시지를 생성한다. 이 메시지는 Info 탭에는 유용하지만 Log 탭에 그대로 쌓이면 가독성이 떨어지므로 compact log를 별도로 둔다.

## 다음 단계

43차에서는 ExtB 압축 해제 실패 유형별 메시지를 정리한다.

- unsupported compression type
- packed size missing
- zlib/brotli stream invalid
- PAK range overflow
- file missing
