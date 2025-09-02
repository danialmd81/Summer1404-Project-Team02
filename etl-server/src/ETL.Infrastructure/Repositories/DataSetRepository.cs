using System.Data;
using ETL.Application.Abstractions.Repositories;
using ETL.Domain.Entities;
using ETL.Infrastructure.Data.Abstractions;
using SqlKata;

namespace ETL.Infrastructure.Repositories;

public sealed class DataSetRepository : IDataSetRepository
{
    private readonly IDbExecutor _dbExecutor;
    private readonly IQueryCompiler _compiler;
    private IDbTransaction? _transaction;

    public DataSetRepository(IDbExecutor dbExecutor, IQueryCompiler compiler)
    {
        _dbExecutor = dbExecutor ?? throw new ArgumentNullException(nameof(dbExecutor));
        _compiler = compiler ?? throw new ArgumentNullException(nameof(compiler));
    }

    public void SetTransaction(IDbTransaction? transaction)
    {
        _transaction = transaction;
    }

    public async Task<IEnumerable<DataSetMetadata>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var query = new Query("data_sets")
            .Select("id", "table_name as TableName",
                "uploaded_by_user_id as UploadedByUserId",
                "uploaded_at as CreatedAt");

        var sql = _compiler.Compile(query);

        return await _dbExecutor.QueryAsync<DataSetMetadata>(sql.Sql, sql.NamedBindings, _transaction);
    }

    public async Task<DataSetMetadata?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var query = new Query("data_sets")
            .Where("id", id)
            .Select("*");

        var sql = _compiler.Compile(query);

        return await _dbExecutor.QuerySingleOrDefaultAsync<DataSetMetadata?>(sql.Sql, sql.NamedBindings, _transaction);
    }

    public async Task<DataSetMetadata?> GetByTableNameAsync(string tableName, CancellationToken cancellationToken = default)
    {
        var query = new Query("data_sets").Where("table_name", tableName).Select("*");

        var sql = _compiler.Compile(query);

        return await _dbExecutor.QueryFirstOrDefaultAsync<DataSetMetadata>(sql.Sql, sql.NamedBindings, _transaction);
    }

    public async Task AddAsync(DataSetMetadata dataSet, CancellationToken cancellationToken = default)
    {
        var query = new Query("data_sets").AsInsert(new
        {
            id = dataSet.Id,
            table_name = dataSet.TableName,
            uploaded_by_user_id = dataSet.UploadedByUserId,
            uploaded_at = dataSet.CreatedAt,
        });

        var sql = _compiler.Compile(query);

        await _dbExecutor.ExecuteAsync(sql.Sql, sql.NamedBindings, _transaction);
    }

    public async Task UpdateAsync(DataSetMetadata dataSet, CancellationToken cancellationToken = default)
    {
        var query = new Query("data_sets")
            .Where("id", dataSet.Id)
            .AsUpdate(new { table_name = dataSet.TableName });

        var sql = _compiler.Compile(query);

        await _dbExecutor.ExecuteAsync(sql.Sql, sql.NamedBindings, _transaction);
    }

    public async Task DeleteAsync(DataSetMetadata dataSet, CancellationToken cancellationToken = default)
    {
        var query = new Query("data_sets").Where("id", dataSet.Id).AsDelete();

        var sql = _compiler.Compile(query);

        await _dbExecutor.ExecuteAsync(sql.Sql, sql.NamedBindings, _transaction);
    }
}
