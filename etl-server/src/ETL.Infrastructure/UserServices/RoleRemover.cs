using ETL.Application.Abstractions.UserServices;
using ETL.Application.Common;
using ETL.Infrastructure.OAuth.Abstractions;
using Microsoft.Extensions.Configuration;

namespace ETL.Infrastructure.UserServices
{
    public class RoleRemover : IRoleRemover
    {
        private readonly IOAuthGetJsonArray _getArray;
        private readonly IOAuthDeleteJson _delete;
        private readonly IConfiguration _configuration;

        public RoleRemover(IOAuthGetJsonArray getArray, IOAuthDeleteJson delete, IConfiguration configuration)
        {
            _getArray = getArray ?? throw new ArgumentNullException(nameof(getArray));
            _delete = delete ?? throw new ArgumentNullException(nameof(delete));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<Result> RemoveAllRealmRolesAsync(string userId, CancellationToken ct = default)
        {
            var realm = _configuration["Authentication:Realm"];

            var rolesPath = $"/admin/realms/{Uri.EscapeDataString(realm)}/users/{Uri.EscapeDataString(userId)}/role-mappings/realm";

            var getRes = await _getArray.GetJsonArrayAsync(rolesPath, ct);
            if (getRes.IsFailure)
                return Result.Failure(getRes.Error);

            var roles = getRes.Value;
            if (roles == null || roles.Count == 0)
                return Result.Success();

            var delRes = await _delete.DeleteJsonAsync(rolesPath, roles, ct);
            if (delRes.IsFailure)
                return Result.Failure(delRes.Error);

            return Result.Success();
        }
    }
}
