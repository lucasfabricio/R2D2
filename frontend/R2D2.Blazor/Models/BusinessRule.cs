namespace R2D2.Blazor.Models;

public class BusinessRule
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public RuleType Type { get; set; } = RuleType.Validation;
    public string ConfigurationJson { get; set; } = "{}";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
