namespace LocalAIRecorder.Services;

public interface ILocalIntelligenceService
{
    Task<string> AskAsync(string prompt, CancellationToken cancellationToken = default);
}
