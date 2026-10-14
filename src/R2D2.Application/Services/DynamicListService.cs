using Microsoft.EntityFrameworkCore;
using R2D2.Application.DTOs;
using R2D2.Application.Interfaces;
using R2D2.Domain.Entities;
using R2D2.Domain.Interfaces;

namespace R2D2.Application.Services;

public class DynamicListService : IDynamicListService
{
    private readonly IRepository<DynamicList> _listRepository;
    private readonly IRepository<DynamicField> _fieldRepository;
    private readonly IPhysicalTableManager _tableManager;

    public DynamicListService(
        IRepository<DynamicList> listRepository,
        IRepository<DynamicField> fieldRepository,
        IPhysicalTableManager tableManager)
    {
        _listRepository = listRepository;
        _fieldRepository = fieldRepository;
        _tableManager = tableManager;
    }

    public async Task<IEnumerable<DynamicListDto>> GetAllAsync()
    {
        var lists = await _listRepository.Query()
            .Include(l => l.Fields)
            .ToListAsync();

        return lists.Select(MapToDto);
    }

    public async Task<DynamicListDto?> GetByIdAsync(Guid id)
    {
        var list = await _listRepository.Query()
            .Include(l => l.Fields)
            .FirstOrDefaultAsync(l => l.Id == id);

        return list != null ? MapToDto(list) : null;
    }

    public async Task<DynamicListDto> CreateAsync(CreateDynamicListDto dto)
    {
        var list = new DynamicList
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            DisplayName = dto.DisplayName,
            Description = dto.Description,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _listRepository.AddAsync(list);
        await _tableManager.CreateTableAsync(EncodeTableName(list.Name));

        if (dto.Fields != null)
        {
            foreach (var fieldDto in dto.Fields)
            {
                var field = new DynamicField
                {
                    Id = Guid.NewGuid(),
                    ListId = list.Id,
                    TechnicalName = fieldDto.TechnicalName,
                    Label = fieldDto.Label,
                    FieldType = fieldDto.FieldType,
                    IsRequired = fieldDto.IsRequired,
                    MaxLength = fieldDto.MaxLength,
                    RelatedListId = fieldDto.RelatedListId,
                    RelatedFieldTechnicalName = fieldDto.RelatedFieldTechnicalName,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _fieldRepository.AddAsync(field);
                await _tableManager.AddColumnAsync(EncodeTableName(list.Name), field);
            }
        }

        return (await GetByIdAsync(list.Id))!;
    }

    public async Task<DynamicListDto?> UpdateAsync(Guid id, UpdateDynamicListDto dto)
    {
        var list = await _listRepository.GetByIdAsync(id);
        if (list == null) return null;

        list.Name = dto.Name;
        list.DisplayName = dto.DisplayName;
        list.Description = dto.Description;
        list.UpdatedAt = DateTime.UtcNow;

        await _listRepository.UpdateAsync(list);
        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var list = await _listRepository.GetByIdAsync(id);
        if (list == null) return false;

        await _listRepository.DeleteAsync(id);
        return true;
    }

    public async Task<DynamicFieldDto> AddFieldAsync(Guid listId, CreateDynamicFieldDto dto)
    {
        var list = await _listRepository.GetByIdAsync(listId);
        if (list == null) throw new Exception("List not found");

        var field = new DynamicField
        {
            Id = Guid.NewGuid(),
            ListId = listId,
            TechnicalName = dto.TechnicalName,
            Label = dto.Label,
            FieldType = dto.FieldType,
            IsRequired = dto.IsRequired,
            MaxLength = dto.MaxLength,
            RelatedListId = dto.RelatedListId,
            RelatedFieldTechnicalName = dto.RelatedFieldTechnicalName,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _fieldRepository.AddAsync(field);
        await _tableManager.AddColumnAsync(EncodeTableName(list.Name), field);

        return MapToFieldDto(field);
    }

    public async Task<bool> RemoveFieldAsync(Guid listId, Guid fieldId)
    {
        var field = await _fieldRepository.Query()
            .FirstOrDefaultAsync(f => f.ListId == listId && f.Id == fieldId);

        if (field == null) return false;

        await _fieldRepository.DeleteAsync(field.Id);
        return true;
    }

    public async Task<DynamicFieldDto?> UpdateFieldAsync(Guid listId, Guid fieldId, UpdateDynamicFieldDto dto)
    {
        var list = await _listRepository.GetByIdAsync(listId);
        if (list == null) throw new Exception("List not found");

        var field = await _fieldRepository.Query()
            .FirstOrDefaultAsync(f => f.ListId == listId && f.Id == fieldId);

        if (field == null) return null;

        var oldTechnicalName = field.TechnicalName;
        var newTechnicalName = dto.TechnicalName;

        if (oldTechnicalName != newTechnicalName)
        {
            await _tableManager.RenameColumnAsync(EncodeTableName(list.Name), oldTechnicalName, newTechnicalName);
        }

        field.TechnicalName = dto.TechnicalName;
        field.Label = dto.Label;
        field.FieldType = dto.FieldType;
        field.IsRequired = dto.IsRequired;
        field.MaxLength = dto.MaxLength;
        field.RelatedListId = dto.RelatedListId;
        field.RelatedFieldTechnicalName = dto.RelatedFieldTechnicalName;
        field.UpdatedAt = DateTime.UtcNow;

        await _fieldRepository.UpdateAsync(field);

        return MapToFieldDto(field);
    }

    private static DynamicListDto MapToDto(DynamicList list) => new(
        list.Id,
        list.Name,
        list.DisplayName,
        list.Description,
        list.CreatedAt,
        list.UpdatedAt,
        list.Fields.Select(MapToFieldDto).ToList()
    );

    private static DynamicFieldDto MapToFieldDto(DynamicField f) => new(
        f.Id,
        f.ListId,
        f.TechnicalName,
        f.Label,
        f.FieldType,
        f.IsRequired,
        f.MaxLength,
        f.RelatedListId,
        f.RelatedFieldTechnicalName,
        f.CreatedAt,
        f.UpdatedAt
    );

    private string EncodeTableName(string name)
    {
        return new string(name.Where(c => char.IsLetterOrDigit(c) || c == '_').ToArray());
    }
}
