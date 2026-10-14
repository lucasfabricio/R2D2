using R2D2.Application.DTOs;
using R2D2.Application.Interfaces;
using R2D2.Domain.Entities;
using R2D2.Domain.Interfaces;

namespace R2D2.Application.Services;

public class BusinessRuleService : IBusinessRuleService
{
    private readonly IRepository<BusinessRule> _repository;

    public BusinessRuleService(IRepository<BusinessRule> repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<BusinessRuleDto>> GetAllAsync()
    {
        var rules = await _repository.GetAllAsync();
        return rules.Select(MapToDto);
    }

    public async Task<BusinessRuleDto?> GetByIdAsync(Guid id)
    {
        var rule = await _repository.GetByIdAsync(id);
        return rule != null ? MapToDto(rule) : null;
    }

    public async Task<BusinessRuleDto> CreateAsync(CreateBusinessRuleDto dto)
    {
        var rule = new BusinessRule
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description,
            Type = dto.Type,
            ConfigurationJson = dto.ConfigurationJson,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(rule);
        return MapToDto(rule);
    }

    public async Task<BusinessRuleDto?> UpdateAsync(Guid id, UpdateBusinessRuleDto dto)
    {
        var rule = await _repository.GetByIdAsync(id);
        if (rule == null) return null;

        rule.Name = dto.Name;
        rule.Description = dto.Description;
        rule.Type = dto.Type;
        rule.ConfigurationJson = dto.ConfigurationJson;
        rule.IsActive = dto.IsActive;
        rule.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(rule);
        return MapToDto(rule);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var rule = await _repository.GetByIdAsync(id);
        if (rule == null) return false;

        await _repository.DeleteAsync(id);
        return true;
    }

    private static BusinessRuleDto MapToDto(BusinessRule rule) => new(
        rule.Id,
        rule.Name,
        rule.Description,
        rule.Type,
        rule.ConfigurationJson,
        rule.IsActive,
        rule.CreatedAt,
        rule.UpdatedAt
    );
}
