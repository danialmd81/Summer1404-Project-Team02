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
        _uow.Begin();
        try
        {
            await _uow.StagingTables.RenameColumnAsync(request.TableName, request.OldColumnName, request.NewColumnName, cancellationToken);
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