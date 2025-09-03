using ETL.Application.Abstractions.Data;
using ETL.Application.Abstractions.Repositories;
using ETL.Application.Common;
using ETL.Application.DataSet;
using ETL.Domain.Entities;
using FluentAssertions;
using NSubstitute;

namespace ETL.Application.Tests.DataSet;

public class DeleteTableCommandHandlerTests
{
    private readonly IUnitOfWork _uow;
    private readonly IDataSetRepository _dataSets;
    private readonly IStagingTableRepository _stagingTables;
    private readonly DeleteTableCommandHandler _sut;

    public DeleteTableCommandHandlerTests()
    {
        _uow = Substitute.For<IUnitOfWork>();
        _dataSets = Substitute.For<IDataSetRepository>();
        _stagingTables = Substitute.For<IStagingTableRepository>();

        _uow.DataSets.Returns(_dataSets);
        _uow.StagingTables.Returns(_stagingTables);

        _sut = new DeleteTableCommandHandler(_uow);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenUnitOfWorkIsNull()
    {
        // Act
        Action act = () => new DeleteTableCommandHandler(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("uow");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenTableDoesNotExist()
    {
        // Arrange
        var command = new DeleteTableCommand("non_existing_table");
        _dataSets.GetByTableNameAsync(command.TableName, Arg.Any<CancellationToken>())
            .Returns((DataSetMetadata?)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Type.Should().Be(ErrorType.NotFound);
        result.Error.Code.Should().Be("TableRemove.Failed");
        await _stagingTables.DidNotReceive().DeleteTableAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _dataSets.DidNotReceive().DeleteAsync(Arg.Any<DataSetMetadata>(), Arg.Any<CancellationToken>());
        _uow.DidNotReceive().Begin();
    }

    [Fact]
    public async Task Handle_ShouldDeleteTableAndMetadata_WhenTableExists()
    {
        // Arrange
        var command = new DeleteTableCommand("existing_table");
        var dataSet = new DataSetMetadata(command.TableName, "user1");

        _dataSets.GetByTableNameAsync(command.TableName, Arg.Any<CancellationToken>())
            .Returns(dataSet);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _uow.Received(1).Begin();
        await _stagingTables.Received(1).DeleteTableAsync(command.TableName, Arg.Any<CancellationToken>());
        await _dataSets.Received(1).DeleteAsync(dataSet, Arg.Any<CancellationToken>());
        _uow.Received(1).Commit();
    }

    [Fact]
    public async Task Handle_ShouldRollbackAndReturnFailure_WhenExceptionThrown()
    {
        // Arrange
        var command = new DeleteTableCommand("failing_table");
        var dataSet = new DataSetMetadata(command.TableName, "user1");

        _dataSets.GetByTableNameAsync(command.TableName, Arg.Any<CancellationToken>())
            .Returns(dataSet);

        _stagingTables.DeleteTableAsync(command.TableName, Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new InvalidOperationException("DB failed"));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Type.Should().Be(ErrorType.Problem);
        result.Error.Code.Should().Be("TableRemove.Failed");

        _uow.Received(1).Begin();
        _uow.Received(1).Rollback();
        _uow.DidNotReceive().Commit();
    }
}