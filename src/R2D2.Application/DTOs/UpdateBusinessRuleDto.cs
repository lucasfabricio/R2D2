using R2D2.Domain.Enums;

namespace R2D2.Application.DTOs;

public record UpdateBusinessRuleDto(
    string Name,
    string? Description,
    RuleType Type,
    string ConfigurationJson,
    bool IsActive
);
