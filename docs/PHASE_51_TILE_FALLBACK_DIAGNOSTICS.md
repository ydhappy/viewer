# 51차 - TIL Preview 진단 보강 및 Raw Fallback

## 목표

TIL preview 실패 시 사용자에게 빈 결과만 보여주지 않고, Raw Byte 진단 이미지로 fallback하여 데이터 패턴을 계속 확인할 수 있게 한다.

## 완료 내용

- `TileConversionResult.WithMessagePrefix()` 추가
- `TileConversionResult.ToDisplayText()`에서 Result를 multiline 형태로 표시
- `DefaultTileImageCache`가 `TileResourceConverterRegistry.ConvertWithFallback()`를 사용하도록 변경
- `TileResourceConverterRegistry.ConvertWithFallback()` 추가
- `.til` 변환에서 primary `L1TIL` 실패 시 `RawByteDiagnostic` 자동 fallback
- fallback 성공 시 primary 실패 메시지와 fallback 사용 사실을 함께 표시
- fallback 실패 시 primary 실패 메시지를 유지하되 fallback 실패 사실을 prefix로 표시

## 변경 파일

```text
src/Viewer.App/Map/TileConversion.cs
src/Viewer.App/Map/TileResourceConverters.cs
```

## 현재 동작

```text
.til 선택/검색
 → L1TIL converter 시도
 → 성공: TIL block/sheet preview
 → 실패: RawByteDiagnostic fallback 시도
 → fallback 성공: Raw byte pattern image 표시 + primary 실패 사유 표시
 → fallback 실패: primary/fallback 실패 정보 표시
```

## 기대 효과

- TIL parser가 아직 모든 변형을 지원하지 않아도 사용자가 데이터 패턴을 확인할 수 있다.
- Detail 탭에서 primary 실패 원인과 fallback 사용 여부를 확인할 수 있다.
- 이후 실제 TIL parser 개선 시 비교 기준이 생긴다.

## 다음 단계

52차에서는 S32 renderer와 tile image cache 연결을 시작한다.

- S32GridRenderPanel이 Tile ID 색상 대신 tile image cache를 우선 사용할 수 있게 준비
- tile image 실패 시 기존 색상 grid fallback 유지
- zoom/pan 성능 확인
