namespace ProjectAllocationManager.Models;

public class Allocation
{
    public string Id { get; set; } = string.Empty;
    public string EngineerId { get; set; } = string.Empty;
    public string ProjectId { get; set; } = string.Empty;
    public int AllocationPercentage { get; set; }
    public string StartDate { get; set; } = string.Empty;
    public string EndDate { get; set; } = string.Empty;
}
