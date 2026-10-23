# Creates a Windows Desktop shortcut for Busk Desk.
# Prefer published BuskDesk.exe; otherwise point at scripts\run.ps1.
# Run from the BuskDesk repo (any directory):  .\scripts\create-desktop-shortcut.ps1

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$desktop = [Environment]::GetFolderPath("Desktop")
$lnkPath = Join-Path $desktop "Busk Desk.lnk"

$candidates = @(
    (Join-Path $root "publish\win-x64\BuskDesk.exe"),
    (Join-Path $root "publish\mock\BuskDesk.exe")
)

$target = $null
$workingDir = $null
$arguments = $null

foreach ($exe in $candidates) {
    if (Test-Path -LiteralPath $exe) {
        $target = $exe
        $workingDir = Split-Path -Parent $exe
        break
    }
}

if (-not $target) {
    $runPs1 = Join-Path $root "scripts\run.ps1"
    if (-not (Test-Path -LiteralPath $runPs1)) {
        throw "Neither publish\win-x64\BuskDesk.exe nor scripts\run.ps1 found under $root"
    }
    $target = Join-Path $env:SystemRoot "System32\WindowsPowerShell\v1.0\powershell.exe"
    $workingDir = $root
    $arguments = "-NoProfile -ExecutionPolicy Bypass -File `"$runPs1`""
}

$shell = New-Object -ComObject WScript.Shell
$shortcut = $shell.CreateShortcut($lnkPath)
$shortcut.TargetPath = $target
$shortcut.WorkingDirectory = $workingDir
if ($arguments) {
    $shortcut.Arguments = $arguments
}
$shortcut.WindowStyle = 1
$shortcut.Description = "Busk Desk — kleines Windows-DMX-Pult"
if ($target -like "*.exe" -and (Test-Path -LiteralPath $target)) {
    $shortcut.IconLocation = "$target,0"
}
$shortcut.Save()

Write-Host "Desktop-Verknuepfung erstellt: $lnkPath" -ForegroundColor Green
Write-Host "Ziel: $target$(if ($arguments) { " $arguments" })"
