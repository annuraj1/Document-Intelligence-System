namespace DocumentIntelligence.Api.DTOs;

public sealed class DocumentResponseDto
{
    public string DocumentId { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string RawText { get; set; } = string.Empty;
    public List<string> Sections { get; set; } = [];
    public List<SummaryCardDto> Summaries { get; set; } = [];
}

public sealed class SummaryCardDto
{
    public string Title { get; set; } = string.Empty;
    public string SummaryText { get; set; } = string.Empty;
    public List<string> KeyPoints { get; set; } = [];
}
