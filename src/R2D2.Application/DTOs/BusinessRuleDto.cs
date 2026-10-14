using R2D2.Domain.Enums;

namespace R2D2.Application.DTOs;

public record BusinessRuleDto(
    Guid Id,
    string Name,
    string? Description,
    RuleType Type,
    string ConfigurationJson,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
