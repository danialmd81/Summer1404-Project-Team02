using ETL.Application.Abstractions.UserServices;
using ETL.Application.Common;
using ETL.Infrastructure.OAuth.Abstractions;
using Microsoft.Extensions.Configuration;

namespace ETL.Infrastructure.UserServices
{
    public class OAuthUserDeleter : IOAuthUserDeleter
    {
        private readonly IOAuthGetJson _getJson;
        private readonly IOAuthDeleteJson _delete;
        private readonly IConfiguration _configuration;

        public OAuthUserDeleter(IOAuthGetJson getJson, IOAuthDeleteJson delete, IConfiguration configuration)
        {
            _getJson = getJson ?? throw new ArgumentNullException(nameof(getJson));
            _delete = delete ?? throw new ArgumentNullException(nameof(delete));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<Result> DeleteUserAsync(string userId, CancellationToken ct = default)
        {
            var realm = _configuration["Authentication:Realm"];

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
}
