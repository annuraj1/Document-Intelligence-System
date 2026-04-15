using DocumentIntelligence.Api.Configuration;
using DocumentIntelligence.Api.Middleware;
using DocumentIntelligence.Api.Repositories;
using DocumentIntelligence.Api.Services;
using MongoDB.Driver;

LoadDotEnvFromCandidates();

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();
builder.WebHost.UseUrls(builder.Configuration["BACKEND_BASE_URL"] ?? "http://localhost:5001");

builder.Services.Configure<MongoDbSettings>(opts =>
{
    opts.ConnectionString = builder.Configuration["MONGODB_CONNECTION_STRING"] ?? string.Empty;
    opts.DatabaseName = builder.Configuration["MONGODB_DATABASE_NAME"] ?? "DocumentIntelligenceDb";
});

builder.Services.Configure<AdobePdfSettings>(opts =>
{
    opts.ClientId = builder.Configuration["ADOBE_CLIENT_ID"] ?? string.Empty;
    opts.ClientSecret = builder.Configuration["ADOBE_CLIENT_SECRET"] ?? string.Empty;
    opts.ExtractEndpoint = builder.Configuration["ADOBE_EXTRACT_ENDPOINT"] ?? opts.ExtractEndpoint;
});

builder.Services.Configure<GeminiSettings>(opts =>
{
    opts.ApiKey = builder.Configuration["GEMINI_API_KEY"] ?? string.Empty;
    opts.Model = builder.Configuration["GEMINI_MODEL"] ?? opts.Model;
});

builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<MongoDbSettings>>().Value;
    if (string.IsNullOrWhiteSpace(settings.ConnectionString))
    {
        throw new InvalidOperationException("MONGODB_CONNECTION_STRING is required.");
    }

    var mongoClientSettings = MongoClientSettings.FromConnectionString(settings.ConnectionString);
    mongoClientSettings.ServerSelectionTimeout = TimeSpan.FromSeconds(3);
    mongoClientSettings.ConnectTimeout = TimeSpan.FromSeconds(3);
    return new MongoClient(mongoClientSettings);
});

builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddHttpClient<IAdobePdfExtractService, AdobePdfExtractService>();
builder.Services.AddHttpClient<IGeminiService, GeminiService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var frontendOrigin = builder.Configuration["FRONTEND_BASE_URL"] ?? "http://localhost:5000";
var frontendOrigins = new[] { frontendOrigin, frontendOrigin.Replace("http://", "https://") }.Distinct().ToArray();
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .WithOrigins(frontendOrigins));
});

var app = builder.Build();

app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("FrontendPolicy");
app.MapControllers();

app.Run();

static void LoadDotEnv(string filePath)
{
    if (!File.Exists(filePath))
    {
        return;
    }

    foreach (var line in File.ReadAllLines(filePath))
    {
        if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith('#'))
        {
            continue;
        }

        var index = line.IndexOf('=');
        if (index <= 0)
        {
            continue;
        }

        var key = line[..index].Trim();
        var value = line[(index + 1)..].Trim().Trim('"');
        Environment.SetEnvironmentVariable(key, value);
    }
}

static void LoadDotEnvFromCandidates()
{
    var candidates = new[]
    {
        Path.Combine(Directory.GetCurrentDirectory(), ".env"),
        Path.Combine(Directory.GetCurrentDirectory(), "..", ".env"),
        Path.Combine(AppContext.BaseDirectory, ".env"),
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".env")
    };

    foreach (var candidate in candidates.Select(Path.GetFullPath).Distinct())
    {
        if (File.Exists(candidate))
        {
            LoadDotEnv(candidate);
            break;
        }
    }
}
