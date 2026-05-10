# 47차 - PakViewer Image Decode 계층 1차 흡수

## 목표

PakViewer의 이미지 처리 구조를 기준으로 우리 viewer의 preview image decode 계층을 확장한다.

## 원본 확인

PakViewer `Utility/ImageConvert.cs`에서 다음 구조를 확인했다.

- ImageSharp 기반 `Image<Rgba32>` → `System.Drawing.Bitmap` 변환
- L1 IMG 로드 구조
- TBT 로드 구조
- TIL 로드 구조
- TIL sheet render 구조
- RGB555 색상 변환 구조

## 이번 차수 완료

- `ImageResourceDecoder` 추가
- System.Drawing 기본 decoder 우선 시도
- 실패 시 ImageSharp decoder fallback
- ImageSharp decoded image를 PNG stream으로 변환 후 `Bitmap` 생성
- PreviewHelper에서 image decode를 `ImageResourceDecoder`로 위임
- 이미지 확장자 후보 확장
  - `.png`
  - `.bmp`
  - `.jpg`
  - `.jpeg`
  - `.gif`
  - `.tga`
  - `.targa`
  - `.tif`
  - `.tiff`
  - `.webp`
- magic header 기반 이미지 감지 확장
  - PNG
  - BMP
  - JPEG
  - GIF
  - TIFF little endian
  - TIFF big endian
  - WEBP

## 추가 파일

```text
src/Viewer.App/Pak/ImageResourceDecoder.cs
```

## 변경 파일

```text
src/Viewer.App/Pak/PreviewHelper.cs
```

## 아직 남은 PakViewer 이미지 흡수 대상

- L1 IMG 실제 decoder
- TBT 실제 decoder
- TIL 실제 tile/block decoder
- RGB555/RGB565 변환
- TIL sheet view
- Gallery viewer
- Pfim 기반 DDS 후보 decode

## 다음 단계

48차에서는 L1 전용 image format 흡수를 시작한다.

- `L1ImageInfo` 모델 추가
- RGB555 변환 helper 추가
- IMG/TIL/TBT decoder skeleton 추가
- TileResourceConverters와 연결 준비
