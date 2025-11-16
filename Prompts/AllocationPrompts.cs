using System.ComponentModel;
using ModelContextProtocol;
using ModelContextProtocol.Server;

namespace ProjectAllocationManager.Prompts;

[McpServerPromptType]
public static class AllocationPrompts
{
    [McpServerPrompt, Description("Instructions for answering 'Who worked on this project?' type questions")]
    public static string WhoWorkedOnProject(
        [Description("The project name or ID the user is asking about")] string project)
    {
        return $"""
            The user wants to know who worked or is working on the project: "{project}"

            To answer this question, follow these steps:

            1. First, use the `list_projects` tool to see all available projects
            2. Find the project that matches "{project}" (could be by name or ID)
            3. Once you have the project ID, use the resource `allocation://project/{{projectId}}` to get detailed information
            4. The resource will show all engineers allocated to that project with their:
               - Names and roles
               - Allocation percentages
               - Time periods (start and end dates)
               - Skills

            Alternative approach:
            - You can also use `get_all_allocations` tool and filter for the specific project

            If the project name is ambiguous or not found:
            - Show the user the list of available projects from `list_projects`
            - Ask them to clarify which project they mean

            Format your response to clearly show:
            - Project name and description
            - Each engineer's name, role, and allocation percentage
            - The time period they're allocated
            """;
    }

    [McpServerPrompt, Description("Instructions for answering 'What projects did this engineer work on?' type questions")]
    public static string WhatProjectsDidEngineerWorkOn(
        [Description("The engineer name or ID the user is asking about")] string engineer)
    {
        return $"""
            The user wants to know what projects the engineer "{engineer}" worked on or is working on.

            To answer this question, follow these steps:

            1. First, use the `list_engineers` tool to see all available engineers
            2. Find the engineer that matches "{engineer}" (could be by name or ID)
            3. Once you have the engineer ID, use the resource `allocation://engineer/{{engineerId}}` to get detailed information
            4. The resource will show:
               - The engineer's current allocations
               - Total allocation percentage
               - Available capacity
               - All projects they're assigned to with percentages and dates

            Alternative approach:
            - Use the `get_engineer_allocations` tool with the engineer ID
            - Or use `get_all_allocations` and filter for this engineer

            If the engineer name is ambiguous or not found:
            - Show the user the list of available engineers from `list_engineers`
            - Ask them to clarify which engineer they mean

            Format your response to clearly show:
            - Engineer's name, role, and skills
            - Each project they're on with allocation percentage
            - Time periods for each allocation
            - Total allocation and remaining capacity
            """;
    }

    [McpServerPrompt, Description("Instructions for providing an overview of all project allocations")]
    public static string GetAllocationOverview()
    {
        return """
            The user wants a comprehensive overview of all project allocations.

            To provide this overview, use the following tools and resources:

            1. Use `list_projects` to get all projects
            2. Use `list_engineers` to get all engineers
            3. Use `get_all_allocations` to get all current allocations
            4. Use `get_bench_engineers` to see who's available

            Organize your response with:

            ## Summary
            - Total number of projects (active, planning, etc.)
            - Total number of engineers
            - Number of engineers on bench (0% allocated)
            - Number of engineers with allocations

            ## Projects Overview
            For each project, show:
            - Project name and status
            - Number of engineers allocated
            - List of engineers with their allocation percentages

            ## Resource Availability
            - List engineers on the bench
            - List engineers with partial availability (< 100% allocated)
            - Show which engineers are fully allocated

            ## Potential Issues
            - Highlight any over-allocation warnings
            - Note projects with no engineers assigned
            - Identify engineers who might be under-utilized

            Use the resources `allocation://projects/list` and `allocation://engineers/list`
            for nicely formatted overview data.
            """;
    }

    [McpServerPrompt, Description("Instructions for finding available engineers, optionally with specific skills")]
    public static string FindAvailableEngineers(
        [Description("(Optional) Specific skill to search for, like 'React' or 'Python'")] string? skill = null)
    {
        var skillFilter = skill != null ? $" with the skill '{skill}'" : "";

        return $"""
            The user wants to find available engineers{skillFilter}.

            To answer this question, follow these steps:

            1. Use `list_engineers` to get all engineers and their skills
            2. Use `get_all_allocations` to see current allocations
            3. For each engineer, calculate their available capacity:
               - Sum up all their allocation percentages
               - Available capacity = 100% - total allocations

            {(skill != null ? $"""
            4. Filter engineers to only show those with the skill '{skill}' in their skills list
            """ : "")}

            Alternative approaches:
            - Use `get_bench_engineers` to find engineers with 0% allocation
            - Use resources `allocation://engineer/{{engineerId}}` for detailed capacity info

            Format your response to show:

            ## Fully Available Engineers (0% allocated)
            - List engineers on the bench
            - Show their roles and skills

            ## Partially Available Engineers
            - List engineers with < 100% allocation
            - Show their available capacity percentage
            - Show their current allocations
            - List their skills

            {(skill != null ? $"""
            Note: Only include engineers who have '{skill}' in their skills list.

            If no engineers with that skill are available, suggest:
            - Engineers with that skill who might have capacity soon
            - Alternative skills that might be suitable
            """ : "")}

            Sort the results by available capacity (highest to lowest) for easy planning.
            """;
    }

    [McpServerPrompt, Description("Instructions for helping plan resource allocation for a new project")]
    public static string PlanProjectAllocation(
        [Description("The project name or description")] string project,
        [Description("(Optional) Required skills for the project")] string? requiredSkills = null)
    {
        return $"""
            The user wants to plan resource allocation for the project: "{project}"
            {(requiredSkills != null ? $"Required skills: {requiredSkills}" : "")}

            To help with this planning, follow these steps:

            1. Use `get_bench_engineers` to find available engineers
            2. Use `list_engineers` to see all engineers and their skills
            3. Use `get_all_allocations` to understand current workload
            4. For each engineer, check their availability using `allocation://engineer/{{engineerId}}`

            {(requiredSkills != null ? $"""
            5. Filter for engineers who have the required skills: {requiredSkills}
            """ : "")}

            Provide recommendations that include:

            ## Available Engineers
            List engineers who are:
            - Fully available (on bench)
            - Partially available (< 100% allocated)
            - Show their skills and current allocations

            ## Suggested Team Composition
            Based on the required skills, suggest:
            - Which engineers would be good fits
            - Their available capacity percentages
            - Potential allocation percentages for this project

            ## Planning Considerations
            - Note when current allocations end (engineers becoming available)
            - Identify any skill gaps that might need hiring/training
            - Suggest optimal team size based on available capacity

            ## Next Steps
            Tell the user they can use the `allocate_engineer` tool to make the allocations:
            - They'll need to provide: engineerId, projectId, percentage, start date, end date
            - Remind them the system prevents over-allocation (validates total doesn't exceed 100%)

            If the project doesn't exist yet, suggest they add it to data/projects.json first.
            """;
    }
}
