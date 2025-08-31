using ETL.Application.Abstractions.Repositories;

namespace ETL.Application.Abstractions.Data;

public interface IUnitOfWork : IDisposable
{
    IDataSetRepository DataSets { get; }
    IStagingTableRepository DynamicTables { get; }

    void Begin();
    void Commit();
    void Rollback();
}