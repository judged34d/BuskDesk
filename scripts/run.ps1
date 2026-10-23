$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root "src\BuskDesk\BuskDesk.csproj"
$out = Join-Path $root "publish\mock"

Write-Host "Building Busk Desk..." -ForegroundColor Cyan
dotnet publish $project -c Release -r win-x64 --self-contained false -o $out

$exe = Join-Path $out "BuskDesk.exe"
Write-Host "Starting $exe" -ForegroundColor Green
Start-Process $exe -WorkingDirectory $out
