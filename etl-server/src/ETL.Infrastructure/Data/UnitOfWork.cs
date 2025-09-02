using System.Data;
using ETL.Application.Abstractions.Data;
using ETL.Application.Abstractions.Repositories;

namespace ETL.Infrastructure.Data;

public class UnitOfWork : IUnitOfWork, IDisposable
{
    private readonly IDbConnection _connection;
    private IDbTransaction? _transaction;

    public IDataSetRepository DataSets { get; }
    public IStagingTableRepository StagingTables { get; }

    public UnitOfWork(IDbConnection connection, IDataSetRepository dataSets, IStagingTableRepository dynamicTables)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        DataSets = dataSets ?? throw new ArgumentNullException(nameof(dataSets));
        StagingTables = dynamicTables ?? throw new ArgumentNullException(nameof(dynamicTables));
    }

    public void Begin()
    {
        if (_connection.State != ConnectionState.Open)
            _connection.Open();

        _transaction = _connection.BeginTransaction();

        DataSets.SetTransaction(_transaction);
        StagingTables.SetTransaction(_transaction);
    }

    public void Commit()
    {
        if (_transaction == null)
            return;

        try
        {
            _transaction.Commit();
        }
        finally
        {
            DataSets.SetTransaction(null);
            StagingTables.SetTransaction(null);

            _transaction.Dispose();
            _transaction = null;
        }
    }

    public void Rollback()
    {
        if (_transaction == null)
            return;

        try
        {
            _transaction.Rollback();
        }
        finally
        {
            DataSets.SetTransaction(null);
            StagingTables.SetTransaction(null);

            _transaction.Dispose();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _transaction = null;

        _connection.Dispose();
    }
}
