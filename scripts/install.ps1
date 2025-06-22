# ContextKeeper Installation Script for Windows

$ErrorActionPreference = "Stop"

$InstallDir = if ($env:CONTEXTKEEPER_INSTALL_DIR) { $env:CONTEXTKEEPER_INSTALL_DIR } else { "$env:USERPROFILE\.contextkeeper" }
$ReleaseUrl = "https://github.com/chasecupp43/contextkeeper-mcp/releases/latest"
$BinName = "contextkeeper"

Write-Host "🚀 Installing ContextKeeper..." -ForegroundColor Green
Write-Host "Installation directory: $InstallDir"

# Create installation directory
New-Item -ItemType Directory -Force -Path $InstallDir | Out-Null

# Detect architecture
$Arch = if ([Environment]::Is64BitOperatingSystem) { "x64" } else { "x86" }
$BinaryName = "${BinName}-win-${Arch}.zip"

# Download latest release
Write-Host "📥 Downloading ContextKeeper for Windows-$Arch..." -ForegroundColor Cyan
$DownloadUrl = "${ReleaseUrl}/download/${BinaryName}"
$ZipPath = "$env:TEMP\contextkeeper.zip"

try {
    Invoke-WebRequest -Uri $DownloadUrl -OutFile $ZipPath -UseBasicParsing
} catch {
    Write-Host "❌ Failed to download ContextKeeper: $_" -ForegroundColor Red
    exit 1
}

# Extract archive
Write-Host "📦 Extracting files..." -ForegroundColor Cyan
Expand-Archive -Path $ZipPath -DestinationPath $InstallDir -Force
Remove-Item $ZipPath

# Add to PATH
$UserPath = [Environment]::GetEnvironmentVariable("Path", "User")
if ($UserPath -notlike "*$InstallDir*") {
    [Environment]::SetEnvironmentVariable("Path", "$UserPath;$InstallDir", "User")
    Write-Host "✅ Added ContextKeeper to PATH" -ForegroundColor Green
    Write-Host "ℹ️  Please restart your terminal for PATH changes to take effect" -ForegroundColor Yellow
} else {
    Write-Host "ℹ️  ContextKeeper already in PATH" -ForegroundColor Yellow
}

# Check for project in current directory
$CurrentDir = Get-Location
if ((Test-Path "CLAUDE.md") -or (Test-Path "README.md") -or (Test-Path "docs")) {
    Write-Host ""
    Write-Host "📁 Detected project in current directory" -ForegroundColor Cyan
    $response = Read-Host "Initialize ContextKeeper here? (y/N)"
    if ($response -eq 'y' -or $response -eq 'Y') {
        & "$InstallDir\$BinName.exe" init
    }
}

Write-Host ""
Write-Host "✅ ContextKeeper installed successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "To get started:" -ForegroundColor Cyan
Write-Host "  1. Restart your terminal"
Write-Host "  2. Run: contextkeeper --help"
Write-Host "  3. Initialize a project: contextkeeper init"
Write-Host ""
Write-Host "For MCP integration with Claude:" -ForegroundColor Cyan
Write-Host "  claude mcp add contextkeeper -- $InstallDir\$BinName.exe"
Write-Host ""