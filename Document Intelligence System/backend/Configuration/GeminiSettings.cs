namespace DocumentIntelligence.Api.Configuration;

public sealed class GeminiSettings
{
    public const string SectionName = "Gemini";
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gemini-1.5-flash";
    public string EndpointTemplate { get; set; } =
        "https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";
}
