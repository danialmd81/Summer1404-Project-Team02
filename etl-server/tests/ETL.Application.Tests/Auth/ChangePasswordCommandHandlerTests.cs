using System.Security.Claims;
using ETL.Application.Abstractions.Security;
using ETL.Application.Auth.ChangePassword;
using ETL.Application.Auth.DTOs;
using ETL.Application.Common;
using FluentAssertions;
using NSubstitute;

namespace ETL.Application.Tests.Auth
{
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

            var result = await _sut.Handle(command, CancellationToken.None);

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

            var result = await _sut.Handle(command, CancellationToken.None);

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

            var result = await _sut.Handle(command, CancellationToken.None);

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

            _credentialValidator
                .ValidateCredentialsAsync("testuser", "wrong", Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(false));

            var result = await _sut.Handle(command, CancellationToken.None);

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

            _credentialValidator
                .ValidateCredentialsAsync("testuser", "old", Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(true));

            _resetPasswordService
                .ResetPasswordAsync("123", "new", Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(Result.Success()));

            var result = await _sut.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            await _resetPasswordService.Received(1)
                .ResetPasswordAsync("123", "new", Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenResetPasswordReturnsFailure()
        {
            var dto = new ChangePasswordDto
            {
                CurrentPassword = "old",
                NewPassword = "new",
                ConfirmPassword = "new"
            };
            var command = new ChangePasswordCommand(dto, CreateUser());

            _credentialValidator
                .ValidateCredentialsAsync("testuser", "old", Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(true));

            var error = Error.Failure("Auth.ResetFailed", "DB is down");
            _resetPasswordService
                .ResetPasswordAsync("123", "new", Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(Result.Failure(error)));

            var result = await _sut.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be("Auth.ResetFailed");
            result.Error.Description.Should().Contain("DB is down");
        }
    }
}
