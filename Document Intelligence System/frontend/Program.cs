using DocumentIntelligence.Web.Services;
using DocumentIntelligence.Web;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.Configuration["BackendBaseUrl"] ?? "http://localhost:5000")
});
builder.Services.AddScoped<IApiService, ApiService>();

await builder.Build().RunAsync();
