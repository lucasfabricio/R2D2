using Microsoft.EntityFrameworkCore;
using R2D2.Application.DTOs;
using R2D2.Application.Interfaces;
using R2D2.Domain.Entities;
using R2D2.Domain.Interfaces;

namespace R2D2.Application.Services;

public class ListViewService : IListViewService
{
    private readonly IRepository<ListView> _listViewRepository;
    private readonly IRepository<ListViewColumn> _columnRepository;

    public ListViewService(
        IRepository<ListView> listViewRepository,
        IRepository<ListViewColumn> columnRepository)
    {
        _listViewRepository = listViewRepository;
        _columnRepository = columnRepository;
    }

    public async Task<IEnumerable<ListViewDto>> GetAllAsync()
    {
        var views = await _listViewRepository.Query()
            .Include(lv => lv.List)
            .Include(lv => lv.CreateForm)
            .Include(lv => lv.UpdateForm)
            .Include(lv => lv.Columns)
            .ToListAsync();

        return views.Select(MapToDto);
    }

    public async Task<ListViewDto?> GetByIdAsync(Guid id)
    {
        var view = await _listViewRepository.Query()
            .Include(lv => lv.List)
            .Include(lv => lv.CreateForm)
            .Include(lv => lv.UpdateForm)
            .Include(lv => lv.Columns)
            .FirstOrDefaultAsync(lv => lv.Id == id);

        return view != null ? MapToDto(view) : null;
    }

    public async Task<ListViewDto> CreateAsync(CreateListViewDto dto)
    {
        var view = new ListView
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            DisplayName = dto.DisplayName,
            ListId = dto.ListId,
            CreateFormId = dto.CreateFormId,
            UpdateFormId = dto.UpdateFormId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _listViewRepository.AddAsync(view);

        if (dto.Columns != null)
        {
            foreach (var colDto in dto.Columns)
            {
                var col = new ListViewColumn
                {
                    Id = Guid.NewGuid(),
                    ListViewId = view.Id,
                    FieldTechnicalName = colDto.FieldTechnicalName,
                    Order = colDto.Order,
                    HeaderLabel = colDto.HeaderLabel
                };
                await _columnRepository.AddAsync(col);
            }
        }

        return (await GetByIdAsync(view.Id))!;
    }

    public async Task<ListViewDto?> UpdateAsync(Guid id, UpdateListViewDto dto)
    {
        var view = await _listViewRepository.GetByIdAsync(id);
        if (view == null) return null;

        view.Name = dto.Name;
        view.DisplayName = dto.DisplayName;
        view.ListId = dto.ListId;
        view.CreateFormId = dto.CreateFormId;
        view.UpdateFormId = dto.UpdateFormId;
        view.UpdatedAt = DateTime.UtcNow;

        await _listViewRepository.UpdateAsync(view);
        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var view = await _listViewRepository.GetByIdAsync(id);
        if (view == null) return false;

        await _listViewRepository.DeleteAsync(id);
        return true;
    }

    public async Task<ListViewColumnDto> AddColumnAsync(Guid listViewId, CreateListViewColumnDto dto)
    {
        var col = new ListViewColumn
        {
            Id = Guid.NewGuid(),
            ListViewId = listViewId,
            FieldTechnicalName = dto.FieldTechnicalName,
            Order = dto.Order,
            HeaderLabel = dto.HeaderLabel
        };

        await _columnRepository.AddAsync(col);
        return MapToColumnDto(col);
    }

    public async Task<bool> RemoveColumnAsync(Guid listViewId, Guid columnId)
    {
        var col = await _columnRepository.Query()
            .FirstOrDefaultAsync(c => c.ListViewId == listViewId && c.Id == columnId);

        if (col == null) return false;
        await _columnRepository.DeleteAsync(col.Id);
        return true;
    }

    public async Task<bool> ReplaceColumnsAsync(Guid listViewId, List<CreateListViewColumnDto> columns)
    {
        // Remove existing columns
        var existing = await _columnRepository.Query()
            .Where(c => c.ListViewId == listViewId)
            .ToListAsync();

        foreach (var col in existing)
        {
            await _columnRepository.DeleteAsync(col.Id);
        }

        // Add new columns
        foreach (var dto in columns)
        {
            var col = new ListViewColumn
            {
                Id = Guid.NewGuid(),
                ListViewId = listViewId,
                FieldTechnicalName = dto.FieldTechnicalName,
                Order = dto.Order,
                HeaderLabel = dto.HeaderLabel
            };
            await _columnRepository.AddAsync(col);
        }

        return true;
    }

    private static ListViewDto MapToDto(ListView lv) => new(
        lv.Id,
        lv.Name,
        lv.DisplayName,
        lv.ListId,
        lv.List?.Name ?? "",
        lv.CreateFormId,
        lv.CreateForm?.Name,
        lv.UpdateFormId,
        lv.UpdateForm?.Name,
        lv.CreatedAt,
        lv.UpdatedAt,
        lv.Columns.OrderBy(c => c.Order).Select(MapToColumnDto).ToList()
    );

    private static ListViewColumnDto MapToColumnDto(ListViewColumn c) => new(
        c.Id,
        c.ListViewId,
        c.FieldTechnicalName,
        c.Order,
        c.HeaderLabel
    );
}
