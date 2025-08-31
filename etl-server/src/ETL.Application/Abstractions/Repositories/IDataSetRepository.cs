using ETL.Domain.Entities;

namespace ETL.Application.Abstractions.Repositories;

public interface IDataSetRepository : IRepository
{
    Task<IEnumerable<DataSetMetadata>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<DataSetMetadata?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<DataSetMetadata?> GetByTableNameAsync(string tableName, CancellationToken cancellationToken = default);
    Task AddAsync(DataSetMetadata dataSetMetadata, CancellationToken cancellationToken = default);
    Task UpdateAsync(DataSetMetadata dataSetMetadata, CancellationToken cancellationToken = default);
    Task DeleteAsync(DataSetMetadata dataSetMetadata, CancellationToken cancellationToken = default);

}