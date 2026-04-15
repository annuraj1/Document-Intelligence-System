using DocumentIntelligence.Api.Models;

namespace DocumentIntelligence.Api.Repositories;

public interface IDocumentRepository
{
    Task<Document> CreateDocumentAsync(Document document, CancellationToken cancellationToken = default);
    Task<Document?> GetDocumentByIdAsync(string documentId, CancellationToken cancellationToken = default);
    Task UpdateDocumentStatusAsync(string documentId, string status, CancellationToken cancellationToken = default);
    Task UpsertExtractedContentAsync(ExtractedContent content, CancellationToken cancellationToken = default);
    Task<ExtractedContent?> GetExtractedContentByDocumentIdAsync(string documentId, CancellationToken cancellationToken = default);
    Task InsertSummaryAsync(Summary summary, CancellationToken cancellationToken = default);
    Task<List<Summary>> GetSummariesByDocumentIdAsync(string documentId, CancellationToken cancellationToken = default);
}
