using ETL.Application.Common;
using ETL.Application.Common.DTOs;
using MediatR;

namespace ETL.Application.Auth.LoginCallback;
public record LoginCallbackCommand(string Code, string RedirectPath) : IRequest<Result<TokenResponse>>;
