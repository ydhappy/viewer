# 48차 - L1 Image Format 흡수 시작

## 목표

PakViewer `ImageConvert.cs`의 L1 이미지 처리 구조를 기반으로 RGB555/TIL block 렌더링 기반을 우리 viewer에 추가한다.

## 완료 내용

- `ImageResourceDecoder`의 ImageSharp PNG 변환 호출 안정화
- `L1ImageInfo` 모델 추가
- `L1ImageFormatDecoder` 추가
- RGB555 → `System.Drawing.Color` 변환 추가
- TIL 24x24 block render foundation 추가
- simple diamond block type 처리 추가
  - 0
  - 1
  - 8
  - 9
  - 16
  - 17
- segmented block 후보 처리 추가
- TIL block sheet render helper 추가

## 추가 파일

```text
src/Viewer.App/Pak/L1ImageFormatDecoder.cs
```

## 변경 파일

```text
src/Viewer.App/Pak/ImageResourceDecoder.cs
```

## 흡수한 원본 개념

PakViewer `ImageConvert.cs`의 다음 개념을 우리 구조에 맞게 분리했다.

- RGB555 color conversion
- 24x24 tile block canvas
- simple diamond tile row width
- segmented tile block decode
- TIL sheet rendering foundation

## 아직 남은 부분

- 실제 `.til` 전체 파일 block split parser
- `.img` L1 RLE decoder
- `.tbt` decoder
- TileResourceConverters 연결
- S32 renderer에 실제 tile bitmap cache 연결

## 다음 단계

49차에서는 `TileResourceConverters`에 `L1ImageFormatDecoder`를 연결한다.

- TIL resource에서 block 후보 추출
- 첫 block preview 표시
- sheet preview 옵션 준비
- Raw Byte 진단 이미지보다 우선순위 조정
