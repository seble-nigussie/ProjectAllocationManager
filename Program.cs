using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol;
using ModelContextProtocol.NET.Server;
using ProjectAllocationManager.Services;
using ProjectAllocationManager.Tools;

var builder = Host.CreateApplicationBuilder(args);

// Register services
builder.Services.AddSingleton<AllocationService>();

var host = builder.Build();

// Create MCP server
var server = new MCPServer(
    new ServerInfo(
        name: "project-allocation-manager",
        version: "1.0.0"
    ),
    new ServerOptions()
);

var allocationService = host.Services.GetRequiredService<AllocationService>();

// Register all tools
server.RegisterAllocationTools(allocationService);
server.RegisterQueryTools(allocationService);

// Start the server
await server.RunAsync();
