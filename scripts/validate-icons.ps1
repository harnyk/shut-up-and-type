# Icon Validation Script for ShutUpAndType
# This script validates icon files and checks for proper sizing and formatting

param(
    [string]$IconPath = "assets/icons",
    [switch]$Verbose
)

Write-Host "🎨 ShutUpAndType Icon Validation" -ForegroundColor Cyan
Write-Host "=================================" -ForegroundColor Cyan

$errorCount = 0
$warningCount = 0

function Write-Status {
    param([string]$Message, [string]$Type = "Info")

    switch ($Type) {
        "Error" {
            Write-Host "❌ $Message" -ForegroundColor Red
            $script:errorCount++
        }
        "Warning" {
            Write-Host "⚠️  $Message" -ForegroundColor Yellow
            $script:warningCount++
        }
        "Success" { Write-Host "✅ $Message" -ForegroundColor Green }
        "Info" { Write-Host "ℹ️  $Message" -ForegroundColor Blue }
    }
}

function Test-IconFile {
    param([string]$FilePath, [int[]]$ExpectedSizes)

    if (-not (Test-Path $FilePath)) {
        Write-Status "Icon file not found: $FilePath" "Error"
        return $false
    }

    $fileInfo = Get-Item $FilePath
    Write-Status "Found $($fileInfo.Name) ($($fileInfo.Length) bytes)"

    # Check file size (warn if too large)
    $maxSize = 1MB
    if ($fileInfo.Length -gt $maxSize) {
        Write-Status "Icon file is quite large ($($fileInfo.Length) bytes). Consider optimization." "Warning"
    }

    return $true
}

function Test-PngIcons {
    $pngPath = Join-Path $IconPath "generated"
    $expectedSizes = @(16, 20, 24, 32, 40, 48, 64, 96, 128, 256, 512)

    Write-Status "Checking PNG icons in $pngPath"

    if (-not (Test-Path $pngPath)) {
        Write-Status "PNG icons directory not found: $pngPath" "Warning"
        Write-Status "Run CI workflow to generate PNG icons" "Info"
        return
    }

    foreach ($size in $expectedSizes) {
        $filename = "microphone-${size}x${size}.png"
        $filepath = Join-Path $pngPath $filename

        if (Test-Path $filepath) {
            Write-Status "Found PNG: ${size}x${size}" "Success"
        } else {
            Write-Status "Missing PNG: ${size}x${size}" "Warning"
        }
    }
}

function Test-IcoIcons {
    $icoPath = Join-Path $IconPath "ico"

    Write-Status "Checking ICO icons in $icoPath"

    if (-not (Test-Path $icoPath)) {
        Write-Status "ICO icons directory not found: $icoPath" "Warning"
        Write-Status "Run CI workflow to generate ICO icons" "Info"
        return
    }

    # Check main application icon
    $mainIcoPath = Join-Path $icoPath "microphone.ico"
    Test-IconFile $mainIcoPath @(16, 32, 48, 256)

    # Check system tray icon
    $trayIcoPath = Join-Path $icoPath "microphone-tray.ico"
    Test-IconFile $trayIcoPath @(16, 20, 24, 32)
}

function Test-SourceIcon {
    $sourcePath = Join-Path $IconPath "source/microphone.svg"

    Write-Status "Checking source SVG"

    if (-not (Test-Path $sourcePath)) {
        Write-Status "Source SVG not found: $sourcePath" "Error"
        return
    }

    $svgContent = Get-Content $sourcePath -Raw

    # Basic SVG validation
    if ($svgContent -match '<svg[^>]*width="(\d+)"[^>]*height="(\d+)"') {
        $width = $matches[1]
        $height = $matches[2]

        if ($width -eq $height) {
            Write-Status "Source SVG is square (${width}x${height})" "Success"
        } else {
            Write-Status "Source SVG is not square (${width}x${height}). Icons should be square." "Warning"
        }

        if ([int]$width -ge 512) {
            Write-Status "Source SVG has good resolution ($width px)" "Success"
        } else {
            Write-Status "Source SVG resolution is low ($width px). Consider using 512px or higher." "Warning"
        }
    } else {
        Write-Status "Could not parse SVG dimensions" "Warning"
    }

    # Check for vector elements
    if ($svgContent -match '<(rect|circle|ellipse|path|polygon)') {
        Write-Status "SVG contains vector elements" "Success"
    } else {
        Write-Status "SVG may not contain proper vector graphics" "Warning"
    }
}

function Test-ProjectIntegration {
    Write-Status "Checking project integration"

    # Check if main icon exists in root
    $rootIcon = "microphone.ico"
    if (Test-Path $rootIcon) {
        Write-Status "Main application icon found in root" "Success"
    } else {
        Write-Status "Main application icon missing from root" "Error"
    }

    # Check project file
    $projFile = "ShutUpAndType.csproj"
    if (Test-Path $projFile) {
        $projContent = Get-Content $projFile -Raw

        if ($projContent -match '<ApplicationIcon>microphone\.ico</ApplicationIcon>') {
            Write-Status "Application icon configured in project file" "Success"
        } else {
            Write-Status "Application icon not configured in project file" "Warning"
        }

        if ($projContent -match '<EmbeddedResource Include="microphone\.ico"') {
            Write-Status "Icon embedded as resource" "Success"
        } else {
            Write-Status "Icon not embedded as resource" "Error"
        }
    }
}

# Run validation tests
Write-Host ""
Test-SourceIcon
Write-Host ""
Test-PngIcons
Write-Host ""
Test-IcoIcons
Write-Host ""
Test-ProjectIntegration

# Summary
Write-Host ""
Write-Host "📊 Validation Summary" -ForegroundColor Cyan
Write-Host "=====================" -ForegroundColor Cyan

if ($errorCount -eq 0 -and $warningCount -eq 0) {
    Write-Status "All icon validations passed! 🎉" "Success"
    exit 0
} elseif ($errorCount -eq 0) {
    Write-Status "$warningCount warning(s) found. Icons should work but could be improved." "Warning"
    exit 0
} else {
    Write-Status "$errorCount error(s) and $warningCount warning(s) found." "Error"
    Write-Host ""
    Write-Host "💡 To fix issues:" -ForegroundColor Yellow
    Write-Host "  1. Ensure source SVG exists in assets/icons/source/"
    Write-Host "  2. Run GitHub Actions workflow to generate icons"
    Write-Host "  3. Check that generated icons are committed to repository"
    exit 1
}