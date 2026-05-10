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

## B. L1MapViewer absorption

### B1. S32 Core

- [x] S32 open
- [x] S32 folder scan
- [x] coordinate candidate extraction
- [x] Layer1 sample parser
- [ ] full S32 file parser
- [ ] Layer1 full parser
- [ ] Layer2 parser
- [ ] Layer3 parser
- [ ] Layer4 object parser
- [ ] Layer5 parser
- [ ] Layer7 parser

### B2. Tile Rendering

- [x] Tile.idx / Tile.pak resource set
- [x] tile id lookup candidate
- [x] L1 TIL block parser candidate
- [x] TIL block/sheet preview candidate
- [ ] TBT metadata parser
- [ ] IMG decoder
- [ ] Tile ID to TIL block mapping
- [ ] accurate isometric tile placement
- [ ] tile cache refresh/invalidation

### B3. Map Viewer UX

- [x] ColorGrid render mode
- [x] IsoTile render mode candidate
- [x] zoom in/out/reset
- [x] middle mouse pan
- [x] hover/select candidate
- [x] viewport clipping
- [ ] minimap
- [ ] layer visibility toggle
- [ ] object overlay
- [ ] viewport virtualization hardening

### B4. Map Editing

- [ ] Layer1 edit
- [ ] Layer2 edit
- [ ] Layer3 edit
- [ ] Layer4 object select/delete
- [ ] Layer5 edit
- [ ] Layer7 edit
- [ ] undo/redo
- [ ] batch operation
- [ ] save S32

### B5. Export / CLI / Benchmark

- [x] PNG snapshot candidate
- [ ] accurate map PNG export
- [ ] CLI info
- [ ] CLI extract-tile
- [ ] CLI render-adjacent
- [ ] benchmark-viewport
- [ ] benchmark-minimap
- [ ] benchmark-thumbnails

## Next append target

```text
C. Integration enhancement
D. Build validation
```
