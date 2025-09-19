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
- **IAudioRecordingService/AudioRecordingService**: NAudio-based microphone recording to WAV files
- **ITranscriptionService/WhisperTranscriptionService**: OpenAI Whisper API integration
- **IKeyboardSimulationService/KeyboardSimulationService**: Windows clipboard-based text input simulation
- **ISystemTrayService/SystemTrayService**: System tray icon and context menu
- **IAutostartService/AutostartService**: Windows startup registry management
- **SettingsForm**: Configuration UI for API key and autostart

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
2. Audio recording to temporary WAV files (8kHz, 8-bit mono)
3. File upload to OpenAI Whisper API for transcription
4. Text input via Windows clipboard simulation
5. Temporary file cleanup

### Constants
Application constants are centralized in `Constants.cs` (AppConstants class) for branding, file names, and UI text.