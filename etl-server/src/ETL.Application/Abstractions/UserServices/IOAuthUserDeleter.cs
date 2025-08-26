using ETL.Application.Common;

namespace ETL.Application.Abstractions.UserServices;
public interface IOAuthUserDeleter
{
    Task<Result> DeleteUserAsync(string userId, CancellationToken ct = default);
}
