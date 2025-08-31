using System.Data;
using Dapper;
using ETL.Application.Abstractions.Repositories;
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
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _compiler = compiler ?? throw new ArgumentNullException(nameof(compiler));
    }

    public void SetTransaction(IDbTransaction? transaction)
    {
        _transaction = transaction;
    }

    public async Task<IEnumerable<DataSetMetadata>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var query = new Query("data_sets").Select("*");

        var sql = _compiler.Compile(query);

        var rows = await _db.QueryAsync<DataSetMetadata>(sql.Sql, sql.NamedBindings, _transaction);

        return rows;
    }

    public async Task<DataSetMetadata?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var query = new Query("data_sets")
            .Where("id", id)
            .Select("*");

        var sql = _compiler.Compile(query);

        return await _db.QuerySingleOrDefaultAsync<DataSetMetadata?>(sql.Sql, sql.NamedBindings, _transaction);
    }

    public async Task<DataSetMetadata?> GetByTableNameAsync(string tableName, CancellationToken cancellationToken = default)
    {
        var query = new Query("data_sets").Where("table_name", tableName).Select("*");

        var sql = _compiler.Compile(query);

        return await _db.QueryFirstOrDefaultAsync<DataSetMetadata>(sql.Sql, sql.NamedBindings, _transaction);
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
