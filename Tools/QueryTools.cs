using ModelContextProtocol.NET.Server;
using ProjectAllocationManager.Services;

namespace ProjectAllocationManager.Tools;

public static class QueryTools
{
    public static void RegisterQueryTools(this MCPServer server, AllocationService allocationService)
    {
        RegisterGetEngineerAllocations(server, allocationService);
        RegisterGetBenchEngineers(server, allocationService);
        RegisterGetAllAllocations(server, allocationService);
        RegisterListEngineers(server, allocationService);
        RegisterListProjects(server, allocationService);
    }

    private static void RegisterGetEngineerAllocations(MCPServer server, AllocationService allocationService)
    {
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
    }

    private static void RegisterGetBenchEngineers(MCPServer server, AllocationService allocationService)
    {
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
    }

    private static void RegisterGetAllAllocations(MCPServer server, AllocationService allocationService)
    {
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
    }

    private static void RegisterListEngineers(MCPServer server, AllocationService allocationService)
    {
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
    }

    private static void RegisterListProjects(MCPServer server, AllocationService allocationService)
    {
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
    }
}
