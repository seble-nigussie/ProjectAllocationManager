using System.ComponentModel;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using ProjectAllocationManager.Services;

namespace ProjectAllocationManager.Tools;

[McpServerToolType]
public static class AllocationTools
{
    [McpServerTool, Description("Allocate an engineer to a project with a specified percentage")]
    public static async Task<object> AllocateEngineer(
        AllocationService allocationService,
        [Description("The ID of the engineer to allocate (e.g., 'eng-001')")] string engineerId,
        [Description("The ID of the project (e.g., 'proj-001')")] string projectId,
        [Description("The percentage of time allocated (0-100)")] int allocationPercentage,
        [Description("Start date in YYYY-MM-DD format")] string startDate,
        [Description("End date in YYYY-MM-DD format")] string endDate)
    {
        var result = await allocationService.AllocateEngineerAsync(
            engineerId, projectId, allocationPercentage, startDate, endDate);

        return new
        {
            success = result.Success,
            message = result.Message,
            allocation = result.Allocation
        };
    }

    [McpServerTool, Description("Update an existing allocation's percentage, start date, or end date")]
    public static async Task<object> UpdateAllocation(
        AllocationService allocationService,
        [Description("The ID of the allocation to update (e.g., 'alloc-001')")] string allocationId,
        [Description("(Optional) New allocation percentage (0-100)")] int? newPercentage = null,
        [Description("(Optional) New start date in YYYY-MM-DD format")] string? newStartDate = null,
        [Description("(Optional) New end date in YYYY-MM-DD format")] string? newEndDate = null)
    {
        var result = await allocationService.UpdateAllocationAsync(
            allocationId, newPercentage, newStartDate, newEndDate);

        return new
        {
            success = result.Success,
            message = result.Message
        };
    }
}
