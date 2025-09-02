using ETL.Application.Common;

namespace ETL.Application.Abstractions.Security;

public interface IAuthRestPasswordService
{
    Task<Result> ResetPasswordAsync(string userId, string newPassword, CancellationToken ct = default);
}
