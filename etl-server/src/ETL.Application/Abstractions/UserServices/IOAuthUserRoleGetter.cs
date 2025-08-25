using ETL.Application.Common;

namespace ETL.Application.Abstractions.UserServices;

public interface IOAuthUserRoleGetter
{
    Task<Result<string?>> GetRoleForUserAsync(string userId, CancellationToken ct = default);
}
