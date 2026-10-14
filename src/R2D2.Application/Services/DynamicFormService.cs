using Microsoft.EntityFrameworkCore;
using R2D2.Application.DTOs;
using R2D2.Application.Interfaces;
using R2D2.Domain.Entities;
using R2D2.Domain.Interfaces;

namespace R2D2.Application.Services;

public class DynamicFormService : IDynamicFormService
{
    private readonly IRepository<DynamicForm> _formRepository;
    private readonly IRepository<FormField> _formFieldRepository;

    public DynamicFormService(
        IRepository<DynamicForm> formRepository,
        IRepository<FormField> formFieldRepository)
    {
        _formRepository = formRepository;
        _formFieldRepository = formFieldRepository;
    }

    public async Task<IEnumerable<DynamicFormDto>> GetAllAsync()
    {
        var forms = await _formRepository.Query()
            .Include(f => f.Workflow)
            .Include(f => f.Fields)
                .ThenInclude(ff => ff.LinkedList)
            .Include(f => f.Fields)
                .ThenInclude(ff => ff.LinkedField)
            .ToListAsync();

        return forms.Select(MapToDto);
    }

    public async Task<DynamicFormDto?> GetByIdAsync(Guid id)
    {
        var form = await _formRepository.Query()
            .Include(f => f.Workflow)
            .Include(f => f.Fields)
                .ThenInclude(ff => ff.LinkedList)
            .Include(f => f.Fields)
                .ThenInclude(ff => ff.LinkedField)
            .FirstOrDefaultAsync(f => f.Id == id);

        return form != null ? MapToDto(form) : null;
    }

    public async Task<DynamicFormDto> CreateAsync(CreateDynamicFormDto dto)
    {
        var form = new DynamicForm
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description,
            FormType = dto.FormType,
            WorkflowId = dto.WorkflowId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _formRepository.AddAsync(form);

        if (dto.Fields != null)
        {
            foreach (var fieldDto in dto.Fields)
            {
                var formField = new FormField
                {
                    Id = Guid.NewGuid(),
                    FormId = form.Id,
                    Label = fieldDto.Label,
                    FieldType = fieldDto.FieldType,
                    Order = fieldDto.Order,
                    ColumnSize = fieldDto.ColumnSize,
                    IsReadOnly = fieldDto.IsReadOnly,
                    LinkedListId = fieldDto.LinkedListId,
                    LinkedFieldId = fieldDto.LinkedFieldId,
                    ParentFormFieldId = fieldDto.ParentFormFieldId,
                    CascadeFilterField = fieldDto.CascadeFilterField,
                    SelectValueField = fieldDto.SelectValueField,
                    SelectDisplayField = fieldDto.SelectDisplayField,
                    SelectSourceListId = fieldDto.SelectSourceListId,
                    SelectSourceFilterField = fieldDto.SelectSourceFilterField,
                    SelectSourceFilterValue = fieldDto.SelectSourceFilterValue
                };
                await _formFieldRepository.AddAsync(formField);
            }
        }

        return (await GetByIdAsync(form.Id))!;
    }

    public async Task<DynamicFormDto?> UpdateAsync(Guid id, UpdateDynamicFormDto dto)
    {
        var form = await _formRepository.GetByIdAsync(id);
        if (form == null) return null;

        form.Name = dto.Name;
        form.Description = dto.Description;
        form.FormType = dto.FormType;
        form.WorkflowId = dto.WorkflowId;
        form.UpdatedAt = DateTime.UtcNow;

        await _formRepository.UpdateAsync(form);

        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var form = await _formRepository.GetByIdAsync(id);
        if (form == null) return false;

        await _formRepository.DeleteAsync(id);
        return true;
    }

    public async Task<FormFieldDto> AddFieldAsync(Guid formId, CreateFormFieldDto dto)
    {
        var formField = new FormField
        {
            Id = Guid.NewGuid(),
            FormId = formId,
            Label = dto.Label,
            FieldType = dto.FieldType,
            Order = dto.Order,
            ColumnSize = dto.ColumnSize,
            IsReadOnly = dto.IsReadOnly,
            LinkedListId = dto.LinkedListId,
            LinkedFieldId = dto.LinkedFieldId,
            ParentFormFieldId = dto.ParentFormFieldId,
            CascadeFilterField = dto.CascadeFilterField,
            SelectValueField = dto.SelectValueField,
            SelectDisplayField = dto.SelectDisplayField,
            SelectSourceListId = dto.SelectSourceListId,
            SelectSourceFilterField = dto.SelectSourceFilterField,
            SelectSourceFilterValue = dto.SelectSourceFilterValue
        };

        await _formFieldRepository.AddAsync(formField);

        // Reload with navigation properties
        var saved = await _formFieldRepository.Query()
            .Include(ff => ff.LinkedList)
            .Include(ff => ff.LinkedField)
            .FirstOrDefaultAsync(ff => ff.Id == formField.Id);

        return MapToFieldDto(saved ?? formField);
    }

    public async Task<bool> RemoveFieldAsync(Guid formId, Guid formFieldId)
    {
        var formField = await _formFieldRepository.Query()
            .FirstOrDefaultAsync(ff => ff.FormId == formId && ff.Id == formFieldId);

        if (formField == null) return false;

        await _formFieldRepository.DeleteAsync(formField.Id);
        return true;
    }

    public async Task<bool> UpdateFieldOrderAsync(Guid formId, List<Guid> fieldOrder)
    {
        var formFields = await _formFieldRepository.Query()
            .Where(ff => ff.FormId == formId)
            .ToListAsync();

        for (int i = 0; i < fieldOrder.Count; i++)
        {
            var formField = formFields.FirstOrDefault(ff => ff.Id == fieldOrder[i]);
            if (formField != null)
            {
                formField.Order = i;
                await _formFieldRepository.UpdateAsync(formField);
            }
        }

        return true;
    }

    public async Task<FormFieldDto?> UpdateFieldAsync(Guid formId, Guid formFieldId, UpdateFormFieldDto dto)
    {
        var formField = await _formFieldRepository.Query()
            .Include(ff => ff.LinkedList)
            .Include(ff => ff.LinkedField)
            .FirstOrDefaultAsync(ff => ff.FormId == formId && ff.Id == formFieldId);

        if (formField == null) return null;

        formField.Label = dto.Label;
        formField.FieldType = dto.FieldType;
        formField.Order = dto.Order;
        formField.ColumnSize = dto.ColumnSize;
        formField.IsReadOnly = dto.IsReadOnly;
        formField.LinkedListId = dto.LinkedListId;
        formField.LinkedFieldId = dto.LinkedFieldId;
        formField.ParentFormFieldId = dto.ParentFormFieldId;
        formField.CascadeFilterField = dto.CascadeFilterField;
        formField.SelectValueField = dto.SelectValueField;
        formField.SelectDisplayField = dto.SelectDisplayField;
        formField.SelectSourceListId = dto.SelectSourceListId;
        formField.SelectSourceFilterField = dto.SelectSourceFilterField;
        formField.SelectSourceFilterValue = dto.SelectSourceFilterValue;

        await _formFieldRepository.UpdateAsync(formField);

        // Reload navigation properties
        var updated = await _formFieldRepository.Query()
            .Include(ff => ff.LinkedList)
            .Include(ff => ff.LinkedField)
            .FirstOrDefaultAsync(ff => ff.Id == formField.Id);

        return MapToFieldDto(updated ?? formField);
    }

    private static DynamicFormDto MapToDto(DynamicForm form) => new(
        form.Id,
        form.Name,
        form.Description,
        form.FormType,
        form.WorkflowId,
        form.Workflow?.Name,
        form.CreatedAt,
        form.UpdatedAt,
        form.Fields.OrderBy(f => f.Order).Select(MapToFieldDto).ToList()
    );

    private static FormFieldDto MapToFieldDto(FormField ff) => new(
        ff.Id,
        ff.FormId,
        ff.Label,
        ff.FieldType,
        ff.Order,
        ff.ColumnSize,
        ff.IsReadOnly,
        ff.LinkedListId,
        ff.LinkedList?.Name,
        ff.LinkedFieldId,
        ff.LinkedField?.TechnicalName,
        ff.ParentFormFieldId,
        ff.CascadeFilterField,
        ff.SelectSourceListId,
        ff.SelectValueField,
        ff.SelectDisplayField,
        ff.SelectSourceFilterField,
        ff.SelectSourceFilterValue
    );
}
