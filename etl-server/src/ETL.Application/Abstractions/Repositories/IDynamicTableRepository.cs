namespace ETL.Application.Abstractions.Repositories;

public interface IDynamicTableRepository : IRepository
{
    Task CreateTableFromCsvAsync(string tableName, Stream csvStream, CancellationToken cancellationToken = default);
    Task RenameColumnAsync(string tableName, string oldColumnName, string newColumnName, CancellationToken cancellationToken = default);
}