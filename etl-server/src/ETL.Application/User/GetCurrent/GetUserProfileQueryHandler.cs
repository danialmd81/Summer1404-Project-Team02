using System.Security.Claims;
using ETL.Application.Common.DTOs;
using MediatR;

namespace ETL.Application.User.GetCurrent;
public class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, UserProfileDto>
{
    public Task<UserProfileDto> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        var user = request.User;

        var userProfile = new UserProfileDto
        {
            Id = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value,
            Username = user.Claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value,
            Email = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value,
            FirstName = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value,
            LastName = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Surname)?.Value,
            Roles = user.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value)
        };

        return Task.FromResult(userProfile);
    }
}
