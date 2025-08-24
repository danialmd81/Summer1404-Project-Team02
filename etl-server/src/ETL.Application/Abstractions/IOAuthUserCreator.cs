using ETL.Application.Common;
using ETL.Application.User.Create;

namespace ETL.Application.Abstractions
{
    public interface IOAuthUserCreator
    {
        Task<Result<string>> CreateUserAsync(CreateUserCommand command, CancellationToken ct = default);
    }
}
