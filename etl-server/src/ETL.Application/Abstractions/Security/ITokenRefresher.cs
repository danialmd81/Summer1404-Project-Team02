using ETL.Application.Common;
using ETL.Application.Common.DTOs;

namespace ETL.Application.Abstractions.Security;

public interface ITokenRefresher
{
    Task<Result<TokenResponse?>> RefreshAsync(string refreshToken, CancellationToken ct = default);
}
