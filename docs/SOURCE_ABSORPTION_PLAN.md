# Source Absorption Plan

## 목표

`tony1223/PakViewer`와 `tony1223/L1MapViewer`의 기능을 우리 `ydhappy/viewer`에 단계적으로 흡수한다.

## 원칙

- 원본 전체를 무작정 복붙하지 않는다.
- 빌드가 깨지지 않도록 작은 단위로 흡수한다.
- 흡수 후 GitHub Actions 빌드로 검증한다.
- UI/렌더러/파서/도구를 분리해서 유지보수 가능한 구조로 재구성한다.

## 원본 규모

```text
ydhappy/viewer        : 약 147 KB
tony1223/PakViewer    : 약 3906 KB
tony1223/L1MapViewer  : 약 3481 KB
원본 2개 합계         : 약 7387 KB
```

## 원본 PakViewer 주요 흡수 대상

- `PakReader.cs`
- `frmMain.cs`
- `MainForm.Core.cs`
- SPR viewer 계열
- IMG viewer 계열
- Gallery viewer 계열
- Text compare 계열
- export / update / delete / rebuild 계열
- protected / ExtB IDX 처리
- ImageSharp / Pfim 기반 image decode
- CodePages 기반 encoding 처리

## 원본 L1MapViewer 주요 흡수 대상

- S32 Layer1/2/3/4/5/7 parser
- Tile.idx / Tile.pak 실제 tile decode
- SkiaSharp 기반 render pipeline
- zoom / pan / minimap
- Undo / Redo
- Layer4 object select/delete
- PNG export
- CLI info / extract-tile / render-adjacent / benchmark
- NLog 기반 logging

## 45차 시작 작업

- 원본 dependency baseline 반영
- `System.Text.Encoding.CodePages` 추가
- `SixLabors.ImageSharp` 추가
- `Pfim` 추가
- `SkiaSharp` 추가
- `NLog` 추가

## 다음 작업

46차:

- PakViewer `PakReader.cs` 정밀 흡수
- 기존 `ExtbIdxParserStrategy` / `PakExtractor`와 원본 로직 비교
- 압축/암호화/offset 계산 누락 보강

47차:

- PakViewer image decode 계층 흡수
- ImageSharp / Pfim 기반 DDS/TGA/BMP/PNG/JPG 후보 처리 추가

48차:

- L1MapViewer S32 layer parser 인벤토리 작성
- Layer1/2/3/4/5/7 구조를 우리 `Map` namespace에 흡수

49차:

- SkiaSharp tile render pipeline 준비
- 기존 color grid render를 실제 tile render로 교체 준비
