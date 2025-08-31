using ETL.Application.Common;

namespace ETL.Application.Abstractions.UserServices;
public interface IOAuthRoleRemover
{
    public Task<Result> RemoveAllRealmRolesAsync(string userId, CancellationToken ct = default);
}
