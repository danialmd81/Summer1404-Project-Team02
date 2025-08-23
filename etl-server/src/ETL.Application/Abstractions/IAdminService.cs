using ETL.Contracts.Security;

namespace ETL.Application.Abstractions;
public interface IAdminService
{
    Task<string?> GetAdminAccessTokenAsync(CancellationToken ct = default);
    Task<string> CreateUserAsync(string adminAccessToken, CreateUserRequest request, CancellationToken ct = default);
    Task AssignRealmRolesAsync(string adminAccessToken, string userId, IEnumerable<string> roleNames, CancellationToken ct = default);
}