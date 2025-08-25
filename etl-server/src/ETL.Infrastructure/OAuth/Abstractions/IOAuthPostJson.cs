using ETL.Application.Common;

namespace ETL.Infrastructure.OAuth.Abstractions;

public interface IOAuthPostJson
{
    Task<Result> PostJsonAsync(string relativePath, object content, CancellationToken ct = default);
}
