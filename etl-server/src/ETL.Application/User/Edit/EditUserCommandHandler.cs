using ETL.Application.Abstractions.UserServices;
using ETL.Application.Common;
using MediatR;

namespace ETL.Application.User.Edit;

public class EditUserCommandHandler : IRequestHandler<EditUserCommand, Result>
{
    private readonly IOAuthUserUpdater _userUpdater;

    public EditUserCommandHandler(IOAuthUserUpdater userUpdater)
    {
        _userUpdater = userUpdater ?? throw new ArgumentNullException(nameof(userUpdater));
    }

    public async Task<Result> Handle(EditUserCommand request, CancellationToken cancellationToken)
    {
        var hasChanges = request.Username is not null
                      || request.Email is not null
                      || request.FirstName is not null
                      || request.LastName is not null;

        if (!hasChanges)
            return Result.Failure(Error.Validation("User.Edit.NoChanges", "At least one updatable field must be provided."));

        try
        {
            var res = await _userUpdater.UpdateUserAsync(
                request,
                cancellationToken);

            return res;
        }
        catch (Exception ex)
        {
            return Result.Failure(Error.Problem("User.Edit.Failed", ex.Message));
        }
    }
}
