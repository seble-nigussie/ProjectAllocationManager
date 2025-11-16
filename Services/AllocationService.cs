using System.Text.Json;
using ProjectAllocationManager.Models;

namespace ProjectAllocationManager.Services;

public class AllocationService
{
    private readonly string _dataPath;
    private readonly JsonSerializerOptions _jsonOptions;

    public AllocationService(string dataPath = "data")
    {
        _dataPath = dataPath;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
    }

    // Read methods
    public async Task<List<Project>> GetProjectsAsync()
    {
        var filePath = Path.Combine(_dataPath, "projects.json");
        var json = await File.ReadAllTextAsync(filePath);
        return JsonSerializer.Deserialize<List<Project>>(json, _jsonOptions) ?? new List<Project>();
    }

    public async Task<List<Engineer>> GetEngineersAsync()
    {
        var filePath = Path.Combine(_dataPath, "engineers.json");
        var json = await File.ReadAllTextAsync(filePath);
        return JsonSerializer.Deserialize<List<Engineer>>(json, _jsonOptions) ?? new List<Engineer>();
    }

    public async Task<List<Allocation>> GetAllocationsAsync()
    {
        var filePath = Path.Combine(_dataPath, "allocations.json");
        var json = await File.ReadAllTextAsync(filePath);
        return JsonSerializer.Deserialize<List<Allocation>>(json, _jsonOptions) ?? new List<Allocation>();
    }

    // Write methods
    private async Task SaveAllocationsAsync(List<Allocation> allocations)
    {
        var filePath = Path.Combine(_dataPath, "allocations.json");
        var json = JsonSerializer.Serialize(allocations, _jsonOptions);
        await File.WriteAllTextAsync(filePath, json);
    }

    // Business logic methods
    public async Task<(bool Success, string Message, Allocation? Allocation)> AllocateEngineerAsync(
        string engineerId,
        string projectId,
        int allocationPercentage,
        string startDate,
        string endDate)
    {
        // Validate inputs
        if (allocationPercentage < 0 || allocationPercentage > 100)
        {
            return (false, "Allocation percentage must be between 0 and 100.", null);
        }

        var engineers = await GetEngineersAsync();
        var projects = await GetProjectsAsync();
        var allocations = await GetAllocationsAsync();

        var engineer = engineers.FirstOrDefault(e => e.Id == engineerId);
        if (engineer == null)
        {
            return (false, $"Engineer with ID '{engineerId}' not found.", null);
        }

        var project = projects.FirstOrDefault(p => p.Id == projectId);
        if (project == null)
        {
            return (false, $"Project with ID '{projectId}' not found.", null);
        }

        // Check if engineer is over-allocated (only count active allocations)
        var existingAllocations = allocations.Where(a => a.EngineerId == engineerId && a.Status == "active").ToList();
        var totalAllocation = existingAllocations.Sum(a => a.AllocationPercentage) + allocationPercentage;

        if (totalAllocation > 100)
        {
            return (false, $"Cannot allocate. Engineer '{engineer.Name}' would be over-allocated ({totalAllocation}%).", null);
        }

        // Create new allocation
        var newAllocation = new Allocation
        {
            Id = $"alloc-{Guid.NewGuid().ToString()[..8]}",
            EngineerId = engineerId,
            ProjectId = projectId,
            AllocationPercentage = allocationPercentage,
            StartDate = startDate,
            EndDate = endDate,
            Status = "active"
        };

        allocations.Add(newAllocation);
        await SaveAllocationsAsync(allocations);

        return (true, $"Successfully allocated {engineer.Name} to {project.Name} at {allocationPercentage}%.", newAllocation);
    }

    public async Task<(bool Success, string Message)> UpdateAllocationAsync(
        string allocationId,
        int? newPercentage = null,
        string? newStartDate = null,
        string? newEndDate = null)
    {
        var allocations = await GetAllocationsAsync();
        var allocation = allocations.FirstOrDefault(a => a.Id == allocationId);

        if (allocation == null)
        {
            return (false, $"Allocation with ID '{allocationId}' not found.");
        }

        var oldPercentage = allocation.AllocationPercentage;

        // Update fields if provided
        if (newPercentage.HasValue)
        {
            if (newPercentage.Value < 0 || newPercentage.Value > 100)
            {
                return (false, "Allocation percentage must be between 0 and 100.");
            }

            // Check if update would cause over-allocation (only count active allocations)
            var engineerAllocations = allocations.Where(a => a.EngineerId == allocation.EngineerId && a.Id != allocationId && a.Status == "active").ToList();
            var totalAllocation = engineerAllocations.Sum(a => a.AllocationPercentage) + newPercentage.Value;

            if (totalAllocation > 100)
            {
                return (false, $"Cannot update. Engineer would be over-allocated ({totalAllocation}%).");
            }

            allocation.AllocationPercentage = newPercentage.Value;
        }

        if (newStartDate != null)
        {
            allocation.StartDate = newStartDate;
        }

        if (newEndDate != null)
        {
            allocation.EndDate = newEndDate;
        }

        await SaveAllocationsAsync(allocations);

        return (true, $"Successfully updated allocation '{allocationId}'.");
    }

    public async Task<(bool Success, string Message, int RemovedCount)> MoveEngineerToBenchAsync(string engineerId)
    {
        var engineers = await GetEngineersAsync();
        var allocations = await GetAllocationsAsync();

        var engineer = engineers.FirstOrDefault(e => e.Id == engineerId);
        if (engineer == null)
        {
            return (false, $"Engineer with ID '{engineerId}' not found.", 0);
        }

        var engineerAllocations = allocations.Where(a => a.EngineerId == engineerId && a.Status == "active").ToList();

        if (!engineerAllocations.Any())
        {
            return (true, $"Engineer '{engineer.Name}' is already on the bench (no active allocations to remove).", 0);
        }

        var removedCount = engineerAllocations.Count;
        var removedAllocationIds = engineerAllocations.Select(a => a.Id).ToList();

        // Mark all active allocations as cancelled instead of deleting them
        foreach (var allocation in engineerAllocations)
        {
            allocation.Status = "cancelled";
        }
        await SaveAllocationsAsync(allocations);

        var removedProjects = string.Join(", ", removedAllocationIds);
        return (true, $"Successfully moved {engineer.Name} to bench. Cancelled {removedCount} allocation(s): {removedProjects}.", removedCount);
    }

    public async Task<string> GetEngineerAllocationsAsync(string engineerId)
    {
        var engineers = await GetEngineersAsync();
        var projects = await GetProjectsAsync();
        var allocations = await GetAllocationsAsync();

        var engineer = engineers.FirstOrDefault(e => e.Id == engineerId);
        if (engineer == null)
        {
            return $"Engineer with ID '{engineerId}' not found.";
        }

        // Only show active allocations
        var engineerAllocations = allocations.Where(a => a.EngineerId == engineerId && a.Status == "active").ToList();

        if (!engineerAllocations.Any())
        {
            return $"Engineer: {engineer.Name} ({engineer.Role})\nStatus: On Bench (0% allocated)";
        }

        var totalAllocation = engineerAllocations.Sum(a => a.AllocationPercentage);
        var result = $"Engineer: {engineer.Name} ({engineer.Role})\n";
        result += $"Total Allocation: {totalAllocation}%\n\n";
        result += "Current Allocations:\n";

        foreach (var allocation in engineerAllocations)
        {
            var project = projects.FirstOrDefault(p => p.Id == allocation.ProjectId);
            result += $"  - {project?.Name ?? allocation.ProjectId}: {allocation.AllocationPercentage}% ({allocation.StartDate} to {allocation.EndDate})\n";
        }

        return result;
    }

    public async Task<string> GetBenchEngineersAsync()
    {
        var engineers = await GetEngineersAsync();
        var allocations = await GetAllocationsAsync();

        var benchEngineers = new List<Engineer>();

        foreach (var engineer in engineers)
        {
            // Only count active allocations
            var totalAllocation = allocations
                .Where(a => a.EngineerId == engineer.Id && a.Status == "active")
                .Sum(a => a.AllocationPercentage);

            if (totalAllocation == 0)
            {
                benchEngineers.Add(engineer);
            }
        }

        if (!benchEngineers.Any())
        {
            return "No engineers are currently on the bench. All engineers are allocated.";
        }

        var result = $"Engineers on Bench ({benchEngineers.Count}):\n\n";

        foreach (var engineer in benchEngineers)
        {
            result += $"  - {engineer.Name} ({engineer.Role})\n";
            result += $"    Skills: {string.Join(", ", engineer.Skills)}\n";
        }

        return result;
    }

    public async Task<string> GetAllAllocationsAsync()
    {
        var engineers = await GetEngineersAsync();
        var projects = await GetProjectsAsync();
        var allocations = await GetAllocationsAsync();

        // Only show active allocations
        var activeAllocations = allocations.Where(a => a.Status == "active").ToList();

        if (!activeAllocations.Any())
        {
            return "No active allocations found.";
        }

        var result = "All Active Allocations:\n\n";

        foreach (var allocation in activeAllocations)
        {
            var engineer = engineers.FirstOrDefault(e => e.Id == allocation.EngineerId);
            var project = projects.FirstOrDefault(p => p.Id == allocation.ProjectId);

            result += $"  [{allocation.Id}] {engineer?.Name ?? allocation.EngineerId} → {project?.Name ?? allocation.ProjectId}\n";
            result += $"    Allocation: {allocation.AllocationPercentage}%\n";
            result += $"    Duration: {allocation.StartDate} to {allocation.EndDate}\n\n";
        }

        return result;
    }

    public async Task<string> GetProjectAllocationHistoryAsync(string projectId)
    {
        var engineers = await GetEngineersAsync();
        var projects = await GetProjectsAsync();
        var allocations = await GetAllocationsAsync();

        var project = projects.FirstOrDefault(p => p.Id == projectId);
        if (project == null)
        {
            return $"Project with ID '{projectId}' not found.";
        }

        var projectAllocations = allocations.Where(a => a.ProjectId == projectId).ToList();

        if (!projectAllocations.Any())
        {
            return $"Project: {project.Name}\n\nNo allocation history found for this project.";
        }

        var result = $"Project: {project.Name} ({project.Status})\n";
        result += $"Description: {project.Description}\n\n";
        result += "Allocation History:\n\n";

        var activeAllocations = projectAllocations.Where(a => a.Status == "active").ToList();
        var completedAllocations = projectAllocations.Where(a => a.Status == "completed").ToList();
        var cancelledAllocations = projectAllocations.Where(a => a.Status == "cancelled").ToList();

        if (activeAllocations.Any())
        {
            result += "CURRENT (Active):\n";
            foreach (var allocation in activeAllocations)
            {
                var engineer = engineers.FirstOrDefault(e => e.Id == allocation.EngineerId);
                result += $"  - {engineer?.Name ?? allocation.EngineerId} ({engineer?.Role}): {allocation.AllocationPercentage}%\n";
                result += $"    Period: {allocation.StartDate} to {allocation.EndDate}\n";
            }
            result += "\n";
        }

        if (completedAllocations.Any())
        {
            result += "COMPLETED:\n";
            foreach (var allocation in completedAllocations)
            {
                var engineer = engineers.FirstOrDefault(e => e.Id == allocation.EngineerId);
                result += $"  - {engineer?.Name ?? allocation.EngineerId} ({engineer?.Role}): {allocation.AllocationPercentage}%\n";
                result += $"    Period: {allocation.StartDate} to {allocation.EndDate}\n";
            }
            result += "\n";
        }

        if (cancelledAllocations.Any())
        {
            result += "CANCELLED:\n";
            foreach (var allocation in cancelledAllocations)
            {
                var engineer = engineers.FirstOrDefault(e => e.Id == allocation.EngineerId);
                result += $"  - {engineer?.Name ?? allocation.EngineerId} ({engineer?.Role}): {allocation.AllocationPercentage}%\n";
                result += $"    Period: {allocation.StartDate} to {allocation.EndDate}\n";
            }
        }

        return result;
    }

    public async Task<string> GetEngineerAllocationHistoryAsync(string engineerId)
    {
        var engineers = await GetEngineersAsync();
        var projects = await GetProjectsAsync();
        var allocations = await GetAllocationsAsync();

        var engineer = engineers.FirstOrDefault(e => e.Id == engineerId);
        if (engineer == null)
        {
            return $"Engineer with ID '{engineerId}' not found.";
        }

        var engineerAllocations = allocations.Where(a => a.EngineerId == engineerId).ToList();

        if (!engineerAllocations.Any())
        {
            return $"Engineer: {engineer.Name} ({engineer.Role})\n\nNo allocation history found.";
        }

        var result = $"Engineer: {engineer.Name} ({engineer.Role})\n";
        result += $"Skills: {string.Join(", ", engineer.Skills)}\n\n";
        result += "Allocation History:\n\n";

        var activeAllocations = engineerAllocations.Where(a => a.Status == "active").ToList();
        var completedAllocations = engineerAllocations.Where(a => a.Status == "completed").ToList();
        var cancelledAllocations = engineerAllocations.Where(a => a.Status == "cancelled").ToList();

        if (activeAllocations.Any())
        {
            var totalActive = activeAllocations.Sum(a => a.AllocationPercentage);
            result += $"CURRENT (Active) - Total: {totalActive}%:\n";
            foreach (var allocation in activeAllocations)
            {
                var project = projects.FirstOrDefault(p => p.Id == allocation.ProjectId);
                result += $"  - {project?.Name ?? allocation.ProjectId}: {allocation.AllocationPercentage}%\n";
                result += $"    Period: {allocation.StartDate} to {allocation.EndDate}\n";
            }
            result += "\n";
        }

        if (completedAllocations.Any())
        {
            result += "COMPLETED:\n";
            foreach (var allocation in completedAllocations)
            {
                var project = projects.FirstOrDefault(p => p.Id == allocation.ProjectId);
                result += $"  - {project?.Name ?? allocation.ProjectId}: {allocation.AllocationPercentage}%\n";
                result += $"    Period: {allocation.StartDate} to {allocation.EndDate}\n";
            }
            result += "\n";
        }

        if (cancelledAllocations.Any())
        {
            result += "CANCELLED:\n";
            foreach (var allocation in cancelledAllocations)
            {
                var project = projects.FirstOrDefault(p => p.Id == allocation.ProjectId);
                result += $"  - {project?.Name ?? allocation.ProjectId}: {allocation.AllocationPercentage}%\n";
                result += $"    Period: {allocation.StartDate} to {allocation.EndDate}\n";
            }
        }

        return result;
    }
}
