#if ANDROID
namespace LocalAIRecorder.Services;

/// <summary>
/// Fallback implementation for Android. Local on-device AI is not yet supported on Android.
/// Returns a helpful message indicating the limitation.
/// </summary>
public class AndroidLocalIntelligenceService : ILocalIntelligenceService
{
    public Task<string> AskAsync(string prompt, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(prompt))
            return Task.FromResult(string.Empty);

        return Task.FromResult(
            "Local AI chat is not yet available on Android. " +
            "This feature requires on-device AI capabilities that are currently only supported on iOS/macOS (Apple Intelligence) and Windows (Ollama)."
        );
    }
}
#endif
