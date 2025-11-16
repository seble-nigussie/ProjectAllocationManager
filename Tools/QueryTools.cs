using System.ComponentModel;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using ProjectAllocationManager.Services;

namespace ProjectAllocationManager.Tools;

[McpServerToolType]
public static class QueryTools
{
    [McpServerTool, Description("View all allocations for a specific engineer")]
    public static async Task<object> GetEngineerAllocations(
        AllocationService allocationService,
        [Description("The ID of the engineer (e.g., 'eng-001')")] string engineerId)
    {
        var result = await allocationService.GetEngineerAllocationsAsync(engineerId);

        return new
        {
            engineerId,
            details = result
        };
    }

    [McpServerTool, Description("Get a list of all engineers with 0% allocation (on bench/available)")]
    public static async Task<object> GetBenchEngineers(AllocationService allocationService)
    {
        var result = await allocationService.GetBenchEngineersAsync();

        return new
        {
            details = result
        };
    }

    [McpServerTool, Description("View all current allocations across all engineers and projects")]
    public static async Task<object> GetAllAllocations(AllocationService allocationService)
    {
        var result = await allocationService.GetAllAllocationsAsync();

        return new
        {
            details = result
        };
    }

    [McpServerTool, Description("List all engineers with their details")]
    public static async Task<object> ListEngineers(AllocationService allocationService)
    {
        var engineers = await allocationService.GetEngineersAsync();

        return new
        {
            count = engineers.Count,
            engineers
        };
    }

    [McpServerTool, Description("List all projects with their details")]
    public static async Task<object> ListProjects(AllocationService allocationService)
    {
        var projects = await allocationService.GetProjectsAsync();

        return new
        {
            count = projects.Count,
            projects
        };
    }
}
