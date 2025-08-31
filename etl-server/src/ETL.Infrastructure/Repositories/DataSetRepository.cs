using System.Data;
using Dapper;
using ETL.Application.Abstractions.Repositories;
using ETL.Application.Common.DTOs;
using ETL.Domain.Entities;
using SqlKata;
using SqlKata.Compilers;

namespace ETL.Infrastructure.Repositories;

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

    public async Task<IEnumerable<DataSetDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var query = new Query("data_sets").Select("*");

        var sql = _compiler.Compile(query);

        var rows = await _db.QueryAsync<DataSetDto>(sql.Sql, sql.NamedBindings, _transaction);

        return rows;
    }

    public async Task<DataSetDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var query = new Query("data_sets")
            .Where("id", id)
            .Select("*");

        var sql = _compiler.Compile(query);

        return await _db.QuerySingleOrDefaultAsync<DataSetDto?>(sql.Sql, sql.NamedBindings, _transaction);
    }

    public async Task<DataSetDto?> GetByTableNameAsync(string tableName, CancellationToken cancellationToken = default)
    {
        var query = new Query("data_sets").Where("table_name", tableName).Select("*");

        var sql = _compiler.Compile(query);

        return await _db.QueryFirstOrDefaultAsync<DataSetDto>(sql.Sql, sql.NamedBindings, _transaction);
    }

    public async Task AddAsync(DataSetMetadata dataSet, CancellationToken cancellationToken = default)
    {
        var query = new Query("data_sets").AsInsert(new
        {
            id = dataSet.Id,
            table_name = dataSet.TableName,
            uploaded_by_user_id = dataSet.UploadedByUserId,
        });

        var sql = _compiler.Compile(query);

        await _db.ExecuteAsync(sql.Sql, sql.NamedBindings, _transaction);
    }

    public async Task UpdateAsync(DataSetMetadata dataSet, CancellationToken cancellationToken = default)
    {
        var query = new Query("data_sets")
            .Where("id", dataSet.Id)
            .AsUpdate(new { table_name = dataSet.TableName });

        var sql = _compiler.Compile(query);

        await _db.ExecuteAsync(sql.Sql, sql.NamedBindings, _transaction);
    }

    public async Task DeleteAsync(DataSetMetadata dataSet, CancellationToken cancellationToken = default)
    {
        var query = new Query("data_sets").Where("id", dataSet.Id).AsDelete();

        var sql = _compiler.Compile(query);

        await _db.ExecuteAsync(sql.Sql, sql.NamedBindings, _transaction);
    }
}
