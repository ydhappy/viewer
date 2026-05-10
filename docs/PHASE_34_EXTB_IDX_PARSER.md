# 34차 - ExtB IDX Parser Skeleton

## 목표

`_EXTB$` 확장 IDX를 단순 marker probe가 아니라 실제 record 후보로 파싱하는 skeleton strategy를 추가한다.

## 원본 확인

PakViewer 원본 `PakReader.cs`에서 `_EXTB$` 관련 구조 후보를 확인했다.

요약 구조:

```text
Magic  : "_EXTB$"
Header : 16 bytes
Entry  : 128 bytes
```

Entry 후보 구조:

```text
entry + 4   : compression 후보
entry + 8   : filename 시작
entry + 120 : PAK offset 후보
entry + 124 : uncompressed size 후보
```

## 완료 내용

- `ExtbIdxParserStrategy` 추가
- `_EXTB$` magic 검사 추가
- 16-byte header + 128-byte entry 구조 파싱 추가
- filename / pakOffset / fileSize / compression 후보 추출 추가
- compression 값이 0인 entry만 기존 extractor 기준 추출 가능 후보로 표시
- compression 값이 0이 아닌 entry는 `extb-128-compressed-{compression}` 포맷으로 표시하고 추출 불가 처리
- `IdxParserStrategyRegistry`에서 `ExtbIdxParserStrategy`를 probe 전략보다 앞에 배치
- README에 ExtB parser skeleton 반영

## 추가 파일

```text
src/Viewer.App/Pak/ExtbIdxParserStrategy.cs
```

## 변경 파일

```text
src/Viewer.App/Pak/IdxParserStrategy.cs
README.md
```

## 현재 한계

- 압축 entry의 compressed size 계산은 아직 구현하지 않았다.
- zlib/brotli 자동 해제는 아직 PAK extractor에 연결하지 않았다.
- ExtB entry의 unknown/sort key 필드는 아직 표시하지 않는다.
- 실제 클라이언트 변형에 따라 entry 구조가 다를 수 있다.

## 다음 단계

35차에서는 ExtB 압축 entry 처리를 준비한다.

- ExtB offset 목록 정렬
- 다음 offset 기준 compressed size 계산
- 마지막 entry는 PAK 파일 크기 기준 compressed size 계산
- compression type 1=zlib, 2=brotli 후보 처리 구조 추가
- ExtB 전용 extractor 또는 PakExtractor 확장 여부 결정
