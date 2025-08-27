using System.Security.Claims;
using ETL.Application.Common.Constants;
using ETL.Application.Data.DeleteColumn;
using ETL.Application.Data.DeleteTable;
using ETL.Application.Data.GetDataSets;
using ETL.Application.Data.RenameColumn;
using ETL.Application.Data.RenameTable;
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
    
    [HttpPut("rename")]
    [Authorize(Policy = Policy.CanUploadFile)]
    public async Task<IActionResult> RenameTable([FromBody] RenameTableCommand request, CancellationToken cancellationToken)
    {
        await _mediator.Send(request, cancellationToken);

        return Ok("Table has been renamed.");
    }


    [HttpDelete("{tableName}")]
    [Authorize(Policy = Policy.CanUploadFile)]
    public async Task<IActionResult> DeleteTable(DeleteTableCommand request, CancellationToken cancellationToken)
    {
        await _mediator.Send(request, cancellationToken);

        return Ok("Table has been deleted.");
    }
    
    [HttpDelete("{tableName}/columns/{columnName}")]
    [Authorize(Policy = Policy.CanUploadFile)]
    public async Task<IActionResult> DeleteColumn(DeleteColumnCommand request, CancellationToken cancellationToken)
    {
        await _mediator.Send(request, cancellationToken);
        return Ok("Column has been deleted.");
    }
    
    [HttpPut("{tableName}/columns/rename")]
    [Authorize(Policy = Policy.CanUploadFile)]
    public async Task<IActionResult> RenameColumn(RenameColumnCommand request, CancellationToken cancellationToken)
    {
        await _mediator.Send(request, cancellationToken);
        return Ok("Column has been renamed.");
    }
}