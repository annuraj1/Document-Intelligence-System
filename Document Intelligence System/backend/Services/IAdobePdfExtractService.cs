namespace DocumentIntelligence.Api.Services;

public interface IAdobePdfExtractService
{
    Task<string> ExtractTextAsync(string filePath, CancellationToken cancellationToken = default);
}
