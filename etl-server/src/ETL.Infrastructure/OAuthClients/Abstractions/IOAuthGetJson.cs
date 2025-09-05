using System.Text.Json;

namespace ETL.Infrastructure.OAuth.Abstractions;

public interface IOAuthGetJson
{
    Task<JsonElement> GetJsonAsync(string relativePath, CancellationToken ct = default);
}
