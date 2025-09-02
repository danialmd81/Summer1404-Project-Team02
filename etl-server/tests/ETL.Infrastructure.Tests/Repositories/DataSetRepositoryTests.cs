using System.Data;
using ETL.Domain.Entities;
using ETL.Infrastructure.Data.Abstractions;
using ETL.Infrastructure.Repositories;
using FluentAssertions;
using NSubstitute;
using SqlKata;

namespace ETL.Infrastructure.Tests.Repositories;

public class DataSetRepositoryTests
{
    private readonly IQueryCompiler _compiler;
    private readonly IDbExecutor _executor;
    private readonly CompiledQuery _defaultCompiled;
    private readonly DataSetRepository _sut;

    public DataSetRepositoryTests()
    {
        _compiler = Substitute.For<IQueryCompiler>();
        _executor = Substitute.For<IDbExecutor>();

        _defaultCompiled = new CompiledQuery("SELECT 1", new { });

        _compiler.Compile(Arg.Any<Query>()).Returns(_defaultCompiled);

        _sut = new DataSetRepository(_executor, _compiler);
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenDbExecutorIsNull()
    {
        // Act
        Action act = () => new DataSetRepository(null!, _compiler);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("dbExecutor");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenCompilerIsNull()
    {
        // Act
        Action act = () => new DataSetRepository(_executor, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("compiler");
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnRows_WhenQuerySucceeds()
    {
        // Arrange
        var expected = new[]
        {
            new DataSetMetadata("t1", "user1")
        };

        _executor.QueryAsync<DataSetMetadata>(_defaultCompiled.Sql, _defaultCompiled.NamedBindings, Arg.Any<IDbTransaction?>())
                 .Returns(Task.FromResult((IEnumerable<DataSetMetadata>)expected));

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().ContainSingle();
        await _executor.Received(1).QueryAsync<DataSetMetadata>(_defaultCompiled.Sql, _defaultCompiled.NamedBindings, Arg.Any<IDbTransaction?>());
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnRow_WhenFound()
    {
        // Arrange
        var expected = new DataSetMetadata("t", "user1");

        _executor.QuerySingleOrDefaultAsync<DataSetMetadata?>(_defaultCompiled.Sql, _defaultCompiled.NamedBindings, Arg.Any<IDbTransaction?>())
                 .Returns(Task.FromResult<DataSetMetadata?>(expected));

        // Act
        var result = await _sut.GetByIdAsync(expected.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(expected.Id);
        await _executor.Received(1).QuerySingleOrDefaultAsync<DataSetMetadata?>(_defaultCompiled.Sql, _defaultCompiled.NamedBindings, Arg.Any<IDbTransaction?>());
    }

    [Fact]
    public async Task GetByTableNameAsync_ShouldReturnNull_WhenNotFound()
    {
        // Arrange
        _executor.QueryFirstOrDefaultAsync<DataSetMetadata>(_defaultCompiled.Sql, _defaultCompiled.NamedBindings, Arg.Any<IDbTransaction?>())
                 .Returns(Task.FromResult<DataSetMetadata?>(null));

        // Act
        var result = await _sut.GetByTableNameAsync("not-exist");

        // Assert
        result.Should().BeNull();
        await _executor.Received(1).QueryFirstOrDefaultAsync<DataSetMetadata>(_defaultCompiled.Sql, _defaultCompiled.NamedBindings, Arg.Any<IDbTransaction?>());
    }

    [Fact]
    public async Task AddAsync_ShouldCallExecuteAsync_WithCompiledSql()
    {
        // Arrange
        _executor.ExecuteAsync(_defaultCompiled.Sql, _defaultCompiled.NamedBindings, Arg.Any<IDbTransaction?>()).Returns(Task.CompletedTask);

        var dto = new DataSetMetadata("t", "user1");

        // Act
        await _sut.AddAsync(dto);

        // Assert
        await _executor.Received(1).ExecuteAsync(_defaultCompiled.Sql, _defaultCompiled.NamedBindings, Arg.Any<IDbTransaction?>());
    }

    [Fact]
    public async Task UpdateAsync_ShouldCallExecuteAsync_WithCompiledSql()
    {
        // Arrange
        _executor.ExecuteAsync(_defaultCompiled.Sql, _defaultCompiled.NamedBindings, Arg.Any<IDbTransaction?>()).Returns(Task.CompletedTask);

        var dto = new DataSetMetadata("t", "user1");

        // Act
        await _sut.UpdateAsync(dto);

        // Assert
        await _executor.Received(1).ExecuteAsync(_defaultCompiled.Sql, _defaultCompiled.NamedBindings, Arg.Any<IDbTransaction?>());
    }

    [Fact]
    public async Task DeleteAsync_ShouldCallExecuteAsync_WithCompiledSql()
    {
        // Arrange
        _executor.ExecuteAsync(_defaultCompiled.Sql, _defaultCompiled.NamedBindings, Arg.Any<IDbTransaction?>()).Returns(Task.CompletedTask);

        var dto = new DataSetMetadata("t", "user1");

        // Act
        await _sut.DeleteAsync(dto);

        // Assert
        await _executor.Received(1).ExecuteAsync(_defaultCompiled.Sql, _defaultCompiled.NamedBindings, Arg.Any<IDbTransaction?>());
    }
}
