using System.Data;
using Dapper;
using ETL.Application.Abstractions.Repositories;
using ETL.Domain.Entities;

namespace ETL.Infrastructure.Repository;

public class DataSetRepository : IDataSetRepository
{
    private readonly IDbConnection _db;
    private IDbTransaction? _transaction;


    public DataSetRepository(IDbConnection db) => _db = db;
    
    public void SetTransaction(IDbTransaction? transaction)
    {
        _transaction = transaction;
    }

    public async Task<IEnumerable<DataSetMetadata>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var rows = await _db.QueryAsync<(Guid Id, string TableName, string UploadedByUserId, DateTime UploadedAt)>(
            "SELECT id, table_name, uploaded_by_user_id, uploaded_at FROM public.data_sets;", _transaction);
        return rows.Select(r => new DataSetMetadata(r.Id, r.TableName, r.UploadedByUserId, r.UploadedAt));
    }

    public async Task<DataSetMetadata?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var row = await _db.QuerySingleOrDefaultAsync<(Guid Id, string TableName, string UploadedByUserId, DateTime UploadedAt)?>(
            "SELECT id, table_name, uploaded_by_user_id, uploaded_at FROM public.data_sets WHERE id = @Id;",
            new { Id = id }, _transaction);
        if (row == null) return null;
        var r = row.Value;
        return new DataSetMetadata(r.Id, r.TableName, r.UploadedByUserId, r.UploadedAt);
    }
    
    public async Task<DataSetMetadata?> GetByTableNameAsync(string tableName, CancellationToken cancellationToken = default)
    {
        return await _db.QueryFirstOrDefaultAsync<DataSetMetadata>(
            "SELECT * FROM \"data_sets\" WHERE \"table_name\" = @TableName", new { TableName = tableName }, _transaction);
    }
    

    public async Task AddAsync(DataSetMetadata dataSet, CancellationToken cancellationToken = default)
    {
        var sql = @"INSERT INTO public.data_sets (id, table_name, uploaded_by_user_id, uploaded_at)
                    VALUES (@Id, @TableName, @UploadedByUserId, @UploadedAt);";
        await _db.ExecuteAsync(sql, new
        {
            Id = dataSet.Id,
            TableName = dataSet.TableName,
            UploadedByUserId = dataSet.UploadedByUserId,
            UploadedAt = dataSet.UploadedAt
        }, _transaction);
    }

    public Task UpdateAsync(DataSetMetadata dataSet, CancellationToken cancellationToken = default)
    {
        var sql = @"UPDATE public.data_sets SET user_friendly_name = @UserFriendlyName WHERE id = @Id;";
        _db.Execute(sql, new { UserFriendlyName = dataSet.TableName, dataSet.Id }, _transaction);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(DataSetMetadata dataSet, CancellationToken cancellationToken = default)
    {
        var sql = "DELETE FROM public.data_sets WHERE id = @Id;";
        _db.Execute(sql, new { dataSet.Id }, _transaction);
        return Task.CompletedTask;
    }
}
