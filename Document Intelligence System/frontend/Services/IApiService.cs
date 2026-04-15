using DocumentIntelligence.Web.Models;
using Microsoft.AspNetCore.Components.Forms;

namespace DocumentIntelligence.Web.Services;

public interface IApiService
{
    Task<UploadResponse> UploadAsync(IBrowserFile file, string userId);
    Task ExtractAsync(string documentId);
    Task AnalyzeAsync(string documentId);
    Task SummarizeAsync(string documentId);
    Task<DocumentDetail?> GetDocumentAsync(string documentId);
}
