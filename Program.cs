using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol;
using ProjectAllocationManager.Services;

var builder = Host.CreateEmptyApplicationBuilder(settings: null);

// Configure logging to prevent any output to stdout
builder.Logging.ClearProviders();
builder.Logging.SetMinimumLevel(LogLevel.None);

builder.Services.AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly()
    .WithResourcesFromAssembly()
    .WithPromptsFromAssembly();

builder.Services.AddSingleton<AllocationService>();

var app = builder.Build();

await app.RunAsync();
