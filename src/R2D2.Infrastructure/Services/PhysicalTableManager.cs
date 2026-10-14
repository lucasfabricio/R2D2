using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using R2D2.Application.Interfaces;
using R2D2.Domain.Entities;
using R2D2.Domain.Enums;

namespace R2D2.Infrastructure.Services;

public class PhysicalTableManager : IPhysicalTableManager
{
    private readonly string _connectionString;

    public PhysicalTableManager(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new ArgumentNullException("Connection string not found");
    }

    private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

    public async Task CreateTableAsync(string tableName)
    {
        var safeTableName = Sanitize(tableName);
        var sql = $@"
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'DATA_{safeTableName}')
            BEGIN
                CREATE TABLE [DATA_{safeTableName}] (
                    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
                    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                    [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE()
                );
            END";

        using var db = CreateConnection();
        await db.ExecuteAsync(sql);
    }

    public async Task AddColumnAsync(string tableName, DynamicField field)
    {
        var safeTableName = Sanitize(tableName);
        var safeColumnName = Sanitize(field.TechnicalName);
        var sqlType = GetSqlType(field);
        
        var sql = $@"
            IF NOT EXISTS (
                SELECT * FROM sys.columns 
                WHERE object_id = OBJECT_ID('DATA_{safeTableName}') 
                AND name = '{safeColumnName}'
            )
            BEGIN
                ALTER TABLE [DATA_{safeTableName}] 
                ADD [{safeColumnName}] {sqlType} NULL;
            END";

        using var db = CreateConnection();
        await db.ExecuteAsync(sql);
    }

    public async Task RenameColumnAsync(string tableName, string oldName, string newName)
    {
        var safeTableName = Sanitize(tableName);
        var safeOldName = Sanitize(oldName);
        var safeNewName = Sanitize(newName);

        var sql = $@"EXEC sp_rename 'DATA_{safeTableName}.{safeOldName}', '{safeNewName}', 'COLUMN'";

        using var db = CreateConnection();
        await db.ExecuteAsync(sql);
    }

    public async Task DropTableAsync(string tableName)
    {
        var safeTableName = Sanitize(tableName);
        var sql = $"DROP TABLE IF EXISTS [DATA_{safeTableName}]";

        using var db = CreateConnection();
        await db.ExecuteAsync(sql);
    }

    private string GetSqlType(DynamicField field)
    {
        return field.FieldType switch
        {
            FieldType.Text => field.MaxLength.HasValue && field.MaxLength.Value > 0 
                ? $"NVARCHAR({field.MaxLength.Value})" 
                : "NVARCHAR(MAX)",
            FieldType.Number => "INT",
            FieldType.Decimal => "DECIMAL(18, 2)",
            FieldType.Date => "DATETIME2",
            FieldType.Boolean => "BIT",
            FieldType.Relationship => "UNIQUEIDENTIFIER",
            _ => "NVARCHAR(MAX)"
        };
    }

    private string Sanitize(string input)
    {
        return new string(input.Where(c => char.IsLetterOrDigit(c) || c == '_').ToArray());
    }
}
