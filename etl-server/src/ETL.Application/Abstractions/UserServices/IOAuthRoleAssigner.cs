using ETL.Application.Common;

namespace ETL.Application.Abstractions.UserServices
{
    public interface IOAuthRoleAssigner
    {
        Task<Result> AssignRoleAsync(string userId, string? roleName, CancellationToken ct = default);
    }
}
