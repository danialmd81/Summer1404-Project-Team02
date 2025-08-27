using System.Security.Claims;
using ETL.Application.Common.Constants;
using ETL.Application.Data.GetDataSets;
using ETL.Application.Data.UploadCsv;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ETL.API.Controllers;

[ApiController]
[Route("api/datasets")]
public class DataSetsController : ControllerBase
{
    private readonly ISender _mediator;

    public DataSetsController(ISender mediator) => _mediator = mediator;

    [HttpPost("upload")]
    [Authorize(Policy = Policy.CanUploadFile)]
    public async Task<IActionResult> Upload(IFormFile file, [FromForm] string tableName, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0) return BadRequest("No file uploaded.");

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";

        var cmd = new UploadCsvCommand(tableName, file.OpenReadStream(), userId);
        var id = await _mediator.Send(cmd, cancellationToken);

        return Ok(new { DataSetId = id });
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetDataSetsQuery(), cancellationToken);
        return Ok(result);
    }
}