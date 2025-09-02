using ETL.Application.Common;

namespace ETL.Application.Abstractions.Security;
public interface IAuthLogoutService
{
    Task<Result> LogoutAsync(string accessToken, string refreshToken, CancellationToken ct = default);
}