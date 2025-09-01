using ETL.Application.Abstractions.Data;
using ETL.Application.Abstractions.Repositories;
using ETL.Application.Common;
using ETL.Application.DataSet.UploadCsv;
using ETL.Domain.Entities;
using FluentAssertions;
using NSubstitute;

namespace ETL.Application.Tests.DataSet;

public class UploadCsvCommandHandlerTests
{
    private readonly IUnitOfWork _uow;
    private readonly IDataSetRepository _dataSets;
    private readonly IStagingTableRepository _stagingTables;
    private readonly UploadCsvCommandHandler _sut;

    public UploadCsvCommandHandlerTests()
    {
        _uow = Substitute.For<IUnitOfWork>();
        _dataSets = Substitute.For<IDataSetRepository>();
        _stagingTables = Substitute.For<IStagingTableRepository>();

        _uow.DataSets.Returns(_dataSets);
        _uow.StagingTables.Returns(_stagingTables);

        _sut = new UploadCsvCommandHandler(_uow);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenUnitOfWorkIsNull()
    {
        // Act
        Action act = () => new UploadCsvCommandHandler(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("uow");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenTableAlreadyExists()
    {
        // Arrange
        var command = new UploadCsvCommand("existing_table", new MemoryStream(), "user1");
        var existingDataSet = new DataSetMetadata(command.TableName, command.UserId);

        _dataSets.GetByTableNameAsync(command.TableName, Arg.Any<CancellationToken>())
            .Returns(existingDataSet);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Type.Should().Be(ErrorType.Conflict);
        result.Error.Code.Should().Be("FileUpload.Failed");

        await _stagingTables.DidNotReceive().CreateTableFromCsvAsync(Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<CancellationToken>());
        await _dataSets.DidNotReceive().AddAsync(Arg.Any<DataSetMetadata>(), Arg.Any<CancellationToken>());
        _uow.DidNotReceive().Begin();
        _uow.DidNotReceive().Commit();
        _uow.DidNotReceive().Rollback();
    }

    [Fact]
    public async Task Handle_ShouldCreateTableAndAddMetadata_WhenTableDoesNotExist()
    {
        // Arrange
        var command = new UploadCsvCommand("new_table", new MemoryStream(), "user1");

        _dataSets.GetByTableNameAsync(command.TableName, Arg.Any<CancellationToken>())
            .Returns((DataSetMetadata?)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _uow.Received(1).Begin();
        await _stagingTables.Received(1).CreateTableFromCsvAsync(command.TableName, command.FileStream, Arg.Any<CancellationToken>());
        await _dataSets.Received(1).AddAsync(Arg.Is<DataSetMetadata>(d => d.TableName == command.TableName && d.UploadedByUserId == command.UserId), Arg.Any<CancellationToken>());
        _uow.Received(1).Commit();
        _uow.DidNotReceive().Rollback();
    }

    [Fact]
    public async Task Handle_ShouldRollbackAndReturnFailure_WhenExceptionThrown()
    {
        // Arrange
        var command = new UploadCsvCommand("new_table", new MemoryStream(), "user1");

        _dataSets.GetByTableNameAsync(command.TableName, Arg.Any<CancellationToken>())
            .Returns((DataSetMetadata?)null);

        _stagingTables.CreateTableFromCsvAsync(command.TableName, command.FileStream, Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new InvalidOperationException("DB failed"));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Type.Should().Be(ErrorType.Problem);
        result.Error.Code.Should().Be("FileUpload.Failed");

        _uow.Received(1).Begin();
        _uow.Received(1).Rollback();
        _uow.DidNotReceive().Commit();
    }
}
