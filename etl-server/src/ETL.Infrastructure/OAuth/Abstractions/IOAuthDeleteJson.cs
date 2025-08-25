using ETL.Application.Common;

namespace ETL.Infrastructure.OAuth.Abstractions
{
    public interface IOAuthDeleteJson
    {
        Task<Result> DeleteJsonAsync(string relativePath, object? content = null, CancellationToken ct = default);
    }
}
