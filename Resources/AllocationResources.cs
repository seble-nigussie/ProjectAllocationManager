using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using ProjectAllocationManager.Services;

namespace ProjectAllocationManager.Resources;

[McpServerResourceType]
public class AllocationResources
{
    // Static resource - List all projects
    [McpServerResource(UriTemplate = "allocation://projects/list")]
    [Description("List of all projects with their details, status, and descriptions")]
    public static async Task<string> GetProjectsList(AllocationService allocationService)
    {
        var projects = await allocationService.GetProjectsAsync();

        var result = "# All Projects\n\n";

        foreach (var project in projects)
        {
            result += $"## {project.Name} ({project.Id})\n";
            result += $"**Status:** {project.Status}\n";
            result += $"**Description:** {project.Description}\n\n";
        }

        result += $"\nTotal Projects: {projects.Count}\n";
        result += $"Last Updated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC\n";

        return result;
    }

    // Static resource - List all engineers
    [McpServerResource(UriTemplate = "allocation://engineers/list")]
    [Description("List of all engineers with their details, roles, and skills")]
    public static async Task<string> GetEngineersList(AllocationService allocationService)
    {
        var engineers = await allocationService.GetEngineersAsync();

        var result = "# All Engineers\n\n";

        foreach (var engineer in engineers)
        {
            result += $"## {engineer.Name} ({engineer.Id})\n";
            result += $"**Role:** {engineer.Role}\n";
            result += $"**Skills:** {string.Join(", ", engineer.Skills)}\n\n";
        }

        result += $"\nTotal Engineers: {engineers.Count}\n";
        result += $"Last Updated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC\n";

        return result;
    }

    // Static resource - List all allocations
    [McpServerResource(UriTemplate = "allocation://allocations/list")]
    [Description("List of all current allocations across engineers and projects")]
    public static async Task<string> GetAllocationsList(AllocationService allocationService)
    {
        var engineers = await allocationService.GetEngineersAsync();
        var projects = await allocationService.GetProjectsAsync();
        var allocations = await allocationService.GetAllocationsAsync();

        var result = "# All Allocations\n\n";

        if (!allocations.Any())
        {
            return result + "No allocations found.\n";
        }

        foreach (var allocation in allocations)
        {
            var engineer = engineers.FirstOrDefault(e => e.Id == allocation.EngineerId);
            var project = projects.FirstOrDefault(p => p.Id == allocation.ProjectId);

            result += $"## Allocation {allocation.Id}\n";
            result += $"**Engineer:** {engineer?.Name ?? allocation.EngineerId}\n";
            result += $"**Project:** {project?.Name ?? allocation.ProjectId}\n";
            result += $"**Allocation:** {allocation.AllocationPercentage}%\n";
            result += $"**Duration:** {allocation.StartDate} to {allocation.EndDate}\n\n";
        }

        result += $"\nTotal Allocations: {allocations.Count}\n";
        result += $"Last Updated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC\n";

        return result;
    }

    // Dynamic resource - Get specific engineer details
    [McpServerResource(UriTemplate = "allocation://engineer/{engineerId}")]
    [Description("Get detailed information about a specific engineer including their current allocations")]
    public static async Task<string> GetEngineerDetails(
        AllocationService allocationService,
        [Description("The engineer ID (e.g., 'eng-001')")] string engineerId)
    {
        var engineers = await allocationService.GetEngineersAsync();
        var projects = await allocationService.GetProjectsAsync();
        var allocations = await allocationService.GetAllocationsAsync();

        var engineer = engineers.FirstOrDefault(e => e.Id == engineerId);
        if (engineer == null)
        {
            return $"# Engineer Not Found\n\nNo engineer found with ID: {engineerId}";
        }

        var engineerAllocations = allocations.Where(a => a.EngineerId == engineerId).ToList();
        var totalAllocation = engineerAllocations.Sum(a => a.AllocationPercentage);

        var result = $"# {engineer.Name}\n\n";
        result += $"**ID:** {engineer.Id}\n";
        result += $"**Role:** {engineer.Role}\n";
        result += $"**Skills:** {string.Join(", ", engineer.Skills)}\n\n";

        result += $"## Allocation Summary\n";
        result += $"**Total Allocation:** {totalAllocation}%\n";
        result += $"**Available Capacity:** {100 - totalAllocation}%\n";
        result += $"**Status:** {(totalAllocation == 0 ? "On Bench (Available)" : totalAllocation == 100 ? "Fully Allocated" : "Partially Allocated")}\n\n";

        if (engineerAllocations.Any())
        {
            result += $"## Current Projects\n\n";
            foreach (var allocation in engineerAllocations)
            {
                var project = projects.FirstOrDefault(p => p.Id == allocation.ProjectId);
                result += $"### {project?.Name ?? allocation.ProjectId}\n";
                result += $"- **Allocation:** {allocation.AllocationPercentage}%\n";
                result += $"- **Duration:** {allocation.StartDate} to {allocation.EndDate}\n";
                result += $"- **Allocation ID:** {allocation.Id}\n\n";
            }
        }
        else
        {
            result += $"## Current Projects\n\nNo active allocations. Engineer is available on the bench.\n\n";
        }

        result += $"\nResource URI: allocation://engineer/{engineerId}\n";
        result += $"Last Updated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC\n";

        return result;
    }

    // Dynamic resource - Get specific project details
    [McpServerResource(UriTemplate = "allocation://project/{projectId}")]
    [Description("Get detailed information about a specific project including assigned engineers")]
    public static async Task<string> GetProjectDetails(
        AllocationService allocationService,
        [Description("The project ID (e.g., 'proj-001')")] string projectId)
    {
        var engineers = await allocationService.GetEngineersAsync();
        var projects = await allocationService.GetProjectsAsync();
        var allocations = await allocationService.GetAllocationsAsync();

        var project = projects.FirstOrDefault(p => p.Id == projectId);
        if (project == null)
        {
            return $"# Project Not Found\n\nNo project found with ID: {projectId}";
        }

        var projectAllocations = allocations.Where(a => a.ProjectId == projectId).ToList();
        var totalAllocation = projectAllocations.Sum(a => a.AllocationPercentage);

        var result = $"# {project.Name}\n\n";
        result += $"**ID:** {project.Id}\n";
        result += $"**Status:** {project.Status}\n";
        result += $"**Description:** {project.Description}\n\n";

        result += $"## Resource Summary\n";
        result += $"**Total Engineer Allocation:** {totalAllocation}% (sum of all engineer percentages)\n";
        result += $"**Engineers Assigned:** {projectAllocations.Count}\n\n";

        if (projectAllocations.Any())
        {
            result += $"## Assigned Engineers\n\n";
            foreach (var allocation in projectAllocations)
            {
                var engineer = engineers.FirstOrDefault(e => e.Id == allocation.EngineerId);
                result += $"### {engineer?.Name ?? allocation.EngineerId}\n";
                result += $"- **Role:** {engineer?.Role ?? "Unknown"}\n";
                result += $"- **Allocation:** {allocation.AllocationPercentage}%\n";
                result += $"- **Duration:** {allocation.StartDate} to {allocation.EndDate}\n";
                result += $"- **Allocation ID:** {allocation.Id}\n\n";
            }
        }
        else
        {
            result += $"## Assigned Engineers\n\nNo engineers currently allocated to this project.\n\n";
        }

        result += $"\nResource URI: allocation://project/{projectId}\n";
        result += $"Last Updated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC\n";

        return result;
    }

    // Static resource - Projects as JSON
    [McpServerResource(UriTemplate = "allocation://projects/json")]
    [Description("Raw JSON data of all projects")]
    public static async Task<string> GetProjectsJson(AllocationService allocationService)
    {
        var projects = await allocationService.GetProjectsAsync();
        return JsonSerializer.Serialize(projects, new JsonSerializerOptions { WriteIndented = true });
    }

    // Static resource - Engineers as JSON
    [McpServerResource(UriTemplate = "allocation://engineers/json")]
    [Description("Raw JSON data of all engineers")]
    public static async Task<string> GetEngineersJson(AllocationService allocationService)
    {
        var engineers = await allocationService.GetEngineersAsync();
        return JsonSerializer.Serialize(engineers, new JsonSerializerOptions { WriteIndented = true });
    }

    // Static resource - Allocations as JSON
    [McpServerResource(UriTemplate = "allocation://allocations/json")]
    [Description("Raw JSON data of all allocations")]
    public static async Task<string> GetAllocationsJson(AllocationService allocationService)
    {
        var allocations = await allocationService.GetAllocationsAsync();
        return JsonSerializer.Serialize(allocations, new JsonSerializerOptions { WriteIndented = true });
    }
}
