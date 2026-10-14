using R2D2.Application.DTOs;

namespace R2D2.Application.Interfaces;

public interface IBusinessRuleService
{
    Task<IEnumerable<BusinessRuleDto>> GetAllAsync();
    Task<BusinessRuleDto?> GetByIdAsync(Guid id);
    Task<BusinessRuleDto> CreateAsync(CreateBusinessRuleDto dto);
    Task<BusinessRuleDto?> UpdateAsync(Guid id, UpdateBusinessRuleDto dto);
    Task<bool> DeleteAsync(Guid id);
}
