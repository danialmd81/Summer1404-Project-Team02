using ETL.Application.Common;

namespace ETL.Application.Abstractions
{
    public interface IOAuthRoleAssigner
    {
        Task<Result> AssignRolesAsync(string userId, IEnumerable<string> roleNames, CancellationToken ct = default);
    }
}
