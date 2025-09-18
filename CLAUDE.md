# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build and Run Commands

- **Build**: `dotnet build`
- **Run**: `dotnet run`
- **Clean**: `dotnet clean`
- **Build Portable**: `powershell -ExecutionPolicy Bypass -File "scripts\build-portable.ps1"`

## Architecture Overview

This is a .NET 8.0 Windows Forms application that provides global voice recording with OpenAI Whisper transcription. The architecture follows SOLID principles with dependency injection.

### Core Components

**Program.cs/MainForm**: Main application entry point containing:
- Windows API global keyboard hook for Scroll Lock detection
- MainForm class that orchestrates all services via dependency injection
- Application lifecycle management (startup, shutdown, tray operations)

**Services Layer**: All business logic is separated into services with interfaces:
- `IConfigurationService` - NPM-style config resolution searching up directory tree
- `IAudioRecordingService` - NAudio-based WAV recording (8kHz, 8-bit mono)
- `ITranscriptionService` - OpenAI Whisper API integration
- `ISystemTrayService` - System tray management with custom microphone icon
- `IKeyboardSimulationService` - Clipboard-based text input simulation

### Key Architectural Patterns

**Configuration Resolution**: Searches for `config.json` in this priority order:
1. `%APPDATA%\WhisperRecorder\config.json`
2. Next to executable
3. Current directory and up to 5 parent directories (npm-style)

**Event-Driven Architecture**: Services communicate via events:
- Recording completion triggers transcription
- Transcription completion triggers text typing
- Tray events trigger window show/hide/settings

**Resource Management**:
- Embedded ICO resource (`microphone.ico`) for tray icon
- Automatic audio file cleanup after transcription
- Proper disposal of Windows API hooks and audio resources

### Windows Integration

**Global Keyboard Hook**: Uses `SetWindowsHookEx` with `WH_KEYBOARD_LL` to capture Scroll Lock globally across all applications.

**System Tray**: Custom microphone icon with context menu (Show/Settings/Exit). Application runs hidden by default.

**Text Input**: Uses clipboard preservation technique - saves current clipboard, types via Ctrl+V, restores original clipboard.

## Configuration

The application automatically creates default config if none exists. Settings window allows GUI configuration of OpenAI API key.

Config format:
```json
{
  "OpenAI": {
    "ApiKey": "sk-your-key-here"
  }
}
```

## Dependencies

- **NAudio 2.2.1**: Audio recording from default microphone
- **.NET 8.0 Windows Forms**: UI framework
- **System.Text.Json**: Configuration serialization
- **Windows API**: Global keyboard hooks and text simulation

## Distribution

**Portable Version**: Lightweight 0.16MB executable requiring .NET 8.0 Runtime. Created via `scripts\build-portable.ps1` - generates ZIP package with single EXE and README.


**Self-Contained Publishing**: Configured for `win-x64` runtime with `PublishSingleFile=true` for minimal deployment footprint.