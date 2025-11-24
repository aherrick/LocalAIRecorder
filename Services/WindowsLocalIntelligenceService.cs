#if WINDOWS
using Ollama;

namespace LocalAIRecorder.Services;

public class WindowsLocalIntelligenceService : ILocalIntelligenceService
{
    private readonly OllamaApiClient _client;

    // Use a general-purpose Phi-3 style model by default; adjust to match your local Ollama tag.
    private const string DefaultModel = "phi3:mini";

    public WindowsLocalIntelligenceService()
    {
        // Uses default base URI (http://localhost:11434) unless overridden.
        _client = new OllamaApiClient();
    }

    public async Task<string> AskAsync(string prompt, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(prompt))
            return string.Empty;

        // Ensure the default model is available by pulling it once.
        await _client.Models.PullModelAsync(DefaultModel, stream: false, cancellationToken: cancellationToken)
            .EnsureSuccessAsync();

        // Simple one-shot completion using the default model.
        var completion = await _client.Completions
            .GenerateCompletionAsync(
                model: DefaultModel,
                prompt: prompt,
                stream: false,
                cancellationToken: cancellationToken)
            .WaitAsync();

        return completion.Response?.Trim() ?? string.Empty;
    }
}

#endif
