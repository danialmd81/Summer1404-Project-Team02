using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using CsvHelper;
using Dapper;
using ETL.Application.Abstractions.Repositories;
using Npgsql;

namespace ETL.Infrastructure.Repositories;

public class StagingTableRepository : IStagingTableRepository
{
    private readonly IDbConnection _dbConnection;
    private IDbTransaction? _transaction;


    public StagingTableRepository(IDbConnection dbConnection)
    {
        _dbConnection = dbConnection ?? throw new ArgumentNullException(nameof(dbConnection));
    }

    public void SetTransaction(IDbTransaction? transaction)
    {
        _transaction = transaction;
    }

    public async Task CreateTableFromCsvAsync(string tableName, Stream csvStream, CancellationToken cancellationToken = default)
    {
        Stream workingStream;
        if (csvStream.CanSeek)
        {
            workingStream = csvStream;
            csvStream.Position = 0;
        }
        else
        {
            workingStream = new MemoryStream();
            await csvStream.CopyToAsync(workingStream, 81920, cancellationToken);
            workingStream.Position = 0;
        }

        using var headerReader = new StreamReader(workingStream, leaveOpen: true);
        using var csv = new CsvReader(headerReader, CultureInfo.InvariantCulture);
        if (!csv.Read()) throw new InvalidOperationException("CSV is empty");
        csv.ReadHeader();
        var headers = csv.HeaderRecord ?? throw new InvalidOperationException("Failed to read CSV header");

        var sanitizedTableName = SanitizeIdentifier(tableName);
        var sanitizedColumns = headers.Select(h => $"{SanitizeIdentifier(h)} TEXT");
        var createTableSql = $"CREATE TABLE IF NOT EXISTS {sanitizedTableName} ({string.Join(", ", sanitizedColumns)});";
        await _dbConnection.ExecuteAsync(createTableSql, _transaction);

        workingStream.Position = 0;

        var copyColumns = string.Join(", ", headers.Select(SanitizeIdentifier));
        var copySql = $"COPY {sanitizedTableName} ({copyColumns}) FROM STDIN (FORMAT CSV, HEADER true)";

        if (_dbConnection is not NpgsqlConnection npgsql) throw new InvalidOperationException("Database connection is not NpgsqlConnection");

        using (var reader = new StreamReader(workingStream, leaveOpen: true))
        using (var writer = await npgsql.BeginTextImportAsync(copySql))
        {
            var buffer = new char[81920];
            while (!cancellationToken.IsCancellationRequested)
            {
                var read = await reader.ReadAsync(buffer, 0, buffer.Length);
                if (read == 0) break;
                await writer.WriteAsync(buffer, 0, read);
            }
            await writer.FlushAsync(cancellationToken);
        }

        if (!csvStream.CanSeek)
        {
            workingStream.Dispose();
        }
    }

    public async Task RenameTableAsync(string oldTableName, string newTableName, CancellationToken cancellationToken = default)
    {
        var sanitizedOld = SanitizeIdentifier(oldTableName);
        var sanitizedNew = SanitizeIdentifier(newTableName);

        var sql = $"ALTER TABLE {sanitizedOld} RENAME TO {sanitizedNew};";
        await _dbConnection.ExecuteAsync(sql, _transaction);
    }

    public async Task RenameColumnAsync(string tableName, string oldColumnName, string newColumnName, CancellationToken cancellationToken = default)
    {
        var sanitizedTable = SanitizeIdentifier(tableName);
        var sanitizedOldCol = SanitizeIdentifier(oldColumnName);
        var sanitizedNewCol = SanitizeIdentifier(newColumnName);

        var sql = $"ALTER TABLE {sanitizedTable} RENAME COLUMN {sanitizedOldCol} TO {sanitizedNewCol};";
        await _dbConnection.ExecuteAsync(sql, _transaction);
    }

    public async Task DeleteTableAsync(string tableName, CancellationToken cancellationToken = default)
    {
        var sanitized = SanitizeIdentifier(tableName);

        var sql = $"DROP TABLE IF EXISTS {sanitized};";
        await _dbConnection.ExecuteAsync(sql, _transaction);
    }


    public async Task DeleteColumnAsync(string tableName, string columnName, CancellationToken cancellationToken = default)
    {
        var sanitizedTable = SanitizeIdentifier(tableName);
        var sanitizedCol = SanitizeIdentifier(columnName);

        var sql = $"ALTER TABLE {sanitizedTable} DROP COLUMN {sanitizedCol};";
        await _dbConnection.ExecuteAsync(sql, _transaction);
    }

    private string SanitizeIdentifier(string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier)) throw new ArgumentException("Identifier cannot be empty.");
        // Only letters, numbers and underscore; remove other chars and keep a max length
        var sanitized = Regex.Replace(identifier, @"[^\w]", "");
        if (sanitized.Length > 63) sanitized = sanitized.Substring(0, 63); // Postgres has 63 char limit by default
        if (string.IsNullOrWhiteSpace(sanitized)) throw new ArgumentException("Invalid identifier format.");
        return $"\"{sanitized}\"";
    }
}
