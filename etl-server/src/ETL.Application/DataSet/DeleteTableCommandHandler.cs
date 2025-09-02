using ETL.Application.Abstractions.Data;
using ETL.Application.Common;
using MediatR;

namespace ETL.Application.DataSet.DeleteTable;

public record DeleteTableCommand(string TableName) : IRequest<Result>;

public sealed class DeleteTableCommandHandler : IRequestHandler<DeleteTableCommand, Result>
{
    private readonly IUnitOfWork _uow;

    public DeleteTableCommandHandler(IUnitOfWork uow)
    {
        _uow = uow ?? throw new ArgumentNullException(nameof(uow));
    }

    public async Task<Result> Handle(DeleteTableCommand request, CancellationToken cancellationToken)
    {
        var existingDataSet = await _uow.DataSets.GetByTableNameAsync(request.TableName, cancellationToken);
        if (existingDataSet == null)
        {
            return Result.Failure(
                Error.NotFound("TableRemove.Failed", $"Table '{request.TableName}' not  found!"));
        }
        
        _uow.Begin();

        try
        {
            await _uow.StagingTables.DeleteTableAsync(request.TableName, cancellationToken);
            await _uow.DataSets.DeleteAsync(existingDataSet, cancellationToken);
            
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