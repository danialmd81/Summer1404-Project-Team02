using System.Data;
using ETL.Infrastructure.Data.Abstractions;
using Npgsql;

namespace ETL.Infrastructure.Data;

public class PostgresCopyAdapter : IPostgresCopyAdapter
{
    public async Task<TextWriter> BeginTextImportAsync(IDbConnection connection, string copySql)
    {
        if (connection is not NpgsqlConnection npgsql)
            throw new InvalidOperationException("Database connection is not NpgsqlConnection");

        return await npgsql.BeginTextImportAsync(copySql);
    }
}
