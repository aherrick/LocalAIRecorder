using System.Text;
using Whisper.net;

namespace LocalAIRecorder.Services;

public class WhisperService : IAsyncDisposable
{
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private WhisperFactory? _factory;
    private bool _initialized;
    private string? _modelPath;

    public async Task InitializeAsync()
    {
        if (_initialized)
            return;

        await _initLock.WaitAsync();

        try
        {
            if (_initialized)
                return;

            _modelPath = await WhisperModelProvider.EnsureModelAsync();
            _factory = WhisperFactory.FromPath(_modelPath);
            _initialized = true;
        }
        finally
        {
            _initLock.Release();
        }
    }

    public async Task<string> TranscribeWavFileAsync(string wavPath, string language = "en")
    {
        if (!_initialized)
            await InitializeAsync();

        if (_factory == null)
            throw new InvalidOperationException("Whisper factory not initialized.");

        using var processor = _factory
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

    public ValueTask DisposeAsync()
    {
        _factory?.Dispose();
        _initLock.Dispose();
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }
}
