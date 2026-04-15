using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DocumentIntelligence.Api.Models;

public sealed class Summary
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("documentId")]
    public string DocumentId { get; set; } = string.Empty;

    [BsonElement("title")]
    public string Title { get; set; } = string.Empty;

    [BsonElement("summaryText")]
    public string SummaryText { get; set; } = string.Empty;

    [BsonElement("keyPoints")]
    public List<string> KeyPoints { get; set; } = [];

    [BsonElement("createdAtUtc")]
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
