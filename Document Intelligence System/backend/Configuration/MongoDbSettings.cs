namespace DocumentIntelligence.Api.Configuration;

public sealed class MongoDbSettings
{
    public const string SectionName = "MongoDb";
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = "DocumentIntelligenceDb";
    public string UsersCollectionName { get; set; } = "Users";
    public string DocumentsCollectionName { get; set; } = "Documents";
    public string ExtractedContentCollectionName { get; set; } = "ExtractedContent";
    public string SummariesCollectionName { get; set; } = "Summaries";
}
