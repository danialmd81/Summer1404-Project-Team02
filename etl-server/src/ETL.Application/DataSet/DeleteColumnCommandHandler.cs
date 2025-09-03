using ETL.Application.Abstractions.Data;
using ETL.Application.Common;
using MediatR;

namespace ETL.Application.DataSet;

public record DeleteColumnCommand(string TableName, string ColumnName) : IRequest<Result>;

public sealed class DeleteColumnCommandHandler : IRequestHandler<DeleteColumnCommand, Result>
{
    private readonly IUnitOfWork _uow;

    public DeleteColumnCommandHandler(IUnitOfWork uow)
    {
        _uow = uow ?? throw new ArgumentNullException(nameof(uow));
    }

    public async Task<Result> Handle(DeleteColumnCommand request, CancellationToken cancellationToken)
    {
        var existing = await _uow.DataSets.GetByTableNameAsync(request.TableName, cancellationToken);
        if (existing == null)
        {
            return Result.Failure(
                Error.NotFound("ColumnDelete.Failed", $"Table '{request.TableName}' not  found!"));
        }

        var columnExist =
            _uow.StagingTables.ColumnExistsAsync(request.TableName, request.ColumnName, cancellationToken);
        if (!columnExist.Result)
        {
            return Result.Failure(
                Error.NotFound("ColumnDelete.Failed", $"Column '{request.ColumnName}' not found!"));
        }

        _uow.Begin();
        try
        {
            await _uow.StagingTables.DeleteColumnAsync(request.TableName, request.ColumnName, cancellationToken);
            _uow.Commit();
            return Result.Success();
        }
        catch (Exception ex)
        {
            _uow.Rollback();
            return Result.Failure(Error.Problem("ColumnDelete.Failed", ex.Message));
        }
    }
}
