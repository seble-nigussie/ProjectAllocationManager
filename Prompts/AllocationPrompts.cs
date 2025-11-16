using System.ComponentModel;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using ProjectAllocationManager.Services;

namespace ProjectAllocationManager.Prompts;

[McpServerPromptType]
public class AllocationPrompts
{
    [McpServerPrompt, Description("Get information about who worked or is working on a specific project")]
    public static async Task<string> WhoWorkedOnProject(
        AllocationService allocationService,
        [Description("The project name or ID (e.g., 'Project Alpha' or 'proj-001')")] string project)
    {
        var projects = await allocationService.GetProjectsAsync();
        var engineers = await allocationService.GetEngineersAsync();
        var allocations = await allocationService.GetAllocationsAsync();

        // Try to find project by ID first, then by name
        var foundProject = projects.FirstOrDefault(p =>
            p.Id.Equals(project, StringComparison.OrdinalIgnoreCase) ||
            p.Name.Equals(project, StringComparison.OrdinalIgnoreCase) ||
            p.Name.Contains(project, StringComparison.OrdinalIgnoreCase));

        if (foundProject == null)
        {
            var availableProjects = string.Join("\n", projects.Select(p => $"- {p.Name} ({p.Id})"));
            return $"""
                Project '{project}' not found.

                Available projects:
                {availableProjects}

                Please use the exact project name or ID.
                """;
        }

        var projectAllocations = allocations.Where(a => a.ProjectId == foundProject.Id).ToList();

        if (!projectAllocations.Any())
        {
            return $"""
                # {foundProject.Name}

                **Status:** {foundProject.Status}
                **Description:** {foundProject.Description}

                No engineers have been allocated to this project yet.
                """;
        }

        var result = $"""
            # {foundProject.Name}

            **Project ID:** {foundProject.Id}
            **Status:** {foundProject.Status}
            **Description:** {foundProject.Description}

            ## Engineers Working on This Project

            """;

        foreach (var allocation in projectAllocations)
        {
            var engineer = engineers.FirstOrDefault(e => e.Id == allocation.EngineerId);
            if (engineer != null)
            {
                result += $"""
                    ### {engineer.Name}
                    - **Role:** {engineer.Role}
                    - **Allocation:** {allocation.AllocationPercentage}%
                    - **Period:** {allocation.StartDate} to {allocation.EndDate}
                    - **Skills:** {string.Join(", ", engineer.Skills)}

                    """;
            }
        }

        var totalAllocation = projectAllocations.Sum(a => a.AllocationPercentage);
        result += $"\n**Total Project Allocation:** {totalAllocation}% (sum of all engineer percentages)\n";
        result += $"**Number of Engineers:** {projectAllocations.Count}\n";

        return result;
    }

    [McpServerPrompt, Description("Find projects that a specific engineer has worked on or is currently working on")]
    public static async Task<string> WhatProjectsDidEngineerWorkOn(
        AllocationService allocationService,
        [Description("The engineer name or ID (e.g., 'Alice Johnson' or 'eng-001')")] string engineer)
    {
        var engineers = await allocationService.GetEngineersAsync();
        var projects = await allocationService.GetProjectsAsync();
        var allocations = await allocationService.GetAllocationsAsync();

        // Try to find engineer by ID first, then by name
        var foundEngineer = engineers.FirstOrDefault(e =>
            e.Id.Equals(engineer, StringComparison.OrdinalIgnoreCase) ||
            e.Name.Equals(engineer, StringComparison.OrdinalIgnoreCase) ||
            e.Name.Contains(engineer, StringComparison.OrdinalIgnoreCase));

        if (foundEngineer == null)
        {
            var availableEngineers = string.Join("\n", engineers.Select(e => $"- {e.Name} ({e.Id})"));
            return $"""
                Engineer '{engineer}' not found.

                Available engineers:
                {availableEngineers}

                Please use the exact engineer name or ID.
                """;
        }

        var engineerAllocations = allocations.Where(a => a.EngineerId == foundEngineer.Id).ToList();

        if (!engineerAllocations.Any())
        {
            return $"""
                # {foundEngineer.Name}

                **Role:** {foundEngineer.Role}
                **Skills:** {string.Join(", ", foundEngineer.Skills)}

                This engineer is currently on the bench (no project allocations).
                """;
        }

        var totalAllocation = engineerAllocations.Sum(a => a.AllocationPercentage);

        var result = $"""
            # {foundEngineer.Name}

            **Engineer ID:** {foundEngineer.Id}
            **Role:** {foundEngineer.Role}
            **Skills:** {string.Join(", ", foundEngineer.Skills)}
            **Total Allocation:** {totalAllocation}%
            **Available Capacity:** {100 - totalAllocation}%

            ## Projects

            """;

        foreach (var allocation in engineerAllocations)
        {
            var project = projects.FirstOrDefault(p => p.Id == allocation.ProjectId);
            if (project != null)
            {
                result += $"""
                    ### {project.Name}
                    - **Project Status:** {project.Status}
                    - **Allocation:** {allocation.AllocationPercentage}%
                    - **Period:** {allocation.StartDate} to {allocation.EndDate}
                    - **Description:** {project.Description}

                    """;
            }
        }

        return result;
    }

    [McpServerPrompt, Description("Get an overview of current resource allocation across all projects and engineers")]
    public static async Task<string> GetAllocationOverview(AllocationService allocationService)
    {
        var engineers = await allocationService.GetEngineersAsync();
        var projects = await allocationService.GetProjectsAsync();
        var allocations = await allocationService.GetAllocationsAsync();

        var result = """
            # Project Allocation Overview

            ## Summary

            """;

        result += $"- **Total Engineers:** {engineers.Count}\n";
        result += $"- **Total Projects:** {projects.Count}\n";
        result += $"- **Total Allocations:** {allocations.Count}\n\n";

        // Calculate bench engineers
        var benchEngineers = engineers.Where(e =>
            !allocations.Any(a => a.EngineerId == e.Id)).ToList();

        result += $"- **Engineers on Bench:** {benchEngineers.Count}\n";
        result += $"- **Engineers Allocated:** {engineers.Count - benchEngineers.Count}\n\n";

        // Project breakdown
        result += "## Projects\n\n";
        foreach (var project in projects)
        {
            var projectAllocations = allocations.Where(a => a.ProjectId == project.Id).ToList();
            var engineerCount = projectAllocations.Count;
            var totalAllocation = projectAllocations.Sum(a => a.AllocationPercentage);

            result += $"### {project.Name} ({project.Status})\n";
            result += $"- **Engineers:** {engineerCount}\n";
            result += $"- **Total Allocation:** {totalAllocation}%\n\n";
        }

        // Bench engineers
        if (benchEngineers.Any())
        {
            result += "## Available Engineers (On Bench)\n\n";
            foreach (var engineer in benchEngineers)
            {
                result += $"- {engineer.Name} ({engineer.Role})\n";
            }
        }

        return result;
    }

    [McpServerPrompt, Description("Find available engineers with specific skills or on the bench")]
    public static async Task<string> FindAvailableEngineers(
        AllocationService allocationService,
        [Description("(Optional) Skill to search for (e.g., 'React', 'Python'). Leave empty to find all available engineers.")] string? skill = null)
    {
        var engineers = await allocationService.GetEngineersAsync();
        var allocations = await allocationService.GetAllocationsAsync();

        var result = "# Available Engineers\n\n";

        var availableEngineers = new List<(Engineer Engineer, int Capacity)>();

        foreach (var engineer in engineers)
        {
            var totalAllocation = allocations
                .Where(a => a.EngineerId == engineer.Id)
                .Sum(a => a.AllocationPercentage);

            var capacity = 100 - totalAllocation;

            // Filter by skill if provided
            if (skill != null && !engineer.Skills.Any(s => s.Contains(skill, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            if (capacity > 0)
            {
                availableEngineers.Add((engineer, capacity));
            }
        }

        if (!availableEngineers.Any())
        {
            result += skill != null
                ? $"No available engineers found with skill: {skill}\n"
                : "No available engineers found. All engineers are fully allocated.\n";
            return result;
        }

        result += skill != null
            ? $"Engineers with skill '{skill}' and available capacity:\n\n"
            : "Engineers with available capacity:\n\n";

        foreach (var (engineer, capacity) in availableEngineers.OrderByDescending(x => x.Capacity))
        {
            result += $"## {engineer.Name}\n";
            result += $"- **Role:** {engineer.Role}\n";
            result += $"- **Available Capacity:** {capacity}%\n";
            result += $"- **Skills:** {string.Join(", ", engineer.Skills)}\n\n";
        }

        return result;
    }
}
