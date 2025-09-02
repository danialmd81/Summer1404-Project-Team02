using ETL.Application.Common;
using ETL.Application.User.Edit;

namespace ETL.Application.Abstractions.UserServices;

public interface IOAuthUserUpdater
{
    Task<Result> UpdateUserAsync(EditUserCommand command, CancellationToken ct = default);
}
