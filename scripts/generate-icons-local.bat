@echo off
echo 🎨 ShutUpAndType Local Icon Generation
echo =====================================
echo.

REM Check if Node.js is available
where node >nul 2>nul
if %errorlevel% neq 0 (
    echo ❌ Node.js not found. Please install Node.js first.
    echo    Download from: https://nodejs.org/
    pause
    exit /b 1
)

REM Check if source SVG exists
if not exist "assets\icons\source\microphone.svg" (
    echo ❌ Source SVG not found: assets\icons\source\microphone.svg
    pause
    exit /b 1
)

echo ℹ️  Installing dependencies...
npm install

echo ℹ️  Running icon generation...
npm run build-icons

if %errorlevel% neq 0 (
    echo ❌ Icon generation failed
    pause
    exit /b 1
)

echo.
echo 🎉 Icon generation finished!
echo.
echo 💡 To validate icons, run: .\scripts\validate-icons.ps1
echo.
pause