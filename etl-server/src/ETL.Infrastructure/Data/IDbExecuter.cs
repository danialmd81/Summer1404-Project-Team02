using System.Data;

namespace ETL.Infrastructure.Data;

public interface IDbExecutor
{
    Task<IEnumerable<T>> QueryAsync<T>(string sql, object? param = null, IDbTransaction? transaction = null);
    Task<T?> QuerySingleOrDefaultAsync<T>(string sql, object? param = null, IDbTransaction? transaction = null);
    Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? param = null, IDbTransaction? transaction = null);
    Task ExecuteAsync(string sql, object? param = null, IDbTransaction? transaction = null);
}
