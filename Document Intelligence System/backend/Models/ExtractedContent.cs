using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DocumentIntelligence.Api.Models;

public sealed class ExtractedContent
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("documentId")]
    public string DocumentId { get; set; } = string.Empty;

    [BsonElement("rawText")]
    public string RawText { get; set; } = string.Empty;

    [BsonElement("sections")]
    public List<string> Sections { get; set; } = [];

    [BsonElement("createdAtUtc")]
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
