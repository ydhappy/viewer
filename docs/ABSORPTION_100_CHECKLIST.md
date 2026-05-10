# 100% Absorption Checklist

## Goal

Absorb the public GUI/CLI capabilities of these source repositories into `ydhappy/viewer`:

```text
tony1223/PakViewer
tony1223/L1MapViewer
```

## Rules

```text
1. Work in small build-safe commits.
2. Keep main buildable.
3. Absorb original behavior first.
4. Add enhancement work after parity is reached.
5. Verify each major batch with GitHub Actions.
```

## Current Branch

```text
absorb-100-phase-1
```

## Sections

```text
A. PakViewer absorption
B. L1MapViewer absorption
C. Integration enhancement
D. Build validation
```

## A. PakViewer absorption

### A1. PAK / IDX Core

- [x] IDX open
- [x] PAK auto resolve
- [x] Classic 28-byte IDX parser
- [x] DES encrypted IDX candidate parser
- [x] ExtB parser candidate
- [x] zlib / brotli candidate decompression
- [ ] Re-check full original PakReader behavior
- [ ] Protected/encrypted IDX variants
- [ ] Packed/unpacked size edge cases
- [ ] Multi-IDX browsing

### A2. PAK Editing

- [ ] Add file
- [ ] Update file
- [ ] Delete file
- [ ] Same-size import
- [ ] Rebuild PAK
- [ ] Backup / rollback
- [ ] Write diagnostics

### A3. Text / Encoding

- [ ] big5
- [ ] euc-kr
- [ ] shift_jis
- [ ] gb2312
- [ ] utf-8
- [ ] Filename suffix encoding auto-detect
- [ ] Text edit and save
- [ ] Text compare

### A4. Image / Gallery

- [x] System.Drawing image decode
- [x] ImageSharp fallback decode
- [ ] Pfim DDS decode
- [ ] Thumbnail cache
- [ ] Gallery browsing
- [ ] Image export
- [ ] PNG optimize flow

### A5. SPR

- [x] list.spr parser candidate
- [x] SPR record mapping candidate
- [ ] SPR frame table parser
- [ ] Palette parser
- [ ] Direction/action parser
- [ ] Animation preview
- [ ] Frame export

### A6. PakViewer CLI

- [ ] list
- [ ] read
- [ ] export
- [ ] import
- [ ] info

## Next append target

```text
B. L1MapViewer absorption
```
