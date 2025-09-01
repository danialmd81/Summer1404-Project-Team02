using ETL.Application.Abstractions.Data;
using ETL.Application.Abstractions.Repositories;
using ETL.Application.Common;
using ETL.Application.DataSet.RenameTable;
using ETL.Domain.Entities;
using FluentAssertions;
using NSubstitute;

namespace ETL.Application.Tests.DataSet;

public class RenameTableCommandHandlerTests
{
    private readonly IUnitOfWork _uow;
    private readonly IDataSetRepository _dataSets;
    private readonly IStagingTableRepository _stagingTables;
    private readonly RenameTableCommandHandler _sut;

    public RenameTableCommandHandlerTests()
    {
        _uow = Substitute.For<IUnitOfWork>();
        _dataSets = Substitute.For<IDataSetRepository>();
        _stagingTables = Substitute.For<IStagingTableRepository>();

        _uow.DataSets.Returns(_dataSets);
        _uow.StagingTables.Returns(_stagingTables);

        _sut = new RenameTableCommandHandler(_uow);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenUnitOfWorkIsNull()
    {
        // Act
        Action act = () => new RenameTableCommandHandler(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("uow");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenOldTableDoesNotExist()
    {
        // Arrange
        var command = new RenameTableCommand("old_table", "new_table");
        _dataSets.GetByTableNameAsync(command.OldTableName, Arg.Any<CancellationToken>())
            .Returns((DataSetMetadata?)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Type.Should().Be(ErrorType.NotFound);
        result.Error.Code.Should().Be("TableRename.Failed");

        await _stagingTables.DidNotReceive().RenameTableAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        _uow.DidNotReceive().Begin();
        _uow.DidNotReceive().Commit();
        _uow.DidNotReceive().Rollback();
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenNewTableAlreadyExists()
    {
        // Arrange
        var command = new RenameTableCommand("old_table", "new_table");
        var oldDataSet = new DataSetMetadata(command.OldTableName, "user1");
        var newDataSet = new DataSetMetadata(command.NewTableName, "user1");

        _dataSets.GetByTableNameAsync(command.OldTableName, Arg.Any<CancellationToken>())
            .Returns(oldDataSet);
        _dataSets.GetByTableNameAsync(command.NewTableName, Arg.Any<CancellationToken>())
            .Returns(newDataSet);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Type.Should().Be(ErrorType.Conflict);
        result.Error.Code.Should().Be("TableRename.Failed");

        await _stagingTables.DidNotReceive().RenameTableAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        _uow.DidNotReceive().Begin();
        _uow.DidNotReceive().Commit();
        _uow.DidNotReceive().Rollback();
    }

    [Fact]
    public async Task Handle_ShouldRenameTable_WhenAllChecksPass()
    {
        // Arrange
        var command = new RenameTableCommand("old_table", "new_table");
        var oldDataSet = new DataSetMetadata(command.OldTableName, "user1");

        _dataSets.GetByTableNameAsync(command.OldTableName, Arg.Any<CancellationToken>())
            .Returns(oldDataSet);
        _dataSets.GetByTableNameAsync(command.NewTableName, Arg.Any<CancellationToken>())
            .Returns((DataSetMetadata?)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _uow.Received(1).Begin();
        await _stagingTables.Received(1).RenameTableAsync(command.OldTableName, command.NewTableName, Arg.Any<CancellationToken>());
        await _dataSets.Received(1).UpdateAsync(oldDataSet, Arg.Any<CancellationToken>());
        _uow.Received(1).Commit();
        _uow.DidNotReceive().Rollback();
    }

    [Fact]
    public async Task Handle_ShouldRollbackAndReturnFailure_WhenRenameThrowsException()
    {
        // Arrange
        var command = new RenameTableCommand("old_table", "new_table");
        var oldDataSet = new DataSetMetadata(command.OldTableName, "user1");

        _dataSets.GetByTableNameAsync(command.OldTableName, Arg.Any<CancellationToken>())
            .Returns(oldDataSet);
        _dataSets.GetByTableNameAsync(command.NewTableName, Arg.Any<CancellationToken>())
            .Returns((DataSetMetadata?)null);

        _stagingTables.RenameTableAsync(command.OldTableName, command.NewTableName, Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new InvalidOperationException("DB failed"));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Type.Should().Be(ErrorType.Problem);
        result.Error.Code.Should().Be("TableRename.Failed");

        _uow.Received(1).Begin();
        _uow.Received(1).Rollback();
        _uow.DidNotReceive().Commit();
    }
}
