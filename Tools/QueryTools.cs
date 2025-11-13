using System.ComponentModel;
using ModelContextProtocol;
using ProjectAllocationManager.Services;

namespace ProjectAllocationManager.Tools;

public class QueryTools
{
    private readonly AllocationService _allocationService;

    public QueryTools(AllocationService allocationService)
    {
        _allocationService = allocationService;
    }

    [McpServerTool, Description("View all allocations for a specific engineer")]
    public async Task<object> GetEngineerAllocations(
        [Description("The ID of the engineer (e.g., 'eng-001')")] string engineerId)
    {
        var result = await _allocationService.GetEngineerAllocationsAsync(engineerId);

        return new
        {
            engineerId,
            details = result
        };
    }

    [McpServerTool, Description("Get a list of all engineers with 0% allocation (on bench/available)")]
    public async Task<object> GetBenchEngineers()
    {
        var result = await _allocationService.GetBenchEngineersAsync();

        return new
        {
            details = result
        };
    }

    [McpServerTool, Description("View all current allocations across all engineers and projects")]
    public async Task<object> GetAllAllocations()
    {
        var result = await _allocationService.GetAllAllocationsAsync();

        return new
        {
            details = result
        };
    }

    [McpServerTool, Description("List all engineers with their details")]
    public async Task<object> ListEngineers()
    {
        var engineers = await _allocationService.GetEngineersAsync();

        return new
        {
            count = engineers.Count,
            engineers
        };
    }

    [McpServerTool, Description("List all projects with their details")]
    public async Task<object> ListProjects()
    {
        var projects = await _allocationService.GetProjectsAsync();

        return new
        {
            count = projects.Count,
            projects
        };
    }
}
