using R2D2.Application.DTOs;

namespace R2D2.Application.Interfaces;

public interface IListViewService
{
    Task<IEnumerable<ListViewDto>> GetAllAsync();
    Task<ListViewDto?> GetByIdAsync(Guid id);
    Task<ListViewDto> CreateAsync(CreateListViewDto dto);
    Task<ListViewDto?> UpdateAsync(Guid id, UpdateListViewDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<ListViewColumnDto> AddColumnAsync(Guid listViewId, CreateListViewColumnDto dto);
    Task<bool> RemoveColumnAsync(Guid listViewId, Guid columnId);
    Task<bool> ReplaceColumnsAsync(Guid listViewId, List<CreateListViewColumnDto> columns);
}
