using ETL.Application.Common;

namespace ETL.Application.Abstractions.UserServices;
public interface IRoleRemover
{
    public Task<Result> RemoveAllRealmRolesAsync(string userId, CancellationToken ct = default);
}
