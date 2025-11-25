using Whisper.net.Ggml;

namespace LocalAIRecorder.Services;

public static class WhisperModelProvider
{
    private const string ModelFileName = "ggml-base.bin";

    public static async Task<string> EnsureModelAsync()
    {
        var modelPath = Path.Combine(FileSystem.AppDataDirectory, ModelFileName);

        if (!File.Exists(modelPath))
        {
            Directory.CreateDirectory(FileSystem.AppDataDirectory);

            using var modelStream = await WhisperGgmlDownloader.Default.GetGgmlModelAsync(
                GgmlType.Base
            );

            using var fileWriter = File.OpenWrite(modelPath);
            await modelStream.CopyToAsync(fileWriter);
        }

        return modelPath;
    }
}