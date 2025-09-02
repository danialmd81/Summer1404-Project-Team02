using ETL.Application.Common;
using ETL.Application.Common.DTOs;

namespace ETL.Application.Abstractions.UserServices;

public interface IOAuthUserReader
{
    Task<Result<UserDto>> GetByIdAsync(string userId, CancellationToken ct = default);
}
