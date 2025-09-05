using System.Text.Json;
using ETL.Application.Abstractions.UserServices;
using ETL.Application.Common.DTOs;
using ETL.Application.Common.Options;
using ETL.Infrastructure.OAuthClients.Abstractions;
using Microsoft.Extensions.Options;

namespace ETL.Infrastructure.UserServices;

public class OAuthAllUserReader : IOAuthAllUserReader
{
    private readonly IOAuthGetJsonArray _getArray;
    private readonly AuthOptions _authOptions;

    public OAuthAllUserReader(IOAuthGetJsonArray getArray, IOptions<AuthOptions> options)
    {
        _getArray = getArray ?? throw new ArgumentNullException(nameof(getArray));
        _authOptions = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    public async Task<List<UserDto>> GetAllAsync(int? first = null, int? max = null, CancellationToken ct = default)
    {
        var realm = _authOptions.Realm;
        var path = $"/admin/realms/{Uri.EscapeDataString(realm)}/users";
        var query = new List<string>();
        if (first.HasValue) query.Add($"first={first.Value}");
        if (max.HasValue) query.Add($"max={max.Value}");
        if (query.Count > 0) path = $"{path}?{string.Join('&', query)}";

        // NOTE: underlying IOAuthGetJsonArray now returns List<JsonElement> or throws
        var list = await _getArray.GetJsonArrayAsync(path, ct) ?? new List<JsonElement>();

        var users = new List<UserDto>(list.Count);
        foreach (var el in list)
        {
            var dto = new UserDto
            {
                Id = el.TryGetProperty("id", out var pId) ? pId.GetString() : null,
                Username = el.TryGetProperty("username", out var pUsername) ? pUsername.GetString() : null,
                Email = el.TryGetProperty("email", out var pEmail) ? pEmail.GetString() : null,
                FirstName = el.TryGetProperty("firstName", out var pFirst) ? pFirst.GetString() : null,
                LastName = el.TryGetProperty("lastName", out var pLast) ? pLast.GetString() : null,
            };

            users.Add(dto);
        }

        return users;
    }
}
