using System.Text.Json;

namespace ETL.Infrastructure.OAuth.Abstractions;

public interface IOAuthGetJsonArray
{
    Task<List<JsonElement>> GetJsonArrayAsync(string relativePath, CancellationToken ct = default);
}
