using ETL.Application.Abstractions.UserServices;
using ETL.Application.Common;
using ETL.Application.Common.DTOs;
using ETL.Application.Common.Options;
using ETL.Infrastructure.OAuth.Abstractions;
using Microsoft.Extensions.Options;

namespace ETL.Infrastructure.UserServices;

public class OAuthUserReader : IOAuthUserReader
{
    private readonly IOAuthGetJson _getJson;
    private readonly AuthOptions _authOptions;

    public OAuthUserReader(IOAuthGetJson getJson, IOptions<AuthOptions> options)
    {
        _getJson = getJson ?? throw new ArgumentNullException(nameof(getJson));
        _authOptions = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    public async Task<Result<UserDto>> GetByIdAsync(string userId, CancellationToken ct = default)
    {
        var realm = _authOptions.Realm;

        var path = $"/admin/realms/{Uri.EscapeDataString(realm)}/users/{Uri.EscapeDataString(userId)}";

        var getRes = await _getJson.GetJsonAsync(path, ct);
        if (getRes.IsFailure)
            return Result.Failure<UserDto>(getRes.Error);

        var root = getRes.Value;

        var dto = new UserDto
        {
            Id = root.TryGetProperty("id", out var pId) ? pId.GetString() : null,
            Username = root.TryGetProperty("username", out var pUsername) ? pUsername.GetString() : null,
            Email = root.TryGetProperty("email", out var pEmail) ? pEmail.GetString() : null,
            FirstName = root.TryGetProperty("firstName", out var pFirst) ? pFirst.GetString() : null,
            LastName = root.TryGetProperty("lastName", out var pLast) ? pLast.GetString() : null,
        };

        return Result.Success(dto);
    }
}
