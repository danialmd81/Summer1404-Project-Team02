using ETL.Application.Common;
using FluentAssertions;

namespace ETL.Application.Tests.Common;

public class ErrorTests
{
    [Fact]
    public void Failure_ShouldCreateFailureError()
    {
        var error = Error.Failure("Code1", "Bad");

        error.Type.Should().Be(ErrorType.Failure);
        error.Code.Should().Be("Code1");
        error.Description.Should().Be("Bad");
    }

    [Fact]
    public void NotFound_ShouldCreateNotFoundError()
    {
        var error = Error.NotFound("Code2", "Missing");

        error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public void Problem_ShouldCreateProblemError()
    {
        var error = Error.Problem("Code3", "Server down");

        error.Type.Should().Be(ErrorType.Problem);
    }

    [Fact]
    public void Conflict_ShouldCreateConflictError()
    {
        var error = Error.Conflict("Code4", "Already exists");

        error.Type.Should().Be(ErrorType.Conflict);
    }
}