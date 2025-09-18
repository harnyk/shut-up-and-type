param(
    [string]$Configuration = "Release",
    [string]$Version = "1.0.0"
)

$ErrorActionPreference = "Stop"

$solutionDir = Split-Path $PSScriptRoot -Parent
$publishDir = "bin\$Configuration\net8.0-windows\win-x64\publish"
$installerDir = "installer"
$issFile = "$installerDir\setup.iss"

if (-not (Test-Path $issFile)) {
    Write-Error "Inno Setup script not found: $issFile"
    exit 1
}

# Ensure Inno Setup is installed
$isccPath = "C:\Program Files (x86)\Inno Setup 6\ISCC.exe"
if (-not (Test-Path $isccPath)) {
    Write-Error "Inno Setup not found at $isccPath. Please install Inno Setup 6."
    exit 1
}

# Build the .NET app first
Write-Host "Publishing ShutUpAndType..." -ForegroundColor Yellow
dotnet publish "$solutionDir\ShutUpAndType.csproj" `
    --configuration $Configuration `
    --runtime win-x64 `
    --self-contained false `
    --output $publishDir

if ($LASTEXITCODE -ne 0) {
    Write-Error "dotnet publish failed"
    exit 1
}

# Build installer
Write-Host "Building installer with Inno Setup..." -ForegroundColor Yellow
& $isccPath "/dMyAppVersion=$Version" $issFile

if ($LASTEXITCODE -eq 0) {
    Write-Host "`n✅ Installer built successfully!" -ForegroundColor Green
} else {
    Write-Error "Inno Setup compilation failed"
    exit 1
}
