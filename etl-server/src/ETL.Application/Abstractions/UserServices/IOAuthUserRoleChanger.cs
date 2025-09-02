using ETL.Application.Common;

namespace ETL.Application.Abstractions.UserServices
{
    public interface IOAuthUserRoleChanger
    {
        Task<Result> ChangeRoleAsync(string userId, string? newRoleName, CancellationToken ct = default);
    }
}
