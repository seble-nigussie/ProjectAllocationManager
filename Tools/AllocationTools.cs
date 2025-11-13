using ModelContextProtocol.NET.Server;
using ProjectAllocationManager.Services;

namespace ProjectAllocationManager.Tools;

public static class AllocationTools
{
    public static void RegisterAllocationTools(this MCPServer server, AllocationService allocationService)
    {
        RegisterAllocateEngineer(server, allocationService);
        RegisterUpdateAllocation(server, allocationService);
    }

    private static void RegisterAllocateEngineer(MCPServer server, AllocationService allocationService)
    {
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
    }

    private static void RegisterUpdateAllocation(MCPServer server, AllocationService allocationService)
    {
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
    }
}
