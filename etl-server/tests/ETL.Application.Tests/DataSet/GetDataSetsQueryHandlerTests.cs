using ETL.Application.Abstractions.Data;
using ETL.Application.Abstractions.Repositories;
using ETL.Application.DataSet;
using ETL.Domain.Entities;
using FluentAssertions;
using NSubstitute;

namespace ETL.Application.Tests.DataSet;

public class GetDataSetsQueryHandlerTests
{
    private readonly IUnitOfWork _uow;
    private readonly IDataSetRepository _dataSets;
    private readonly GetDataSetsQueryHandler _sut;

    public GetDataSetsQueryHandlerTests()
    {
        _uow = Substitute.For<IUnitOfWork>();
        _dataSets = Substitute.For<IDataSetRepository>();

        _uow.DataSets.Returns(_dataSets);

        _sut = new GetDataSetsQueryHandler(_uow);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenUnitOfWorkIsNull()
    {
        // Act
        Action act = () => new GetDataSetsQueryHandler(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("uow");
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoDataSetsExist()
    {
        // Arrange
        _dataSets.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Enumerable.Empty<DataSetMetadata>());

        var query = new GetDataSetsQuery();

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldReturnMappedDataSets_WhenDataSetsExist()
    {
        // Arrange
        var dataSets = new[]
        {
            new DataSetMetadata("table1", "user1"),
            new DataSetMetadata("table2", "user2")
        };

        _dataSets.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(dataSets);

        var query = new GetDataSetsQuery();

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Select(d => d.TableName).Should().BeEquivalentTo("table1", "table2");
        result.Value.Select(d => d.UploadedByUserId).Should().BeEquivalentTo("user1", "user2");
    }
}
