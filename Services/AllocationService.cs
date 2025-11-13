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

        // Check if engineer is over-allocated
        var existingAllocations = allocations.Where(a => a.EngineerId == engineerId).ToList();
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
            EndDate = endDate
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

            // Check if update would cause over-allocation
            var engineerAllocations = allocations.Where(a => a.EngineerId == allocation.EngineerId && a.Id != allocationId).ToList();
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

        var engineerAllocations = allocations.Where(a => a.EngineerId == engineerId).ToList();

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
            var totalAllocation = allocations
                .Where(a => a.EngineerId == engineer.Id)
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

        if (!allocations.Any())
        {
            return "No allocations found.";
        }

        var result = "All Allocations:\n\n";

        foreach (var allocation in allocations)
        {
            var engineer = engineers.FirstOrDefault(e => e.Id == allocation.EngineerId);
            var project = projects.FirstOrDefault(p => p.Id == allocation.ProjectId);

            result += $"  [{allocation.Id}] {engineer?.Name ?? allocation.EngineerId} → {project?.Name ?? allocation.ProjectId}\n";
            result += $"    Allocation: {allocation.AllocationPercentage}%\n";
            result += $"    Duration: {allocation.StartDate} to {allocation.EndDate}\n\n";
        }

        return result;
    }
}
