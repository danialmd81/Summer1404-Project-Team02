namespace ETL.API.Services.Abstraction;

public interface ISsoAdminService
{
    Task<string?> GetAdminAccessTokenAsync();

}
