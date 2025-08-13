# MES Lite — WorkOrder CRUD (WPF + WinForms + EF Core)

작업지시(WorkOrder) CRUD, 서버사이드 필터/페이지네이션, WPF(MVVM)·WinForms UI를 포함한 데스크톱 포트폴리오입니다. 공통 Domain/Data 레이어를 공유하여 유지보수성과 확장성을 확보했습니다.






## 목차
- [주요 기능](#주요-기능)
- [기술 스택](#기술-스택)
- [아키텍처](#아키텍처)
- [프로젝트 구조](#프로젝트-구조)
- [사전 준비](#사전-준비)
- [설정(appsettings.json)](#설정appsettingsjson)
- [DB 준비(마이그레이션/시드)](#db-준비마이그레이션시드)
- [실행(개발)](#실행개발)
- [배포/실행(릴리스)](#배포실행릴리스)
- [사용 방법](#사용-방법)
- [트러블슈팅](#트러블슈팅)
---

## 주요 기능
- WorkOrder CRUD (등록/수정/삭제/조회)
- 서버사이드 검색/필터(품목코드·상태·기간) + 페이지네이션
- 현재 페이지 건수/합계(WinForms 하단 표시)
- DI/구성(Host + appsettings.json), 재시도(EnableRetryOnFailure), 암호화 연결(Encrypt)

## 기술 스택
- C# 12, .NET 8
- WPF(MVVM, CommunityToolkit.Mvvm), WinForms
- EF Core + SQL Server(LocalDB 기본, MSSQL 전환 가능)
- Microsoft.Extensions.Hosting, Microsoft.Extensions.Configuration

## 아키텍처
- 레이어드 구조: `Domain`(엔티티) → `Data`(DbContext/Repository) → `WPF/WinForms`(UI)
- UI 동시성/비동기 안정화: `AddDbContextFactory` + `IDbContextFactory<T>`
- 구성 로드: `AppContext.BaseDirectory` 기반 `appsettings.json`

## 프로젝트 구조
```
Solution.Domain/
  Entities/WorkOrder.cs
Solution.Data/
  MesLiteDbContext.cs
  Repositories/
    IWorkOrderRepository.cs
    WorkOrderRepository.cs
  DesignTimeDbContextFactory.cs
  Migrations/
Solution/ (WPF)
  App.xaml, App.xaml.cs
  MainWindow.xaml, MainWindow.xaml.cs
  ViewModels/WorkOrdersViewModel.cs
  appsettings.json
Solution.WinForms/
  Program.cs
  WorkOrdersBoardForm.cs
  appsettings.json
Solution.Seed/  (더미 데이터 500건 삽입 콘솔)
README.md
```

## 사전 준비
- .NET 8 SDK
- SQL Server LocalDB 또는 MSSQL (Express/Developer/Server)
- Optional: Visual Studio or VS Code

## 설정(appsettings.json)
기본(LocalDB):
```json
{
  "ConnectionStrings": {
    "SqlServer": "Server=(localdb)\\MSSQLLocalDB;Database=MesLite;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True;"
  }
}
```
MSSQL(Windows 인증) 예시:
```
Server=localhost\SQLEXPRESS;Database=MesLite;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True;
```

## DB 준비(마이그레이션/시드)
```powershell
# (선택) LocalDB 시작
sqllocaldb start MSSQLLocalDB

# 마이그레이션 적용 (솔루션 루트)
dotnet ef database update -s .\Solution\Solution.csproj -p .\Solution.Data\Solution.Data.csproj

# (선택) 더미 500건 시드
dotnet run --project .\Solution.Seed\Solution.Seed.csproj
```

## 실행(개발)
- WPF
```powershell
dotnet run --project .\Solution\Solution.csproj
```
- WinForms (출력 폴더 권장 실행)
```powershell
Start-Process ".\Solution.WinForms\bin\Debug\net8.0-windows\Solution.WinForms.exe" -WorkingDirectory ".\Solution.WinForms\bin\Debug\net8.0-windows"
```

## 배포/실행(릴리스)
런타임 필요(가벼움):
```powershell
dotnet publish .\Solution\Solution.csproj -c Release -r win-x64 --self-contained false
dotnet publish .\Solution.WinForms\Solution.WinForms.csproj -c Release -r win-x64 --self-contained false
```
단일 파일(무설치):
```powershell
dotnet publish .\Solution\Solution.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
dotnet publish .\Solution.WinForms\Solution.WinForms.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```
전달물: 각 프로젝트 `bin\Release\net8.0-windows\win-x64\publish\` 폴더 내용
rue 유지

## 사용 방법
- WPF
  - 상단 필터(품목/상태/기간, 페이지크기) 설정 → 불러오기
  - ◀/▶로 페이지 이동, 신규/편집/삭제 후 저장
- WinForms
  - 상단 필터/페이지크기 → 검색
  - ◀/▶로 페이지 이동
  - 하단에 “건수(현재 페이지)” 표시

## 트러블슈팅
- 연결 경고/오류 → 연결문자열에 `Encrypt=True;TrustServerCertificate=True` 포함
- “A second operation was started” → DbContextFactory 사용(이미 적용)
- “ConnectionString property not initialized” → 실행 폴더 `appsettings.json` 확인
- 빌드 중 파일 잠김(MSB3021/3027) → 실행 프로세스 종료 후 재빌드:
```powershell
Get-Process Solution, Solution.WinForms -ErrorAction SilentlyContinue | Stop-Process -Force
```

