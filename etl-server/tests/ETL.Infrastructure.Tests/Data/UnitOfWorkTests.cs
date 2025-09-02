using System.Data;
using ETL.Application.Abstractions.Repositories;
using ETL.Infrastructure.Data;
using FluentAssertions;
using NSubstitute;

namespace ETL.Infrastructure.Tests;

public class UnitOfWorkTests : IDisposable
{
    private readonly IDbConnection _connection;
    private readonly IDataSetRepository _dataSets;
    private readonly IStagingTableRepository _staging;
    private readonly IDbTransaction _transaction;
    private readonly UnitOfWork _sut;

    public UnitOfWorkTests()
    {
        _connection = Substitute.For<IDbConnection>();
        _dataSets = Substitute.For<IDataSetRepository>();
        _staging = Substitute.For<IStagingTableRepository>();
        _transaction = Substitute.For<IDbTransaction>();

        _connection.State.Returns(ConnectionState.Closed);
        _connection.BeginTransaction().Returns(_transaction);

        _sut = new UnitOfWork(_connection, _dataSets, _staging);
    }

    public void Dispose()
    {
        try { _sut.Dispose(); } catch { /* ignore */ }
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenConnectionIsNull()
    {
        // Act
        Action act = () => new UnitOfWork(null!, _dataSets, _staging);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("connection");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenDataSetsIsNull()
    {
        // Act
        Action act = () => new UnitOfWork(_connection, null!, _staging);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("dataSets");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenStagingIsNull()
    {
        // Act
        Action act = () => new UnitOfWork(_connection, _dataSets, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("dynamicTables");
    }

    [Fact]
    public void Begin_ShouldOpenConnectionAndBeginTransactionAndSetTransactions_WhenConnectionIsClosed()
    {
        // Act
        _sut.Begin();

        // Assert
        _connection.Received(1).Open();
        _connection.Received(1).BeginTransaction();
        _dataSets.Received(1).SetTransaction(_transaction);
        _staging.Received(1).SetTransaction(_transaction);
    }

    [Fact]
    public void Begin_ShouldNotOpenConnection_WhenConnectionAlreadyOpen()
    {
        _connection.State.Returns(ConnectionState.Open);

        // Act
        _sut.Begin();

        // Assert
        _connection.DidNotReceive().Open();
        _connection.Received(1).BeginTransaction();
        _dataSets.Received(1).SetTransaction(_transaction);
        _staging.Received(1).SetTransaction(_transaction);
    }

    [Fact]
    public void Commit_ShouldCommitAndDisposeAndClearTransactionOnRepositories_WhenTransactionExists()
    {
        // Arrange
        _sut.Begin();

        // Act
        _sut.Commit();

        // Assert
        _transaction.Received(1).Commit();
        _transaction.Received(1).Dispose();
        _dataSets.Received(1).SetTransaction(null);
        _staging.Received(1).SetTransaction(null);
    }

    [Fact]
    public void Rollback_ShouldRollbackAndDisposeAndClearTransactionOnRepositories_WhenTransactionExists()
    {
        // Arrange
        _sut.Begin();

        // Act
        _sut.Rollback();

        // Assert
        _transaction.Received(1).Rollback();
        _transaction.Received(1).Dispose();
        _dataSets.Received(1).SetTransaction(null);
        _staging.Received(1).SetTransaction(null);
    }

    [Fact]
    public void Dispose_ShouldDisposeTransactionAndConnection_WhenCalledAfterBegin()
    {
        // Arrange
        _sut.Begin();

        // Act
        _sut.Dispose();

        // Assert
        _transaction.Received(1).Dispose();
        _connection.Received(1).Dispose();
    }

    [Fact]
    public void Dispose_ShouldDisposeConnection_WhenNoTransactionExists()
    {
        // Act
        _sut.Dispose();

        // Assert
        _connection.Received(1).Dispose();

        _transaction.DidNotReceive().Dispose();
    }
}
