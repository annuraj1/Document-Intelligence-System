using System.Text.RegularExpressions;
using DocumentIntelligence.Api.DTOs;
using DocumentIntelligence.Api.Models;
using DocumentIntelligence.Api.Repositories;

namespace DocumentIntelligence.Api.Services;

public sealed class DocumentService : IDocumentService
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IAdobePdfExtractService _adobePdfExtractService;
    private readonly IGeminiService _geminiService;
    private readonly IWebHostEnvironment _environment;

    public DocumentService(
        IDocumentRepository documentRepository,
        IAdobePdfExtractService adobePdfExtractService,
        IGeminiService geminiService,
        IWebHostEnvironment environment)
    {
        _documentRepository = documentRepository;
        _adobePdfExtractService = adobePdfExtractService;
        _geminiService = geminiService;
        _environment = environment;
    }

    public async Task<UploadResponseDto> UploadAsync(IFormFile file, string userId, CancellationToken cancellationToken = default)
    {
        if (file.Length <= 0)
        {
            throw new ArgumentException("File is empty.");
        }

        var uploadsDir = Path.Combine(_environment.ContentRootPath, "uploads");
        Directory.CreateDirectory(uploadsDir);

        var fileName = $"{Guid.NewGuid():N}_{Path.GetFileName(file.FileName)}";
        var fullPath = Path.Combine(uploadsDir, fileName);

        await using (var stream = File.Create(fullPath))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        var document = new Document
        {
            UserId = userId,
            FileName = file.FileName,
            ContentType = file.ContentType,
            FileSizeBytes = file.Length,
            StoragePath = fullPath,
            Status = "Uploaded"
        };

        var created = await _documentRepository.CreateDocumentAsync(document, cancellationToken);
        return new UploadResponseDto
        {
            DocumentId = created.Id,
            FileName = created.FileName,
            Status = created.Status
        };
    }

    public async Task<string> ExtractAsync(string documentId, CancellationToken cancellationToken = default)
    {
        var document = await _documentRepository.GetDocumentByIdAsync(documentId, cancellationToken)
            ?? throw new KeyNotFoundException("Document not found.");

        var rawText = await _adobePdfExtractService.ExtractTextAsync(document.StoragePath, cancellationToken);

        var extracted = new ExtractedContent
        {
            Id = MongoDB.Bson.ObjectId.GenerateNewId().ToString(),
            DocumentId = documentId,
            RawText = rawText,
            Sections = []
        };

        await _documentRepository.UpsertExtractedContentAsync(extracted, cancellationToken);
        await _documentRepository.UpdateDocumentStatusAsync(documentId, "Extracted", cancellationToken);

        return rawText;
    }

    public async Task<List<string>> AnalyzeAsync(string documentId, CancellationToken cancellationToken = default)
    {
        var existing = await _documentRepository.GetExtractedContentByDocumentIdAsync(documentId, cancellationToken)
            ?? throw new InvalidOperationException("Document content has not been extracted yet.");

        var sections = SplitIntoSections(existing.RawText);
        existing.Sections = sections;

        await _documentRepository.UpsertExtractedContentAsync(existing, cancellationToken);
        await _documentRepository.UpdateDocumentStatusAsync(documentId, "Analyzed", cancellationToken);

        return sections;
    }

    public async Task<string> SummarizeAsync(string documentId, CancellationToken cancellationToken = default)
    {
        var existing = await _documentRepository.GetExtractedContentByDocumentIdAsync(documentId, cancellationToken)
            ?? throw new InvalidOperationException("Document content has not been extracted yet.");

        var summaryText = await _geminiService.GenerateSummaryAsync(existing.RawText, cancellationToken);
        var summary = new Summary
        {
            DocumentId = documentId,
            Title = BuildTitle(summaryText),
            SummaryText = summaryText,
            KeyPoints = ExtractKeyPoints(summaryText)
        };

        await _documentRepository.InsertSummaryAsync(summary, cancellationToken);
        await _documentRepository.UpdateDocumentStatusAsync(documentId, "Summarized", cancellationToken);

        return summaryText;
    }

    public async Task<DocumentResponseDto?> GetDocumentAsync(string documentId, CancellationToken cancellationToken = default)
    {
        var document = await _documentRepository.GetDocumentByIdAsync(documentId, cancellationToken);
        if (document is null)
        {
            return null;
        }

        var extracted = await _documentRepository.GetExtractedContentByDocumentIdAsync(documentId, cancellationToken);
        var summaries = await _documentRepository.GetSummariesByDocumentIdAsync(documentId, cancellationToken);

        return new DocumentResponseDto
        {
            DocumentId = document.Id,
            FileName = document.FileName,
            Status = document.Status,
            RawText = extracted?.RawText ?? string.Empty,
            Sections = extracted?.Sections ?? [],
            Summaries = summaries.Select(x => new SummaryCardDto
            {
                Title = x.Title,
                SummaryText = x.SummaryText,
                KeyPoints = x.KeyPoints
            }).ToList()
        };
    }

    private static List<string> SplitIntoSections(string rawText)
    {
        return rawText
            .Split(["\r\n\r\n", "\n\n"], StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .Where(x => x.Length > 40)
            .Take(12)
            .ToList();
    }

    private static string BuildTitle(string summaryText)
    {
        var line = summaryText.Split('\n', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
        return string.IsNullOrWhiteSpace(line) ? "Document Summary" : line.Trim();
    }

    private static List<string> ExtractKeyPoints(string summaryText)
    {
        var matches = Regex.Matches(summaryText, @"^\s*[-*]\s+(.*)$", RegexOptions.Multiline);
        if (matches.Count > 0)
        {
            return matches.Select(m => m.Groups[1].Value.Trim()).Where(x => x.Length > 0).ToList();
        }

        return summaryText
            .Split('.', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .Where(x => x.Length > 20)
            .Take(5)
            .ToList();
    }
}
