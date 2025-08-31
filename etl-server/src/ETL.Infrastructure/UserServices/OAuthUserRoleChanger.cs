using ETL.Application.Abstractions.UserServices;
using ETL.Application.Common;

namespace ETL.Infrastructure.UserServices;

public class OAuthUserRoleChanger : IOAuthUserRoleChanger
{
    private readonly IOAuthRoleRemover _roleRemover;
    private readonly IOAuthRoleAssigner _roleAssigner;

    public OAuthUserRoleChanger(IOAuthRoleRemover roleRemover, IOAuthRoleAssigner roleAssigner)
    {
        _roleRemover = roleRemover ?? throw new ArgumentNullException(nameof(roleRemover));
        _roleAssigner = roleAssigner ?? throw new ArgumentNullException(nameof(roleAssigner));
    }

    public async Task<Result> ChangeRoleAsync(string userId, string newRoleName, CancellationToken ct = default)
    {
        var rm = await _roleRemover.RemoveAllRealmRolesAsync(userId, ct);
        if (rm.IsFailure) return rm;

        var assign = await _roleAssigner.AssignRoleAsync(userId, newRoleName, ct);
        if (assign.IsFailure)
            return Result.Failure(Error.Problem("User.ChangeRole.AssignFailed", $"Failed to assign role '{newRoleName}': {assign.Error.Code} - {assign.Error.Description}"));

        return Result.Success();
    }
}
