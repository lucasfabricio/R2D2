using R2D2.Application.DTOs;

namespace R2D2.Application.Interfaces;

public interface IDynamicFormService
{
    Task<IEnumerable<DynamicFormDto>> GetAllAsync();
    Task<DynamicFormDto?> GetByIdAsync(Guid id);
    Task<DynamicFormDto> CreateAsync(CreateDynamicFormDto dto);
    Task<DynamicFormDto?> UpdateAsync(Guid id, UpdateDynamicFormDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<FormFieldDto> AddFieldAsync(Guid formId, CreateFormFieldDto dto);
    Task<bool> RemoveFieldAsync(Guid formId, Guid formFieldId);
    Task<bool> UpdateFieldOrderAsync(Guid formId, List<Guid> fieldOrder);
    Task<FormFieldDto?> UpdateFieldAsync(Guid formId, Guid formFieldId, UpdateFormFieldDto dto);
}
