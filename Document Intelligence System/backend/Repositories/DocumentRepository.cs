using DocumentIntelligence.Api.Configuration;
using DocumentIntelligence.Api.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Collections.Concurrent;

namespace DocumentIntelligence.Api.Repositories;

public sealed class DocumentRepository : IDocumentRepository
{
    private readonly IMongoCollection<Document> _documents;
    private readonly IMongoCollection<ExtractedContent> _extractedContents;
    private readonly IMongoCollection<Summary> _summaries;
    private volatile bool _useInMemoryStore;

    private static readonly ConcurrentDictionary<string, Document> InMemoryDocuments = new();
    private static readonly ConcurrentDictionary<string, ExtractedContent> InMemoryExtracted = new();
    private static readonly ConcurrentDictionary<string, List<Summary>> InMemorySummaries = new();

    public DocumentRepository(IMongoClient mongoClient, IOptions<MongoDbSettings> settings)
    {
        var db = mongoClient.GetDatabase(settings.Value.DatabaseName);
        _documents = db.GetCollection<Document>(settings.Value.DocumentsCollectionName);
        _extractedContents = db.GetCollection<ExtractedContent>(settings.Value.ExtractedContentCollectionName);
        _summaries = db.GetCollection<Summary>(settings.Value.SummariesCollectionName);
    }

    public async Task<Document> CreateDocumentAsync(Document document, CancellationToken cancellationToken = default)
    {
        if (!_useInMemoryStore)
        {
            try
            {
                await _documents.InsertOneAsync(document, cancellationToken: cancellationToken);
                return document;
            }
            catch
            {
                _useInMemoryStore = true;
            }
        }

        InMemoryDocuments[document.Id] = document;
        return document;
    }

    public async Task<Document?> GetDocumentByIdAsync(string documentId, CancellationToken cancellationToken = default)
    {
        if (!_useInMemoryStore)
        {
            try
            {
                return await _documents.Find(x => x.Id == documentId).FirstOrDefaultAsync(cancellationToken);
            }
            catch
            {
                _useInMemoryStore = true;
            }
        }

        InMemoryDocuments.TryGetValue(documentId, out var document);
        return document;
    }

    public async Task UpdateDocumentStatusAsync(string documentId, string status, CancellationToken cancellationToken = default)
    {
        if (!_useInMemoryStore)
        {
            try
            {
                var update = Builders<Document>.Update
                    .Set(x => x.Status, status)
                    .Set(x => x.UpdatedAtUtc, DateTime.UtcNow);
                await _documents.UpdateOneAsync(x => x.Id == documentId, update, cancellationToken: cancellationToken);
                return;
            }
            catch
            {
                _useInMemoryStore = true;
            }
        }

        if (InMemoryDocuments.TryGetValue(documentId, out var document))
        {
            document.Status = status;
            document.UpdatedAtUtc = DateTime.UtcNow;
        }
    }

    public async Task UpsertExtractedContentAsync(ExtractedContent content, CancellationToken cancellationToken = default)
    {
        if (!_useInMemoryStore)
        {
            try
            {
                await _extractedContents.ReplaceOneAsync(
                    x => x.DocumentId == content.DocumentId,
                    content,
                    new ReplaceOptions { IsUpsert = true },
                    cancellationToken);
                return;
            }
            catch
            {
                _useInMemoryStore = true;
            }
        }

        InMemoryExtracted[content.DocumentId] = content;
    }

    public async Task<ExtractedContent?> GetExtractedContentByDocumentIdAsync(string documentId, CancellationToken cancellationToken = default)
    {
        if (!_useInMemoryStore)
        {
            try
            {
                return await _extractedContents
                    .Find(x => x.DocumentId == documentId)
                    .FirstOrDefaultAsync(cancellationToken);
            }
            catch
            {
                _useInMemoryStore = true;
            }
        }

        InMemoryExtracted.TryGetValue(documentId, out var content);
        return content;
    }

    public async Task InsertSummaryAsync(Summary summary, CancellationToken cancellationToken = default)
    {
        if (!_useInMemoryStore)
        {
            try
            {
                await _summaries.InsertOneAsync(summary, cancellationToken: cancellationToken);
                return;
            }
            catch
            {
                _useInMemoryStore = true;
            }
        }

        var list = InMemorySummaries.GetOrAdd(summary.DocumentId, _ => []);
        lock (list)
        {
            list.Add(summary);
        }
    }

    public async Task<List<Summary>> GetSummariesByDocumentIdAsync(string documentId, CancellationToken cancellationToken = default)
    {
        if (!_useInMemoryStore)
        {
            try
            {
                return await _summaries
                    .Find(x => x.DocumentId == documentId)
                    .SortByDescending(x => x.CreatedAtUtc)
                    .ToListAsync(cancellationToken);
            }
            catch
            {
                _useInMemoryStore = true;
            }
        }

        if (!InMemorySummaries.TryGetValue(documentId, out var list))
        {
            return [];
        }

        lock (list)
        {
            return list.OrderByDescending(x => x.CreatedAtUtc).ToList();
        }
    }
}
