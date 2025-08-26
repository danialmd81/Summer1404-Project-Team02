using ETL.Application.Abstractions.UserServices;
using ETL.Application.Common;
using ETL.Application.Common.DTOs;
using MediatR;

namespace ETL.Application.User.GetAll;

public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, Result<IEnumerable<UserDto>>>
{
    private readonly IOAuthAllUserReader _allUserReader;
    private readonly IOAuthUserRoleGetter _roleGetter;

    private const int DefaultMaxConcurrency = 8;

    public GetAllUsersQueryHandler(IOAuthAllUserReader allUserReader, IOAuthUserRoleGetter roleGetter)
    {
        _allUserReader = allUserReader ?? throw new ArgumentNullException(nameof(allUserReader));
        _roleGetter = roleGetter ?? throw new ArgumentNullException(nameof(roleGetter));
    }

    public async Task<Result<IEnumerable<UserDto>>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var usersResult = await _allUserReader.GetAllAsync(request.First, request.Max, cancellationToken);
        if (usersResult.IsFailure)
            return Result.Failure<IEnumerable<UserDto>>(usersResult.Error);

        var users = usersResult.Value;
        if (users.Count == 0)
            return Result.Success<IEnumerable<UserDto>>(Array.Empty<UserDto>());

        var semaphore = new SemaphoreSlim(DefaultMaxConcurrency, DefaultMaxConcurrency);
        var tasks = users.Select(async user =>
        {
            await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var roleResult = await _roleGetter.GetRoleForUserAsync(user.Id ?? string.Empty, cancellationToken).ConfigureAwait(false);
                return (user, roleResult);
            }
            finally
            {
                semaphore.Release();
            }
        }).ToList();

        await Task.WhenAll(tasks).ConfigureAwait(false);

        var failures = tasks.Select(t => t.Result).Where(x => x.roleResult.IsFailure).ToList();
        if (failures.Any())
        {
            var first = failures[0];
            return Result.Failure<IEnumerable<UserDto>>(Error.Problem(
                "User.GetAll.RoleFetchFailed",
                $"Failed to fetch role for user '{first.user.Id}': {first.roleResult.Error.Code} - {first.roleResult.Error.Description}"
            ));
        }

        foreach (var (user, roleResult) in tasks.Select(t => t.Result))
        {
            user.Role = roleResult.Value;
        }

        return Result.Success<IEnumerable<UserDto>>(users.Where(u => u.Role is not null));
    }
}