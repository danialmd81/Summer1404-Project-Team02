using ETL.Application.Common;

namespace ETL.Application.Abstractions;
public interface IOAuthUserDeleter
{
    Task<Result> DeleteUserAsync(string userId, CancellationToken ct = default);
}
