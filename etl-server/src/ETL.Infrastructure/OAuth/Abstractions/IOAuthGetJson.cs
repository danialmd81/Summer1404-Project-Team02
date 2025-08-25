using System.Text.Json;
using ETL.Application.Common;

namespace ETL.Infrastructure.OAuth.Abstractions
{
    public interface IOAuthGetJson
    {
        Task<Result<JsonElement>> GetJsonAsync(string relativePath, CancellationToken ct = default);
    }
}
