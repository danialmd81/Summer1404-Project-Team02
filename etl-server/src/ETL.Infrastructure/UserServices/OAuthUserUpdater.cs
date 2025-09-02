using ETL.Application.Abstractions.UserServices;
using ETL.Application.Common;
using ETL.Application.User.Edit;
using ETL.Infrastructure.OAuth.Abstractions;
using Microsoft.Extensions.Configuration;

namespace ETL.Infrastructure.UserServices;

public class OAuthUserUpdater : IOAuthUserUpdater
{
    private readonly IOAuthPutJson _putJson;
    private readonly IConfiguration _configuration;

    public OAuthUserUpdater(IOAuthPutJson putJson, IConfiguration configuration)
    {
        _putJson = putJson ?? throw new ArgumentNullException(nameof(putJson));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public async Task<Result> UpdateUserAsync(EditUserCommand command, CancellationToken ct = default)
    {

        var realm = _configuration["Authentication:Realm"];
        var path = $"/admin/realms/{Uri.EscapeDataString(realm)}/users/{Uri.EscapeDataString(command.UserId)}";

        var payload = new Dictionary<string, object?>();
        if (command.Username is not null) payload["username"] = command.Username;
        if (command.Email is not null) payload["email"] = command.Email;
        if (command.FirstName is not null) payload["firstName"] = command.FirstName;
        if (command.LastName is not null) payload["lastName"] = command.LastName;

        if (payload.Count == 0)
            return Result.Success();

        var res = await _putJson.PutJsonAsync(path, payload, ct);
        if (res.IsFailure)
            return res;

        return Result.Success();
    }
}
