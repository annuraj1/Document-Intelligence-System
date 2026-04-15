using System.Net.Http.Headers;
using System.Text.Json;
using DocumentIntelligence.Api.Configuration;
using Microsoft.Extensions.Options;

namespace DocumentIntelligence.Api.Services;

public sealed class AdobePdfExtractService : IAdobePdfExtractService
{
    private readonly HttpClient _httpClient;
    private readonly AdobePdfSettings _settings;

    public AdobePdfExtractService(HttpClient httpClient, IOptions<AdobePdfSettings> settings)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
    }

    public async Task<string> ExtractTextAsync(string filePath, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            try
            {
                using var document = UglyToad.PdfPig.PdfDocument.Open(filePath);
                var sb = new System.Text.StringBuilder();
                foreach (var page in document.GetPages())
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var text = page.Text;
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        sb.AppendLine(text);
                        sb.AppendLine();
                    }
                }

                var result = sb.ToString().Trim();
                if (string.IsNullOrWhiteSpace(result))
                {
                    return "No text could be extracted from this PDF document.";
                }

                return result;
            }
            catch (Exception)
            {
                return "Text extraction could not read this PDF format. Please try another PDF file.";
            }
        }, cancellationToken);
    }
}
