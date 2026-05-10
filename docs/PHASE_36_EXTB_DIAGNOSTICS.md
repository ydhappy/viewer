# 36차 - ExtB 진단 표시 강화

## 목표

ExtB 압축 entry 처리 기반을 UI/Info/Log에서 확인하기 쉽게 만든다.

## 완료 내용

- PAK ListView에 `Compression` 컬럼 자동 추가
- PAK ListView에 `Packed` 컬럼 자동 추가
- `IdxLoadUiBinder.FillListView()`에서 `Compression`, `CompressedSize` 표시
- IDX Info 탭에 Compression Summary 추가
- IDX Log 메시지에 compressed record 개수 추가
- Known Issues에 ExtB compression 처리 한계 반영

## 변경 파일

```text
src/Viewer.App/Pak/IdxLoadUiBinder.cs
src/Viewer.App/Pak/IdxParseResultPresenter.cs
docs/KNOWN_ISSUES.md
```

## 현재 표시 항목

PAK 목록:

```text
No / FileName / Size / Offset / Extract / Format / Compression / Packed
```

IDX Info 탭:

```text
Compression Summary
===================
Raw records
Compressed records
Packed size known
By Type
```

Log:

```text
IDX loaded: ..., strategy=..., probeOnly=..., records=..., extractable=..., compressed=...
```

## 주의사항

- `CompressedSize`는 offset 정렬 기반 후보값이다.
- ExtB compression 1=zlib, 2=brotli 처리는 샘플 검증 전 후보 구현이다.
- 클라이언트 변형에 따라 압축 해제 실패 가능성이 있다.

## 다음 단계

37차에서는 ExtB preview/extract 실패 진단을 강화한다.

- 압축 해제 실패 시 compression/packed/unpacked 정보를 함께 표시
- `PakExtractor` 예외 메시지에 record metadata 포함
- Info 탭에서 압축 record 선택 시 상세 진단 표시
