using R2D2.Domain.Enums;

namespace R2D2.Application.DTOs;

public record CreateBusinessRuleDto(
    string Name,
    string? Description,
    RuleType Type,
    string ConfigurationJson,
    bool IsActive = true
);
