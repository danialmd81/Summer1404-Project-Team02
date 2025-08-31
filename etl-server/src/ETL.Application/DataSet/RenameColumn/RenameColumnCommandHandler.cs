using ETL.Application.Abstractions.Data;
using ETL.Application.Common;
using MediatR;

namespace ETL.Application.DataSet.RenameColumn;

public class RenameColumnCommandHandler : IRequestHandler<RenameColumnCommand, Result>
{
    private readonly IUnitOfWork _uow;

    public RenameColumnCommandHandler(IUnitOfWork uow)
    {
        _uow = uow ?? throw new ArgumentNullException(nameof(uow));
    }

    public async Task<Result> Handle(RenameColumnCommand request, CancellationToken cancellationToken)
    {
        var existing = await _uow.DataSets.GetByTableNameAsync(request.TableName, cancellationToken);
        if (existing == null)
        {
            return Result.Failure(
                Error.NotFound("ColumnRename.Failed", $"Table '{request.TableName}' not  found!"));
        }

        var oldColumnExist = await 
            _uow.StagingTables.ColumnExistsAsync(request.TableName, request.OldColumnName, cancellationToken);
        if (!oldColumnExist)
        {
            return Result.Failure(Error.NotFound("ColumnRename.Failed",
                $"Column '{request.OldColumnName}' not  found!"));
        }

        var newColumnExist =
            await _uow.StagingTables.ColumnExistsAsync(request.TableName, request.NewColumnName, cancellationToken);
        if (newColumnExist)
        {
            return Result.Failure(Error.Conflict("ColumnRename.Failed",
                $"Column '{request.NewColumnName}' already exists."));
        }
        
        _uow.Begin();
        try
        {
            await _uow.StagingTables.RenameColumnAsync(request.TableName, request.OldColumnName, request.NewColumnName,
                cancellationToken);
            _uow.Commit();
            return Result.Success();
        }
        catch (Exception ex)
        {
            _uow.Rollback();
            return Result.Failure(Error.Problem("ColumnRename.Failed", ex.Message));
        }
    }
}