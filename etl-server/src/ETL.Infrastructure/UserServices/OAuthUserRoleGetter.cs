using ETL.Application.Abstractions.UserServices;
using ETL.Application.Common;
using ETL.Infrastructure.OAuth.Abstractions;
using Microsoft.Extensions.Configuration;

namespace ETL.Infrastructure.UserServices
{
    public class OAuthUserRoleGetter : IOAuthUserRoleGetter
    {
        private readonly IOAuthGetJsonArray _getArray;
        private readonly IConfiguration _configuration;

        public OAuthUserRoleGetter(IOAuthGetJsonArray getArray, IConfiguration configuration)
        {
            _getArray = getArray ?? throw new ArgumentNullException(nameof(getArray));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<Result<string?>> GetRoleForUserAsync(string userId, CancellationToken ct = default)
        {
            var realm = _configuration["Authentication:Realm"];

            var path = $"/admin/realms/{Uri.EscapeDataString(realm)}/users/{Uri.EscapeDataString(userId)}/role-mappings/realm";

            var getRes = await _getArray.GetJsonArrayAsync(path, ct);
            if (getRes.IsFailure)
            {
                if (getRes.Error.Type == ErrorType.NotFound)
                    return Result.Success<string?>(null);

                return Result.Failure<string?>(getRes.Error);
            }

            var arr = getRes.Value;
            if (arr == null || arr.Count == 0)
                return Result.Success<string?>(null);

            foreach (var el in arr)
            {
                if (el.TryGetProperty("name", out var nameProp))
                {
                    var rn = nameProp.GetString();
                    if (!string.IsNullOrEmpty(rn))
                        return Result.Success<string?>(rn);
                }
            }

            return Result.Success<string?>(null);
        }
    }
}
