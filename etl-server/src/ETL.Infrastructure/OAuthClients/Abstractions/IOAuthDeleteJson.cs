namespace ETL.Infrastructure.OAuth.Abstractions;

public interface IOAuthDeleteJson
{
    Task DeleteJsonAsync(string relativePath, object? content = null, CancellationToken ct = default);
}
