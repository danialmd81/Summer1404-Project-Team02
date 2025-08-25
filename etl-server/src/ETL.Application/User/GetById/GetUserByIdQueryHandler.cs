using ETL.Application.Abstractions.UserServices;
using ETL.Application.Common;
using ETL.Application.Common.DTOs;
using MediatR;

namespace ETL.Application.User.GetById;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, Result<UserDto>>
{
    private readonly IOAuthUserReader _userReader;
    private readonly IOAuthUserRoleGetter _roleGetter;

    public GetUserByIdQueryHandler(IOAuthUserReader userReader, IOAuthUserRoleGetter roleGetter)
    {
        _userReader = userReader ?? throw new ArgumentNullException(nameof(userReader));
        _roleGetter = roleGetter ?? throw new ArgumentNullException(nameof(roleGetter));
    }

    public async Task<Result<UserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UserId))
            return Result.Failure<UserDto>(Error.NotFound("User.InvalidId", "User id is required."));

        var userResult = await _userReader.GetByIdAsync(request.UserId, cancellationToken);
        if (userResult.IsFailure)
            return userResult;

        var user = userResult.Value;


        var roleResult = await _roleGetter.GetRoleForUserAsync(request.UserId, cancellationToken);
        if (roleResult.IsFailure)
            return Result.Failure<UserDto>(roleResult.Error);

        var role = roleResult.Value;

        user.Role = role;

        return Result.Success(user);
    }
}