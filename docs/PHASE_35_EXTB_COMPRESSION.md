# 35차 - ExtB 압축 Entry 처리 기반

## 목표

`_EXTB$` 확장 IDX에서 압축 entry를 읽기 위한 기반을 추가한다.

## 완료 내용

- `IdxRecord`에 `Compression` 필드 추가
- `IdxRecord`에 `CompressedSize` 필드 추가
- `ExtbIdxParserStrategy`에서 PAK offset 목록 정렬
- 다음 offset 기준 compressed size 계산 추가
- 마지막 entry는 PAK 파일 크기 기준 compressed size 계산
- ExtB entry의 compression 값을 record에 저장
- ExtB entry의 compressed size를 record에 저장
- `PakExtractor.ReadBytes()`가 compression metadata를 기반으로 읽기 크기를 결정하도록 확장
- compression 0: raw read 후 expected size 기준 trim
- compression 1: zlib 해제 후보 지원
- compression 2: brotli 해제 후보 지원
- 지원하지 않는 compression type은 명확한 NotSupportedException 발생

## 변경 파일

```text
src/Viewer.App/Pak/IdxRecord.cs
src/Viewer.App/Pak/ExtbIdxParserStrategy.cs
src/Viewer.App/Pak/PakExtractor.cs
README.md
```

## 현재 ExtB 읽기 흐름

```text
ExtbIdxParserStrategy
 → filename / pakOffset / fileSize / compression 추출
 → offset 정렬
 → compressed size 계산
 → IdxRecord.Compression / CompressedSize 저장
 → PakExtractor.ReadBytes()
 → compression type별 raw/zlib/brotli 처리
```

## 주의사항

- ExtB compression type 값은 원본 PakViewer의 후보 구조를 기준으로 한다.
- 실제 클라이언트별 변형 IDX에서는 compression 값이나 entry 구조가 다를 수 있다.
- zlib/brotli 해제가 실패하면 preview/extract 단계에서 예외가 발생한다.
- 아직 실제 샘플 IDX/PAK로 검증하지 않았다.

## 다음 단계

36차에서는 ExtB 진단 표시를 강화한다.

- PAK 탭 Info에 Compression / CompressedSize 표시
- ListView에 Compression 컬럼 추가 검토
- ExtB record preview 실패 시 상세 원인 표시
- Known Issues에 ExtB 압축 처리 한계 반영
