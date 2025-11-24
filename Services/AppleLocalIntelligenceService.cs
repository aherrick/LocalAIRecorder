#if IOS || MACCATALYST
using CrossIntelligence;

namespace LocalAIRecorder.Services;

public class AppleLocalIntelligenceService : ILocalIntelligenceService
{
    private readonly IntelligenceSession _session;

    public AppleLocalIntelligenceService()
    {
        _session = new IntelligenceSession();
    }

    public async Task<string> AskAsync(string prompt, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(prompt))
            return string.Empty;

        var response = await _session.RespondAsync(prompt);
        return response ?? string.Empty;
    }
}

#endif
