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
absorb-pak-idx-rewrite
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
- [x] Update file: same-size raw import service and UI
- [ ] Delete file
- [x] Delete file safe plan model
- [x] Same-size import
- [x] Rebuild PAK writer without IDX rewrite
- [x] Rebuild PAK with Classic28 IDX rewrite
- [ ] Rebuild PAK with non-Classic IDX rewrite
- [x] Backup / rollback core service
- [x] Backup / rollback UI
- [x] Write diagnostics

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

## C. Integration enhancement

### C1. UI Structure

- [ ] Split MainForm
- [ ] Split PakPanel
- [ ] Split SpritePanel
- [ ] Split MapPanel
- [ ] Shared status/log panel
- [ ] Recent files
- [ ] Korean UI label cleanup

### C2. Service Structure

- [ ] Command/service layer
- [ ] Diagnostics presenter unification
- [ ] Settings service
- [ ] Cache service
- [ ] Export service
- [ ] Error report service

### C3. Release Packaging

- [x] GitHub Actions build
- [x] Build artifact upload
- [ ] Version stamping
- [ ] Release zip layout
- [ ] Sample config
- [ ] Smoke test checklist

## D. Build validation

- [x] Main branch build success baseline
- [ ] Branch build after each absorption batch
- [ ] PR validation before merge
- [ ] Artifact download verification
- [ ] Runtime smoke test

## Next work target

```text
A2 Delete file actual rebuild flow UI
A2 Add file / full rebuild flow
```
