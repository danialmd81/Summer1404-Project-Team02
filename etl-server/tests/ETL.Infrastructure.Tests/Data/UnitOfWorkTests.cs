using System.Data;
using ETL.Application.Abstractions.Repositories;
using ETL.Infrastructure.Data;
using NSubstitute;

namespace ETL.Infrastructure.Tests.Data;

public class UnitOfWorkTests
{
    [Fact]
    public void Begin_ShouldOpenConnectionAndBeginTransactionAndSetTransactions_WhenConnectionIsClosed()
    {
        // Arrange
        var conn = Substitute.For<IDbConnection>();
        conn.State.Returns(ConnectionState.Closed);

        var tx = Substitute.For<IDbTransaction>();
        conn.BeginTransaction().Returns(tx);

        var dataSets = Substitute.For<IDataSetRepository>();
        var staging = Substitute.For<IStagingTableRepository>();

        var uow = new UnitOfWork(conn, dataSets, staging);

        // Act
        uow.Begin();

        // Assert
        conn.Received(1).Open();
        conn.Received(1).BeginTransaction();
        dataSets.Received(1).SetTransaction(tx);
        staging.Received(1).SetTransaction(tx);
    }

    [Fact]
    public void Begin_ShouldNotOpenConnection_WhenConnectionAlreadyOpen()
    {
        // Arrange
        var conn = Substitute.For<IDbConnection>();
        conn.State.Returns(ConnectionState.Open);

        var tx = Substitute.For<IDbTransaction>();
        conn.BeginTransaction().Returns(tx);

        var dataSets = Substitute.For<IDataSetRepository>();
        var staging = Substitute.For<IStagingTableRepository>();

        var uow = new UnitOfWork(conn, dataSets, staging);

        // Act
        uow.Begin();

        // Assert
        conn.DidNotReceive().Open();
        conn.Received(1).BeginTransaction();
        dataSets.Received(1).SetTransaction(tx);
        staging.Received(1).SetTransaction(tx);
    }

    [Fact]
    public void Commit_ShouldCommitAndDisposeAndClearTransactionOnRepositories_WhenTransactionExists()
    {
        // Arrange
        var conn = Substitute.For<IDbConnection>();
        conn.State.Returns(ConnectionState.Closed);

        var tx = Substitute.For<IDbTransaction>();
        conn.BeginTransaction().Returns(tx);

        var dataSets = Substitute.For<IDataSetRepository>();
        var staging = Substitute.For<IStagingTableRepository>();

        var uow = new UnitOfWork(conn, dataSets, staging);

        uow.Begin();

        // Act
        uow.Commit();

        // Assert
        tx.Received(1).Commit();
        tx.Received(1).Dispose();
        dataSets.Received(1).SetTransaction(null);
        staging.Received(1).SetTransaction(null);
    }

    [Fact]
    public void Rollback_ShouldRollbackAndDisposeAndClearTransactionOnRepositories_WhenTransactionExists()
    {
        // Arrange
        var conn = Substitute.For<IDbConnection>();
        conn.State.Returns(ConnectionState.Closed);

        var tx = Substitute.For<IDbTransaction>();
        conn.BeginTransaction().Returns(tx);

        var dataSets = Substitute.For<IDataSetRepository>();
        var staging = Substitute.For<IStagingTableRepository>();

        var uow = new UnitOfWork(conn, dataSets, staging);

        uow.Begin();

        // Act
        uow.Rollback();

        // Assert
        tx.Received(1).Rollback();
        tx.Received(1).Dispose();
        dataSets.Received(1).SetTransaction(null);
        staging.Received(1).SetTransaction(null);
    }

    [Fact]
    public void Dispose_ShouldDisposeTransactionAndConnection_WhenCalledAfterBegin()
    {
        // Arrange
        var conn = Substitute.For<IDbConnection>();
        conn.State.Returns(ConnectionState.Closed);

        var tx = Substitute.For<IDbTransaction>();
        conn.BeginTransaction().Returns(tx);

        var dataSets = Substitute.For<IDataSetRepository>();
        var staging = Substitute.For<IStagingTableRepository>();

        var uow = new UnitOfWork(conn, dataSets, staging);

        uow.Begin();

        // Act
        uow.Dispose();

        // Assert
        tx.Received(1).Dispose();
        conn.Received(1).Dispose();
    }

    [Fact]
    public void Dispose_ShouldDisposeConnection_WhenNoTransactionExists()
    {
        // Arrange
        var conn = Substitute.For<IDbConnection>();
        var dataSets = Substitute.For<IDataSetRepository>();
        var staging = Substitute.For<IStagingTableRepository>();

        var uow = new UnitOfWork(conn, dataSets, staging);

        // Act
        uow.Dispose();

        // Assert
        conn.Received(1).Dispose();
    }
}
