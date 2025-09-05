namespace ETL.Infrastructure.OAuth.Abstractions;

public interface IOAuthPutJson
{
    Task PutJsonAsync(string relativePath, object content, CancellationToken ct = default);
}
