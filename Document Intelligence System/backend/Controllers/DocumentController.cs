using DocumentIntelligence.Api.DTOs;
using DocumentIntelligence.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace DocumentIntelligence.Api.Controllers;

[ApiController]
[Route("")]
public sealed class DocumentController : ControllerBase
{
    private readonly IDocumentService _documentService;

    public DocumentController(IDocumentService documentService)
    {
        _documentService = documentService;
    }

    [HttpPost("upload")]
    [RequestSizeLimit(50_000_000)]
    public async Task<ActionResult<UploadResponseDto>> Upload(
        [FromForm] UploadDocumentRequestDto request,
        CancellationToken cancellationToken)
    {
        if (request.File is null)
        {
            return BadRequest(new { error = "File is required." });
        }

        if (!request.File.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { error = "Only PDF files are supported." });
        }

        var result = await _documentService.UploadAsync(request.File, request.UserId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("extract")]
    public async Task<ActionResult<object>> Extract([FromQuery] string? documentId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(documentId))
        {
            throw new ArgumentException("DocumentId is required.");
        }

        var text = await _documentService.ExtractAsync(documentId, cancellationToken);
        return Ok(new { documentId, extractedText = text });
    }

    [HttpPost("analyze")]
    public async Task<ActionResult<object>> Analyze([FromQuery] string? documentId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(documentId))
        {
            throw new ArgumentException("DocumentId is required.");
        }

        var sections = await _documentService.AnalyzeAsync(documentId, cancellationToken);
        return Ok(new { documentId, sections });
    }

    [HttpPost("summarize")]
    public async Task<ActionResult<object>> Summarize([FromQuery] string? documentId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(documentId))
        {
            throw new ArgumentException("DocumentId is required.");
        }

        var summary = await _documentService.SummarizeAsync(documentId, cancellationToken);
        return Ok(new { documentId, summary });
    }

    [HttpGet("document/{id}")]
    public async Task<ActionResult<DocumentResponseDto>> GetDocument([FromRoute] string id, CancellationToken cancellationToken)
    {
        var document = await _documentService.GetDocumentAsync(id, cancellationToken);
        if (document is null)
        {
            return NotFound(new { error = "Document not found." });
        }

        return Ok(document);
    }
}
