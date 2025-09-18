Hi. Please patch the **ShutUpAndType** (.NET 8 WinForms) project with the following changes:

1. **Services/IconService.cs**
   - The method `CreateMicrophoneIconFromICO()` currently looks for a non-existent resource `dotnet_whisper.microphone.ico`.
   - Update it to search `Assembly.GetExecutingAssembly().GetManifestResourceNames()` for any name ending with `.microphone.ico` (case-insensitive).
   - If none is found, throw a `FileNotFoundException("Embedded microphone.ico not found")`.
   - Load the icon from the matched resource stream.

2. **.github/workflows/release.yml**
   - The workflow currently verifies `WhisperVoiceRecorder-Portable.zip`, but the build script actually produces `ShutUpAndType-Portable.zip`.
   - Replace the filename check accordingly.
   - Keep the existing logic for size calculation and ✅/❌ messages.

3. **Project configuration consistency**
   - In `ShutUpAndType.csproj`, `SelfContained=true` is set. However, README and BUILD.md claim the app requires .NET Runtime.
   - Pick one consistent approach:
     - **(lighter ZIP, requires runtime):** change `<SelfContained>false</SelfContained>` in csproj and adjust the build script to `--self-contained false`. Then docs remain correct.

4. **AudioRecordingService.cs**
   - Currently writes recordings into `Directory.GetCurrentDirectory()`. Change this to `%TEMP%\ShutUpAndType\` (use `Path.GetTempPath()` + “ShutUpAndType”), ensure the directory exists.
   - Use a filename like `yyyyMMdd_HHmmss-recording.wav`.
   - Also, update the WaveFormat to 16 kHz, 16-bit, mono (`new WaveFormat(16000, 16, 1)`).

5. **Program.cs (keyboard hook)**
   - Right now toggling happens on `WM_KEYDOWN`, causing repeats if the key is held.
   - Change it to trigger only on `WM_KEYUP` (0x0101) to debounce.

6. **KeyboardSimulationService.cs**
   - The current clipboard handling clears and restores text, which breaks when clipboard contains non-text data (files, images, HTML).
   - Fix it so that if the clipboard did not originally contain text, don’t clear/restore it.

7. **Minor robustness:**
   - In Program.cs, ignore Scroll Lock presses while status is “Transcribing...” (to avoid race conditions).
   - Add error logging (e.g. `%LOCALAPPDATA%\ShutUpAndType\logs\`) for transcription/config failures instead of swallowing exceptions silently.

Make all these edits in one go.
