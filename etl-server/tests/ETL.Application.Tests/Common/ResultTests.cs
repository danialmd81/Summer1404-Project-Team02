using ETL.Application.Common;
using FluentAssertions;

namespace ETL.Application.Tests.Common;

public class ResultTests
{
    [Fact]
    public void Success_ShouldCreateSuccessResult()
    {
        var result = Result.Success();

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().Be(Error.None);
    }

    [Fact]
    public void Failure_ShouldCreateFailureResult()
    {
        var error = Error.Failure("Code", "Description");

        var result = Result.Failure(error);

        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenInvalidCombination()
    {
        Action act1 = () => new Result(true, Error.Failure("code", "desc"));// success with error
        Action act2 = () => new Result(false, Error.None); // failure with no error

        act1.Should().Throw<ArgumentException>();
        act2.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void SuccessOfT_ShouldContainValue()
    {
        var result = Result.Success("hello");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("hello");
    }

    [Fact]
    public void FailureOfT_ShouldThrow_WhenAccessingValue()
    {
        var error = Error.Failure("x", "bad");
        var result = Result.Failure<string>(error);

        var act = () => { var _ = result.Value; };

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ImplicitConversion_ShouldReturnSuccess_WhenValueNotNull()
    {
        Result<string> result = "abc";

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("abc");
    }

    [Fact]
    public void ImplicitConversion_ShouldReturnFailure_WhenValueIsNull()
    {
        Result<string> result = (string?)null;

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Error.NullValue);
    }

    [Fact]
    public void ValidationFailure_ShouldReturnFailureResult()
    {
        var error = Error.Failure("Validation", "Invalid input");

        var result = Result<string>.ValidationFailure(error);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(error);
    }
}
