using ETL.API.Infrastructure;
using ETL.Application.Common.Constants;
using ETL.Application.User.ChangeRole;
using ETL.Application.User.Create;
using ETL.Application.User.Delete;
using ETL.Application.User.GetAll;
using ETL.Application.User.GetById;
using ETL.Application.User.GetCurrent;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ETL.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IMediator _mediator;

    public UserController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    [Authorize]
    [HttpGet("profile")]
    public async Task<IActionResult> GetUserProfile(CancellationToken ct)
    {
        var dto = await _mediator.Send(new GetUserProfileQuery(User), ct);
        return Ok(dto);
    }

    [Authorize(Policy = Policy.CanReadUser)]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(string id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetUserByIdQuery(id), ct);
        return this.FromResult(result);
    }

    [Authorize(Policy = Policy.CanReadAllUsers)]
    [HttpGet("all")]
    public async Task<IActionResult> GetAllUsers([FromQuery] int? first, [FromQuery] int? max, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAllUsersQuery(first, max), ct);
        return this.FromResult(result);
    }

    [Authorize(Policy = Policy.CanCreateUser)]
    [HttpPost("create")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserCommand command, CancellationToken ct)
    {
        if (command is null)
            return BadRequest(new { error = "User.Create.InvalidRequest", message = "Request body is required." });

        var result = await _mediator.Send(command, ct);

        if (result.IsFailure)
            return this.ToActionResult(result.Error);

        var createdId = result.Value;
        var location = Url.Action(null, "User", new { id = createdId }) ?? $"/api/user/{createdId}";

        return Created(location, new { id = createdId, message = $"User '{command.Username}' created." });
    }

    [Authorize(Policy = Policy.CanChangeUserRole)]
    [HttpPatch("/change-role")]
    public async Task<IActionResult> ChangeUserRole([FromBody] ChangeUserRoleCommand request, CancellationToken ct)
    {
        var result = await _mediator.Send(request, ct);

        if (result.IsFailure)
            return this.ToActionResult(result.Error);

        return Ok(new { message = "User role updated." });
    }

    [Authorize(Policy = Policy.CanDeleteUser)]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(string id, CancellationToken ct)
    {
        var result = await _mediator.Send(new DeleteUserCommand(id), ct);
        return this.FromResult(result);
    }
}
