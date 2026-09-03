using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BlazingStory.Docs;
using BlazingStory.Docs.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

ConfigureServices(builder.Services, builder.HostEnvironment.BaseAddress);

var host = builder.Build();
await host.Services.GetRequiredService<DocumentPreloadService>().PreloadCurrentDocumentAsync();
await host.RunAsync();

static void ConfigureServices(IServiceCollection services, string baseAddress)
{
    services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(baseAddress) });
    services.AddScoped<DocsCatalogService>();
    services.AddScoped<MarkdownService>();
    services.AddScoped<DocumentPreloadService>();
    services.AddScoped<DocsUiState>();
    services.AddSingleton<EmbeddingService>();
    services.AddScoped<SearchIndexService>();
    services.AddScoped<ThemeService>();
}
