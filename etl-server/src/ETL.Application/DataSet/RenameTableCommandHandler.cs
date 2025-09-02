using ETL.Application.Abstractions.Data;
using ETL.Application.Common;
using MediatR;

namespace ETL.Application.DataSet.RenameTable;

public record RenameTableCommand(string OldTableName, string NewTableName) : IRequest<Result>;

public sealed class RenameTableCommandHandler : IRequestHandler<RenameTableCommand, Result>
{
    private readonly IUnitOfWork _uow;

    public RenameTableCommandHandler(IUnitOfWork uow)
    {
        _uow = uow ?? throw new ArgumentNullException(nameof(uow));
    }

    public async Task<Result> Handle(RenameTableCommand request, CancellationToken cancellationToken)
    {
        var existingDataSet = await _uow.DataSets.GetByTableNameAsync(request.OldTableName, cancellationToken);
        if (existingDataSet == null)
        {
            return Result.Failure(
                Error.NotFound("TableRename.Failed", $"Table '{request.OldTableName}' not found!"));
        }
        
        var newDataSet = await _uow.DataSets.GetByTableNameAsync(request.NewTableName, cancellationToken);
        if (newDataSet != null)
        {
            return Result.Failure(Error.Conflict("TableRename.Failed",
                $"Table '{request.NewTableName}' already exists."));
        }
        
        _uow.Begin();
        try
        {
            await _uow.StagingTables.RenameTableAsync(request.OldTableName, request.NewTableName, cancellationToken);

            existingDataSet.Rename(request.NewTableName);
            await _uow.DataSets.UpdateAsync(existingDataSet, cancellationToken);

            _uow.Commit();
            return Result.Success();
        }
        catch (Exception e)
        {
            _uow.Rollback();
            return Result.Failure(Error.Problem("TableRename.Failed", e.Message));
        }
    }
}
