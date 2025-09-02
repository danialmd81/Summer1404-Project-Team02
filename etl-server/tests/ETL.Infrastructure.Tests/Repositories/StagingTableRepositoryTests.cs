using System.Data;
using System.Text;
using ETL.Infrastructure.Data.Abstractions;
using ETL.Infrastructure.Repositories;
using FluentAssertions;
using NSubstitute;
using SqlKata;

namespace ETL.Infrastructure.Tests.Repositories;

public class StagingTableRepositoryTests
{
    private readonly IDbConnection _dbConnection;
    private readonly IDbExecutor _executor;
    private readonly IQueryCompiler _compiler;
    private readonly IPostgresCopyAdapter _copyAdapter;
    private readonly StagingTableRepository _sut;

    public StagingTableRepositoryTests()
    {
        _dbConnection = Substitute.For<IDbConnection>();
        _executor = Substitute.For<IDbExecutor>();
        _compiler = Substitute.For<IQueryCompiler>();
        _copyAdapter = Substitute.For<IPostgresCopyAdapter>();

        _sut = new StagingTableRepository(_dbConnection, _executor, _compiler, _copyAdapter);
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenDbConnectionIsNull()
    {
        Action act = () => new StagingTableRepository(null!, _executor, _compiler, _copyAdapter);
        act.Should().Throw<ArgumentNullException>().WithParameterName("dbConnection");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenDbExecutorIsNull()
    {
        Action act = () => new StagingTableRepository(_dbConnection, null!, _compiler, _copyAdapter);
        act.Should().Throw<ArgumentNullException>().WithParameterName("dbExecutor");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenCompilerIsNull()
    {
        Action act = () => new StagingTableRepository(_dbConnection, _executor, null!, _copyAdapter);
        act.Should().Throw<ArgumentNullException>().WithParameterName("compiler");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenCopyAdapterIsNull()
    {
        Action act = () => new StagingTableRepository(_dbConnection, _executor, _compiler, null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("copyAdapter");
    }

    [Fact]
    public async Task CreateTableFromCsvAsync_ShouldThrow_WhenCsvIsEmpty()
    {
        // Arrange
        using var ms = new MemoryStream(); // empty stream
        // Act
        Func<Task> act = async () => await _sut.CreateTableFromCsvAsync("t", ms, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("CSV is empty");
    }

    [Fact]
    public async Task CreateTableFromCsvAsync_ShouldCreateTableAndCopyData_WhenCsvHasHeaderAndData_AndStreamIsSeekable()
    {
        // Arrange
        var csv = "col1,col2\r\nval1,val2\r\n";
        var bytes = Encoding.UTF8.GetBytes(csv);
        using var ms = new MemoryStream();
        ms.Write(bytes, 0, bytes.Length);
        ms.Position = 0;

        var sw = new StringWriter();
        _copyAdapter.BeginTextImportAsync(Arg.Any<IDbConnection>(), Arg.Any<string>())
            .Returns(Task.FromResult((TextWriter)sw));

        _executor.ExecuteAsync(Arg.Any<string>(), Arg.Any<object?>(), Arg.Any<IDbTransaction?>())
            .Returns(Task.CompletedTask);

        // Act
        await _sut.CreateTableFromCsvAsync("my_table", ms, CancellationToken.None);

        // Assert
        await _executor.Received(1).ExecuteAsync(Arg.Is<string>(s => s.Contains("\"my_table\"") && s.Contains("\"col1\"") && s.Contains("TEXT")), null, Arg.Any<IDbTransaction?>());

        // Assert
        await _copyAdapter.Received(1).BeginTextImportAsync(_dbConnection, Arg.Is<string>(sql => sql.Contains("COPY") && sql.Contains("\"col1\"")));
        sw.ToString().Should().Contain("col1,col2");
        sw.ToString().Should().Contain("val1,val2");
    }

    [Fact]
    public async Task CreateTableFromCsvAsync_ShouldCreateTableAndCopyData_WhenCsvIsNonSeekableStream()
    {
        // Arrange
        var csv = "a,b\r\n1,2\r\n";
        var bytes = Encoding.UTF8.GetBytes(csv);
        using var inner = new MemoryStream(bytes);
        using var nonSeekable = new NonSeekableReadOnlyStream(inner);

        var sw = new StringWriter();
        _copyAdapter.BeginTextImportAsync(Arg.Any<IDbConnection>(), Arg.Any<string>())
            .Returns(Task.FromResult((TextWriter)sw));

        _executor.ExecuteAsync(Arg.Any<string>(), Arg.Any<object?>(), Arg.Any<IDbTransaction?>())
            .Returns(Task.CompletedTask);

        // Act
        await _sut.CreateTableFromCsvAsync("tbl", nonSeekable, CancellationToken.None);

        // Assert
        await _executor.Received(1).ExecuteAsync(Arg.Is<string>(s => s.Contains("\"tbl\"")), null, Arg.Any<IDbTransaction?>());
        await _copyAdapter.Received(1).BeginTextImportAsync(_dbConnection, Arg.Any<string>());
        sw.ToString().Should().Contain("a,b");
    }

    [Fact]
    public async Task RenameTableAsync_ShouldCallExecuteAsync_WithSanitizedNames()
    {
        // Arrange
        _executor.ExecuteAsync(Arg.Any<string>(), Arg.Any<object?>(), Arg.Any<IDbTransaction?>()).Returns(Task.CompletedTask);

        // Act
        await _sut.RenameTableAsync("old-name", "new name");

        // Assert
        await _executor.Received(1).ExecuteAsync(Arg.Is<string>(s => s.Contains("ALTER TABLE") && s.Contains("\"oldname\"") && s.Contains("\"newname\"")), null, Arg.Any<IDbTransaction?>());
    }

    [Fact]
    public async Task RenameColumnAsync_ShouldCallExecuteAsync_WithSanitizedNames()
    {
        // Arrange
        _executor.ExecuteAsync(Arg.Any<string>(), Arg.Any<object?>(), Arg.Any<IDbTransaction?>()).Returns(Task.CompletedTask);

        // Act
        await _sut.RenameColumnAsync("tbl", "old-col", "new-col");

        // Assert
        await _executor.Received(1).ExecuteAsync(Arg.Is<string>(s => s.Contains("RENAME COLUMN") && s.Contains("\"oldcol\"") && s.Contains("\"newcol\"")), null, Arg.Any<IDbTransaction?>());
    }

    [Fact]
    public async Task DeleteTableAsync_ShouldCallExecuteAsync_WithSanitizedName()
    {
        // Arrange
        _executor.ExecuteAsync(Arg.Any<string>(), Arg.Any<object?>(), Arg.Any<IDbTransaction?>()).Returns(Task.CompletedTask);

        // Act
        await _sut.DeleteTableAsync("t!able");

        // Assert
        await _executor.Received(1).ExecuteAsync(Arg.Is<string>(s => s.Contains("DROP TABLE IF EXISTS") && s.Contains("\"table\"")), null, Arg.Any<IDbTransaction?>());
    }

    [Fact]
    public async Task DeleteColumnAsync_ShouldCallExecuteAsync_WithSanitizedNames()
    {
        // Arrange
        _executor.ExecuteAsync(Arg.Any<string>(), Arg.Any<object?>(), Arg.Any<IDbTransaction?>()).Returns(Task.CompletedTask);

        // Act
        await _sut.DeleteColumnAsync("t", "co-l");

        // Assert
        await _executor.Received(1).ExecuteAsync(Arg.Is<string>(s => s.Contains("DROP COLUMN") && s.Contains("\"col\"")), null, Arg.Any<IDbTransaction?>());
    }

    [Fact]
    public async Task ColumnExistsAsync_ShouldReturnTrue_WhenCountGreaterThanZero()
    {
        // Arrange
        var compiled = new CompiledQuery("SELECT COUNT(*) ...", new { });
        _compiler.Compile(Arg.Any<Query>()).Returns(compiled);

        _executor.ExecuteScalarAsync<int>(compiled.Sql, compiled.NamedBindings, Arg.Any<IDbTransaction?>(), Arg.Any<CancellationToken>())
                 .Returns(Task.FromResult(1));

        // Act
        var exists = await _sut.ColumnExistsAsync("t", "c");

        // Assert
        exists.Should().BeTrue();
        await _executor.Received(1).ExecuteScalarAsync<int>(compiled.Sql, compiled.NamedBindings, Arg.Any<IDbTransaction?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ColumnExistsAsync_ShouldReturnFalse_WhenCountIsZero()
    {
        // Arrange
        var compiled = new CompiledQuery("SELECT COUNT(*) ...", new { });
        _compiler.Compile(Arg.Any<Query>()).Returns(compiled);

        _executor.ExecuteScalarAsync<int>(compiled.Sql, compiled.NamedBindings, Arg.Any<IDbTransaction?>(), Arg.Any<CancellationToken>())
                 .Returns(Task.FromResult(0));

        // Act
        var exists = await _sut.ColumnExistsAsync("t", "c");

        // Assert
        exists.Should().BeFalse();
    }

    private sealed class NonSeekableReadOnlyStream : Stream
    {
        private readonly Stream _inner;
        public NonSeekableReadOnlyStream(Stream inner) { _inner = inner; }

        public override bool CanRead => _inner.CanRead;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => _inner.Length;
        public override long Position { get => _inner.Position; set => throw new NotSupportedException(); }

        public override void Flush() => _inner.Flush();
        public override int Read(byte[] buffer, int offset, int count) => _inner.Read(buffer, offset, count);
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        public override async Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            await _inner.CopyToAsync(destination, bufferSize, cancellationToken);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _inner.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
