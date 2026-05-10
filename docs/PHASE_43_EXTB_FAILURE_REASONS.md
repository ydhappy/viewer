# 43차 - ExtB 압축 실패 유형별 메시지 정리

## 목표

ExtB preview/extract 실패 시 모든 오류를 하나의 압축 해제 실패로 묶지 않고, 실패 유형을 구분해서 표시한다.

## 완료 내용

- `PakExtractor.ReadBytes()` 내부 실패 reason 세분화
- raw record size 오류 메시지 분리
- compressed record packed size 누락 메시지 분리
- PAK range overflow 메시지 분리
- PAK read 실패 메시지 분리
- raw conversion 실패 메시지 분리
- zlib decompression 실패 메시지 분리
- brotli decompression 실패 메시지 분리
- unsupported compression type 메시지 분리
- `GetBytesToRead()` helper 추가
- `DecodeRecordData()` helper 추가
- `BuildPackedSizeFailureReason()` helper 추가
- `BuildDecodeFailureReason()` helper 추가

## 변경 파일

```text
src/Viewer.App/Pak/PakExtractor.cs
```

## 구분되는 실패 유형

```text
Invalid raw record size
Packed size missing
PAK range overflow
PAK read failed
Raw record conversion failed
Zlib decompression failed
Brotli decompression failed
Unsupported compression type
```

## 기대 효과

Info 탭과 compact Log에서 ExtB 압축 record 실패 원인을 빠르게 식별할 수 있다.

## 다음 단계

44차에서는 ExtB record 목록/Info에 compression type 설명을 추가한다.

- compression 0 = raw
- compression 1 = zlib candidate
- compression 2 = brotli candidate
- unknown compression = unsupported
