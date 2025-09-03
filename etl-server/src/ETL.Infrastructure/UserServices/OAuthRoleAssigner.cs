using ETL.Application.Abstractions.UserServices;
using ETL.Application.Common;
using ETL.Application.Common.Options;
using ETL.Infrastructure.OAuth.Abstractions;
using Microsoft.Extensions.Options;

namespace ETL.Infrastructure.UserServices;

public class OAuthRoleAssigner : IOAuthRoleAssigner
{
    private readonly IOAuthGetJson _getJson;
    private readonly IOAuthPostJson _postJson;
    private readonly AuthOptions _authOptions;

    public OAuthRoleAssigner(IOAuthGetJson getJson, IOAuthPostJson postJson, IOptions<AuthOptions> options)
    {
        _getJson = getJson ?? throw new ArgumentNullException(nameof(getJson));
        _postJson = postJson ?? throw new ArgumentNullException(nameof(postJson));
        _authOptions = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    public async Task<Result> AssignRoleAsync(string userId, string roleName, CancellationToken ct = default)
    {

        if (string.IsNullOrWhiteSpace(roleName))
            return Result.Success();

        var realm = _authOptions.Realm;

        var getRolePath = $"/admin/realms/{Uri.EscapeDataString(realm)}/roles/{Uri.EscapeDataString(roleName)}";

        var roleRes = await _getJson.GetJsonAsync(getRolePath, ct);
        if (roleRes.IsFailure)
        {
            if (roleRes.Error.Type == ErrorType.NotFound)
                return Result.Failure(Error.NotFound("OAuth.RoleNotFound", $"Role '{roleName}' not found."));

            return Result.Failure(roleRes.Error);
        }

        var assignPath = $"/admin/realms/{Uri.EscapeDataString(realm)}/users/{Uri.EscapeDataString(userId)}/role-mappings/realm";
        var assignRes = await _postJson.PostJsonAsync(assignPath, new[] { roleRes.Value }, ct);
        if (assignRes.IsFailure)
            return Result.Failure(assignRes.Error);

        return Result.Success();
    }
}
