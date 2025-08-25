using ETL.Application.Common;
using ETL.Application.Common.Constants;
using ETL.Application.User.Create;
using ETL.Application.User.Delete;
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
        _mediator = mediator;
    }

    [Authorize]
    [HttpGet("profile")]
    public async Task<IActionResult> GetUserProfile(CancellationToken ct)
    {
        var dto = await _mediator.Send(new GetUserProfileQuery(User), ct);
        return Ok(dto);
    }

    [Authorize(Policy = Policy.CanCreateUser)]
    [HttpPost("create")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserCommand command, CancellationToken ct)
    {
        if (command is null)
            return BadRequest(new { error = "User.Create.InvalidRequest", message = "Request body is required." });

        var result = await _mediator.Send(command, ct);

        if (result.IsFailure)
        {
            var err = result.Error;
            return err.Type switch
            {
                ErrorType.Validation => BadRequest(new { error = err.Code, message = err.Description }),
                ErrorType.NotFound => NotFound(new { error = err.Code, message = err.Description }),
                ErrorType.Conflict => Conflict(new { error = err.Code, message = err.Description }),
                ErrorType.Problem => StatusCode(500, new { error = err.Code, message = err.Description }),
                _ => StatusCode(500, new { error = err.Code, message = err.Description })
            };
        }

        var createdId = result.Value;

        var location = Url.Action(null, "User", new { id = createdId }) ?? $"/api/user/{createdId}";

        return Created(location, new { id = createdId, message = $"User '{command.Username}' created." });
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(string id, CancellationToken ct)
    {
        var result = await _mediator.Send(new DeleteUserCommand(id), ct);

        if (result.IsFailure)
        {
            var err = result.Error;
            return err.Type switch
            {
                ErrorType.Validation => BadRequest(new { error = err.Code, message = err.Description }),
                ErrorType.NotFound => NotFound(new { error = err.Code, message = err.Description }),
                ErrorType.Problem => StatusCode(500, new { error = err.Code, message = err.Description }),
                _ => StatusCode(500, new { error = err.Code, message = err.Description })
            };
        }

        return Ok(new { message = "User deleted successfully." });
    }
}
