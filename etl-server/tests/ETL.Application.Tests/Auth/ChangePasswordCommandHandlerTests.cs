using System.Security.Claims;
using ETL.Application.Abstractions.Security;
using ETL.Application.Auth.ChangePassword;
using ETL.Application.Auth.DTOs;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace ETL.Application.Tests.Auth;

public class ChangePasswordCommandHandlerTests
{
    private readonly IAuthCredentialValidator _credentialValidator;
    private readonly IAuthRestPasswordService _resetPasswordService;
    private readonly ChangePasswordCommandHandler _sut;

    public ChangePasswordCommandHandlerTests()
    {
        _credentialValidator = Substitute.For<IAuthCredentialValidator>();
        _resetPasswordService = Substitute.For<IAuthRestPasswordService>();
        _sut = new ChangePasswordCommandHandler(_credentialValidator, _resetPasswordService);
    }

    private static ClaimsPrincipal CreateUser(string userId = "123", string username = "testuser")
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim("preferred_username", username)
        };
        return new ClaimsPrincipal(new ClaimsIdentity(claims));
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenRequestIsNull()
    {
        var command = new ChangePasswordCommand(null!, CreateUser());

        var result = await _sut.Handle(command, default);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Auth.InvalidRequest");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenPasswordsDoNotMatch()
    {
        var dto = new ChangePasswordDto
        {
            CurrentPassword = "old",
            NewPassword = "new",
            ConfirmPassword = "different"
        };
        var command = new ChangePasswordCommand(dto, CreateUser());

        var result = await _sut.Handle(command, default);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Auth.PasswordMismatch");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserClaimsMissing()
    {
        var dto = new ChangePasswordDto
        {
            CurrentPassword = "old",
            NewPassword = "new",
            ConfirmPassword = "new"
        };
        var command = new ChangePasswordCommand(dto, new ClaimsPrincipal());

        var result = await _sut.Handle(command, default);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Auth.UserNotFound");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenCurrentPasswordInvalid()
    {
        var dto = new ChangePasswordDto
        {
            CurrentPassword = "wrong",
            NewPassword = "new",
            ConfirmPassword = "new"
        };
        var command = new ChangePasswordCommand(dto, CreateUser());

        _credentialValidator.ValidateCredentialsAsync("testuser", "wrong", default)
            .Returns(false);

        var result = await _sut.Handle(command, default);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Auth.InvalidCurrentPassword");
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenPasswordResetSucceeds()
    {
        var dto = new ChangePasswordDto
        {
            CurrentPassword = "old",
            NewPassword = "new",
            ConfirmPassword = "new"
        };
        var command = new ChangePasswordCommand(dto, CreateUser());

        _credentialValidator.ValidateCredentialsAsync("testuser", "old", default)
            .Returns(true);

        var result = await _sut.Handle(command, default);

        result.IsSuccess.Should().BeTrue();
        await _resetPasswordService.Received(1).ResetPasswordAsync("123", "new", default);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenResetPasswordThrows()
    {
        var dto = new ChangePasswordDto
        {
            CurrentPassword = "old",
            NewPassword = "new",
            ConfirmPassword = "new"
        };
        var command = new ChangePasswordCommand(dto, CreateUser());

        _credentialValidator.ValidateCredentialsAsync("testuser", "old", default)
            .Returns(true);

        _resetPasswordService.ResetPasswordAsync("123", "new", default)
            .Throws(new Exception("DB is down"));

        var result = await _sut.Handle(command, default);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Auth.ResetFailed");
        result.Error.Description.Should().Contain("DB is down");
    }
}