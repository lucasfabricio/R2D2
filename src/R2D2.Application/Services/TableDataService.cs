using System.Data;
using System.Text.Json;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using R2D2.Application.DTOs;
using R2D2.Application.Interfaces;
using R2D2.Domain.Entities;
using R2D2.Domain.Interfaces;

namespace R2D2.Application.Services;

public class TableDataService : ITableDataService
{
    private readonly IRepository<DynamicList> _listRepository;
    private readonly string _connectionString;

    public TableDataService(
        IRepository<DynamicList> listRepository,
        IConfiguration configuration)
    {
        _listRepository = listRepository;
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new ArgumentNullException("Connection string not found");
    }

    private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

    private string Sanitize(string input)
    {
        return new string(input.Where(c => char.IsLetterOrDigit(c) || c == '_').ToArray());
    }

    public async Task<IEnumerable<TableDataDto>> GetByListIdAsync(Guid listId)
    {
        var list = await _listRepository.GetByIdAsync(listId);
        if (list == null) return Enumerable.Empty<TableDataDto>();

        var tableName = "DATA_" + Sanitize(list.Name);
        var sql = $"SELECT * FROM [{tableName}]";

        try
        {
            using var db = CreateConnection();
            var result = await db.QueryAsync<dynamic>(sql);
            
            return result.Select(row => (TableDataDto)MapToDto(row, listId)).ToList();
        }
        catch (SqlException)
        {
            // Table might not exist yet if created via migration or other means
            return Enumerable.Empty<TableDataDto>();
        }
    }

    public async Task<TableDataDto?> GetByIdAsync(Guid id)
    {
        // Finding which list this ID belongs to is tricky without the listId.
        // For now, we assume the caller knows specific IDs if they are calling this.
        // But since we removed TableData table, we can't look up by ID globally easily 
        // unless we join all tables or change the interface.
        // The current interface assumes we can get by ID. 
        // We might need to scan all lists or change how we access data.
        
        // Strategy: Search in all dynamic tables (expensive) OR rely on the context.
        // Given the MVP nature, we will implement optimized search if listId is known, 
        // but the interface only provides Id.
        
        var lists = await _listRepository.GetAllAsync();
        foreach (var list in lists)
        {
            var tableName = "DATA_" + Sanitize(list.Name);
            var sql = $"SELECT * FROM [{tableName}] WHERE Id = @Id";
            
            try
            {
                using var db = CreateConnection();
                var result = await db.QueryFirstOrDefaultAsync<dynamic>(sql, new { Id = id });
                
                if (result != null)
                {
                    return MapToDto(result, list.Id);
                }
            }
            catch {}
        }

        return null;
    }

    public async Task<TableDataDto> CreateAsync(CreateTableDataDto dto)
    {
        var list = await _listRepository.Query()
            .Include(l => l.Fields)
            .FirstOrDefaultAsync(l => l.Id == dto.ListId);
            
        if (list == null) throw new Exception("List not found");

        var tableName = "DATA_" + Sanitize(list.Name);
        var fields = list.Fields;
        
        var columns = new List<string>();
        var parameters = new DynamicParameters();
        var id = Guid.NewGuid();
        
        columns.Add("Id");
        parameters.Add("Id", id);
        
        columns.Add("CreatedAt");
        parameters.Add("CreatedAt", DateTime.UtcNow);
        
        columns.Add("UpdatedAt");
        parameters.Add("UpdatedAt", DateTime.UtcNow);

        foreach (var field in fields)
        {
            if (dto.Values.TryGetValue(field!.TechnicalName, out var value))
            {
                var safeCol = Sanitize(field.TechnicalName);
                columns.Add(safeCol);
                
                // Convert JsonElement to .NET primitives for Dapper
                parameters.Add(safeCol, ConvertValue(value));
            }
        }

        var sql = $"INSERT INTO [{tableName}] ({string.Join(", ", columns.Select(c => $"[{c}]"))}) VALUES ({string.Join(", ", columns.Select(c => $"@{c}"))})";

        using var db = CreateConnection();
        await db.ExecuteAsync(sql, parameters);

        return await GetByIdInListAsync(id, list.Id, list.Name);
    }

    public async Task<TableDataDto?> UpdateAsync(Guid id, UpdateTableDataDto dto)
    {
        // First find the list
        var existingResult = await FindRecordAndList(id);
        if (existingResult.Record == null) return null;
        
        var list = existingResult.List;
        var tableName = "DATA_" + Sanitize(list.Name);
        
        var fields = list.Fields;

        var setClauses = new List<string>();
        var parameters = new DynamicParameters();
        parameters.Add("Id", id);
        
        setClauses.Add("UpdatedAt = @UpdatedAt");
        parameters.Add("UpdatedAt", DateTime.UtcNow);

        foreach (var field in fields)
        {
            if (dto.Values.TryGetValue(field!.TechnicalName, out var value))
            {
                var safeCol = Sanitize(field.TechnicalName);
                setClauses.Add($"[{safeCol}] = @{safeCol}");
                parameters.Add(safeCol, ConvertValue(value));
            }
        }

        var sql = $"UPDATE [{tableName}] SET {string.Join(", ", setClauses)} WHERE Id = @Id";

        using var db = CreateConnection();
        await db.ExecuteAsync(sql, parameters);

        return await GetByIdInListAsync(id, list.Id, list.Name);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var existingResult = await FindRecordAndList(id);
        if (existingResult.Record == null) return false;

        var tableName = "DATA_" + Sanitize(existingResult.List.Name);
        var sql = $"DELETE FROM [{tableName}] WHERE Id = @Id";

        using var db = CreateConnection();
        await db.ExecuteAsync(sql, new { Id = id });

        return true;
    }

    // Helper to optimize finding record without Scan if possible, or Scan if needed
    private async Task<(dynamic? Record, DynamicList List)> FindRecordAndList(Guid id)
    {
        var lists = await _listRepository.Query()
            .Include(l => l.Fields)
            .ToListAsync();
        foreach (var list in lists)
        {
            var tableName = "DATA_" + Sanitize(list.Name);
            var sql = $"SELECT * FROM [{tableName}] WHERE Id = @Id";
            
            try
            {
                using var db = CreateConnection();
                var result = await db.QueryFirstOrDefaultAsync<dynamic>(sql, new { Id = id });
                
                if (result != null)
                {
                    return (result, list);
                }
            }
            catch {}
        }
        return (null, null!);
    }

    private async Task<TableDataDto> GetByIdInListAsync(Guid id, Guid listId, string listName)
    {
        var tableName = "DATA_" + Sanitize(listName);
        var sql = $"SELECT * FROM [{tableName}] WHERE Id = @Id";
        
        using var db = CreateConnection();
        var result = await db.QueryFirstAsync<dynamic>(sql, new { Id = id });
        
        return MapToDto(result, listId);
    }

    private static TableDataDto MapToDto(dynamic row, Guid listId)
    {
        var dict = (IDictionary<string, object>)row;
        var values = new Dictionary<string, object?>();
        var id = Guid.Empty;
        var createdAt = DateTime.MinValue;
        var updatedAt = DateTime.MinValue;

        foreach (var kvp in dict)
        {
            if (kvp.Key == "Id") id = (Guid)kvp.Value;
            else if (kvp.Key == "CreatedAt") createdAt = (DateTime)kvp.Value;
            else if (kvp.Key == "UpdatedAt") updatedAt = (DateTime)kvp.Value;
            else
            {
                values[kvp.Key] = kvp.Value;
            }
        }

        return new TableDataDto(
            id,
            listId,
            createdAt,
            updatedAt,
            values
        );
    }

    private static object? ConvertValue(object? value)
    {
        if (value is JsonElement je)
        {
            return je.ValueKind switch
            {
                JsonValueKind.String => je.GetString(),
                JsonValueKind.Number => je.TryGetInt64(out var l) ? l : je.GetDouble(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Null => null,
                JsonValueKind.Undefined => null,
                _ => je.GetRawText()
            };
        }
        return value;
    }
}
