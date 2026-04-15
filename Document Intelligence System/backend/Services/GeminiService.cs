using System.Text.Json;
using System.Text.RegularExpressions;
using DocumentIntelligence.Api.Configuration;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace DocumentIntelligence.Api.Services;

public sealed class GeminiService : IGeminiService
{
    private readonly HttpClient _httpClient;
    private readonly GeminiSettings _settings;

    public GeminiService(HttpClient httpClient, IOptions<GeminiSettings> settings)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
    }

    public async Task<string> GenerateSummaryAsync(string rawText, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
        {
            return "Gemini API key is missing. Add GEMINI_API_KEY in .env to enable AI summarization.";
        }

        var endpoint = _settings.EndpointTemplate
            .Replace("{model}", _settings.Model, StringComparison.Ordinal)
            .Replace("{apiKey}", _settings.ApiKey, StringComparison.Ordinal);

        var prompt = """
                     Summarize the following document in concise notebook format.
                     Return:
                     1) A short title line
                     2) A concise summary paragraph
                     3) 3-6 key bullet points

                     Document text:
                     """;

        var requestPayload = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new
                        {
                            text = $"{prompt}\n{rawText}"
                        }
                    }
                }
            }
        };

        var response = await _httpClient.PostAsJsonAsync(endpoint, requestPayload, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return BuildLocalSummary(rawText);
        }

        try
        {
            using var doc = JsonDocument.Parse(body);
            var text = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            return text ?? "No summary generated.";
        }
        catch
        {
            return BuildLocalSummary(rawText);
        }
    }

    private static string BuildLocalSummary(string rawText)
    {
        var normalized = Regex.Replace(rawText ?? string.Empty, @"\s+", " ").Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return "Document Summary\n\nNo readable text was found in the document.\n\n- Please upload a clearer text-based PDF";
        }

        var sentences = Regex.Split(normalized, @"(?<=[.!?])\s+")
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .ToList();

        var title = sentences.FirstOrDefault() ?? "Document Summary";
        if (title.Length > 80)
        {
            title = title[..80].TrimEnd() + "...";
        }

        var summary = string.Join(" ", sentences.Take(3));
        var bullets = sentences.Skip(3).Take(3).ToList();
        if (bullets.Count == 0)
        {
            bullets = sentences.Take(3).ToList();
        }

        var bulletLines = string.Join("\n", bullets.Select(x => $"- {x}"));
        return $"{title}\n\n{summary}\n\n{bulletLines}";
    }
}
