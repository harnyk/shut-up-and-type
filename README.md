# ShutUpAndType

🎤 A lightweight Windows voice recording application with automatic speech-to-text transcription using OpenAI Whisper API.

## Features

✅ **Global hotkey** - Record using Scroll Lock from any application
✅ **Real-time transcription** - Automatic speech recognition via OpenAI Whisper
✅ **Smart text input** - Types transcribed text directly into any focused application
✅ **Always on top** - Stays visible while working
✅ **Low resource usage** - Records in low-quality mono WAV (8kHz, 8-bit)
✅ **Auto cleanup** - Deletes audio files after transcription
✅ **Smart config** - Finds configuration files like npm (searches up directory tree)

## Quick Start

1. **Download** the latest release or build from source
2. **Configure API key** - Create `config.json` (see Configuration section)
3. **Run** the application
4. **Record** - Press `Scroll Lock` to start/stop recording
5. **Type** - Transcribed text automatically appears in your focused application

## Configuration

The application searches for `config.json` in this order:
1. `%APPDATA%\ShutUpAndType\config.json` (recommended)
2. Next to the executable file
3. Current directory and up to 5 parent directories (npm-style resolution)

### Example config.json:
```json
{
  "OpenAI": {
    "ApiKey": "sk-your-openai-api-key-here"
  }
}
```

## How It Works

1. **Press Scroll Lock** → Starts recording from default microphone
2. **Press Scroll Lock again** → Stops recording and uploads to OpenAI Whisper
3. **Text appears** → Transcribed text is automatically typed via clipboard (preserves original clipboard)
4. **File cleanup** → Temporary audio file is deleted

## Requirements

- Windows 10/11
- .NET 8.0 Runtime
- OpenAI API key with Whisper access
- Microphone

## Building from Source

```bash
git clone https://github.com/harnyk/shut-up-and-type.git
cd shut-up-and-type
dotnet build
```

## Technical Details

- **Framework**: .NET 8.0 Windows Forms
- **Audio**: NAudio for microphone capture
- **Format**: WAV 8kHz 8-bit mono (minimal file size)
- **API**: OpenAI Whisper-1 model
- **Input**: Windows API keyboard simulation via clipboard

## Use Cases

- 📝 **Dictating code comments** while programming
- 📧 **Voice-to-text in emails** and documents
- 🌐 **Filling web forms** with voice input
- 💬 **Chat applications** and messaging
- 📋 **Any text input field** in Windows

## License

MIT License - feel free to modify and distribute

## Contributing

Pull requests welcome! This was built as a practical tool for voice-driven text input.

---

*Built with ❤️ for developers who prefer talking to typing*