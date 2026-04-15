namespace DocumentIntelligence.Api.DTOs;

public sealed class UploadDocumentRequestDto
{
    public string UserId { get; set; } = string.Empty;
    public IFormFile File { get; set; } = null!;
}
