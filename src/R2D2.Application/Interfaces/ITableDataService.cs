using R2D2.Application.DTOs;

namespace R2D2.Application.Interfaces;

public interface ITableDataService
{
    Task<IEnumerable<TableDataDto>> GetByListIdAsync(Guid listId);
    Task<TableDataDto?> GetByIdAsync(Guid id);
    Task<TableDataDto> CreateAsync(CreateTableDataDto dto);
    Task<TableDataDto?> UpdateAsync(Guid id, UpdateTableDataDto dto);
    Task<bool> DeleteAsync(Guid id);
}
