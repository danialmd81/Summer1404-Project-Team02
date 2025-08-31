using ETL.Application.Abstractions.Data;
using ETL.Application.Common;
using MediatR;

namespace ETL.Application.DataSet.RenameTable;

public class RenameTableCommandHandler : IRequestHandler<RenameTableCommand, Result>
{
    private readonly IUnitOfWork _uow;

    public RenameTableCommandHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Result> Handle(RenameTableCommand request, CancellationToken cancellationToken)
    {
        _uow.Begin();

        try
        {
            await _uow.DynamicTables.RenameTableAsync(request.OldTableName, request.NewTableName, cancellationToken);

            var dataSet = await _uow.DataSets.GetByTableNameAsync(request.OldTableName, cancellationToken);
            if (dataSet == null)
                return Result.Failure(Error.NotFound("TableRename.Failed", $"Dataset '{request.OldTableName}' not found!"));

            dataSet.Rename(request.NewTableName);
            await _uow.DataSets.UpdateAsync(dataSet, cancellationToken);

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
