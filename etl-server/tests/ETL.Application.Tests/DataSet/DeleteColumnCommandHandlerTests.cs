using ETL.Application.Abstractions.Data;
using ETL.Application.Abstractions.Repositories;
using ETL.Application.Common;
using ETL.Application.DataSet.DeleteColumn;
using ETL.Domain.Entities;
using FluentAssertions;
using NSubstitute;

namespace ETL.Application.Tests.DataSet;

public class DeleteColumnCommandHandlerTests
{
    private readonly IUnitOfWork _uow;
    private readonly IDataSetRepository _dataSets;
    private readonly IStagingTableRepository _stagingTables;
    private readonly DeleteColumnCommandHandler _sut;

    public DeleteColumnCommandHandlerTests()
    {
        _uow = Substitute.For<IUnitOfWork>();
        _dataSets = Substitute.For<IDataSetRepository>();
        _stagingTables = Substitute.For<IStagingTableRepository>();

        _uow.DataSetsRepo.Returns(_dataSets);
        _uow.StagingTablesRepo.Returns(_stagingTables);

        _sut = new DeleteColumnCommandHandler(_uow);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenUnitOfWorkIsNull()
    {
        // Act
        Action act = () => new DeleteColumnCommandHandler(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("uow");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenTableDoesNotExist()
    {
        // Arrange
        var command = new DeleteColumnCommand("non_existing_table", "some_column");
        _dataSets.GetByTableNameAsync(command.TableName, Arg.Any<CancellationToken>())
            .Returns((DataSetMetadata?)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Type.Should().Be(ErrorType.NotFound);
        result.Error.Code.Should().Be("ColumnDelete.Failed");

        _uow.DidNotReceive().Begin();
        await _stagingTables.DidNotReceive().DeleteColumnAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenColumnDoesNotExist()
    {
        // Arrange
        var command = new DeleteColumnCommand("existing_table", "non_existing_column");
        var dataSet = new DataSetMetadata(command.TableName, "user1");

        _dataSets.GetByTableNameAsync(command.TableName, Arg.Any<CancellationToken>())
            .Returns(dataSet);

        _stagingTables.ColumnExistsAsync(command.TableName, command.ColumnName, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Type.Should().Be(ErrorType.NotFound);
        result.Error.Code.Should().Be("ColumnDelete.Failed");

        _uow.DidNotReceive().Begin();
        await _stagingTables.DidNotReceive().DeleteColumnAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldDeleteColumn_WhenTableAndColumnExist()
    {
        // Arrange
        var command = new DeleteColumnCommand("existing_table", "existing_column");
        var dataSet = new DataSetMetadata(command.TableName, "user1");

        _dataSets.GetByTableNameAsync(command.TableName, Arg.Any<CancellationToken>())
            .Returns(dataSet);

        _stagingTables.ColumnExistsAsync(command.TableName, command.ColumnName, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _uow.Received(1).Begin();
        await _stagingTables.Received(1).DeleteColumnAsync(command.TableName, command.ColumnName, Arg.Any<CancellationToken>());
        _uow.Received(1).Commit();
    }

    [Fact]
    public async Task Handle_ShouldRollbackAndReturnFailure_WhenDeleteColumnThrows()
    {
        // Arrange
        var command = new DeleteColumnCommand("existing_table", "failing_column");
        var dataSet = new DataSetMetadata(command.TableName, "user1");

        _dataSets.GetByTableNameAsync(command.TableName, Arg.Any<CancellationToken>())
            .Returns(dataSet);

        _stagingTables.ColumnExistsAsync(command.TableName, command.ColumnName, Arg.Any<CancellationToken>())
            .Returns(true);

        _stagingTables.DeleteColumnAsync(command.TableName, command.ColumnName, Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new InvalidOperationException("DB failed"));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Type.Should().Be(ErrorType.Problem);
        result.Error.Code.Should().Be("ColumnDelete.Failed");

        _uow.Received(1).Begin();
        _uow.Received(1).Rollback();
        _uow.DidNotReceive().Commit();
    }
}
