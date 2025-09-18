# Build Instructions

This document describes how to build ShutUpAndType for distribution.

## Quick Build - Portable Version ✅

**Ready to use!** Creates a self-contained ZIP package:

```powershell
powershell -ExecutionPolicy Bypass -File "scripts\build-portable.ps1"
```

**Output**:
- `bin\Release\ShutUpAndType-Portable.zip` (0.09MB)
- Contains `dotnet-whisper.exe` (0.16MB)
- Requires .NET 8.0 Runtime on target machine

## Distribution Options

Only portable ZIP distribution is currently supported. MSI installer has been removed due to WiX toolset complications.

## Manual Build Steps

### 1. Self-Contained Executable

```bash
# Clean previous builds
dotnet clean --configuration Release

# Publish self-contained for Windows x64
dotnet publish --configuration Release --runtime win-x64 --self-contained true
```

**Output**: `bin\Release\net8.0-windows\win-x64\publish\dotnet-whisper.exe`

### 2. Development Build

```bash
# Standard development build
dotnet build

# Run for testing
dotnet run
```

## Project Configuration

The project is configured for self-contained publishing:

```xml
<PropertyGroup>
  <SelfContained>true</SelfContained>
  <RuntimeIdentifier>win-x64</RuntimeIdentifier>
  <PublishSingleFile>true</PublishSingleFile>
</PropertyGroup>
```

## Distribution Files

- **Portable**: Single EXE + README in ZIP archive
- **Size**: ~0.16MB (requires .NET 8.0 Runtime)
- **Target**: Windows 10/11 x64

## Next Steps

1. ✅ Portable distribution ready
2. 📦 GitHub Actions workflow for automated releases
3. 🔄 Auto-updater integration