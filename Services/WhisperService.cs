using System.Text;
using Whisper.net;
using Whisper.net.Ggml;

namespace LocalAIRecorder.Services;

public class WhisperService
{
    private const string ModelFileName = "ggml-base.bin";

    public async Task EnsureModelAsync()
    {
        var modelPath = Path.Combine(FileSystem.AppDataDirectory, ModelFileName);

        if (!File.Exists(modelPath))
        {
            Directory.CreateDirectory(FileSystem.AppDataDirectory);

            using var modelStream = await WhisperGgmlDownloader.Default.GetGgmlModelAsync(GgmlType.Base);
            using var fileWriter = File.OpenWrite(modelPath);
            await modelStream.CopyToAsync(fileWriter);
        }
    }

    public async Task<string> TranscribeWavFileAsync(string wavPath, string language = "en")
    {
        var modelPath = Path.Combine(FileSystem.AppDataDirectory, ModelFileName);
        
        if (!File.Exists(modelPath))
            await EnsureModelAsync();

        using var factory = WhisperFactory.FromPath(modelPath);
        using var processor = factory
            .CreateBuilder()
            .WithLanguage(language)
            .WithThreads(Environment.ProcessorCount)
            .Build();

        await using var fileStream = File.OpenRead(wavPath);

        var sb = new StringBuilder();
        await foreach (var segment in processor.ProcessAsync(fileStream))
        {
            sb.Append(segment.Text);
            sb.Append(' ');
        }

        return sb.ToString().Trim();
    }
}