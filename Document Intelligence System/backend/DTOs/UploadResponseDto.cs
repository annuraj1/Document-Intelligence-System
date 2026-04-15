namespace DocumentIntelligence.Api.DTOs;

public sealed class UploadResponseDto
{
    public string DocumentId { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
