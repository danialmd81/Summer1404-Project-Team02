using ETL.Application.Abstractions.UserServices;
using ETL.Application.Common;
using ETL.Application.Common.DTOs;
using ETL.Infrastructure.OAuth.Abstractions;
using Microsoft.Extensions.Configuration;

namespace ETL.Infrastructure.UserServices;

public class OAuthUserReader : IOAuthUserReader
{
    private readonly IOAuthGetJson _getJson;
    private readonly IConfiguration _configuration;

    public OAuthUserReader(IOAuthGetJson getJson, IConfiguration configuration)
    {
        _getJson = getJson ?? throw new ArgumentNullException(nameof(getJson));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public async Task<Result<UserDto>> GetByIdAsync(string userId, CancellationToken ct = default)
    {
        var realm = _configuration["Authentication:Realm"];

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
