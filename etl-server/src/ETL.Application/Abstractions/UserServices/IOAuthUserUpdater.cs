using ETL.Application.Common;
using ETL.Application.User;

namespace ETL.Application.Abstractions.UserServices;

public interface IOAuthUserUpdater
{
    Task<Result> UpdateUserAsync(EditUserCommand command, CancellationToken ct = default);
}
