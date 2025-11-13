using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol;
using ModelContextProtocol.NET.Server;
using ProjectAllocationManager.Services;

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

// Tool 1: Allocate Engineer to Project
server.AddTool(
    "allocate_engineer",
    "Allocate an engineer to a project with a specified percentage",
    new Dictionary<string, object>
    {
        ["engineerId"] = new
        {
            type = "string",
            description = "The ID of the engineer to allocate (e.g., 'eng-001')"
        },
        ["projectId"] = new
        {
            type = "string",
            description = "The ID of the project (e.g., 'proj-001')"
        },
        ["allocationPercentage"] = new
        {
            type = "number",
            description = "The percentage of time allocated (0-100)"
        },
        ["startDate"] = new
        {
            type = "string",
            description = "Start date in YYYY-MM-DD format"
        },
        ["endDate"] = new
        {
            type = "string",
            description = "End date in YYYY-MM-DD format"
        }
    },
    async (arguments) =>
    {
        var engineerId = arguments["engineerId"]?.ToString() ?? "";
        var projectId = arguments["projectId"]?.ToString() ?? "";
        var allocationPercentage = Convert.ToInt32(arguments["allocationPercentage"]);
        var startDate = arguments["startDate"]?.ToString() ?? "";
        var endDate = arguments["endDate"]?.ToString() ?? "";

        var result = await allocationService.AllocateEngineerAsync(
            engineerId, projectId, allocationPercentage, startDate, endDate);

        return new
        {
            success = result.Success,
            message = result.Message,
            allocation = result.Allocation
        };
    }
);

// Tool 2: Update Allocation
server.AddTool(
    "update_allocation",
    "Update an existing allocation's percentage, start date, or end date",
    new Dictionary<string, object>
    {
        ["allocationId"] = new
        {
            type = "string",
            description = "The ID of the allocation to update (e.g., 'alloc-001')"
        },
        ["newPercentage"] = new
        {
            type = "number",
            description = "(Optional) New allocation percentage (0-100)",
            @default = (object?)null
        },
        ["newStartDate"] = new
        {
            type = "string",
            description = "(Optional) New start date in YYYY-MM-DD format",
            @default = (object?)null
        },
        ["newEndDate"] = new
        {
            type = "string",
            description = "(Optional) New end date in YYYY-MM-DD format",
            @default = (object?)null
        }
    },
    async (arguments) =>
    {
        var allocationId = arguments["allocationId"]?.ToString() ?? "";

        int? newPercentage = null;
        if (arguments.ContainsKey("newPercentage") && arguments["newPercentage"] != null)
        {
            newPercentage = Convert.ToInt32(arguments["newPercentage"]);
        }

        string? newStartDate = arguments.ContainsKey("newStartDate")
            ? arguments["newStartDate"]?.ToString()
            : null;

        string? newEndDate = arguments.ContainsKey("newEndDate")
            ? arguments["newEndDate"]?.ToString()
            : null;

        var result = await allocationService.UpdateAllocationAsync(
            allocationId, newPercentage, newStartDate, newEndDate);

        return new
        {
            success = result.Success,
            message = result.Message
        };
    }
);

// Tool 3: Get Engineer Allocations
server.AddTool(
    "get_engineer_allocations",
    "View all allocations for a specific engineer",
    new Dictionary<string, object>
    {
        ["engineerId"] = new
        {
            type = "string",
            description = "The ID of the engineer (e.g., 'eng-001')"
        }
    },
    async (arguments) =>
    {
        var engineerId = arguments["engineerId"]?.ToString() ?? "";
        var result = await allocationService.GetEngineerAllocationsAsync(engineerId);

        return new
        {
            engineerId,
            details = result
        };
    }
);

// Tool 4: Get Bench Engineers
server.AddTool(
    "get_bench_engineers",
    "Get a list of all engineers with 0% allocation (on bench/available)",
    new Dictionary<string, object>(),
    async (arguments) =>
    {
        var result = await allocationService.GetBenchEngineersAsync();

        return new
        {
            details = result
        };
    }
);

// Tool 5: Get All Allocations
server.AddTool(
    "get_all_allocations",
    "View all current allocations across all engineers and projects",
    new Dictionary<string, object>(),
    async (arguments) =>
    {
        var result = await allocationService.GetAllAllocationsAsync();

        return new
        {
            details = result
        };
    }
);

// Tool 6: List Engineers
server.AddTool(
    "list_engineers",
    "List all engineers with their details",
    new Dictionary<string, object>(),
    async (arguments) =>
    {
        var engineers = await allocationService.GetEngineersAsync();

        return new
        {
            count = engineers.Count,
            engineers
        };
    }
);

// Tool 7: List Projects
server.AddTool(
    "list_projects",
    "List all projects with their details",
    new Dictionary<string, object>(),
    async (arguments) =>
    {
        var projects = await allocationService.GetProjectsAsync();

        return new
        {
            count = projects.Count,
            projects
        };
    }
);

// Start the server
await server.RunAsync();
