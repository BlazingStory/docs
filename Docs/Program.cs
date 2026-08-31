using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BlazingStory.Docs;
using BlazingStory.Docs.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<DocsCatalogService>();
builder.Services.AddScoped<MarkdownService>();
builder.Services.AddScoped<DocsUiState>();
builder.Services.AddScoped<ThemeService>();

await builder.Build().RunAsync();
