using System.Net.Http.Json;
using DocumentIntelligence.Web.Models;
using Microsoft.AspNetCore.Components.Forms;

namespace DocumentIntelligence.Web.Services;

public sealed class ApiService : IApiService
{
    private readonly HttpClient _httpClient;

    public ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<UploadResponse> UploadAsync(IBrowserFile file, string userId)
    {
        using var form = new MultipartFormDataContent();
        await using var fileStream = file.OpenReadStream(50 * 1024 * 1024);
        using var streamContent = new StreamContent(fileStream);

        streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
        form.Add(streamContent, "file", file.Name);
        form.Add(new StringContent(userId), "userId");

        var response = await _httpClient.PostAsync("/upload", form);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<UploadResponse>() ?? new UploadResponse();
    }

    public async Task ExtractAsync(string documentId)
    {
        var response = await _httpClient.PostAsync($"/extract?documentId={Uri.EscapeDataString(documentId)}", content: null);
        response.EnsureSuccessStatusCode();
    }

    public async Task AnalyzeAsync(string documentId)
    {
        var response = await _httpClient.PostAsync($"/analyze?documentId={Uri.EscapeDataString(documentId)}", content: null);
        response.EnsureSuccessStatusCode();
    }

    public async Task SummarizeAsync(string documentId)
    {
        var response = await _httpClient.PostAsync($"/summarize?documentId={Uri.EscapeDataString(documentId)}", content: null);
        response.EnsureSuccessStatusCode();
    }

    public async Task<DocumentDetail?> GetDocumentAsync(string documentId)
    {
        return await _httpClient.GetFromJsonAsync<DocumentDetail>($"/document/{documentId}");
    }
}
