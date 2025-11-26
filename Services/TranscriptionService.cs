using Whisper.net;

namespace LocalAIRecorder.Services;

public static class TranscriptionService
{
    public static async Task<string> TranscribeAsync(string audioFilePath, string modelPath)
    {
        if (!File.Exists(modelPath))
        {
            throw new FileNotFoundException("Model file not found", modelPath);
        }

        if (!File.Exists(audioFilePath))
        {
            throw new FileNotFoundException("Audio file not found", audioFilePath);
        }

        await using var whisperFactory = WhisperFactory.FromPath(modelPath);
        await using var processor = whisperFactory.CreateBuilder().WithLanguage("auto").Build();

        await using var fileStream = File.OpenRead(audioFilePath);

        // Whisper.net expects a WAV file with specific format (16kHz, 16-bit, mono)
        // The fileStream here is just the raw bytes of the file.
        // If the file is a valid WAV, Whisper.net can process it directly if we don't use WaveFileReader wrapper
        // BUT using WaveFileReader is safer if we want to ensure we are reading audio data correctly.
        // However, the provided snippet uses ProcessAsync(fileStream) directly which assumes raw PCM or WAV handling.
        // Let's stick to the simple approach first, but if it fails we might need to strip WAV headers.

        var transcript = "";
        await foreach (var result in processor.ProcessAsync(fileStream))
        {
            transcript += result.Text + " ";
        }

        return transcript.Trim();
    }
}