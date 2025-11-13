using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol;
using ModelContextProtocol.NET.Server;
using ProjectAllocationManager.Services;
using ProjectAllocationManager.Tools;

var builder = Host.CreateApplicationBuilder(args);

// Register services
builder.Services.AddSingleton<AllocationService>();

// Register tool classes
builder.Services.AddSingleton<AllocationTools>();
builder.Services.AddSingleton<QueryTools>();

var host = builder.Build();

// Create MCP server
var server = new MCPServer(
    new ServerInfo(
        name: "project-allocation-manager",
        version: "1.0.0"
    ),
    new ServerOptions()
);

// Auto-discover and register all tools from assembly using reflection
server.WithToolsFromAssembly(host.Services);

// Start the server
await server.RunAsync();
