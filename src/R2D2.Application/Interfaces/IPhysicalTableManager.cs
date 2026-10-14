using R2D2.Domain.Entities;

namespace R2D2.Application.Interfaces;

public interface IPhysicalTableManager
{
    Task CreateTableAsync(string tableName);
    Task AddColumnAsync(string tableName, DynamicField field);
    Task RenameColumnAsync(string tableName, string oldName, string newName);
    Task DropTableAsync(string tableName);
}
