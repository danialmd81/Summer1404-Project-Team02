using ETL.Contracts.Security;

namespace ETL.Application.Abstractions;
public interface IAuthService
{
    string BuildLoginUrl(string redirectPath);
    Task<TokenResponse> ExchangeCodeForTokensAsync(string code, string redirectPath, CancellationToken ct = default);
    Task LogoutAsync(string accessToken, string refreshToken, CancellationToken ct = default);
}