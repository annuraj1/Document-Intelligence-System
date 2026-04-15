namespace DocumentIntelligence.Api.Configuration;

public sealed class AdobePdfSettings
{
    public const string SectionName = "AdobePdf";
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string ExtractEndpoint { get; set; } = "https://pdf-services.adobe.io/operation/extractpdf";
}
