namespace R2D2.Application.RuleEngine.Configs;

public class ValidationRuleConfig
{
    public string FieldName { get; set; } = string.Empty;
    public bool Required { get; set; }
    public string? Pattern { get; set; }
}
