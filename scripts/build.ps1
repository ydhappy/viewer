$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
Set-Location $root

Write-Host "[viewer] restore" -ForegroundColor Cyan
dotnet restore .\Viewer.sln

Write-Host "[viewer] build" -ForegroundColor Cyan
dotnet build .\Viewer.sln -c Release --no-restore

Write-Host "[viewer] build completed" -ForegroundColor Green
