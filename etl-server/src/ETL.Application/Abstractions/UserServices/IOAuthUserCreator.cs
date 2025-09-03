using ETL.Application.Common;
using ETL.Application.User;

namespace ETL.Application.Abstractions.UserServices;

public interface IOAuthUserCreator
{
    Task<Result<string>> CreateUserAsync(CreateUserCommand command, CancellationToken ct = default);
}
