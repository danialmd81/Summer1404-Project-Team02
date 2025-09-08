using ETL.Application.Abstractions.Data;
using ETL.Application.Abstractions.Repositories;
using ETL.Application.Common;
using ETL.Application.DataSet.RenameColumn;
using ETL.Domain.Entities;
using FluentAssertions;
using NSubstitute;

namespace ETL.Application.Tests.DataSet;

public class RenameColumnCommandHandlerTests
{
    private readonly IUnitOfWork _uow;
    private readonly IDataSetRepository _dataSets;
    private readonly IStagingTableRepository _stagingTables;
    private readonly RenameColumnCommandHandler _sut;

    public RenameColumnCommandHandlerTests()
    {
        _uow = Substitute.For<IUnitOfWork>();
        _dataSets = Substitute.For<IDataSetRepository>();
        _stagingTables = Substitute.For<IStagingTableRepository>();

        _uow.DataSetsRepo.Returns(_dataSets);
        _uow.StagingTablesRepo.Returns(_stagingTables);

        _sut = new RenameColumnCommandHandler(_uow);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenUnitOfWorkIsNull()
    {
        // Act
        Action act = () => new RenameColumnCommandHandler(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("uow");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenTableDoesNotExist()
    {
        // Arrange
        var command = new RenameColumnCommand("non_existing_table", "OldCol", "NewCol");
        _dataSets.GetByTableNameAsync(command.TableName, Arg.Any<CancellationToken>())
            .Returns((DataSetMetadata?)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Type.Should().Be(ErrorType.NotFound);
        result.Error.Code.Should().Be("ColumnRename.Failed");

        await _stagingTables.DidNotReceive().ColumnExistsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _stagingTables.DidNotReceive().RenameColumnAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        _uow.DidNotReceive().Begin();
        _uow.DidNotReceive().Commit();
        _uow.DidNotReceive().Rollback();
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenOldColumnDoesNotExist()
    {
        // Arrange
        var command = new RenameColumnCommand("existing_table", "OldCol", "NewCol");
        var dataSet = new DataSetMetadata(command.TableName, "user1");

        _dataSets.GetByTableNameAsync(command.TableName, Arg.Any<CancellationToken>())
            .Returns(dataSet);

        _stagingTables.ColumnExistsAsync(command.TableName, command.OldColumnName, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Type.Should().Be(ErrorType.NotFound);
        result.Error.Code.Should().Be("ColumnRename.Failed");

        _uow.DidNotReceive().Begin();
        _uow.DidNotReceive().Commit();
        _uow.DidNotReceive().Rollback();
        await _stagingTables.DidNotReceive().RenameColumnAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenNewColumnAlreadyExists()
    {
        // Arrange
        var command = new RenameColumnCommand("existing_table", "OldCol", "NewCol");
        var dataSet = new DataSetMetadata(command.TableName, "user1");

        _dataSets.GetByTableNameAsync(command.TableName, Arg.Any<CancellationToken>())
            .Returns(dataSet);

        _stagingTables.ColumnExistsAsync(command.TableName, command.OldColumnName, Arg.Any<CancellationToken>())
            .Returns(true);

        _stagingTables.ColumnExistsAsync(command.TableName, command.NewColumnName, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Type.Should().Be(ErrorType.Conflict);
        result.Error.Code.Should().Be("ColumnRename.Failed");

        _uow.DidNotReceive().Begin();
        _uow.DidNotReceive().Commit();
        _uow.DidNotReceive().Rollback();
        await _stagingTables.DidNotReceive().RenameColumnAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldRenameColumn_WhenAllChecksPass()
    {
        // Arrange
        var command = new RenameColumnCommand("existing_table", "OldCol", "NewCol");
        var dataSet = new DataSetMetadata(command.TableName, "user1");

        _dataSets.GetByTableNameAsync(command.TableName, Arg.Any<CancellationToken>())
            .Returns(dataSet);

        _stagingTables.ColumnExistsAsync(command.TableName, command.OldColumnName, Arg.Any<CancellationToken>())
            .Returns(true);

        _stagingTables.ColumnExistsAsync(command.TableName, command.NewColumnName, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _uow.Received(1).Begin();
        await _stagingTables.Received(1).RenameColumnAsync(command.TableName, command.OldColumnName, command.NewColumnName, Arg.Any<CancellationToken>());
        _uow.Received(1).Commit();
        _uow.DidNotReceive().Rollback();
    }

    [Fact]
    public async Task Handle_ShouldRollbackAndReturnFailure_WhenRenameThrowsException()
    {
        // Arrange
        var command = new RenameColumnCommand("existing_table", "OldCol", "NewCol");
        var dataSet = new DataSetMetadata(command.TableName, "user1");

        _dataSets.GetByTableNameAsync(command.TableName, Arg.Any<CancellationToken>())
            .Returns(dataSet);

        _stagingTables.ColumnExistsAsync(command.TableName, command.OldColumnName, Arg.Any<CancellationToken>())
            .Returns(true);

        _stagingTables.ColumnExistsAsync(command.TableName, command.NewColumnName, Arg.Any<CancellationToken>())
            .Returns(false);

        _stagingTables.RenameColumnAsync(command.TableName, command.OldColumnName, command.NewColumnName, Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new InvalidOperationException("DB failed"));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Type.Should().Be(ErrorType.Problem);
        result.Error.Code.Should().Be("ColumnRename.Failed");

        _uow.Received(1).Begin();
        _uow.Received(1).Rollback();
        _uow.DidNotReceive().Commit();
    }
}
