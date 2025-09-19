# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**ShutUpAndType** is a Windows voice recording application with automatic speech-to-text transcription using OpenAI Whisper API. The app provides global hotkey recording (Scroll Lock) and types transcribed text directly into any focused application.

## Development Commands

### Build and Run
```bash
# Standard build
dotnet build

# Release build
dotnet build -c Release

# Run the application (dev mode)
dotnet run

# Publish single-file executable
dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true
```

### Installer
The project includes an Inno Setup installer script:
- Script: `installer/setup.iss`
- Build output from: `bin/Release/net8.0-windows/win-x64/publish/`

## Architecture

### Core Components
- **MainForm** (`Program.cs`): Main application entry point with global keyboard hook for Scroll Lock detection
- **Services**: Dependency-injected services implementing the core functionality

### Service Layer (Services/)
- **IConfigurationService/ConfigurationService**: Handles config.json loading with npm-style resolution (AppData, executable dir, current dir + 5 parents)
- **IAudioRecordingService/AudioRecordingService**: Thread-safe NAudio-based microphone recording to WAV files
- **ITranscriptionService/WhisperTranscriptionService**: OpenAI Whisper API integration
- **IKeyboardSimulationService/KeyboardSimulationService**: Windows clipboard-based text input simulation
- **ISystemTrayService/SystemTrayService**: System tray icon and context menu
- **IAutostartService/AutostartService**: Windows startup registry management
- **IApplicationStateService/ApplicationStateService**: Thread-safe application state management with atomic transitions
- **SettingsForm**: Configuration UI for API key and autostart
- **IconService**: Microphone icon management from embedded resources

### Configuration
- Config file: `config.json`
- Search order: `%APPDATA%\ShutUpAndType\config.json` → executable directory → npm-style directory traversal
- Required: OpenAI API key for Whisper service

### Key Dependencies
- **Target Framework**: .NET 8.0 Windows (`net8.0-windows`)
- **UI Framework**: Windows Forms (`UseWindowsForms: true`)
- **Audio Library**: NAudio 2.2.1 for microphone recording
- **API Integration**: Built-in HTTP client for OpenAI Whisper API

### Application Flow
1. Global Scroll Lock hook detection via Win32 API
2. Thread-safe state management through ApplicationStateService
3. Audio recording to temporary WAV files (8kHz, 8-bit mono)
4. File upload to OpenAI Whisper API for transcription
5. Text input via Windows clipboard simulation
6. Temporary file cleanup

### State Management
The application uses a comprehensive state machine with the following states:
- **Idle**: Ready to start recording
- **Recording**: Audio is being captured
- **Processing**: Audio file is being prepared
- **Transcribing**: Sending to Whisper API
- **TranscriptionComplete**: Success window with typed text
- **Error**: Error state with automatic recovery

State transitions are atomic and thread-safe, preventing race conditions.

### Single Instance Application
The application ensures only one instance runs per user using a named Mutex (`ShutUpAndType_SingleInstance_Mutex`).

### Constants
Application constants are centralized in `Constants.cs` (AppConstants class) for branding, file names, and UI text.

## Release Workflow

### Manual Versioning Process
The project uses manual version management with GitHub Actions automation:
1. **Development**: Make commits to `master` branch
2. **Version Update**: Manually update version in `ShutUpAndType.csproj`:
   ```xml
   <AssemblyVersion>1.0.X</AssemblyVersion>
   <FileVersion>1.0.X</FileVersion>
   ```
3. **Create Release Tag**:
   ```bash
   git tag v1.0.X
   git push origin v1.0.X
   ```
4. **Automated CI**: GitHub Actions automatically:
   - Extracts version from Git tag
   - Updates `.csproj` file with tag version
   - Builds portable ZIP and installer EXE
   - Creates GitHub release with artifacts

### GitHub Actions Workflow
- **Trigger**: Push of version tags (`v*.*.*`)
- **Output**:
  - `ShutUpAndType-Portable.zip`
  - `ShutUpAndTypeSetup.exe` (Inno Setup installer)
- **Automatic version sync** between Git tags and compiled binaries