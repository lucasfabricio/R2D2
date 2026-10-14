using R2D2.Application.DTOs;

namespace R2D2.Application.Interfaces;

public interface IDynamicListService
{
    Task<IEnumerable<DynamicListDto>> GetAllAsync();
    Task<DynamicListDto?> GetByIdAsync(Guid id);
    Task<DynamicListDto> CreateAsync(CreateDynamicListDto dto);
    Task<DynamicListDto?> UpdateAsync(Guid id, UpdateDynamicListDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<DynamicFieldDto> AddFieldAsync(Guid listId, CreateDynamicFieldDto dto);
    Task<bool> RemoveFieldAsync(Guid listId, Guid fieldId);
    Task<DynamicFieldDto?> UpdateFieldAsync(Guid listId, Guid fieldId, UpdateDynamicFieldDto dto);
}
