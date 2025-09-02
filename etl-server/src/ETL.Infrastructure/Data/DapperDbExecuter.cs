using System.Data;
using Dapper;

namespace ETL.Infrastructure.Data;

public class DapperDbExecutor : IDbExecutor
{
    private readonly IDbConnection _connection;

    public DapperDbExecutor(IDbConnection connection)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
    }

    public Task<IEnumerable<T>> QueryAsync<T>(string sql, object? param = null, IDbTransaction? transaction = null)
        => _connection.QueryAsync<T>(sql, param, transaction);

    public Task<T?> QuerySingleOrDefaultAsync<T>(string sql, object? param = null, IDbTransaction? transaction = null)
        => _connection.QuerySingleOrDefaultAsync<T>(sql, param, transaction);

    public Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? param = null, IDbTransaction? transaction = null)
        => _connection.QueryFirstOrDefaultAsync<T>(sql, param, transaction);

    public Task ExecuteAsync(string sql, object? param = null, IDbTransaction? transaction = null)
        => _connection.ExecuteAsync(sql, param, transaction);
}
