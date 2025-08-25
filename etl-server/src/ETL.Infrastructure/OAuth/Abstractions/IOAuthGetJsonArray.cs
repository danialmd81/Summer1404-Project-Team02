using System.Text.Json;
using ETL.Application.Common;

namespace ETL.Infrastructure.OAuth.Abstractions;

public interface IOAuthGetJsonArray
{
    Task<Result<List<JsonElement>>> GetJsonArrayAsync(string relativePath, CancellationToken ct = default);
}
