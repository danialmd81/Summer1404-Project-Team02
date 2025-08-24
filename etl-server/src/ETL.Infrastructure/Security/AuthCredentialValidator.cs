using ETL.Application.Abstractions.Security;
using Microsoft.Extensions.Configuration;

namespace ETL.Infrastructure.Security;

public class AuthCredentialValidator : IAuthCredentialValidator
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public AuthCredentialValidator(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task<bool> ValidateCredentialsAsync(string username, string password, CancellationToken ct = default)
    {
        var httpClient = _httpClientFactory.CreateClient();
        var tokenEndpoint = $"{_configuration["Authentication:Authority"]}/protocol/openid-connect/token";
        var clientId = _configuration["Authentication:ClientId"];
        var clientSecret = _configuration["Authentication:ClientSecret"];

        var body = new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["client_id"] = clientId,
            ["client_secret"] = clientSecret,
            ["username"] = username,
            ["password"] = password
        };

        var response = await httpClient.PostAsync(tokenEndpoint, new FormUrlEncodedContent(body), ct);

        return response.IsSuccessStatusCode;
    }
}
