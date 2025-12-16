# Caffeine Installer Script

param(
    [switch]$Uninstall
)

$AppName = "Caffeine"
$InstallDir = "$env:LOCALAPPDATA\$AppName"
$ExeName = "Caffeine.exe"

if ($Uninstall) {
    # Remove installation folder
    if (Test-Path $InstallDir) {
        Remove-Item $InstallDir -Recurse -Force
        Write-Host "Folder removed: $InstallDir" -ForegroundColor Green
    }

    # Remove desktop shortcut
    $desktopFolder = [Environment]::GetFolderPath('Desktop')
    $desktopPath = "$desktopFolder\$AppName.lnk"
    if (Test-Path $desktopPath) {
        Remove-Item $desktopPath -Force
        Write-Host "Desktop shortcut removed" -ForegroundColor Green
    }

    Write-Host "`nUninstall complete!" -ForegroundColor Green
    exit 0
}

# Build
Write-Host "Building..." -ForegroundColor Cyan
dotnet publish "$PSScriptRoot\Caffeine" -c Release -o "$InstallDir" --nologo -v q
if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}
Write-Host "Build complete" -ForegroundColor Green

# Create desktop shortcut
$WshShell = New-Object -ComObject WScript.Shell
$desktopFolder = [Environment]::GetFolderPath('Desktop')
$desktopPath = "$desktopFolder\$AppName.lnk"
$Shortcut = $WshShell.CreateShortcut($desktopPath)
$Shortcut.TargetPath = "$InstallDir\$ExeName"
$Shortcut.WorkingDirectory = $InstallDir
$Shortcut.Save()
Write-Host "Desktop shortcut created" -ForegroundColor Green

Write-Host "`nInstallation complete!" -ForegroundColor Green
