namespace ETL.Infrastructure.OAuth.Abstractions;

public interface IOAuthPostJson
{
    Task PostJsonAsync(string relativePath, object content, CancellationToken ct = default);
}
