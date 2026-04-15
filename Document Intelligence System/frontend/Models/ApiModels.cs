namespace DocumentIntelligence.Web.Models;

public sealed class UploadResponse
{
    public string DocumentId { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public sealed class DocumentDetail
{
    public string DocumentId { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string RawText { get; set; } = string.Empty;
    public List<string> Sections { get; set; } = [];
    public List<SummaryCard> Summaries { get; set; } = [];
}

public sealed class SummaryCard
{
    public string Title { get; set; } = string.Empty;
    public string SummaryText { get; set; } = string.Empty;
    public List<string> KeyPoints { get; set; } = [];
}
