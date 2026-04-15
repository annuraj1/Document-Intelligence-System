using DocumentIntelligence.Api.DTOs;

namespace DocumentIntelligence.Api.Services;

public interface IDocumentService
{
    Task<UploadResponseDto> UploadAsync(IFormFile file, string userId, CancellationToken cancellationToken = default);
    Task<string> ExtractAsync(string documentId, CancellationToken cancellationToken = default);
    Task<List<string>> AnalyzeAsync(string documentId, CancellationToken cancellationToken = default);
    Task<string> SummarizeAsync(string documentId, CancellationToken cancellationToken = default);
    Task<DocumentResponseDto?> GetDocumentAsync(string documentId, CancellationToken cancellationToken = default);
}
