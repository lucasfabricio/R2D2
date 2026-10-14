namespace R2D2.Application.RuleEngine.Configs;

public class DataRuleConfig
{
    public Guid TargetListId { get; set; }
    public List<string> TargetColumns { get; set; } = new();
    public Dictionary<string, string> FieldMappings { get; set; } = new();
}
