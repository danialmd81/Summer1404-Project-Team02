using System.Data;
using Dapper;
using ETL.Application.Abstractions.Repositories;
using ETL.Domain.Entities;
using SqlKata;
using SqlKata.Compilers;

namespace ETL.Infrastructure.Repository;

public class DataSetRepository : IDataSetRepository
{
    private readonly IDbConnection _db;
    private readonly Compiler _compiler;
    private IDbTransaction? _transaction;

    public DataSetRepository(IDbConnection db, Compiler compiler)
    {
        _db = db;
        _compiler = compiler;
    }

    public void SetTransaction(IDbTransaction? transaction)
    {
        _transaction = transaction;
    }

    public async Task<IEnumerable<DataSetMetadata>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var query = new Query("data_sets")
            .Select("id", "table_name", "uploaded_by_user_id", "uploaded_at");

        var sql = _compiler.Compile(query);

        var rows = await _db.QueryAsync<(Guid Id, string TableName, string UploadedByUserId, DateTime UploadedAt)>(
            sql.Sql, sql.NamedBindings, _transaction);

        return rows.Select(r => new DataSetMetadata(r.Id, r.TableName, r.UploadedByUserId, r.UploadedAt));
    }

    public async Task<DataSetMetadata?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var query = new Query("data_sets")
            .Where("id", id)
            .Select("id", "table_name", "uploaded_by_user_id", "uploaded_at");

        var sql = _compiler.Compile(query);

        var row = await _db.QuerySingleOrDefaultAsync<(Guid Id, string TableName, string UploadedByUserId, DateTime UploadedAt)?>( 
            sql.Sql, sql.NamedBindings, _transaction);

        if (row == null) return null;
        var r = row.Value;
        return new DataSetMetadata(r.Id, r.TableName, r.UploadedByUserId, r.UploadedAt);
    }

    public async Task<DataSetMetadata?> GetByTableNameAsync(string tableName, CancellationToken cancellationToken = default)
    {
        var query = new Query("data_sets").Where("table_name", tableName).Select("*");

        var sql = _compiler.Compile(query);

        return await _db.QueryFirstOrDefaultAsync<DataSetMetadata>(
            sql.Sql, sql.NamedBindings, _transaction);
    }

    public async Task AddAsync(DataSetMetadata dataSet, CancellationToken cancellationToken = default)
    {
        var query = new Query("data_sets").AsInsert(new
        {
            id = dataSet.Id,
            table_name = dataSet.TableName,
            uploaded_by_user_id = dataSet.UploadedByUserId,
            uploaded_at = dataSet.UploadedAt
        });

        var sql = _compiler.Compile(query);

        await _db.ExecuteAsync(sql.Sql, sql.NamedBindings, _transaction);
    }

    public Task UpdateAsync(DataSetMetadata dataSet, CancellationToken cancellationToken = default)
    {
        var query = new Query("data_sets")
            .Where("id", dataSet.Id)
            .AsUpdate(new { table_name = dataSet.TableName });

        var sql = _compiler.Compile(query);

        _db.Execute(sql.Sql, sql.NamedBindings, _transaction);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(DataSetMetadata dataSet, CancellationToken cancellationToken = default)
    {
        var query = new Query("data_sets").Where("id", dataSet.Id).AsDelete();

        var sql = _compiler.Compile(query);

        _db.Execute(sql.Sql, sql.NamedBindings, _transaction);
        return Task.CompletedTask;
    }
}
