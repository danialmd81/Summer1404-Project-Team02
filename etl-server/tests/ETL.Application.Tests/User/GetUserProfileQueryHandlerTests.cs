using System.Security.Claims;
using ETL.Application.User.GetCurrent;
using FluentAssertions;

namespace ETL.Application.Tests.User;

public class GetUserProfileQueryHandlerTests
{
    private readonly GetUserProfileQueryHandler _sut;

    public GetUserProfileQueryHandlerTests()
    {
        _sut = new GetUserProfileQueryHandler();
    }

    private static ClaimsPrincipal CreateUser(
        string? id = "123",
        string? username = "testuser",
        string? email = "test@mail.com",
        string? firstName = "John",
        string? lastName = "Doe",
        params string[] roles)
    {
        var claims = new List<Claim>();

        if (id != null) claims.Add(new Claim(ClaimTypes.NameIdentifier, id));
        if (username != null) claims.Add(new Claim("preferred_username", username));
        if (email != null) claims.Add(new Claim(ClaimTypes.Email, email));
        if (firstName != null) claims.Add(new Claim(ClaimTypes.GivenName, firstName));
        if (lastName != null) claims.Add(new Claim(ClaimTypes.Surname, lastName));
        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        return new ClaimsPrincipal(new ClaimsIdentity(claims));
    }

    [Fact]
    public async Task Handle_ShouldMapAllClaims_WhenPresent()
    {
        var user = CreateUser(roles: new[] { "Admin", "User" });
        var command = new GetUserProfileQuery(user);

        var result = await _sut.Handle(command, default);

        result.Id.Should().Be("123");
        result.Username.Should().Be("testuser");
        result.Email.Should().Be("test@mail.com");
        result.FirstName.Should().Be("John");
        result.LastName.Should().Be("Doe");
        result.Roles.Should().Contain(new[] { "Admin", "User" });
    }

    [Fact]
    public async Task Handle_ShouldReturnNullProperties_WhenClaimsMissing()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity()); // no claims
        var command = new GetUserProfileQuery(user);

        var result = await _sut.Handle(command, default);

        result.Id.Should().BeNull();
        result.Username.Should().BeNull();
        result.Email.Should().BeNull();
        result.FirstName.Should().BeNull();
        result.LastName.Should().BeNull();
        result.Roles.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldReturnMultipleRoles_WhenTheyExist()
    {
        var user = CreateUser(roles: new[] { "Manager", "Editor" });
        var command = new GetUserProfileQuery(user);

        var result = await _sut.Handle(command, default);

        result.Roles.Should().BeEquivalentTo(new[] { "Manager", "Editor" });
    }
}