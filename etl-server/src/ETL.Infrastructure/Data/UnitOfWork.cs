using System.Data;
using ETL.Application.Abstractions.Data;
using ETL.Application.Abstractions.Repositories;

namespace ETL.Infrastructure.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly IDbConnection _connection;
    private IDbTransaction? _transaction;

    public IDataSetRepository DataSets { get; }
    public IDynamicTableRepository DynamicTables { get; }

    public UnitOfWork(IDbConnection connection, IDataSetRepository dataSets, IDynamicTableRepository dynamicTables)
    {
        _connection = connection;
        DataSets = dataSets;
        DynamicTables = dynamicTables;
    }

    public void Begin()
    {
        if (_connection.State != ConnectionState.Open)
            _connection.Open();
        _transaction = _connection.BeginTransaction();

        DataSets.SetTransaction(_transaction);
        DynamicTables.SetTransaction(_transaction);
    }

    public void Commit()
    {
        _transaction?.Commit();
        _transaction = null;
    }

    public void Rollback()
    {
        _transaction?.Rollback();
        _transaction = null;
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _connection.Dispose();
    }
}