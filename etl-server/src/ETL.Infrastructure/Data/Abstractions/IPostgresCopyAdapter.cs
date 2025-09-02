using System.Data;

namespace ETL.Infrastructure.Data.Abstractions;

/// <summary>
/// Abstraction for starting Postgres text import (COPY ... FROM STDIN).
/// Implementations should call NpgsqlConnection.BeginTextImportAsync(copySql).
/// </summary>
public interface IPostgresCopyAdapter
{
    Task<TextWriter> BeginTextImportAsync(IDbConnection connection, string copySql);
}
