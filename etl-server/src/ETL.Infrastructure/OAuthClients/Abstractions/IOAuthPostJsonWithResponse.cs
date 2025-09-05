namespace ETL.Infrastructure.OAuth.Abstractions;

public interface IOAuthPostJsonWithResponse
{
    Task<HttpResponseMessage> PostJsonForResponseAsync(string relativePath, object content, CancellationToken ct = default);
}
