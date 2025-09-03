using ETL.Application.Abstractions.UserServices;
using ETL.Application.Common;
using ETL.Application.Common.Options;
using ETL.Infrastructure.OAuth.Abstractions;
using Microsoft.Extensions.Options;

namespace ETL.Infrastructure.UserServices;

public class OAuthUserDeleter : IOAuthUserDeleter
{
    private readonly IOAuthGetJson _getJson;
    private readonly IOAuthDeleteJson _delete;
    private readonly AuthOptions _authOptions;

    public OAuthUserDeleter(IOAuthGetJson getJson, IOAuthDeleteJson delete, IOptions<AuthOptions> options)
    {
        _getJson = getJson ?? throw new ArgumentNullException(nameof(getJson));
        _delete = delete ?? throw new ArgumentNullException(nameof(delete));
        _authOptions = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    public async Task<Result> DeleteUserAsync(string userId, CancellationToken ct = default)
    {
        var realm = _authOptions.Realm;

        var getPath = $"/admin/realms/{Uri.EscapeDataString(realm)}/users/{Uri.EscapeDataString(userId)}";

        var getRes = await _getJson.GetJsonAsync(getPath, ct);
        if (getRes.IsFailure)
        {
            if (getRes.Error.Type == ErrorType.NotFound)
                return Result.Failure(Error.NotFound("OAuth.UserNotFound", $"User '{userId}' not found."));

            return Result.Failure(getRes.Error);
        }

        var deletePath = getPath;
        var delRes = await _delete.DeleteJsonAsync(deletePath, null, ct);
        if (delRes.IsFailure)
            return Result.Failure(delRes.Error);

        return Result.Success();
    }
}
