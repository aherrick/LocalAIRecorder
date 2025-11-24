# LocalAIRecorder

![Build Status](https://github.com/YOUR_USERNAME/YOUR_REPO_NAME/actions/workflows/build.yml/badge.svg)

LocalAIRecorder is a cross-platform .NET MAUI application that allows you to record audio, transcribe it locally using Whisper, and chat with your recordings using on-device Large Language Models (LLMs).

## 🚀 Features

- **Local Audio Recording**: Capture high-quality audio directly within the app.
- **On-Device Transcription**: Uses [Whisper.net](https://github.com/sandrohanea/whisper.net) to transcribe audio completely offline. No data leaves your device.
- **AI-Powered Chat**: Ask questions about your recordings and get answers based on the transcript context.
- **Privacy Focused**: All processing (transcription and chat) is performed locally on your machine.

## 🛠 Tech Stack

- **Framework**: .NET MAUI (Multi-platform App UI)
- **Transcription**: Whisper.net (C# bindings for Whisper.cpp)
- **Local Intelligence**:
  - **Windows**: [Ollama](https://ollama.com/) (using `phi3:mini`)
  - **iOS / macOS**: Native Local Intelligence (via CrossIntelligence)

## 📋 Prerequisites

### Windows
1. Install [Ollama](https://ollama.com/download/windows).
2. Ensure Ollama is running in the background (`http://localhost:11434`).
   > The app will automatically pull the required model (`phi3:mini`) on first use.

### iOS / macOS
- A device capable of running local CoreML models.

## 🏗 Getting Started

1. Clone the repository.
2. Open the solution `LocalAIRecorder.sln` in Visual Studio or VS Code.
3. Restore NuGet packages.
4. Build and run the project for your desired platform (Windows, iOS, or MacCatalyst).

## 📱 Usage

1. **Record**: Tap the record button on the main page to start capturing audio.
2. **Transcribe**: Open the recording details and tap "Transcribe". The app will use the local Whisper model to convert speech to text.
3. **Chat**: Use the chat interface to ask questions about the transcript. The local LLM will analyze the text and provide answers.

## 📄 License

[MIT](LICENSE)
