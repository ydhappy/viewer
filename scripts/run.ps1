$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
Set-Location $root

Write-Host "[viewer] run" -ForegroundColor Cyan
dotnet run --project .\src\Viewer.App\Viewer.App.csproj
