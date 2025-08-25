using ETL.Application.Common;

namespace ETL.Infrastructure.OAuth.Abstractions;

public interface IOAuthPostJsonWithResponse
{
    Task<Result<HttpResponseMessage>> PostJsonForResponseAsync(string relativePath, object content, CancellationToken ct = default);
}
