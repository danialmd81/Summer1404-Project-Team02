using ETL.Application.Common;
using ETL.Application.Common.DTOs;

namespace ETL.Application.Abstractions.Security;

public interface IAuthCodeForTokenExchanger
{
    Task<Result<TokenResponse>> ExchangeCodeForTokensAsync(string code, string redirectPath, CancellationToken ct = default);
}
