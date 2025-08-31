using ETL.Application.Abstractions.Data;
using MediatR;

namespace ETL.Application.DataSet.RenameColumn;

public class RenameColumnCommandHandler : IRequestHandler<RenameColumnCommand, Unit>
{
    private readonly IUnitOfWork _uow;

    public RenameColumnCommandHandler(IUnitOfWork uow)
    {
        _uow = uow ?? throw new ArgumentNullException(nameof(uow));
    }

    public async Task<Unit> Handle(RenameColumnCommand request, CancellationToken cancellationToken)
    {
        _uow.Begin();
        try
        {
            await _uow.DynamicTables.RenameColumnAsync(request.TableName, request.OldColumnName, request.NewColumnName, cancellationToken);
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