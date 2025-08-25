using ETL.Application.Common;

namespace ETL.Infrastructure.OAuth.Abstractions;

public interface IOAuthPutJson
{
    Task<Result> PutJsonAsync(string relativePath, object content, CancellationToken ct = default);
}
