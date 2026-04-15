namespace DocumentIntelligence.Api.Services;

public interface IGeminiService
{
    Task<string> GenerateSummaryAsync(string rawText, CancellationToken cancellationToken = default);
}
