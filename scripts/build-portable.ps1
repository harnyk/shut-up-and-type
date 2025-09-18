# Simple portable build script for ShutUpAndType

param(
    [string]$Configuration = "Release"
)

# Set error action preference
$ErrorActionPreference = "Stop"

# Get script directory and solution root
$scriptDir = $PSScriptRoot
$solutionDir = Split-Path $scriptDir -Parent

Write-Host "Building ShutUpAndType portable version..." -ForegroundColor Green
Write-Host "Solution directory: $solutionDir" -ForegroundColor Gray

# Step 1: Clean previous builds
Write-Host "`n1. Cleaning previous builds..." -ForegroundColor Yellow
Set-Location $solutionDir
dotnet clean --configuration $Configuration

# Step 2: Publish framework-dependent application
Write-Host "`n2. Publishing framework-dependent application..." -ForegroundColor Yellow
$publishDir = "bin\$Configuration\net8.0-windows\win-x64\publish"
dotnet publish --configuration $Configuration --runtime win-x64 --self-contained false --output $publishDir

if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to publish application"
    exit 1
}

# Step 3: Create portable package
Write-Host "`n3. Creating portable package..." -ForegroundColor Yellow
$packageDir = "bin\$Configuration\ShutUpAndType-Portable"
$zipPath = "bin\$Configuration\ShutUpAndType-Portable.zip"

# Remove existing package
if (Test-Path $packageDir) {
    Remove-Item $packageDir -Recurse -Force
}
if (Test-Path $zipPath) {
    Remove-Item $zipPath -Force
}

# Create package directory
New-Item -ItemType Directory -Force -Path $packageDir | Out-Null

# Copy main executable
Copy-Item "$publishDir\dotnet-whisper.exe" $packageDir

# Create README for portable version
$readmeContent = @"
# ShutUpAndType - Portable Version

## Quick Start
1. Run dotnet-whisper.exe
2. Right-click system tray icon -> Settings
3. Enter your OpenAI API key
4. Press Scroll Lock to record voice
5. Transcribed text will be typed automatically

## Configuration
The application will create config.json in:
%APPDATA%\ShutUpAndType\config.json

## Requirements
- Windows 10/11
- .NET 8.0 Runtime (download from https://dotnet.microsoft.com/download/dotnet/8.0)
- OpenAI API key
- Microphone

Version: 1.0.0
"@

Set-Content -Path "$packageDir\README.txt" -Value $readmeContent -Encoding UTF8

# Create ZIP archive
Add-Type -AssemblyName System.IO.Compression.FileSystem
[System.IO.Compression.ZipFile]::CreateFromDirectory($packageDir, $zipPath)

# Step 4: Success message
Write-Host "`n✅ Build completed successfully!" -ForegroundColor Green
Write-Host "Portable package created: $zipPath" -ForegroundColor Green

# Show file sizes
$exeFile = "$packageDir\dotnet-whisper.exe"
$zipFile = $zipPath

if (Test-Path $exeFile) {
    $exeSize = [math]::Round((Get-Item $exeFile).Length / 1MB, 2)
    Write-Host "Executable size: $exeSize MB" -ForegroundColor Gray
}

if (Test-Path $zipFile) {
    $zipSize = [math]::Round((Get-Item $zipFile).Length / 1MB, 2)
    Write-Host "ZIP package size: $zipSize MB" -ForegroundColor Gray
}