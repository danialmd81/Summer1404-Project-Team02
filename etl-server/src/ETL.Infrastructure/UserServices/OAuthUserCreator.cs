using ETL.Application.Abstractions.UserServices;
using ETL.Application.Common;
using ETL.Application.User;
using ETL.Infrastructure.OAuth.Abstractions;
using Microsoft.Extensions.Configuration;

namespace ETL.Infrastructure.UserServices;

public class OAuthUserCreator : IOAuthUserCreator
{
    private readonly IOAuthPostJsonWithResponse _postWithResponse;
    private readonly IConfiguration _configuration;

    public OAuthUserCreator(IOAuthPostJsonWithResponse postWithResponse, IConfiguration configuration)
    {
        _postWithResponse = postWithResponse ?? throw new ArgumentNullException(nameof(postWithResponse));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public async Task<Result<string>> CreateUserAsync(CreateUserCommand command, CancellationToken ct = default)
    {
        var realm = _configuration["Authentication:Realm"];

        var path = $"/admin/realms/{Uri.EscapeDataString(realm)}/users";

        var newUserPayload = new
        {
            username = command.Username,
            email = command.Email,
            firstName = command.FirstName,
            lastName = command.LastName,
            enabled = true,
            credentials = new[]
            {
                new { type = "password", value = command.Password, temporary = false }
            }
        };

        var respRes = await _postWithResponse.PostJsonForResponseAsync(path, newUserPayload, ct);
        if (respRes.IsFailure)
            return Result.Failure<string>(respRes.Error);

        using var resp = respRes.Value;

        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync(ct);
            return Result.Failure<string>(Error.Problem("OAuth.CreateUserFailed", $"Create user failed: {resp.StatusCode} - {body}"));
        }

        var location = resp.Headers.Location;
        if (location == null)
        {
            return Result.Failure<string>(Error.Problem("OAuth.NoLocationHeader", "Provider did not return Location header for created user."));
        }

        string newUserId;
        try
        {
            var segments = location.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            newUserId = segments[^1];
        }
        catch
        {
            return Result.Failure<string>(Error.Problem("OAuth.ParseUserIdFailed", "Failed to parse created user id from Location header."));
        }

        return Result.Success(newUserId);
    }
}
