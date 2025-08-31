using ETL.Application.Abstractions.Data;
using ETL.Application.Common;
using MediatR;

namespace ETL.Application.DataSet.DeleteTable;

public class DeleteTableCommandHandler : IRequestHandler<DeleteTableCommand, Result>
{
    private readonly IUnitOfWork _uow;

    public DeleteTableCommandHandler(IUnitOfWork uow)
    {
        _uow = uow ?? throw new ArgumentNullException(nameof(uow));
    }

    public async Task<Result> Handle(DeleteTableCommand request, CancellationToken cancellationToken)
    {
        _uow.Begin();

        try
        {
            await _uow.StagingTables.DeleteTableAsync(request.TableName, cancellationToken);

            var dataSet = await _uow.DataSets.GetByTableNameAsync(request.TableName, cancellationToken);
            if (dataSet != null)
            {
                await _uow.DataSets.DeleteAsync(dataSet, cancellationToken);
            }

            _uow.Commit();
            return Result.Success();
        }
        catch (Exception ex)
        {
            _uow.Rollback();
            return Result.Failure(Error.Problem("TableRemove.Failed", ex.Message));
        }
    }
}