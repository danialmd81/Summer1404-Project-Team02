using ETL.Application.Abstractions.Data;
using MediatR;

namespace ETL.Application.DataSet.DeleteColumn;

public class DeleteColumnCommandHandler : IRequestHandler<DeleteColumnCommand, Unit>
{
    private readonly IUnitOfWork _uow;

    public DeleteColumnCommandHandler(IUnitOfWork uow)
    {
        _uow = uow ?? throw new ArgumentNullException(nameof(uow));
    }

    public async Task<Unit> Handle(DeleteColumnCommand request, CancellationToken cancellationToken)
    {
        _uow.Begin();
        try
        {
            await _uow.StagingTables.DeleteColumnAsync(request.TableName, request.ColumnName, cancellationToken);
            _uow.Commit();
            return Unit.Value;
        }
        catch
        {
            _uow.Rollback();
            throw;
        }
    }
}
