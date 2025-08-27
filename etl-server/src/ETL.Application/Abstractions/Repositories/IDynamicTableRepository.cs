namespace ETL.Application.Abstractions.Repositories;

public interface IDynamicTableRepository : IRepository
{
    Task CreateTableFromCsvAsync(string tableName, Stream csvStream, CancellationToken cancellationToken = default);

    Task RenameColumnAsync(string tableName, string oldColumnName, string newColumnName,
        CancellationToken cancellationToken = default);

    Task RenameTableAsync(string oldTableName, string newTableName, CancellationToken cancellationToken = default);

    Task DeleteTableAsync(string tableName, CancellationToken cancellationToken = default);
    Task DeleteColumnAsync(string tableName, string columnName, CancellationToken cancellationToken = default);
}