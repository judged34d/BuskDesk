$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root "src\BuskDesk\BuskDesk.csproj"
$out = Join-Path $root "publish\win-x64"

Write-Host "Publishing self-contained Busk Desk (win-x64)..." -ForegroundColor Cyan
dotnet publish $project -c Release -r win-x64 --self-contained true `
  -p:PublishSingleFile=false `
  -p:IncludeNativeLibrariesForSelfExtract=true `
  -o $out

Write-Host "Fertig: $out\BuskDesk.exe" -ForegroundColor Green
Write-Host "Diesen Ordner kannst du kopieren; .NET Runtime ist enthalten."
