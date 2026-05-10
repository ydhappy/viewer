# viewer

`viewer`는 `PakViewer`와 `L1MapViewer`의 기능을 하나의 Windows 데스크톱 뷰어로 통합하기 위한 새 프로젝트입니다.

## 통합 대상

- `tony1223/PakViewer`: IDX/PAK 리소스 목록 조회, 추출, SPR/DAT/이미지/텍스트 계열 뷰어 구조 참고
- `tony1223/L1MapViewer`: S32 지도 파일 분석, 지도 폴더 스캔, 타일/레이어 기반 지도 뷰어 구조 참고

## 1차 목표

큰 원본을 한 번에 복사하지 않고, 먼저 실행 가능한 통합 뼈대를 구성합니다.

- Windows Forms 통합 GUI
- PAK/IDX 탭
- S32 Map 탭
- 로그 탭
- UTF-8 기본 설정
- 추후 원본 기능을 모듈별로 흡수할 수 있는 구조

## 빌드

Windows + .NET SDK 환경에서 실행합니다.

```bash
dotnet build src/Viewer.App/Viewer.App.csproj
dotnet run --project src/Viewer.App/Viewer.App.csproj
```

## 현재 구조

```text
src/Viewer.App/
├─ Viewer.App.csproj
├─ Program.cs
├─ MainForm.cs
├─ Pak/
│  ├─ IdxRecord.cs
│  ├─ IdxParser.cs
│  └─ PakExtractor.cs
└─ Map/
   ├─ S32Info.cs
   └─ S32Analyzer.cs
```

## 진행 방식

1. 1차: 실행 가능한 통합 뼈대
2. 2차: PakViewer IDX/PAK 포맷 세부 처리 흡수
3. 3차: SPR/IMG/TIL 미리보기 추가
4. 4차: L1MapViewer S32 렌더링/레이어 처리 흡수
5. 5차: 편집/저장/Export 기능 추가
