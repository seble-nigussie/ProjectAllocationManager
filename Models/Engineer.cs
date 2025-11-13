namespace ProjectAllocationManager.Models;

public class Engineer
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public List<string> Skills { get; set; } = new();
}
